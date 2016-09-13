using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class HudThing : MonoBehaviour
{
	[HideInInspector]
	public Camera hudCam;

	public LayerMask layerMask;
	private bool inputGrabbed = false;

	protected event System.Action<RaycastHit> OnHit;

	private bool shoot()
	{
		if( !hudCam )
			return false;

		Vector3 mp = Input.mousePosition;
		RaycastHit hit;
		if( !Physics.Raycast( hudCam.ScreenPointToRay( mp ), out hit, 50.0f, layerMask.value ) )
			return false;

		if( hit.collider.gameObject != this.gameObject )
			return false;

		/*Renderer r = hit.collider.renderer;
		if( r == null || renderer.sharedMaterial == null || renderer.sharedMaterial.mainTexture == null )
		{
			Debug.LogError( "Something not set!" );
			return false;
		}*/

		if( OnHit != null )
			OnHit( hit );

		return true;
	}

	private void mouseDown()
	{
		if( Input.GetMouseButtonDown( 0 ) )
		{
			if( shoot() )
				inputGrabbed = true;
		}
	}

	private void mouseMove()
	{
		if( !Input.GetMouseButton( 0 ) || inputGrabbed == false )
			return;

		shoot();
	}

	private void mouseUp()
	{
		if( Input.GetMouseButtonUp( 0 ) )
			inputGrabbed = false;
	}


	public virtual void setVisibility( float f )
	{
		//transform.eulerAngles = new Vector3( Mathf.Lerp( 90.0f, 0.0f, f ), 0, 0 );
	}

	void Update()
	{
		//	Simpler to reason about.
		mouseDown();
		mouseMove();
		mouseUp();
	}
}