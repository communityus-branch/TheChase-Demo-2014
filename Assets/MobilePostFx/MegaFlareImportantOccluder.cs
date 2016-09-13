using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class MegaFlareImportantOccluder : MonoBehaviour {
	void OnEnable ()
	{
		MegaFlare.AddImportantOccluder (this);
	}

	void OnDisable ()
	{
		MegaFlare.RemoveImportantOccluder (this);
	}
}

