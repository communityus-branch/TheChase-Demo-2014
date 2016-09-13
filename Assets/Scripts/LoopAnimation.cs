using UnityEngine;
using System.Collections;

public class LoopAnimation : MonoBehaviour {

	void Start () {
		if (GetComponent<Animation>()) GetComponent<Animation>().wrapMode = WrapMode.Loop;
	}	
}
