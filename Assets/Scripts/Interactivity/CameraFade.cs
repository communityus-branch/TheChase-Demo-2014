using UnityEngine;
using System;
 
public class CameraFade : MonoBehaviour
{
	private AnimationCurve Ramp = null;

	#region Curves
	public AnimationCurve	TransitionRamp = new AnimationCurve(
			new Keyframe[3]
			{
				new Keyframe(0.0f,0.0f),
				new Keyframe(0.5f,1.0f),
				new Keyframe(1.0f,0.0f)
			}
		);

	public AnimationCurve	FadeoutRamp = new AnimationCurve(
			new Keyframe[2]
			{
				new Keyframe(0.0f,0.0f),
				new Keyframe(1.0f,1.0f)
			}
		);

	public AnimationCurve	FadeinRamp = new AnimationCurve(
			new Keyframe[2]
			{
				new Keyframe(0.0f,1.0f),
				new Keyframe(1.0f,0.0f)
			}
		);
	#endregion
	
	private Action onComplete = null;
	private Action onAction = null;
	private float onActionTime = 0.5f;

	private float accTime = 0;
	private float lastRealTime = 0;
	private float Duration = 3;
	private bool m_Active = false;
	public bool Active { get { return m_Active; } protected set { m_Active = value; } }

	private static BulletTimeCinematron m_Cinematron = null;
	protected BulletTimeCinematron Cinematron
	{
		get
		{
			if( m_Cinematron )
				return m_Cinematron;

			var go = GameObject.Find( "__Cinematron" );
			if( go )
				m_Cinematron = go.GetComponent<BulletTimeCinematron>();

			return m_Cinematron;
		}
	}

	void Update()
	{
		if( !Active )
			return;

		accTime += (Time.realtimeSinceStartup - lastRealTime );
		lastRealTime = Time.realtimeSinceStartup;
		var unitTime = accTime / Duration;
		
		if( unitTime > 1.0f )
		{
			unitTime = 1.0f;
			
			if( onComplete != null )
				onComplete();
			
			Active = false;
		}
		
		var f = Ramp.Evaluate( unitTime );
		if( f > 0.0f )
		{
			if( unitTime >= onActionTime && onAction != null )
			{
				onAction();
				onAction = null;
			}
			
			Cinematron.setFade( 1.0f - f );
		}
	}
	
	private void setParams( float duration, float unitActionTime, Action action, Action complete )
	{
		if( duration <= 0 )
			return;

		Duration = duration;
		accTime = 0.0f;
		lastRealTime = Time.realtimeSinceStartup;
		Active = true;
		onComplete = complete;
		onAction = action;
		onActionTime = unitActionTime;
	}
	
	public void Transition( float duration, float unitActionTime = 0.5f, Action action = null, Action completeAction = null )
	{
		Ramp = TransitionRamp;
		setParams( duration, unitActionTime, action, completeAction );
	}
	
	public void FadeIn( float duration, float unitActionTime = 1.0f, Action action = null )
	{
		Ramp = FadeinRamp;
		setParams( duration, unitActionTime, action, null );
	}
	
	public void FadeOut( float duration, float unitActionTime = 1.0f, Action action = null )
	{
		Ramp = FadeoutRamp;
		setParams( duration, unitActionTime, action, null );
	}
}