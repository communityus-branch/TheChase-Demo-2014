using UnityEngine;
using System.Collections;

public class ShowFPS : MonoBehaviour {
	private float deltaTime = 0.033f;
	private float lastRealtime;

	// Use this for initialization
	void Start () {
		lastRealtime = Time.realtimeSinceStartup;
	}
	
	// Update is called once per frame
	void Update () {
		var realDeltaTime = (Time.timeScale < Mathf.Epsilon)?
			(Time.deltaTime / Time.timeScale):
			Time.realtimeSinceStartup - lastRealtime;
			
		deltaTime = Mathf.Lerp(deltaTime, realDeltaTime, 0.1f);
		float fps = Mathf.Round(1.0f/deltaTime);
		
		string text = string.Format( "{0} fps", fps.ToString());
		if( Debug.isDebugBuild ) // more info in Development Build
		{
			if( Camera.main != null )
				text = string.Format( "({0}) {1} {2} {3}", text, BulletTime.TimeCode, Camera.main.name, BulletTime.frame );
		}

		// Don't show anything on Android (FPS can be embarassing on some devices) unless in Development Build
		if (GetComponent<GUIText>() && (Application.platform != RuntimePlatform.Android || Debug.isDebugBuild))
			GetComponent<GUIText>().text = text;

		lastRealtime = Time.realtimeSinceStartup;
	}	
}
