using System.Collections;
using System;
using UnityEngine;
using System.Threading;

public enum NuitrackInitState
{
	INIT_OK = 0,
	INIT_NUITRACK_MANAGER_NOT_INSTALLED = 1,
	INIT_NUITRACK_RESOURCES_NOT_INSTALLED = 2,
	INIT_NUITRACK_SERVICE_ERROR = 3,
	INIT_NUITRACK_NOT_SUPPORTED = 4
}

public class NuitrackAndroidLoader
{
    public static NuitrackInitState initState = NuitrackInitState.INIT_NUITRACK_NOT_SUPPORTED;
    public static bool initComplete;
    public static NuitrackInitState InitNuitrackLibraries()
    {
        #if UNITY_ANDROID // preprocessor macro - this code will only run on android
            Debug.Log("InitNuitrackLibraries() -- starts.");

            try 
            {
                initComplete = false;

                // get current android context
                AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");

                // locate nuitrackhelperjar java class and allocate to javaNuitrackClass
                AndroidJavaClass javaNuitrackClass = new AndroidJavaClass("com.tdv.nuitrack.sdk.Nuitrack");

                // Load nuitrack libraries
                javaNuitrackClass.CallStatic("init", jo, new NuitrackCallback());
                
                // wait for nuitrack libraries to load
                while (!initComplete)
                {
                    Thread.Sleep(50);
			    }
            } 
            catch (System.Exception ex) 
            {
                Debug.Log("Exception: " + ex);
            }
        #endif
        return initState;
    }

}

// implement callbacks based on template
public class NuitrackCallback : AndroidJavaProxy 
{
    public NuitrackCallback() : base("com.tdv.nuitrack.sdk.Nuitrack$NuitrackCallback") { }

    void onInitSuccess(AndroidJavaObject context)
    {
        Debug.Log("Nuitrack callback: onInitSuccess");
        NuitrackAndroidLoader.initState = NuitrackInitState.INIT_OK;
        NuitrackAndroidLoader.initComplete = true;
    }

    void onInitFailure(int errorId)
    {
        Debug.Log("Nuitrack callback: onInitFailure");
        NuitrackAndroidLoader.initState = (NuitrackInitState)errorId;
        NuitrackAndroidLoader.initComplete = true;
    }
}