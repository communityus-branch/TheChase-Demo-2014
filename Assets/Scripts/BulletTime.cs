// TODO:
// DONE:
// + Cinematic camera
// + Camera constraints - extract from CameraBox (LookAt, Dynamic, SemiDynamic, Off)
// + Letter box
// + Lens in mm -> FOV
// + Sequence of cameras - from CameraBox to time
// + Local camera animation
// + Local camera animation (in playback mode)
// + Frame stepping
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

public class BulletTime : MonoBehaviour
{
	static public string TimeCode {
		get { var t = playhead * m_Duration;
			return string.Format ("{0:0}:{1:00}", Mathf.Floor (t / 60), t % 60); }
	}
	
	static private double m_PlaybackTime = 0.0f;
	static private double m_Time = 0.0f;
	static private float m_DeltaTime = 0.0f;

	static public event System.Action OnRestart;
	static public void Restart()
	{
		playhead = 0;
		if( OnRestart != null )
			OnRestart();
	}

	static public void addRestartListener( System.Action f )
	{
		OnRestart += f;
	}
	
	static public void Refresh()
	{
		MaintainTimeDbl();
	}

#if UNITY_EDITOR
	static public bool activeInEditor = true;
	static public bool scrubParticlesInEditor = true;
#endif

	static public bool paused = false;
	static public bool allowCameraTriggers = true;
	static public float frameRate = 30.0f;

	static public float invFrameRate	{ get { return 1.0f / frameRate; } }

	#region Unit time
	static public float playhead {
		get { return (float)playbackTime / m_Duration; }
		set {
			playbackTime = value * m_Duration;
		}
	}

	static private float m_Duration = 87.5333f;	//	Script-adjusted duration gets set in MaintainTimeDbl().
	static public float duration {
		get { return m_Duration; }
		private set {
			if (Mathf.Abs (value) < Mathf.Epsilon)
				m_Duration = Mathf.Epsilon;
			else
				m_Duration = value;
		}
	}
	#endregion
	
	#region Time(remapped)

	static ScriptEntry findScriptAtFrame( int t )
	{
		if( playbackScript == null || playbackScript.Length <= 0 )
			return null;

		foreach( ScriptEntry se in playbackScript )
		{
			if( t >= se.from && t <= se.to )
				return se;
		}

		return null;
	}

	static double scripted2original( double t, out int activeScriptEntry)
	{
		activeScriptEntry = 0;
		if (!hasPlaybackScript)
			return t;
		
		double totalLength = m_StartTimePerPlaybackEntry[m_StartTimePerPlaybackEntry.Length - 1];	
		if (t > totalLength)
			t = t % totalLength;
		
		while (activeScriptEntry < m_StartTimePerPlaybackEntry.Length && t > m_StartTimePerPlaybackEntry[activeScriptEntry])
			activeScriptEntry++;
		
		if (activeScriptEntry <= 0)
			activeScriptEntry = 1;
		if (activeScriptEntry > playbackScript.Length)
			activeScriptEntry = playbackScript.Length;
		activeScriptEntry--;
		
		int fromFrame = playbackScript [activeScriptEntry].from;
		double timeOffset = MakeFrameTimeDbl (fromFrame, frameRate) - m_StartTimePerPlaybackEntry [activeScriptEntry];
		return t + timeOffset;
		
/*		m_StartTimePerPlaybackEntry -> playbackScript.from .. to
		
		for (int q = 0; q < m_StartTimePerPlaybackEntry.Length; ++q)
			if (t > m_StartTimePerPlaybackEntry[q])
		
		while (
*/
/*
		int f = (int)Mathf.Round( (float)MakeDescreetTimeDbl( t, frameRate ) * frameRate );
		var se = findScriptAtFrame( f );
		if( se == null )
			return t;

		return t;
*/
	}
	
	/*
	static public float remapped_time
	{
		get { return playbackTime; }
		//get { return original2scripted( time ); }
		set { time = scripted2original( value ); playbackTime = value; }
	}
	*/
	#endregion

	#region Time(non-remapped)
	static public float time {
		get { InternalUpdate ();
			return (float)highPrecisionTime; }
#if UNITY_EDITOR
		set { highPrecisionTime = (double)value; }
#endif
	}
	static public float playbackTime {
		get { InternalUpdate (); MaintainTimeDbl (); return (float)m_PlaybackTime; }
		set { MaintainTimeDbl(); m_PlaybackTime = (double)value; m_Time = scripted2original((double)value, out m_ActiveScriptEntry); timeJump = true; }
	}

