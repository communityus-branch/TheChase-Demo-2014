using UnityEngine;
using UnityEditor;
using System.Collections;

public class BulletTimeScreenshot : MonoBehaviour {

	[MenuItem("Bullet Time/Screenshot x2", false, 1)]
	static void CaptureX2()
	{
		Capture(2);
	}

	[MenuItem ("Bullet Time/Screenshot x4", false, 2)]
	static void CaptureX4 () 
	{
		Capture(4);
	}

	[MenuItem ("Bullet Time/Screenshot x8", false, 3)]
	static void CaptureX8 () 
	{
		Capture(8);
	}
	
	[MenuItem ("Bullet Time/Video")]
	static void CaptureVideo() 
	{
		EditorApplication.isPlaying = true;

		//StartCoroutine( meh );
		GameObject go = GameObject.Find("__VideoCapture");
		if (!go)
		{
			go = new GameObject ("__VideoCapture");
			go.AddComponent(typeof(BulletTimeCapture));
		}
	}

	static void Capture (int superSize) 
	{
		//var antiAliasing = QualitySettings.antiAliasing;
		//QualitySettings.antiAliasing = 8;
		
		//var scenePath = EditorApplication.currentScene.Split(char.Parse("/"));
		var sceneName = System.IO.Path.GetFileNameWithoutExtension(EditorApplication.currentScene);// Application.loadedLevelName;//scenePath[scenePath.Length - 1];
		
		var shotName = sceneName + "_" + BulletTime.frame + ".png";
		Debug.Log("SCREENSHOT " + shotName);
		Application.CaptureScreenshot(shotName, superSize);
		
		//QualitySettings.antiAliasing = antiAliasing;
	}
}