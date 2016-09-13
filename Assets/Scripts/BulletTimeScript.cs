using UnityEngine;
using System.Collections;

public class BulletTimeScript : MonoBehaviour {

	public BulletTime.ScriptEntry[] playbackScript;

	// Use this for initialization
	void Awake () {
		if (Application.isPlaying)
			BulletTime.playbackScript = playbackScript;
	}	
}