	static public double highPrecisionTime {
		get { return MaintainTimeDbl (); }
#if UNITY_EDITOR
		set { m_Time = m_PlaybackTime = value; }
#endif
	}

	static public float deltaTime {
		get { InternalUpdate ();
			return (Application.isPlaying) ? Time.deltaTime : m_DeltaTime; }
		set { m_DeltaTime = value; }
	}

	static public float frameDescreetTime {
		get { return (float)MakeDescreetTimeDbl (time, frameRate); }
#if UNITY_EDITOR	
		set { highPrecisionTime = MakeDescreetTimeDbl (value, frameRate); }
#endif
	}

	static public int frame {
		get { return (int)Mathf.Round (frameDescreetTime * frameRate); }
#if UNITY_EDITOR
		set { highPrecisionTime = MakeFrameTimeDbl (value, frameRate); }
#endif
	}

	static public float frameFraction {
		get { double t = highPrecisionTime * (double)frameRate;
			return Mathf.Max (0f, (float)(0.5 + t - System.Math.Round (t))); }
	}
	
	static public float MakeDescreetTime (float t, float framerate)
	{
		return (float)MakeDescreetTimeDbl (t, framerate);
	}

	static public float MakeFrameTime (int frame, float framerate, float frameFraction = 0)
	{
		return (float)MakeFrameTimeDbl (frame, framerate, frameFraction);
	}

	static public double MakeDescreetTimeDbl (double t, double framerate)
	{
		return System.Math.Round (t * framerate) / framerate;
	}
	
	static public double MakeFrameTimeDbl (int frame, double framerate, double frameFraction = 0)
	{
		return (((double)frame + frameFraction)) / framerate;
	}
	#endregion

	static public bool isEditing { get { return Application.isEditor && !Application.isPlaying; } }

	static void InternalUpdate ()
	{
		m_LastAnimationSampleWasInPlayMode |= Application.isPlaying;
	}

	static public bool m_LastAnimationSampleWasInPlayMode = false;

	static public bool justFinishedPlaying {
		get { bool finished = (m_LastAnimationSampleWasInPlayMode && !Application.isPlaying);
			if (finished)
				m_LastAnimationSampleWasInPlayMode = false;
			return finished; }
	}
	
	#region ScriptEntry
	[System.Serializable]
	public class ScriptEntry
	{
		public ScriptEntry (int f, int t)
		{
			from = f;
			to = t;
		}
		public int from;
		public int to;
	}

	static public bool hasPlaybackScript { get { return playbackScript != null; } }

	static public ScriptEntry[] playbackScript;/* = new ScriptEntry[] {
		new ScriptEntry(0, 25),
		new ScriptEntry(50, 114),
		new ScriptEntry(134, 193),
		new ScriptEntry(205, 235),
//		new ScriptEntry(122, 235),
		new ScriptEntry(250, 320),
		new ScriptEntry(348, 462),
		new ScriptEntry(0, 100),
		new ScriptEntry(502, 600),//2000)
		new ScriptEntry(0, 100),
		new ScriptEntry(200, 2000),
	};
	*/

	static public bool timeJump = false;
	static private int m_LastFrameCount = 0;
	static public int m_ActiveScriptEntry = -1;
	static private double[] m_StartTimePerPlaybackEntry = null;
	/*static public ScriptEntry playbackScriptEntry
	{
		get
		{
			if (playbackScript != null && m_ActiveScriptEntry >= 0 && m_ActiveScriptEntry < playbackScript.Length)
				return playbackScript[m_ActiveScriptEntry];
			else
				return new ScriptEntry(0,99999);
		}
	}
	static public int playbackScriptFrame
	{
		get { return (int)Mathf.Round((float)MakeDescreetTimeDbl(m_Time, frameRate) * frameRate); }
	}
	static public float timeOffset
	{
		get { return (float)m_TimeOffset; }
	}*/
	#endregion

