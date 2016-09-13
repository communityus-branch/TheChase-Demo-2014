using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class SplineAnimationTrigger : SplineCameraTrigger
{
	[HideInInspector]
	public Animation target;
	[HideInInspector]
	public string clipName;
	[HideInInspector]
	public BulletTime.AnimatedObject localAnimation = new BulletTime.AnimatedObject();
	[HideInInspector]
	public Animation target2;
	[HideInInspector]
	public string clipName2;
	[HideInInspector]
	public BulletTime.AnimatedObject localAnimation2 = new BulletTime.AnimatedObject();
	[HideInInspector]
	public float fadeLength = 0.5f;

	new public AudioSource[] audio;
	public ParticleSystem[] fx;
	public bool stopFxOnExit = false;

	[HideInInspector]
	public MethodFire[] methodFires;

	// Hack: use LateUpdate to get Update from SplineCameraTrigger working
	void LateUpdate ()
	{ 
		if (BulletTime.isEditing)
		{
			localAnimation.Reset(target, clipName);
			localAnimation2.Reset(target2, clipName2);
		}

#if UNITY_EDITOR
		if( BulletTime.isEditing && inside )
		{
			// scrub particle time
			foreach (ParticleSystem ps in fx)
				if (ps)
					ps.Simulate(updateTimeInTrigger, true, false);
		}
#endif
	}
	
	public override void Start()
	{
		base.Start();
		methodFires = this.GetComponentsInChildren<MethodFire>();
	}
	

	override public void OnCameraEnter()
	{
		//inside = true;
		base.OnCameraEnter();
		
		if( methodFires != null )
		{
			for( int i=0; i<methodFires.Length; i++ )
			{
				var mf = methodFires[i];
				if( mf && mf.enabled )
					mf.Fire();
			}
		}
		
		if (target)
		{
			target.Stop();
			target.Play(clipName);
			//target.CrossFade(clipName, fadeLength);
		}
		if (target2)
		{
			target2.Stop();
			target2.Play(clipName2);
		}
		
		// HACK: skip couple of first frames from playing sound
		// WORKAROUND for preventing audio from the last trigger to start playing on 1st frame
		// Keffo: Don't think this is an issue anymore, with the forced-stop of all sounds on bullettime restart?
		if (BulletTime.frame > 2 && audio != null)
			foreach (AudioSource a in audio)
				if( a && a.enabled == true && gameObject.activeInHierarchy)
					a.Play();

		foreach (ParticleSystem ps in fx)
			if (ps)
			{
				ps.Stop();
				ps.Clear();
				ps.Play();
			}
	}

	private float updateTimeInTrigger = -1.0f;

	override public void OnCameraStay(float timeInTrigger, float normalizedTimeInTrigger)
	{
#if UNITY_EDITOR
		if (BulletTime.isEditing)
		{
			var t = (localAnimation.cachedState != null)? Mathf.Repeat(timeInTrigger, localAnimation.cachedState.length): timeInTrigger;
			BulletTime.SampleAnimation(localAnimation, t, true);
			//t = (localAnimation2.cachedState != null) ? Mathf.Repeat(timeInTrigger, localAnimation2.cachedState.length) : timeInTrigger;
			t = timeInTrigger;
			BulletTime.SampleAnimation(localAnimation2, t, true);

			updateTimeInTrigger = timeInTrigger;	
		}
#endif

		if (Application.isPlaying && BulletTime.timeJump)
		{
			//;;Debug.Log("Animation Time Jump " + timeInTrigger);
			if (target != null && target.GetComponent<Animation>() != null)
				foreach (AnimationState state in target.GetComponent<Animation>())
					//if (state.enabled && state.weight > 0)
						state.time = timeInTrigger;
			if (target2 != null && target2.GetComponent<Animation>() != null)
				foreach (AnimationState state in target2.GetComponent<Animation>())
					//if (state.enabled && state.weight > 0)
						state.time = timeInTrigger;
		}
	}
		
	override public void OnCameraExit()
	{
		if (BulletTime.isEditing)
		{
			BulletTime.SampleAnimation(localAnimation, (localAnimation.cachedState != null)? localAnimation.cachedState.length: 0, true);
			BulletTime.SampleAnimation(localAnimation2, (localAnimation2.cachedState != null) ? localAnimation2.cachedState.length : 0, true);
		}

		if (audio != null)
			foreach (var a in audio)
				if (a && a.loop)
					a.Stop();

		foreach (ParticleSystem ps in fx)
			if (ps && (ps.loop || stopFxOnExit))
			{
				ps.Stop();
				ps.Clear();
			}
		
		inside = false;
	}
}
