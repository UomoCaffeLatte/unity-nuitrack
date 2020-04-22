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

    private bool firstTime = false; //in order to prevent double NuitrackInit() calls on startup (in Awake and in OnApplicationPause)
    private static nuitrack.SkeletonTracker skeletonTracker;
    public static nuitrack.SkeletonTracker SkeletonTracker {get {return skeletonTracker; }}
    private NuitrackInitState initState = NuitrackInitState.INIT_NUITRACK_MANAGER_NOT_INSTALLED;
    public NuitrackInitState InitState { get { return NuitrackAndroidLoader.initState; }}
    static nuitrack.SkeletonData skeletonData;
    public static nuitrack.SkeletonData SkeletonData { get { return skeletonData; } }
    static nuitrack.Skeleton[] trackedSkeletons;
    static nuitrack.Skeleton trackedSkeleton;

    [SerializeField] InitEvent initEvent;
    [SerializeField] bool runInBackground = false;

    public Text numSkeletonText;

    void Awake()
    {
        initState = NuitrackAndroidLoader.InitNuitrackLibraries();

        if (initEvent != null)
        {
            initEvent.Invoke(initState);
        }

        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.targetFrameRate = 60;
        Application.runInBackground = runInBackground;

        if (initState == NuitrackInitState.INIT_OK)
        {
            NuitrackInit();
        }
    }

    void NuitrackInit()
    {
        nuitrack.Nuitrack.Init();
    
        // Create and setup all required modules
        skeletonTracker = nuitrack.SkeletonTracker.Create();

        // Add event handlers for all modules created (Subscribing to the onSkeletonUpdateEvent)
        skeletonTracker.OnSkeletonUpdateEvent += OnSkeletonUpdate;
        
        // Run Nuitrack. This starts sensor data processing
        nuitrack.Nuitrack.Run();
        Debug.Log("Run OK");
    }

    void FixedUpdate()
    {
        nuitrack.Nuitrack.Update();
    }
    void OnSkeletonUpdate(nuitrack.SkeletonData _skeletonData)
    {
        numSkeletonText.text = "Tracked skeletons:  LOL";

        skeletonData = _skeletonData;

        numSkeletonText.text = "Tracked skeletons: " + skeletonData.NumUsers.ToString();

    
    }

}