	static private double MaintainTimeDbl ()
	{
		if (hasPlaybackScript && m_StartTimePerPlaybackEntry == null) {
			m_StartTimePerPlaybackEntry = new double[ playbackScript.Length + 1 ];
			int startFrame = 0;
			for (int q = 0; q < playbackScript.Length; ++q) {
				m_StartTimePerPlaybackEntry [q] = MakeFrameTimeDbl(startFrame, frameRate);
				Debug.Log (string.Format ("INIT script entry {0} {1} --- {2} {3}", startFrame, m_StartTimePerPlaybackEntry [q], playbackScript [q].from, playbackScript [q].to));
				startFrame += playbackScript [q].to - playbackScript [q].from + 1;
			}

			float lastFrameTime = (float)MakeFrameTimeDbl (startFrame, frameRate);
			m_StartTimePerPlaybackEntry [m_StartTimePerPlaybackEntry.Length - 1] = lastFrameTime;
			duration = lastFrameTime;

			Debug.Log (string.Format ("ScriptEntry adjusted duration is {0}", lastFrameTime));
		}

		if (m_LastFrameCount != Time.frameCount) {
			m_PlaybackTime += (double)Time.deltaTime;
			m_LastFrameCount = Time.frameCount;

			if (hasPlaybackScript && Application.isPlaying) {
				timeJump = false;
				/*
				 * int nextScriptEntry = m_ActiveScriptEntry + 1;
				if (nextScriptEntry < playbackScript.Length && m_PlaybackTime >= m_StartTimePerPlaybackEntry [nextScriptEntry]) {
					m_ActiveScriptEntry = nextScriptEntry;
					timeJump = true;
				} else if (m_PlaybackTime > m_StartTimePerPlaybackEntry [m_StartTimePerPlaybackEntry.Length - 1]) {
					// restart
					Debug.Log ("RESTART script");
					m_ActiveScriptEntry = 0;
					timeJump = true;
					m_PlaybackTime = 0;
					Restart();
				}

				int fromFrame = playbackScript [m_ActiveScriptEntry].from;
				double timeOffset = MakeFrameTimeDbl (fromFrame, frameRate) - m_StartTimePerPlaybackEntry [m_ActiveScriptEntry];

				if (timeJump)
					Debug.Log ("TimeJump " + m_ActiveScriptEntry + " " + (float)timeOffset);
					
				m_Time = m_PlaybackTime + timeOffset;
				*/
				
				if (m_PlaybackTime > m_StartTimePerPlaybackEntry [m_StartTimePerPlaybackEntry.Length - 1])
				{
					// restart
					Debug.Log ("RESTART script");
					Restart();
				}
				
				var prevScriptEntry = m_ActiveScriptEntry;
				m_Time = scripted2original(m_PlaybackTime, out m_ActiveScriptEntry);
				if (prevScriptEntry != m_ActiveScriptEntry)
				{
					timeJump = true;
					Debug.Log ("TimeJump!!! " + m_ActiveScriptEntry);
				}
			}
			else {
				m_Time = m_PlaybackTime;
			}

		}

		return m_Time;
	}
	
	#region Animation
	public class AnimatedObject
	{
		public void Reset (Animation a = null, string n = "", bool mat = false)
		{
			animation = a;
			clipName = n;
			if (clipName == "" && a && a.clip)
				clipName = a.clip.name;
			restoreMaterials = mat;
			cachedState = null;
		}
		
		public Animation animation = null;
		public AnimationState cachedState = null;
		public string clipName = "";
		public bool restoreMaterials = false;
	}

	static public AnimationState CacheAnimationState (AnimatedObject ao)
	{
		if (ao.animation) {
			if (ao.cachedState == null) {
				if (ao.clipName == "")
				if (ao.animation.clip)
					ao.clipName = ao.animation.clip.name;
				else
					Debug.LogError ("Missing default clip (" + ao.animation + ")");
				ao.cachedState = ao.animation [ao.clipName];
			}
			if (ao.cachedState == null)
				Debug.LogError ("State is null (" + ao.animation + " " + ao.clipName + ")");
		} else
			ao.cachedState = null;
		return ao.cachedState;
	}
	
	static public AnimatedObject[] CollectAnimatedObjects ()
	{
		return CollectAnimatedObjects (null);
	}
	
	static public AnimatedObject[] CollectAnimatedObjects (Transform root)
	{
		var animationComponents = (root == null) ?
			Object.FindObjectsOfType (typeof(Animation)) as Animation[] :
			root.GetComponentsInChildren<Animation> (false) as Animation[];

		var animatedObjects = new ArrayList ();
		foreach (var anim in animationComponents)
			if (anim.clip != null && anim.enabled && anim.playAutomatically) {
				var ao = new AnimatedObject ();
				ao.Reset (anim);
				CacheAnimationState (ao);
				/*#if UNITY_EDITOR
				var assetPath = AssetDatabase.GetAssetPath(anim.clip);
				ao.restoreMaterials = Path.GetExtension(assetPath) == ".anim";
				#endif*/

				animatedObjects.Add (ao);
			}
			
		return animatedObjects.ToArray (typeof(AnimatedObject)) as AnimatedObject[];
	}

