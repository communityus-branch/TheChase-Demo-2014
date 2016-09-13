using UnityEngine;
using System.Collections;

public class InvisibleOnStart : MonoBehaviour {
	
	public bool affectChildren = true;

	void Start () {
		if (affectChildren)
		{
			var rs = gameObject.GetComponentsInChildren<Renderer>();
			foreach (var r in rs)
				r.enabled = false;
		}
		
		if (GetComponent<Renderer>())
			GetComponent<Renderer>().enabled = false;
	}
	
}
