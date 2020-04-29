using System.Collections;
using UnityEngine;
using nuitrack;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public class InitEvent : UnityEvent<NuitrackInitState>
{
}

public class NuitrackManager : MonoBehaviour
{
    // [SerializeField]
    // bool
    // depthModuleOn = false,
    // userTrackerModuleOn = false,
    // skeletonTrackerModuleOn = true;

    //private bool firstTime = false; //in order to prevent double NuitrackInit() calls on startup (in Awake and in OnApplicationPause)
    private static nuitrack.SkeletonTracker skeletonTracker;
    private static nuitrack.SkeletonData skeletonData;
    public static nuitrack.SkeletonData SkeletonData {get { return skeletonData; }}
    private static nuitrack.UserTracker userTracker;
    private static int numUsers;
    public static int NumUsers {get {return numUsers; }}
    private static nuitrack.User[] users;
    public static nuitrack.User[] Users {get { return users; }}
    private static nuitrack.ColorSensor colourSensor;
    private static nuitrack.ColorFrame colourFrame;
    public static nuitrack.ColorFrame ColourFrame {get { return colourFrame; }}
    public static event nuitrack.ColorSensor.OnUpdate onColorUpdate;
    private static NuitrackInitState initState = NuitrackInitState.INIT_NUITRACK_MANAGER_NOT_INSTALLED;
    public static NuitrackInitState InitState {get { return initState; }}


    [SerializeField] InitEvent initEvent;


    void Awake()
    {
        initState = NuitrackAndroidLoader.InitNuitrackLibraries();

        if (initEvent != null) // Triggers for any listeners 
        {
            initEvent.Invoke(initState);
        }

        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.targetFrameRate = 60;

        if (initState == NuitrackInitState.INIT_OK)
        {
            NuitrackInit();

            nuitrack.Nuitrack.SetConfigValue("AstraProPerseeDepthProvider.RGB.Width","1280");
            nuitrack.Nuitrack.SetConfigValue("AstraProPerseeDepthProvider.RGB.Height","720");
            nuitrack.Nuitrack.SetConfigValue("AstraProPerseeDepthProvider.Depth.Width","640");
            nuitrack.Nuitrack.SetConfigValue("AstraProPerseeDepthProvider.Depth.Height","480");
            nuitrack.Nuitrack.SetConfigValue("Skeletonization.MaxDistance","7000");
        }
    }

    void NuitrackInit()
    {
        nuitrack.Nuitrack.Init();
    
        // Create and setup all required modules
        skeletonTracker = nuitrack.SkeletonTracker.Create();
        userTracker = nuitrack.UserTracker.Create();
        colourSensor = nuitrack.ColorSensor.Create();

        // Add event handlers for all modules created (Subscribing to the onSkeletonUpdateEvent)
        skeletonTracker.OnSkeletonUpdateEvent += onSkeletonUpdate;
        userTracker.OnUpdateEvent += onUpdateUser;
        colourSensor.OnUpdateEvent += onColorSensorUpdate;
        
        // Run Nuitrack. This starts sensor data processing
        nuitrack.Nuitrack.Run();
        Debug.Log("Run OK");
    }

    void onNewUser(int _userID)
    {

    }

    void onUpdateUser(nuitrack.UserFrame _userFrame)
    {
        numUsers = _userFrame.NumUsers;
        users = _userFrame.Users;
    }

    void onSkeletonUpdate(nuitrack.SkeletonData _skeletonData)
    {
        if (_skeletonData.NumUsers > 0)
        {
            skeletonData = _skeletonData;
        }
    }

    void onColorSensorUpdate(nuitrack.ColorFrame _frame)
    {
        colourFrame = _frame;
        
        if (onColorUpdate != null) onColorUpdate(colourFrame); //prevents error occuring incase nothing has been assigned to the delegate 
    }

    void Update()
    {
        if (NuitrackAndroidLoader.initState == NuitrackInitState.INIT_OK)
        {
            nuitrack.Nuitrack.Update();
        }
    }


}
