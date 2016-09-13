using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class SplineCameraSequencer : MonoBehaviour {
	public SplineAnimator master;
	public LightGroup defaultLightGroup;
	private SplineCameraTrigger[] m_Triggers = new SplineCameraTrigger[0];
	private SplineCameraTrigger m_ActiveTrigger = null;
	[HideInInspector]
	public SplineCameraTrigger[] triggers { get { return m_Triggers; } }
	[HideInInspector]
	public SplineCameraTrigger activeTrigger { get { return m_ActiveTrigger; } }
	public float timeLength { get { return GetMaxTimeLength(); } }
	
	void Start () {
		Invalidate ();
	}
	
	public void Update () {
		if (BulletTime.isEditing)
		{
			m_Triggers = CollectTriggers(transform);
			SortTriggers(m_Triggers);
		}
	
		int frameCount = (master)? master.frameCount: (int)(timeLength * BulletTime.frameRate);
		WrapMode wrapMode = (master)? master.wrapMode: WrapMode.Loop;
		Sample(BulletTime.frame, BulletTime.frameFraction, frameCount, wrapMode);
	}
	
	public float GetMaxTimeLength()
	{
		var maxLength = 0.0f;
		foreach (var trigger in triggers)
			if (trigger.spline)
				maxLength = Mathf.Max(trigger.spline.timeLength, maxLength);
		return maxLength;
	}
	
	static public SplineCameraTrigger[] CollectTriggers(Transform root)
	{
		var triggers = (root == null) ?
			Object.FindObjectsOfType(typeof(SplineCameraTrigger)) as SplineCameraTrigger[]:
			root.GetComponentsInChildren<SplineCameraTrigger>(true) as SplineCameraTrigger[];
		return triggers;
	}

	static public void SortTriggers(SplineCameraTrigger[] triggers)
	{
		System.Array.Sort(triggers, delegate(SplineCameraTrigger a, SplineCameraTrigger b)
		{
			return a.splineAnimatorFrame.CompareTo(b.splineAnimatorFrame);
		});
	}
	
	static public void ForceDisableCameras(SplineCameraTrigger[] triggers)
	{
		foreach (var trigger in triggers)
		{
			if (trigger.camera)
				trigger.camera.gameObject.SetActive(false);
			if (trigger.localAudioListener)
				trigger.localAudioListener.enabled = false;
		}
		
		var cameras = Object.FindObjectsOfType(typeof(Camera)) as Camera[];
		foreach (var cam in cameras)
		{
			var al = cam.gameObject.GetComponent<AudioListener>();
			if (al)
				al.enabled = false;
			cam.enabled = false;
		}			
	}

	static public void SetLightGroup(LightGroup defaultLightGroup)
	{
		var lightGroups = Object.FindObjectsOfType(typeof(LightGroup)) as LightGroup[];
		var lightGroup = defaultLightGroup;
		if (lightGroup)
		{
			BulletTime.DisableLightGroups(lightGroups);
			BulletTime.SetActiveLightGroup(lightGroup);
		}
	}
	
	public void Sample(int frame, float frameFraction, int frameCount, WrapMode wrapMode)
	{
		if (BulletTime.paused && !BulletTime.isEditing)
		{
			if (m_ActiveTrigger)
			{
				m_ActiveTrigger.OnCameraExit();
				m_ActiveTrigger = null;
			}
			return;
		}

		if (wrapMode == WrapMode.Loop)
			frame %= frameCount;
		else if (wrapMode == WrapMode.PingPong)
			frame = (int)Mathf.PingPong((float)frame, (float)frameCount);
		
		var lastTrigger = m_ActiveTrigger;
		m_ActiveTrigger = null;

		int triggerIndex = -1;
		if (wrapMode == WrapMode.Loop && triggers.Length > 0)
		{
			triggerIndex = triggers.Length-1;
			m_ActiveTrigger = triggers[triggerIndex];
		}
		
		for (int q = 0; q < triggers.Length; ++q)
			if (frame >= triggers[q].splineAnimatorFrame || m_ActiveTrigger == null)
				triggerIndex = q;
		
		int triggerLengthInFrames = 0;
		if (triggerIndex >= 0)
		{
			m_ActiveTrigger = triggers[triggerIndex];
			var nextTriggerIndex = (triggerIndex+1)%triggers.Length;
			triggerLengthInFrames = triggers[nextTriggerIndex].splineAnimatorFrame - triggers[triggerIndex].splineAnimatorFrame;
			
			if (triggerLengthInFrames < 0) // wrap
				triggerLengthInFrames = frameCount + triggerLengthInFrames;
		}
		

		if (lastTrigger != m_ActiveTrigger)
		{
			if (lastTrigger)
				lastTrigger.OnCameraExit();
			m_ActiveTrigger.OnCameraEnter();

			var lightGroup = m_ActiveTrigger.lightGroup;
			if (!lightGroup)
				lightGroup = defaultLightGroup;
			if (lightGroup)
				BulletTime.SetActiveLightGroup(lightGroup);
		}
		if (m_ActiveTrigger)
		{
			var timeInTrigger = BulletTime.MakeFrameTimeDbl(frame - m_ActiveTrigger.splineAnimatorFrame, BulletTime.frameRate, frameFraction);
			if (timeInTrigger < 0)
				timeInTrigger = BulletTime.MakeFrameTimeDbl(frameCount, BulletTime.frameRate) + timeInTrigger;
			
			double normalizedTimeInTrigger = 0;
			if (triggerLengthInFrames > 0)
				normalizedTimeInTrigger = timeInTrigger / BulletTime.MakeFrameTimeDbl(triggerLengthInFrames, BulletTime.frameRate);

			m_ActiveTrigger.OnCameraStay((float)timeInTrigger, Mathf.Clamp((float)normalizedTimeInTrigger, 0f, 1f));
		}

		if (BulletTime.isEditing)
		{
			var lightGroup = m_ActiveTrigger.lightGroup;
			if (!lightGroup)
				lightGroup = defaultLightGroup;
			if (lightGroup)
				SetLightGroup(lightGroup);
		}
	}
	
	public void Invalidate ()
	{
		if (m_ActiveTrigger)
			m_ActiveTrigger.OnCameraExit();
		m_ActiveTrigger = null;
		m_Triggers = CollectTriggers(transform);
		SortTriggers(m_Triggers);
		foreach (var trigger in m_Triggers)
			trigger.UpdatePositionOnSpline();
		SortTriggers(m_Triggers);
		ForceDisableCameras(m_Triggers);

		SetLightGroup(defaultLightGroup);
	}

	void OnDrawGizmos()
	{
/*		if (activeTrigger)
		{
			Gizmos.color = new Color (1.0f,0.8f,0.3f,0.75f);
			var pos = activeTrigger.position;
			var posUp = pos + Vector3.up * activeTrigger.transform.lossyScale.magnitude*2;
   			Gizmos.DrawCube(posUp, activeTrigger.transform.lossyScale*2);
   			Gizmos.DrawLine(posUp, pos);
   			if (activeTrigger.camera)
	   			Gizmos.DrawLine(posUp, activeTrigger.camera.transform.position);
		}*/
   	}

}
