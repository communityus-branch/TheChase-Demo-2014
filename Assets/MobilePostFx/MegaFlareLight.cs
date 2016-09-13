using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class MegaFlareLight : MonoBehaviour {
	void OnEnable ()
	{
		MegaFlare.AddFlare (this);
	}

	void OnDisable ()
	{
		MegaFlare.RemoveFlare (this);
	}
}
