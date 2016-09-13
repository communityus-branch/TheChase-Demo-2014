using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class BulletTimeCinematron : MonoBehaviour
{
	public float playbackSpeedInFramesPerSecond = 25.0f;
	public float musicSpeedInFramesPerSecond = 25.0f;
	
	//	Resync music to BulletTime
	public float allowedTimeDiscrepancy = 0.1f;
	
	public AudioSource[] audiosToStart = null;
	public AudioSource music = null;
	
	public float durationIn = 2.0f;
	public float durationOut = 2.0f;
	public AnimationCurve	rampIn = new AnimationCurve( new Keyframe[2] { new Keyframe(0.0f,0.0f), new Keyframe(1.0f,1.0f) } );
	public AnimationCurve	rampOut = new AnimationCurve( new Keyframe[2] { new Keyframe(0.0f,1.0f), new Keyframe(1.0f,0.0f) } );
	
	private AudioSource[] audios = null;
	
	public GameObject	fadePlane;
	
	public bool featureMode = false;

	public void ducTime( float _to, float _duration, iTween.EaseType easeType = iTween.EaseType.easeOutCubic, bool bDucAudio = false )
	{
		Interactivity interactivity = null;
		if( bDucAudio == true )
		{
			var go = GameObject.Find( "__Interactivity" );
			if( go )
				interactivity = go.GetComponent<Interactivity>();

			if( interactivity == null )
				Debug.LogError( "Unable to find __Interactivity" );
		}

		var fullSpeed = playbackSpeedInFramesPerSecond / 30.0f;
		var from = Time.timeScale;

		Debug.Log( string.Format( "duc {0} > {1}", from, _to ) );
		if( from == _to )
			return;

		var id = "CinematronDucTween";
		iTween.StopByName( gameObject, id );
		iTween.ValueTo( gameObject, iTween.Hash( "name", id, "from", 0.0f, "to", 1.0f, "time", _duration, "easetype", easeType, "ignoretimescale", true,
			"onUpdate", (System.Action<object>)( ( x ) =>
			{
				float f = Mathf.Clamp01( (float)x );
				var ts = Mathf.Lerp( from, _to, f );

				Time.timeScale = ts;

				if( bDucAudio == true )
				{
					AudioListener.volume = Mathf.Clamp( f, 0.0f, 1.0f );
					foreach( var a in interactivity.allAudios )
					{
						if( a.isPlaying && a.gameObject.activeInHierarchy && a.gameObject.activeSelf )
							a.pitch = ts * ( 1 / fullSpeed ) * ( playbackSpeedInFramesPerSecond / musicSpeedInFramesPerSecond );
					}
				}
			} ),
			"onComplete", (System.Action<object>)( ( x ) =>
			{
				Time.timeScale = _to;
				if( bDucAudio == true )
					resyncMusic();
			} ) ) );
	}

	public void enableCinematics()
	{
		featureMode = false;
		startGlobals();
	}
	
	public void OnDisable()
	{
		stopAll( false );
	}

	private void stopAll( bool ignorePlayOnAwakes = true )
	{
		foreach( AudioSource a in audios )
		{
			if( a )
			{
				if( ignorePlayOnAwakes == true )
				{
					if( a.playOnAwake != true )
						a.Stop();
				}
				else
					a.Stop();
			}
		}
	}

	public void resyncMusic()
	{
		float adjustedTime = BulletTime.playbackTime * ( 30.0f / musicSpeedInFramesPerSecond );
		float diff = Mathf.Abs( music.time - adjustedTime );
		if( diff > allowedTimeDiscrepancy )
		{
			Debug.LogWarning( string.Format( "Resyncing music, drift was {0} sec", diff ) );
			music.time = adjustedTime;
		}
	}

	//	Start playing ambience & music, and whatnot, stuff in the list up there.
	private void startGlobals()
	{
		if (!Application.isPlaying)
			return;
		
		/*foreach( AudioSource a in audios )
		{
			if( a.playOnAwake == true && a.isPlaying == false )
				a.Play();
		}*/

		foreach( AudioSource a in audiosToStart )
			a.Play();
		
		music.Play();
		resyncMusic();
	}

	private void onRestart()
	{
		stopAll();
		setFade( 0 );
		setVolume( 0 );
	}
	
	void Awake()
	{
		audios = FindObjectsOfType( typeof(AudioSource) ) as AudioSource[];
		stopAll();
	}

	void Start()
	{
		//	Full speed.
		Time.timeScale = playbackSpeedInFramesPerSecond / BulletTime.frameRate;

		onRestart();
		BulletTime.addRestartListener( onRestart );
	}

	public void setVolume( float volume )
	{
		AudioListener.volume = volume;
	}

	public void setFade( float fade )
	{
		if( fadePlane != null )
		{
			var fader = fadePlane.GetComponent<FadePlane>();
			if( fader )
				fader.Factor = fade;
		}
	}

	void LateUpdate()
	{
		if( audios.Length <= 0 )
			return;

		if( featureMode == true )
			return;

		float t = BulletTime.playbackTime;
		if( t < 0 || t > BulletTime.duration )
		{
			setFade( 0 );
			setVolume( 0 );
			return;
		}

		float v = 1.0f;
		if( t < durationIn )
			v = rampIn.Evaluate( t / durationIn );
		else
		{
			t = BulletTime.duration - t;
			v = rampOut.Evaluate( 1.0f - ( t / durationOut ) );
		}

		setFade( v );
		setVolume( v );
	}
}