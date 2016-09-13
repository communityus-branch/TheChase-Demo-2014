using UnityEngine;
using System.Collections;
using System;

[RequireComponent( typeof( CameraFade )) ]
[RequireComponent( typeof( ScrubGlitcher )) ]
[RequireComponent( typeof( Swiper2 )) ]
public partial class Interactivity : MonoBehaviour
{
	public Camera hudCamera;
	[Range(0.0f, 1.0f)]
	public float hudDistance = 0.01f;	

	public float bulletTimeSpeed = 0.25f;
	public float bulletTimeSnappiness = 0.03f;
	public float transitionDuration = 3.0f;
	public bool allowPause = true;
	public bool allowFeatureDemonstration = false;

	private bool paused = false;

	private BulletTimeCinematron m_Cinematron;
	public BulletTimeCinematron cinematron { get {
		if (m_Cinematron)
			return m_Cinematron;
		var go = GameObject.Find( "__Cinematron" );
		if( go )
			m_Cinematron = go.GetComponent<BulletTimeCinematron>();
		return m_Cinematron;
	} }
	
	static public Interactivity instance { get {
		var go = GameObject.Find( "__Interactivity" );
		if (!go) return null;
		Interactivity i = go.GetComponent<Interactivity>();
		return i;
	} }

	public float playbackSpeedInFramesPerSecond { get { return cinematron? cinematron.playbackSpeedInFramesPerSecond: BulletTime.frameRate; } }

	void Start()
	{
		setupInput();
		setupAudio();
		setupFeatures();
		setupScrubbing();
		setupAiming();
	}

	private bool insidePauseButton()
	{
		return (Input.mousePosition.x > Screen.width - 128 && Input.mousePosition.y > Screen.height - 128);
	}

	private bool handlePause()
	{
		bool wasPaused = paused;
		if( allowPause )
		{
			if( !paused && ((Input.GetMouseButtonDown(0) && insidePauseButton ()) || Input.GetKeyUp(KeyCode.Space)) )
			{
				paused = true;
				cinematron.ducTime( Mathf.Epsilon, 0.25f, iTween.EaseType.easeOutCubic, true );
				/*Time.timeScale = Mathf.Epsilon;
				AudioListener.volume = 0;
				foreach( var a in allAudios )
				{
					if( a.isPlaying && a.gameObject.activeInHierarchy && a.gameObject.activeSelf )
						a.pitch = Mathf.Epsilon;
				}*/
			}
			else if (paused && ((Input.GetMouseButtonUp(0) && !insidePauseButton ()) || Input.GetKeyUp(KeyCode.Space)))
			{
				paused = false;
				cinematron.ducTime( 1.0f, 0.25f, iTween.EaseType.easeOutCubic, true );
				/*var fullSpeed = cinematron.playbackSpeedInFramesPerSecond / BulletTime.frameRate;
				Time.timeScale = fullSpeed;
				resyncMusic();
				foreach( var a in allAudios )
				{
					if( a.isPlaying && a.gameObject.activeInHierarchy && a.gameObject.activeSelf )
						a.pitch = cinematron.playbackSpeedInFramesPerSecond / cinematron.musicSpeedInFramesPerSecond;
				}*/
			}
		}
		else
			paused = false;

		return (wasPaused || paused);
	}
	
	public Vector3 ScreenPointToHUDPlane(Vector3 pos)
	{
		if (!hudCamera)
			return ScreenPointToHUDNearPlane(pos);
		
		return hudCamera.ScreenPointToRay(pos).GetPoint(Mathf.Lerp(hudCamera.nearClipPlane, hudCamera.farClipPlane, hudDistance));
	}

	public Vector3 ScreenPointToHUDNearPlane(Vector3 pos)
	{
		if (!hudCamera)
			return new Vector3(pos.x / Screen.width, pos.y/Screen.height, pos.z);
		
		return hudCamera.ScreenPointToRay(pos).GetPoint(hudCamera.nearClipPlane);
	}

	void Update()
	{
		if( handlePause() )
			return;
		
		if( Input.GetKeyDown(KeyCode.Escape) )
			Application.Quit();

		if( Input.GetKeyUp(KeyCode.LeftArrow) )
			leftFeature();

		if( Input.GetKeyUp(KeyCode.RightArrow) )
			rightFeature();

		updateInput();
		updateAudio();
		updateScrubbing();
	}

	void LateUpdate()
	{
		updateAiming();
	}
}