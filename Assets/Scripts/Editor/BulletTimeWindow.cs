using UnityEngine;
using UnityEditor;
using System.Collections;

public class BulletTimeWindow : EditorWindow
{
	private string m_CurrentScene = "";
	private float m_MaxTime = 0.0f;
	private bool m_SubFrames = false;
	private BulletTime.AnimatedObject[] m_AnimatedObjects = null;
	private SplineCameraSequencer[] m_Sequencers = null;
	private SplineAnimator[] m_SplineAnimators = null;
	private ImportantObject[] m_ImportantObjects = null;

	[MenuItem ("Bullet Time/Bullet Time Window")]
	static void ShowWindow () 
	{
		var window = EditorWindow.GetWindow<BulletTimeWindow> (false, "Bullet Time");
		
		window.minSize = new Vector2(200, 120);
		window.autoRepaintOnSceneChange = true;
		
		window.Show();
		window.Refresh(0.0f);
		window.SampleAnimation();
	}

	void Refresh (float startTime)
	{
		m_AnimatedObjects = BulletTime.CollectAnimatedObjects();
		m_MaxTime = BulletTime.GetAnimationLength(m_AnimatedObjects);

		m_CurrentScene = EditorApplication.currentScene;
		m_Sequencers = Object.FindObjectsOfType(typeof(SplineCameraSequencer)) as SplineCameraSequencer[];
		m_SplineAnimators = Object.FindObjectsOfType(typeof(SplineAnimator)) as SplineAnimator[];
		m_ImportantObjects = Object.FindObjectsOfType(typeof(ImportantObject)) as ImportantObject[];

		foreach (var sa in m_SplineAnimators)
			sa.Invalidate();
		
		foreach (var seq in m_Sequencers)
			seq.Invalidate();
		
		float maxSplineTime = 0.0f;
		if (m_Sequencers != null)
			foreach (var seq in m_Sequencers)
				maxSplineTime = Mathf.Max(seq.timeLength, maxSplineTime);

		if (maxSplineTime < Mathf.Epsilon)
			foreach (var sa in m_SplineAnimators)
				maxSplineTime = Mathf.Max(sa.timeLength, maxSplineTime);
		
		m_MaxTime = Mathf.Max(m_MaxTime, maxSplineTime);

		BulletTime.time = startTime; BulletTime.deltaTime = m_MaxTime;
	}

	void SampleAnimation ()
	{
		if (!BulletTime.isEditing)
			return;

		BulletTime.SampleAnimation(m_AnimatedObjects, true);
		foreach (var seq in m_Sequencers)
			if (seq)
				seq.Update();
	}

	static protected void GUITimeRange(string name, ref float minValue, ref float maxValue, float minLimit, float maxLimit)
	{
		EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(name, GUILayout.Width(50));
			EditorGUILayout.LabelField(minLimit.ToString(), GUILayout.Width(40));
			minValue = BulletTime.MakeDescreetTime(minValue, BulletTime.frameRate);
			maxValue = BulletTime.MakeDescreetTime(maxValue, BulletTime.frameRate);
			minLimit = BulletTime.MakeDescreetTime(minLimit, BulletTime.frameRate);
			maxLimit = BulletTime.MakeDescreetTime(maxLimit, BulletTime.frameRate);
			EditorGUILayout.MinMaxSlider(ref minValue, ref maxValue, minLimit, maxLimit);
			EditorGUILayout.LabelField(maxLimit.ToString(), GUILayout.Width(50));
			minValue = BulletTime.MakeDescreetTime(minValue, BulletTime.frameRate);
			maxValue = BulletTime.MakeDescreetTime(maxValue, BulletTime.frameRate);
		EditorGUILayout.EndHorizontal();
	}

	static protected float GUISlider(string name, float value, float start, float end, bool enabled, bool fullRange)
	{
		var width = GUILayout.Width(50);
		var delta = fullRange ? 0.0f : 0.01f;
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField(name, GUILayout.Width(50+44));
//		EditorGUILayout.LabelField(start.ToString(), width);
		if (!enabled)
			GUI.enabled = false;
		var f = EditorGUILayout.Slider(value, start + delta, end - delta);
		if (!enabled)
			GUI.enabled = true;
		EditorGUILayout.LabelField(end.ToString(), width);
		EditorGUILayout.EndHorizontal();
		
		return f;
	}

	static protected void GUIObjectLink(Object o, System.Type t)
	{
		if (!o)
			return;
		GUIObjectLink(o, t, o.name);
	}

