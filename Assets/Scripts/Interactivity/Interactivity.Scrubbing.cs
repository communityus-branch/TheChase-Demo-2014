using UnityEngine;
using System.Collections;
using System;

public partial class Interactivity
{	
	public bool allowScrubbing = true;
	public float scrubReactivity = 0.1f;
	public float interactiveScrubberLimitInUnitScreenHeight = 0.3f;
	public GameObject scrubTimecode;
	public GameObject scrubIndicator;
	public GameObject scrubBackground;

	public Vector2 scrubBackgroundScale = new Vector2(1.2f, 0.1f);
	public float scrubTimecodeToBackgroundGap = 0.03f;
	private Rect scrubTimecodeRect;
	public float scrubTimecodeVerticalOffset = 0.075f;
	public Color scrubColor = new Color( 1, 1, 1, 1 );

	private float scrubGoal;
	private bool _scrubbing = false;
	public bool scrubbing {
		get { return _scrubbing; }
		private set
		{
			_scrubbing = value;
			if( currentFeature == null )
			{
				var v = 0.0f;
				if( value == true )
					v = 1.0f;

				//	Duc audio.
				var fullSpeed = cinematron.playbackSpeedInFramesPerSecond / BulletTime.frameRate;
				iTween.ValueTo( gameObject, iTween.Hash(
					"from", AudioListener.volume,
					"to", v,
					"time", 0.1f,
					"ignoretimescale", true,
					"onUpdate", (Action<object>)( ( x ) =>
					{
						if( paused )
							return;

						float f = 1.0f - Mathf.Clamp01( (float)x );
						float ts = Mathf.Clamp( f, Mathf.Epsilon, fullSpeed );
						Time.timeScale = ts;
						AudioListener.volume = Mathf.Clamp( f, 0.0f, 1.0f );
						foreach( var a in allAudios )
						{
							if( a.isPlaying && a.gameObject.activeInHierarchy && a.gameObject.activeSelf )
								a.pitch = ts * (1/fullSpeed) * (cinematron.playbackSpeedInFramesPerSecond / cinematron.musicSpeedInFramesPerSecond);
						}
					} ),
					"onComplete", (Action<object>)( ( x ) =>
					{
					} )
				) );
			}
		}
	}

	public bool IsScreenPointInsideSrubber(Vector2 pos)
	{
		var y = pos.y / Screen.height;
		return (y <= interactiveScrubberLimitInUnitScreenHeight);
	}
	
	public float ScreenPointToPlayhead (Vector2 pos)
	{
		return Mathf.Clamp ((float)(pos.x - scrubTimecodeRect.width) / (float)(Screen.width - scrubTimecodeRect.width*2), 0.01f, 0.99f);
	}
	
	public Vector3 PlayheadToScreenPoint (float playhead)
	{
		return new Vector3(scrubTimecodeRect.width + (Screen.width-scrubTimecodeRect.width*2)*BulletTime.playhead, Screen.height*interactiveScrubberLimitInUnitScreenHeight*0.5f, 0.0f);
	}

	private void setupScrubbing()
	{
		if (scrubTimecode)
		{
			scrubTimecode.GetComponent<GUIText>().anchor = TextAnchor.MiddleLeft;
			scrubTimecode.GetComponent<GUIText>().fontSize = (int)(interactiveScrubberLimitInUnitScreenHeight * 0.5f * (float)Screen.height);
			scrubTimecode.GetComponent<GUIText>().text = "0:00";
			
			if (hudCamera)
			{
				scrubTimecodeRect = scrubTimecode.GetComponent<GUIText>().GetScreenRect (hudCamera);
				scrubTimecodeRect.width += Screen.width * scrubTimecodeToBackgroundGap;
			}
		}

		if( allowScrubbing == false )
			return;
		
		OnInputDown += (pos) => 
		{
			if (IsScreenPointInsideSrubber (pos))
			{
				scrubbing = true;
				Time.timeScale = Mathf.Epsilon;

				//	Scrubbing always brings you out of feature-mode.
				if( currentFeature != null )
					switchToFeature( null, Vector2.zero );
			}
		};

		OnInputMove += (pos) => 
		{
			if( currentFeature != null )
				return;
				
			scrubGoal = ScreenPointToPlayhead (pos);
			
			//	Itch my GL, bitch.
			var glitcher = GetComponent<ScrubGlitcher>() as ScrubGlitcher;
			if (glitcher)
				glitcher.OnScrub (scrubGoal);
		};

		OnInputUp += (pos) => 
		{
			if( scrubbing == false )
				return;

			scrubbing = false;

			BulletTime.playhead = ScreenPointToPlayhead (pos);
			resyncMusic();
			Time.timeScale = cinematron.playbackSpeedInFramesPerSecond / 30.0f;
		};
	}

	private void updateScrubbing()
	{
		if (scrubbing)
			BulletTime.playhead = Mathf.Lerp (BulletTime.playhead, scrubGoal, scrubReactivity);
		
		//	Update the scrub indicator.
		if (scrubIndicator && scrubIndicator.GetComponent<Renderer>().enabled)
		{
			float x = (scrubbing)? scrubGoal: BulletTime.playhead;
			scrubIndicator.transform.position = ScreenPointToHUDNearPlane (PlayheadToScreenPoint (x));
		}
		
		//	Update the scrub timecode gui element.
		if (scrubTimecode && scrubTimecode.activeSelf && scrubTimecode.GetComponent<GUIText>())
		{
			scrubTimecode.transform.position = new Vector3(0f, interactiveScrubberLimitInUnitScreenHeight*0.5f, 0);
			scrubTimecode.GetComponent<GUIText>().text = BulletTime.TimeCode;
		}

		// Stretch scrub background to cover bottom part of the screen
		if (scrubBackground)
		{
			scrubBackground.transform.position = ScreenPointToHUDPlane (new Vector3(Screen.width*0.5f, Screen.height*interactiveScrubberLimitInUnitScreenHeight*0.5f, 0));
			float screenAspect = (float)(Screen.width - scrubTimecodeRect.width * 2) / (float)Screen.height;
			scrubBackground.transform.localScale = new Vector3(screenAspect*scrubBackgroundScale.x, scrubBackgroundScale.y, 1);
		}
	}
}