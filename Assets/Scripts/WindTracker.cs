using UnityEngine;
using System.Collections;

public class WindTracker : MonoBehaviour
{
	[RangeAttribute(0.5f, 5.0f)]
	public float	Strength = 2.5f;

	[RangeAttribute(0.0f, 1.0f)]
	public float	Responsiveness = 0.8f;
	public Vector3	ObjectSpaceWind = new Vector3(-0.5f, 0.45f, 0.0f);

	[HideInInspector]
	public bool ignoreTimescale = false;

	private Vector3 lastPos = Vector3.zero;
	private Vector3 softDirection = Vector3.zero;
	private MaterialPropertyBlock properties;

	void Start()
	{
		lastPos = transform.position;
		properties = new MaterialPropertyBlock();
	}

	void LateUpdate()
	{
		float t = 0.0f;
		float dt = Time.deltaTime;

		if( ignoreTimescale == true )
			t = Time.realtimeSinceStartup;
		else
			t = BulletTime.time;

		t = t / 20.0f;	//	To match with Time.x in the shader.
		
		softDirection = Vector3.Lerp( softDirection, Vector3.Normalize(lastPos - transform.position), Responsiveness * 30f * dt );
		lastPos = transform.position;

		var wind = softDirection * Strength + transform.TransformDirection( ObjectSpaceWind );

		if( properties == null )
			return;

		properties.Clear();
		properties.AddVector( "_Wind", wind.normalized );
		properties.AddFloat( "_T", t );
		GetComponent<Renderer>().SetPropertyBlock( properties );
	}
}
