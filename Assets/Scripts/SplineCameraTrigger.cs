using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class SplineCameraTrigger : MonoBehaviour
{
	[HideInInspector]
	public Animation cameraAnimation = null;
	public BulletTime.AnimatedObject localCameraAnimation = new BulletTime.AnimatedObject();
	[HideInInspector]
	public AudioListener localAudioListener = null;
	protected bool inside = false;
	private bool deactivateOnExit = false;
	private bool disableOnExit = false;
	private bool stopLocalAnimationOnExit = false;
	private bool disableLocalAnimationOnExit = false;
	private bool disableAudioListenerOnExit = false;

	public SplineAnimator spline = null;
	private SplineAnimator prevSpline = null;
	private float prevSplineTimeLength = 0.0f;
	private Vector3 prevPosition;
	[HideInInspector]
	public int splineAnimatorFrame;
	public float splineAnimatorT { get { return BulletTime.MakeFrameTime(splineAnimatorFrame, BulletTime.frameRate); } }

	new public Camera camera = null;

	public Renderer temporaryVisibleObject = null;
	public Renderer temporaryVisibleObject2 = null;

	public LightGroup lightGroup = null;
	

	private Vector3 splinePosition;
	public Vector3 position { get { return (spline)? splinePosition: transform.position; } }
	
	public bool restartDemo = false;
	
	public SplineAnimatorTweak[] tweaks;
	[System.Serializable]
	public class SplineAnimatorTweak
	{
		public SplineAnimator target = null;
		public float offset = 0f;
		public float offsetSideways = 0f;
		public float offsetUp = 0f;
		public float sway = 0f;
	}

	public virtual void Start()
	{
		prevPosition = transform.position;
		if (!spline)
			spline = FindClosestSpline(transform);
		if (!localAudioListener && camera)
			localAudioListener = FindAudioListener(camera.gameObject);
		if (!cameraAnimation && camera)
			cameraAnimation = camera.GetComponent<Animation>();
	}
	
	void Update () {
		if (requestDemoRestart)
		{
			requestDemoRestart = false;
			Debug.Log ("Restart demo");
			BulletTime.Restart();
		}	
		
		if (BulletTime.isEditing)
		{
			var sameLength = Mathf.Approximately(prevSplineTimeLength, spline.timeLength);
			var samePosition = Vector3.Distance(transform.position, prevPosition) <= Mathf.Epsilon;
			if (spline != prevSpline || !sameLength || !samePosition)
				UpdatePositionOnSpline();
			
			if (camera)
			{
				cameraAnimation = camera.GetComponent<Animation>();
				// local animations must be disabled or not played automatically 
				if (cameraAnimation && cameraAnimation.enabled && cameraAnimation.playAutomatically)
					cameraAnimation = null;
				if (cameraAnimation && !cameraAnimation.clip)
					cameraAnimation = null;
				
				if (localCameraAnimation.animation != cameraAnimation)
					localCameraAnimation.Reset(cameraAnimation);

				localAudioListener = FindAudioListener(camera.gameObject);
			}
		}
	}
	
	static AudioListener FindAudioListener(GameObject go)
	{
		var listeners = go.GetComponentsInChildren<AudioListener>(true); // retrieve disabled AudioListeners as well
		if (listeners.Length == 0)
			return null;
		return listeners[0];
	}
	
	public void UpdatePositionOnSpline()
	{
		if (!spline || !spline.spline)
			return;
		
		prevPosition = transform.position;
		prevSpline = spline;
		prevSplineTimeLength = spline.timeLength;
		
		//Debug.Log ("UpdatePositionOnSpline " + this);
		splineAnimatorFrame = GetClosestFrameOnSpline(spline, transform.position, ref splinePosition);
	}

	static public SplineAnimator FindClosestSpline(Transform root)
	{
		var t = root;
		while (t)
		{
			var seq = t.gameObject.GetComponent<SplineCameraSequencer>();
			if (seq && seq.master)
				return seq.master;
			t = t.parent;
		}
		SplineAnimator s = null;
		if (root.parent)
			s = root.parent.GetComponentInChildren<SplineAnimator>() as SplineAnimator;
		if (!s)
			s = Object.FindObjectOfType(typeof(SplineAnimator)) as SplineAnimator;
		return s;
	}

/*
	static public float GetClosestPositionOnSpline(SplineAnimator spline, Vector3 pos, float accuracy)
	{
		return GetClosestPositionOnSpline(spline, pos, accuracy, 256);
	}

	static public float GetClosestPositionOnSpline(SplineAnimator spline, Vector3 pos, float accuracy, int maxStepCount)
	{		
		float bestT = 0;
		var len = spline.timeLength;
		var step = Mathf.Max(accuracy, len / (float)maxStepCount);
		for (float t = 0; t < len; t += step)
			if (Vector3.Distance(pos, spline.GetPositionOnSpline(t)) < Vector3.Distance(pos, spline.GetPositionOnSpline(bestT)))
				bestT = t;
		
		return bestT;
	}
*/
	static public int GetClosestFrameOnSpline(SplineAnimator spline, Vector3 pos, ref Vector3 splinePosition)
	{
		int bestFrame = 0;
		float bestDistance = Mathf.Infinity;
		var len = spline.frameCount;
		
		int cachedFrameNumber = 0; double cachedTime = 0;		
		for (var q = 0; q < len; q++)
		{
			var posOnSpline = spline.GetPositionOnSpline(q, ref cachedFrameNumber, ref cachedTime);
			var distance = Vector3.Distance(pos, posOnSpline);
			if (distance >= bestDistance)
				continue;
			bestFrame = q;
			bestDistance = distance;
			splinePosition = posOnSpline;
		}

		return bestFrame;
	}

	static public void ApplyTweaks(SplineAnimatorTweak[] tweaks)
	{
		if (tweaks != null)
			foreach (var t in tweaks)
				if (t != null && t.target)
				{
					t.target.additionalOffSet = t.offset * 0.001f;
					t.target.additionalOffSetSideways = t.offsetSideways;
					t.target.additionalOffSetUp = t.offsetUp;
					t.target.additionalSway = t.sway;
				}
	}

	static public void ResetTweaks(SplineAnimatorTweak[] tweaks)
	{
		if (tweaks != null)
			foreach (var t in tweaks)
				if (t != null && t.target)
				{
					t.target.additionalOffSet = 0f;
					t.target.additionalOffSetSideways = 0f;
					t.target.additionalOffSetUp = 0f;
					t.target.additionalSway = 0f;
				}
	}
	
	///// Camera related ///////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	private bool requestDemoRestart = false;
	private bool cameraActive = false;
	virtual public void OnCameraEnter()
	{
		inside = true;
		
		if (restartDemo)
			requestDemoRestart = true;
		
		if (!camera)
			return;

		ActivateCamera();
		ApplyTweaks();
		
		SplineAnimator.mainCamera = camera;
	}

	virtual public void OnCameraStay(float timeInTrigger, float normalizedTimeInTrigger)
	{
		if (!camera)
			return;

		if (BulletTime.allowCameraTriggers && !cameraActive)
			ActivateCamera();
		else if (!BulletTime.allowCameraTriggers && cameraActive)
			DeactivateCamera();

		if (!BulletTime.allowCameraTriggers)
			return;
	
		camera.gameObject.SendMessage("OnCameraStay", normalizedTimeInTrigger, SendMessageOptions.DontRequireReceiver);

		if (BulletTime.isEditing)
		{
			BulletTime.SampleAnimation(localCameraAnimation, timeInTrigger, true);
			ApplyTweaks();
		}
	}
	
	virtual public void OnCameraExit()
	{
		inside = false;
		if (!camera)
			return;

		DeactivateCamera();
		ResetTweaks();
	}
	
	// helpers
	protected void ActivateCamera()
	{
		deactivateOnExit = disableOnExit = stopLocalAnimationOnExit = disableLocalAnimationOnExit = disableAudioListenerOnExit = false;
		if (BulletTime.isEditing)
			disableOnExit = true;

		if (BulletTime.allowCameraTriggers && !cameraActive)
		{
			cameraActive = true;

			// not active, but has parent which is active
			if (!camera.gameObject.activeInHierarchy && camera.transform.parent && camera.transform.parent.gameObject.activeInHierarchy)
			{			
				camera.gameObject.SetActive(true);
				deactivateOnExit = true;
			}
			
			if (camera.gameObject.activeInHierarchy && !camera.enabled)
			{
				disableOnExit = true;
				camera.enabled = true;
			}
			
			if (cameraAnimation && Application.isPlaying)
			{
				stopLocalAnimationOnExit = true;
				if (!cameraAnimation.enabled)
					disableLocalAnimationOnExit = true;
				cameraAnimation.enabled = true;
				cameraAnimation.Rewind();
				cameraAnimation.Play();
			}
		
			if (localAudioListener)
			{
				disableAudioListenerOnExit = true;
				localAudioListener.enabled = true;
			}
			
			camera.gameObject.SendMessage("OnCameraEnter", SendMessageOptions.DontRequireReceiver);
		}
		else
			cameraActive = false;
	}

	protected void DeactivateCamera()
	{
		if (!cameraActive)
			return;

		camera.gameObject.SendMessage("OnCameraExit", SendMessageOptions.DontRequireReceiver);
			
		if (disableOnExit)
			camera.enabled = false;
			
		if (stopLocalAnimationOnExit)
			cameraAnimation.Stop();

		if (disableLocalAnimationOnExit)
			cameraAnimation.enabled = false;

		if (disableAudioListenerOnExit)
			localAudioListener.enabled = false;

		if (deactivateOnExit)
			camera.gameObject.SetActive(false);

		cameraActive = false;
	}

	public void ApplyTweaks()
	{
		if (temporaryVisibleObject)
			temporaryVisibleObject.enabled = true;
		if (temporaryVisibleObject2)
			temporaryVisibleObject2.enabled = true;
		ApplyTweaks(tweaks);
	}

	public void ResetTweaks()
	{
		if (temporaryVisibleObject)
			temporaryVisibleObject.enabled = false;
		if (temporaryVisibleObject2)
			temporaryVisibleObject2.enabled = false;
		ResetTweaks(tweaks);
	}
	
	void OnDrawGizmos () 
	{
		Gizmos.color = (camera)? new Color (0.5f,0.0f,0.5f,0.5f): new Color (0.9f,0.9f,0.9f,0.5f);
   		Gizmos.DrawSphere (position, transform.lossyScale.magnitude);
		Gizmos.color *= 3.0f;
   		Gizmos.DrawLine (position, transform.position);
   		
   		if (inside)
   		{		
			Gizmos.color = (camera)? new Color (1.0f,0.8f,0.3f,0.75f): new Color (0.9f,0.9f,0.9f,0.75f);
			var posUp = position + Vector3.up * transform.lossyScale.magnitude*2;
			Gizmos.DrawCube(posUp, transform.lossyScale*2);
			Gizmos.DrawLine(posUp, position);
			if (camera)
				Gizmos.DrawLine(posUp, camera.transform.position);
   		}
   	}

	void OnDrawGizmosSelected () 
	{
		OnDrawGizmos ();
		
		if (!inside)
		{	
			Gizmos.color = new Color (1.0f,0.8f,0.3f,0.25f);
			if (camera)
		   		Gizmos.DrawLine(transform.position, camera.transform.position);
		}
	}
}