	static protected void GUIObjectLink(Object o, System.Type t, string displayName)
	{
		if (!o)
			return;
		if (GUILayout.Button(displayName, EditorStyles.miniButton, GUILayout.Width(80)))
			Selection.activeObject = o;
	}

	static double DescreetTime(double t, double step)
	{
		int d = (int)(t / step);
		return (double)(d) * step;
	}

	private float m_MinSliderTime = 0;
	private float m_MaxSliderTime = -1;
	private Vector2[] m_SequencerScrollViewPos = new Vector2[0];
	private Vector2 m_SplineAnimatorsScrollViewPos;
	private Vector2 m_AnimatedObjectsScrollViewPos;
	void OnGUI ()
	{
		if (BulletTime.justFinishedPlaying)
			Refresh(m_MinSliderTime);
		
		if (m_AnimatedObjects == null || m_CurrentScene != EditorApplication.currentScene)
			Refresh(0f);
		
		var lastTime = BulletTime.time;
		if (m_MaxSliderTime <= m_MinSliderTime)
			m_MaxSliderTime = m_MaxTime;
		m_MinSliderTime = (m_MinSliderTime > 0f)? m_MinSliderTime: 0f;
		m_MaxSliderTime = (m_MaxSliderTime <= m_MaxTime)? m_MaxSliderTime: m_MaxTime;
			
		if (BulletTime.activeInEditor)
		{		
			GUITimeRange("Range", ref m_MinSliderTime, ref m_MaxSliderTime, 0.0f, m_MaxTime);
			if (Application.isPlaying)
				GUI.enabled = false;
			if (m_SubFrames)
			{
				var t = GUISlider("Time", BulletTime.time, m_MinSliderTime, m_MaxSliderTime, true, true);
				if (!Application.isPlaying)
					BulletTime.time = t;
			}
			else
			{
				var t = GUISlider("Time", BulletTime.frameDescreetTime, m_MinSliderTime, m_MaxSliderTime, true, true);
				if (!Application.isPlaying)
					BulletTime.frameDescreetTime = t;
			}
			GUI.enabled = true;
		}
		
		EditorGUILayout.BeginHorizontal();
		if (Application.isPlaying)
			GUI.enabled = false;
		if (GUILayout.Button("<", /*EditorStyles.toolbarButton, */GUILayout.Width(24)))
			BulletTime.frame = BulletTime.frame - 1;
		if (GUILayout.Button(">", /*EditorStyles.toolbarButton, */GUILayout.Width(24)))
			BulletTime.frame = BulletTime.frame + 1;
		BulletTime.frameRate = (float)EditorGUILayout.IntField((int)BulletTime.frameRate, GUILayout.Width(48)); GUILayout.Label("fps", GUILayout.Width(32));
		GUI.enabled = true;
		m_SubFrames = GUILayout.Toggle(m_SubFrames, "sub frames");
		if (BulletTime.activeInEditor && !Application.isPlaying)
		{
			BulletTime.time = Mathf.Clamp(BulletTime.time, 0.0f, m_MaxTime);
			if (!Mathf.Approximately(BulletTime.time, lastTime))
			{
				BulletTime.deltaTime = Mathf.Abs (BulletTime.time - lastTime);
				if (BulletTime.time <= m_MinSliderTime)
				{
					BulletTime.time = m_MinSliderTime;
					BulletTime.deltaTime = m_MaxTime;
				}
			}
		}
		GUILayout.Label("      Info: " + BulletTime.frame + ", " + BulletTime.time + ", " + BulletTime.deltaTime + ", " + BulletTime.frameFraction);
		EditorGUILayout.Space();
		BulletTime.activeInEditor = GUILayout.Toggle(BulletTime.activeInEditor, BulletTime.activeInEditor ? "ACTIVE" : "INACTIVE", GUILayout.MaxWidth(74));
		EditorGUILayout.EndHorizontal();

		if (GUILayout.Button("Refresh"))
			Refresh(m_MinSliderTime);

		SampleAnimation();
		

		// Display important objects
		if (m_ImportantObjects != null)
		{
			if (m_ImportantObjects.Length > 0)
			{
				EditorGUILayout.BeginHorizontal("box", GUILayout.MaxHeight(124));
				GUILayout.Label("Important objects:", GUILayout.Width(124));
				foreach (var io in m_ImportantObjects)
					if (io)
						GUIObjectLink(io.gameObject, typeof(GameObject), io.displayName);
				EditorGUILayout.EndHorizontal();
			}
		}


		// Spline sequencers
		var showLabel = true;
		if (m_Sequencers != null)
		{
			if (m_SequencerScrollViewPos.Length != m_Sequencers.Length)
				m_SequencerScrollViewPos = new Vector2[m_Sequencers.Length];
			for (int q = 0; q < m_Sequencers.Length; ++q)
			{
				var seq = m_Sequencers[q];
				if (!seq)
					continue;
				
				if (showLabel)
				{
					EditorGUILayout.BeginVertical("box", GUILayout.MaxHeight (124));
					GUILayout.Label("Sequencers:");
					showLabel = false;
				}

				/*EditorGUILayout.BeginVertical("box",GUILayout.MaxHeight (124));
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label("Spline sequencer on ", GUILayout.MaxWidth (120));
				GUIObjectLink(seq.gameObject, typeof(GameObject));
				EditorGUILayout.EndHorizontal();
				*/
				//EditorGUILayout.Space();

				m_SequencerScrollViewPos[q] = EditorGUILayout.BeginScrollView(m_SequencerScrollViewPos[q], GUILayout.Height (60), GUILayout.Height (84));
				EditorGUILayout.BeginHorizontal();
				foreach (var trigger in seq.triggers)
				{
					var animTrigger = trigger as SplineAnimationTrigger;
					Color defaultBgColor = GUI.backgroundColor;
					if (trigger == seq.activeTrigger)
						GUI.backgroundColor = Color.green;

					EditorGUILayout.BeginVertical("box");
					GUILayout.Label(""+Mathf.Floor(trigger.splineAnimatorT*100.0f)/100.0f+((animTrigger)?("   " + animTrigger.clipName):""), EditorStyles.miniLabel);
					GUIObjectLink(trigger.gameObject, typeof(GameObject));
					if (animTrigger)
						GUIObjectLink(animTrigger.target, typeof(Animation));
					GUIObjectLink(trigger.camera, typeof(Camera));
					if (trigger.cameraAnimation)
						GUIObjectLink(trigger.cameraAnimation.clip, typeof(AnimationClip));

					EditorGUILayout.EndVertical();
					
					if (trigger == seq.activeTrigger)
						GUI.backgroundColor = defaultBgColor;
				}
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.EndScrollView();
			}
		}
		if (!showLabel)
		{
			EditorGUILayout.EndVertical();
		}
		
		// Spline animator
		showLabel = true;
		if (m_SplineAnimators != null)
			foreach (var sa in m_SplineAnimators)
			{
				if (!sa)
					continue;
				if (showLabel)
				{
					EditorGUILayout.BeginVertical("box",GUILayout.MaxHeight (124));
					GUILayout.Label("Spline animators:");
					m_SplineAnimatorsScrollViewPos = EditorGUILayout.BeginScrollView(m_SplineAnimatorsScrollViewPos, GUILayout.Height (60), GUILayout.MaxHeight (124));
					showLabel = false;
				}

				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(64);
				GUIObjectLink(sa, typeof(SplineAnimator));
				GUIObjectLink(sa.spline, typeof(Spline));
				GUILayout.Label(""+Mathf.Floor(sa.timeLength*100.0f)/100.0f+" sec  " + sa.wrapMode, EditorStyles.miniLabel,  GUILayout.MaxWidth (128));
				EditorGUILayout.EndHorizontal();
			}
		if (!showLabel)
		{
			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();
		}
		
		// Autoplay animations
		showLabel = true;
		if (m_AnimatedObjects != null)
			foreach (var ao in m_AnimatedObjects)
			{
				if (!ao.animation)
					continue;
					
				if (showLabel)
				{
					EditorGUILayout.BeginVertical("box",GUILayout.MaxHeight (124));
					GUILayout.Label("Automatically played animations:");
					m_AnimatedObjectsScrollViewPos = EditorGUILayout.BeginScrollView(m_AnimatedObjectsScrollViewPos, GUILayout.Height (60), GUILayout.MaxHeight (124));
					showLabel = false;
				}
				
				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(64);
				GUIObjectLink(ao.animation, typeof(Animation));
				GUIObjectLink(ao.animation.clip, typeof(AnimationClip));
				GUILayout.Label(""+Mathf.Floor(ao.animation.clip.length*100.0f)/100.0f+" sec  " + ao.animation.clip.wrapMode, EditorStyles.miniLabel,  GUILayout.MaxWidth (128));
				EditorGUILayout.EndHorizontal();
			}
		if (!showLabel)
		{
			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();
		}
		
		if (GUI.changed)
			foreach (var a in m_SplineAnimators)
				EditorUtility.SetDirty(a);		
	}

	void OnHierarchyChange()
	{
	}
}
