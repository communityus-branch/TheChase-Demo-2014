using UnityEngine;
using System.Collections;

public class Loader : MonoBehaviour {

	// Use this for initialization
	void Start () {
				
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		Screen.autorotateToLandscapeLeft = true;
		Screen.autorotateToLandscapeRight = true;
		Screen.autorotateToPortrait = false;
		Screen.autorotateToPortraitUpsideDown = false;
				
		Application.targetFrameRate = 60;
		#if UNITY_IPHONE
			// resolution
			switch (iPhone.generation)
			{
			case iPhoneGeneration.iPhone4:
			case iPhoneGeneration.iPodTouch4Gen:
				Screen.SetResolution(640, 480, true); break;
			case iPhoneGeneration.iPad3Gen:
				Screen.SetResolution(1536, 1152, true); break;
			}

			// performance
			switch (iPhone.generation)
			{
			case iPhoneGeneration.iPhone4:
			case iPhoneGeneration.iPodTouch4Gen:
				CameraQuality.disableBlobShadows = true;
				CameraQuality.cullDetailLayer = true;
				CameraQuality.cheapSkin = true;
				Shader.globalMaximumLOD = 100;
				QualitySettings.blendWeights = BlendWeights.OneBone;
				QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
				break;
			case iPhoneGeneration.iPhone4S:
			case iPhoneGeneration.iPad2Gen:
			case iPhoneGeneration.iPadMini1Gen:
				QualitySettings.antiAliasing = 0;
				CameraQuality.cullDetailLayer = true;
				QualitySettings.blendWeights = BlendWeights.TwoBones;
				break;
			case iPhoneGeneration.iPhone5S:
				QualitySettings.antiAliasing = 2;
				QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
				break;
			case iPhoneGeneration.iPad5Gen:
				QualitySettings.antiAliasing = 2;
				QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
				break;
			
			}

			// memory
			switch (iPhone.generation)
			{
			case iPhoneGeneration.iPhone4:
			case iPhoneGeneration.iPhone4S:
			case iPhoneGeneration.iPodTouch4Gen:
			case iPhoneGeneration.iPad2Gen:
			case iPhoneGeneration.iPadMini1Gen:
				 QualitySettings.masterTextureLimit = 1; break;
			}
						
		#endif
		#if UNITY_ANDROID
			if (SystemInfo.graphicsDeviceName.Contains("NVIDIA"))
				 QualitySettings.antiAliasing = 0;
		#endif

		StartCoroutine(Load());
	}
	
	IEnumerator Load()
    {
        #if UNITY_IPHONE
            Handheld.SetActivityIndicatorStyle(iOSActivityIndicatorStyle.Gray);
        #elif UNITY_ANDROID
            Handheld.SetActivityIndicatorStyle(AndroidActivityIndicatorStyle.Small);
        #endif

		#if UNITY_IPHONE || UNITY_ANDROID 
        Handheld.StartActivityIndicator();
		#endif

        yield return new WaitForSeconds(0);
        #if UNITY_ANDROID
        yield return AndroidHandleOBBDownload();
        #endif
        Application.LoadLevel(1);
    }
    
    void OnDestroy ()
    {
		#if UNITY_IPHONE || UNITY_ANDROID 
    	Handheld.StopActivityIndicator();
		#endif
	}

    #if UNITY_ANDROID
    IEnumerator AndroidHandleOBBDownload()
    {
    	if (GooglePlayDownloader.RunningOnAndroid())
    	{
			string expPath = GooglePlayDownloader.GetExpansionFilePath();
			if (expPath != null)
			{
				string mainPath = GooglePlayDownloader.GetMainOBBPath(expPath);
				string patchPath = GooglePlayDownloader.GetPatchOBBPath(expPath);
				
				if (mainPath == null || patchPath == null)
				{
					message = "Downloading OBB file from the server. May take couple of minutes. Please wait!";
			 		yield return new WaitForSeconds(0); // skip some frames to show the message
			 		yield return new WaitForSeconds(0);
			 		yield return new WaitForSeconds(0);
					GooglePlayDownloader.FetchOBB();
					message = "";
				}
			}
    	}
 		
 		yield return new WaitForSeconds(0);
    }
    #endif

    private string message = "";
    void OnGUI ()
    {
    	if (message.Length > 0)
			GUI.Label(new Rect(16, 64, 300, 300), message);
    }
}
