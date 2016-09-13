using UnityEngine;
using System.Collections;
using System;

public partial class Interactivity
{
	public GameObject aimIndicator;

	public event System.Action<bool> OnAim;

	public bool allowAiming = true;
	public float maxAngleX = 12f;
	public float maxAngleY = 10f;
	public float sensitivity = 5f;
	public float cameraSnappiness = 0.1f;
	
	private Vector2 touchDownPosition = new Vector3( 0.5f, 0.5f, 0.0f );
	private Quaternion goalAim = Quaternion.identity;
	private Quaternion currentAim = Quaternion.identity;
	private bool _aiming = false;
	public bool aiming	{
		get { return _aiming; }
		private set
		{
			_aiming = value;
			if( Application.isPlaying == false || allowAiming == false )
				return;

			if( _aiming == false )
				resyncMusic();

			if( OnAim != null )
				OnAim( _aiming );

			var v = ( _aiming ) ? 1.0f : 0.0f;
			var fullSpeed = cinematron.playbackSpeedInFramesPerSecond / 30.0f;
			
			//	Duc audio volume & pitch.
			iTween.Stop( gameObject );
			iTween.ValueTo( gameObject, iTween.Hash(
				"from", v,
				"to", 1.0f - v,
				"time", 0.5f,
				"easetype", iTween.EaseType.easeOutCubic,
				"ignoretimescale", true,
				"onUpdate", (Action<object>)( ( x ) =>
				{
					if( paused )
						return;

					float f = Mathf.Clamp01( (float)x );
					float ts = Mathf.Clamp( f, bulletTimeSpeed, fullSpeed );

					Time.timeScale = ts;
					AudioListener.volume = Mathf.Clamp( f, 0.1f, 1.0f );
					foreach( var a in allAudios )
					{
						if( a.isPlaying && a.gameObject.activeInHierarchy && a.gameObject.activeSelf )
							a.pitch = ts * (1/fullSpeed) * (cinematron.playbackSpeedInFramesPerSecond / cinematron.musicSpeedInFramesPerSecond);
					}
				} ) ) );
		}
	}

	private void setupAiming()
	{
		if( allowAiming == false )
			return;

		OnInputDown += (pos) => 
		{
			touchDownPosition = pos;
			var y = pos.y / Screen.height;
			if( y > interactiveScrubberLimitInUnitScreenHeight )
				aiming = true;
		};

		OnInputMove += (pos) => 
		{
			var d = pos - touchDownPosition;
			d.x /= Screen.width;
			d.y /= Screen.height;
			d.x = -d.x;
			goalAim = Quaternion.Euler(
				Mathf.Clamp( ( d.y - Mathf.Pow( d.y, 2.0f ) ) * maxAngleY * sensitivity, -maxAngleY, maxAngleY ),
				Mathf.Clamp( ( d.x - Mathf.Pow( d.x, 2.0f ) ) * maxAngleX * sensitivity, -maxAngleX, maxAngleX ),
				0 );
				
			// Update the aim indicator
			if (aimIndicator != null)
			{
				aimIndicator.transform.position = ScreenPointToHUDPlane(pos);
			}
		};

		OnInputUp += (pos) => 
		{
			goalAim = Quaternion.identity;
			if( aiming == true )
				aiming = false;
		};
	}
	
	private void updateAiming()
	{
		if( scrubbing == false && allowAiming == true )
		{
			var dt = ( Time.timeScale < Mathf.Epsilon ) ? ( Time.deltaTime / Time.timeScale ) : 0.1f;
			currentAim = Quaternion.Slerp( currentAim, goalAim, cameraSnappiness * 30.0f * dt );
			var cam = Camera.main;
			if( cam != null )
			{
				CinematicCamera cc = cam.GetComponent<CinematicCamera>();
				if( cc )
					cc.OnExternalRotation( currentAim );
			}
		}
	}
}