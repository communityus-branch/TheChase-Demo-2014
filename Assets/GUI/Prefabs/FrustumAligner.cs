using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class FrustumAligner : MonoBehaviour
{
	public Camera _cam = null;

	[Range(0.0f, 1.0f)]
	public float  _distance = 0.0f;
	
	[Range(0.0f, 5.0f)]
	public float  _offset = 1.42f;

	void Update()
	{
		if( !_cam )
			return;

		Misc.FitUnitPlaneInFrustum( transform, _cam, _distance, _offset );
	}
}