	static public float GetAnimationLength (AnimatedObject[] animatedObjects)
	{
		var maxClipLength = 0.0f;
		foreach (var ao in animatedObjects)
			if (CacheAnimationState (ao))
				maxClipLength = Mathf.Max (maxClipLength, ao.cachedState.length);
		return maxClipLength;
	}
	
	/*
	static public void SampleAnimationByFrame (AnimatedObject[] animatedObjects, float frame, float frameRate, bool forceSample)
	{
		// rounding frames, so we never play in-between frames
		time = Mathf.Round (frame) / frameRate;
		SampleAnimation (animatedObjects, forceSample);
	}
	*/

	static public void SampleAnimation (AnimatedObject[] animatedObjects, bool forceSample)
	{
		if (animatedObjects == null)
			return;

		Profiler.BeginSample ("BulletTime.SampleAnimation");
		
		foreach (var ao in animatedObjects)
			SampleAnimation (ao, time, forceSample);
		
		Profiler.EndSample ();
	}
	
	static public void SampleAnimation (AnimatedObject ao, float t, bool forceSample)
	{
#if UNITY_EDITOR 
		if (!activeInEditor)
			return;
#endif
		var animation = ao.animation;
		var state = CacheAnimationState (ao);
		if (state == null)
			return;
		
		state.time = t;

		if (forceSample) {
			state.enabled = true;
			state.weight = 1;
			animation.Sample ();
			state.enabled = false;
			
			#if UNITY_EDITOR				
			EditorUtility.SetDirty(animation);
			#endif
		}
	}
	#endregion

	#region LightGroups
	static public LightGroup activeLightGroup { get { return m_LightGroup; } }

	static private LightGroup m_LightGroup = null;

	static public void SetActiveLightGroup (LightGroup lightGroup)
	{
		if (m_LightGroup == lightGroup)
			return;
		
		if (m_LightGroup)
			foreach (var l in m_LightGroup.lights)
				if (l)
					l.enabled = false;

		//;;Debug.Log ("SetActiveLightGroup " + lightGroup);
		m_LightGroup = lightGroup;

		if (m_LightGroup)
			foreach (var l in m_LightGroup.lights)
				if (l)
					l.enabled = true;
	}

	static public void DisableLightGroups (LightGroup[] lightGroups)
	{
		foreach (var g in lightGroups) {
			foreach (var l in g.lights)
				if (l)
					l.enabled = false;
			if (m_LightGroup == g)
				m_LightGroup = null;
		}
		SetActiveLightGroup (m_LightGroup);
	}
	#endregion
	
	#region Misc Support code
	static public Object RandomFromArray (Object[] array)
	{
		if (array.Length <= 0)
			return null;
		var i = Random.Range (0, array.Length - 1);
		return array [i];
	}

	// hacky 1D noise implementation:
	static double R (int x)
	{
		x = (x << 13) ^ x;
		double v = 1.0 - ((x * (x * x * 15731 + 789221) + 1376312589) & 0x7fffffff) / 2147483647.0;
		return v;
	}

	static double SmoothR (int x)
	{
		double v = R (x) / 2.0 + R (x) / 4.0 + R (x) / 4.0;
		return v;
	}

	static float InterpolateNoise (float x)
	{
		int integer_X = Mathf.FloorToInt (x);
		float fractional_X = x - (float)integer_X;

		double v1 = R (integer_X);
		double v2 = R (integer_X + 1);

		float v = Mathf.Lerp ((float)v1, (float)v2, fractional_X);
		return v;
	}

	public static float Noise1D (float x)
	{
		float totalAmplitude = 0.0f;
		float total = 0.0f;
		float p = 0.75f;//persistence
		int n = 4;//Number_Of_Octaves - 1
		
		for (int i = 0; i < n; i++) {
			float frequency = Mathf.Pow (2.0f, i);
			float amplitude = Mathf.Pow (p, i);
			total += InterpolateNoise (x * frequency) * amplitude;
			totalAmplitude += amplitude;
		}
		total /= totalAmplitude;
		return total;
	}

	#endregion
}