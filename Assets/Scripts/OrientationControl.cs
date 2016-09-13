using UnityEngine;
using System.Collections;

public class OrientationControl : MonoBehaviour {
	
	void Awake () {
		//Screen.orientation = ScreenOrientation.LandscapeLeft;
	}

	// Use this for initialization
	void Start () {	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButton(0))
			Time.timeScale = 0f;
		else
			Time.timeScale = 1f;
	}
}
