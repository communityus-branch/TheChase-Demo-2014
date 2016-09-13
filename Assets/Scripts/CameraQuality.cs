using UnityEngine;
using System.Collections;

public class CameraQuality : MonoBehaviour {
	
	static public bool disableBlobShadows = false;
	static public bool cullDetailLayer = false;
	static public bool cheapSkin = false;

	void Awake () {
		if (disableBlobShadows)
			gameObject.GetComponent<BlobShadows>().enabled = false;
		if (cullDetailLayer)
			GetComponent<Camera>().cullingMask &= ~(1 << LayerMask.NameToLayer("Detail"));
		if (cheapSkin)
			gameObject.GetComponent<CameraSkinScattering>().scattering = CameraSkinScattering.ScatteringModel.Fallback;
	}	
}
