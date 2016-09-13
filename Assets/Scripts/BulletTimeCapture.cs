using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

public class BulletTimeCapture : MonoBehaviour {

	public static string CaptureDir = "Capture";
	
#if UNITY_EDITOR
	private int frameNumber = 0;
	private int framesToCapture = 0;
	private float timeToCapture = 0;
	void Start ()
	{
		if (!Application.isPlaying)
			return;

		if (!System.IO.Directory.Exists(CaptureDir))
			System.IO.Directory.CreateDirectory(CaptureDir);

		Application.runInBackground = true;
		
		BulletTime.Refresh();
		
		timeToCapture = BulletTime.duration;
		Time.captureFramerate = (int)BulletTime.frameRate;
		
		Debug.Log("CAPTURE STARTED. Time length " + timeToCapture + ". Frames to capture " + framesToCapture);
		frameNumber = 0;
	}		
	
	void OnDestroy ()
	{
	}
	
	private int lastCapturedFrame = 0;
	void Update()
	{
		if( BulletTime.playbackTime > timeToCapture )
		{
			Debug.Log("CAPTURE ENDED. Frames captured " + lastCapturedFrame);
			Time.captureFramerate = 0;
			Time.timeScale = 0;
			BulletTime.time = 0;
			frameNumber = 0;
			
			Application.runInBackground = false;
			EditorApplication.isPlaying = false;

			this.gameObject.SetActive( false );
			Object.Destroy( this.gameObject, 1.0f );
			return;
		}
		
		//lastCapturedFrame = BulletTime.frame;
		lastCapturedFrame++;
		
		var sceneName = System.IO.Path.GetFileNameWithoutExtension(EditorApplication.currentScene);// Application.loadedLevelName;//scenePath[scenePath.Length - 1];
		var shotName = System.IO.Path.Combine(CaptureDir, sceneName + "_" + frameNumber.ToString("0000") + ".png");
		frameNumber++;

		Application.CaptureScreenshot(shotName);
	}
#endif
}