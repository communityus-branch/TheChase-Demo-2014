using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class Hud : MonoBehaviour
{
	//	UI type prefabs.
	public GameObject valuePrefab;
	public GameObject colorPrefab;

	public float distance = 0.3f;
	public float spanStart = 0.4f;
	public float spanEnd = 0.6f;
	public float edge = 0.9f;

	private Vector3 genPos( Camera cam, float t )
	{
		var ray = cam.ViewportPointToRay( new Vector3( edge, Mathf.Lerp( spanStart, spanEnd, t ) ) );
		var p = ray.GetPoint( Mathf.Lerp( cam.nearClipPlane, cam.farClipPlane, distance ) );
		return p;
	}

	public HudValue createSlider( float alongSpline )
	{
		Debug.Log( "Creating value slider" );

		object o = Instantiate( valuePrefab );
		GameObject instance = o as GameObject;
		if( instance == null )
		{
			Debug.LogError( "failed to instantiate valuePrefab" );
			return null;
		}

		Camera cam = gameObject.GetComponent<Camera>();
		if( cam == null )
		{
			Debug.LogError( "No Camera component" );
			return null;
		}

		HudValue hudValue = instance.GetComponentInChildren<HudValue>();
		if( hudValue == null )
		{
			Debug.LogError( "Missing HudValue component in ...errr.... the HudValue prefab" );
			return null;
		}

		instance.transform.position = genPos( cam, alongSpline );
		instance.transform.parent = transform;

		hudValue.hudCam = cam;
		return hudValue;
	}

	public HudColor createColorPicker( float alongSpline )
	{
		Debug.Log( "Creating colorpicker" );

		object o = Instantiate( colorPrefab );
		GameObject instance = o as GameObject;
		if( instance == null )
		{
			Debug.LogError( "failed to instantiate valuePrefab" );
			return null;
		}

		Camera cam = gameObject.GetComponent<Camera>();
		if( cam == null )
		{
			Debug.LogError( "No Camera component" );
			return null;
		}

		HudColor hudColor = instance.GetComponent<HudColor>();
		if( hudColor == null )
		{
			Debug.LogError( "Missing hudColor component in ...errr.... the hudColor prefab" );
			return null;
		}

		instance.transform.position = genPos( cam, alongSpline );
		instance.transform.parent = transform;

		hudColor.hudCam = cam;
		return hudColor;
	}
	
	public void Update()
	{
		this.GetComponent<Camera>().enabled = true;
	}
}