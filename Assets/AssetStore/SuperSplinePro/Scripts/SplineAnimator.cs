using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

//This class animates a gameobject along the spline at a specific speed.
[ExecuteInEditMode]
public class SplineAnimator : MonoBehaviour
{
	static public bool legacyOnboardCamera = false; // Until Erland checks that everything is OK with cameras

	public Spline spline;
	public Transform steadyCamRoot;
	public Transform onboardCamRoot;
	
	public float speed { get { return NeverStop(velocity / Mathf.Max(Mathf.Epsilon, (spline) ? spline.Length : 0f)); } }
	public float velocity = 100f;
	public float velocityNoise = 0.2f;
	public AnimationCurve velocityCurve;
	public bool useVelocityCurve = false;
	public float offSet = 0f;
	public float additionalOffSet { get { return _additionalOffSet; } set { _additionalOffSet = value; } }
	private float _additionalOffSet = 0f;
	public float offSetSideways = 0f;
	public float offSetUp = 0.5f;
	public float coolness = 0.5f;
	public float additionalOffSetSideways { get { return _additionalOffSetSideways; } set { _additionalOffSetSideways = value; } }
	private float _additionalOffSetSideways = 0f;
	public float additionalOffSetUp { get { return _additionalOffSetUp; } set { _additionalOffSetUp = value; } }
	private float _additionalOffSetUp = 0f;
	public float additionalLean = 0.0f;
	public float maxLeanAngle = 40f;
	public Transform originOfLean = null;
	public float sway = 0f;
	public float swayLeanAmplitude = 10f;
	public float swayFrequency = 1f;
	public float additionalSway { get { return _additionalSway; } set { _additionalSway = value; } }
	private float _additionalSway = 0f;

	public float leanAngleToTriggerFx = 35f;
	public ParticleSystem leftFx;
	public ParticleSystem rightFx;
	public AudioSource[] leftAudio;
	public AudioSource[] rightAudio;

	public WrapMode wrapMode = WrapMode.Clamp;

	
	//private Quaternion _r = Quaternion.identity; 
	private Vector3 _offs = Vector3.zero;
	//private Vector3 _tang = Vector3.right;
	
	//private float _radius = 5.0f;

	private float _noiseSeed = 0.1337f;

	private float _currLeanAngle = 0f;
	
	private double cacheWarpedTime = 0.0;
	private int cacheWarpedTimeFrameNumber = 0;

	public bool debug = false;
	
	static public Camera mainCamera;
	
	//private 
	public float timeLength { get { return EvaluateTimeLength(); /*_timeLength;*/ } }
	public int frameCount { get { return (int)(EvaluateTimeLength() * BulletTime.frameRate); } }

	void Start()
	{
		_noiseSeed = (gameObject.GetInstanceID() % 1024) / 1024f;
		//_tang = transform.right;
		_offs = Vector3.zero;
		_currLeanAngle = 0f;
		//_radius = renderer.bounds.extents.
	}
	
	private static bool SphereIntersectsCone (Vector3 spherePos, float sphereRadius, Vector3 conePos, Vector3 coneAxis, float coneAngle, float nearPlane, float farPlane)
    {
		var sina = Mathf.Sin(coneAngle * Mathf.Deg2Rad);
		var cosa = Mathf.Cos(coneAngle * Mathf.Deg2Rad);
        var U = conePos - (sphereRadius / sina) * coneAxis;
        var D = spherePos - U;
        var dsqr = Vector3.Dot(D, D);
        var e = Vector3.Dot(coneAxis, D);
        if (dsqr < nearPlane*nearPlane)
        	return true;
        if (e > 0 && e*e >= dsqr*cosa*cosa /*&& dsqr < farPlane*farPlane*/)
        {
            D = spherePos - conePos;
            dsqr = Vector3.Dot(D, D);
            e = -Vector3.Dot(coneAxis, D);
            if (e > 0 && e*e >= dsqr*sina*sina)
                return dsqr <= sphereRadius*sphereRadius;
            else
            	return true;
		}
	    return false;
	}
	
	void Update() 
	{
		if (!spline)
			return;
			
		EvaluateTimeLength();

		var splineAnimatorT = ConvertFrameToWarpedTime(BulletTime.frame, BulletTime.frameFraction, ref cacheWarpedTimeFrameNumber, ref cacheWarpedTime) + offSet + _additionalOffSet;
		var splineT = ConvertWarpedTimeToSplineNormalizedParameter(splineAnimatorT);
		
		Vector3 newPos = spline.GetPositionOnSpline(splineT);	

		newPos += _offs;
		newPos += Vector3.up * (offSetUp + _additionalOffSetUp);

		Quaternion qRot = spline.GetOrientationOnSpline(splineT);
		Vector3 tang = qRot * Vector3.right;

		float totalSway = sway + _additionalSway;
		float swayNoise = SwayNoise(swayFrequency);
		float swayAngle = swayNoise * swayLeanAmplitude * Mathf.Min(totalSway, 1.0f);
		float swayOffSet = swayNoise * totalSway;
		
		newPos += (offSetSideways + _additionalOffSetSideways + swayOffSet) * tang;

		var splineTMinusEpsilon = ConvertWarpedTimeToSplineNormalizedParameter(splineAnimatorT - 0.01f);
		
		Quaternion qPast = spline.GetOrientationOnSpline(splineTMinusEpsilon);
		float yrot = qPast.eulerAngles.y;
		yrot = yrot - qRot.eulerAngles.y;

		yrot = yrot % 360;
		if (yrot > 180)
			yrot = yrot - 360;
		if (yrot < -180)
			yrot = 360 + yrot;

		var leanAngle = Mathf.Clamp(coolness * (yrot % 180) + swayAngle, -maxLeanAngle, maxLeanAngle);
		leanAngle = Mathf.Clamp(leanAngle + additionalLean % 180, -maxLeanAngle, maxLeanAngle);

		var realDeltaTime = (Time.timeScale < Mathf.Epsilon) ? (Time.deltaTime / Time.timeScale) : 0.1f;
		
		//	Keffo: Changed this to prevent the 'swimming' when scrubbing, seems to work.
		//	Original: _currLeanAngle = Mathf.Lerp(_currLeanAngle, leanAngle, BulletTime.isEditing ? 1.0f : 0.15f * 30f * realDeltaTime);
		if( BulletTime.isEditing || BulletTime.paused )
			_currLeanAngle = leanAngle;
		else
			_currLeanAngle = Mathf.Lerp(_currLeanAngle, leanAngle, 0.15f * 30f * realDeltaTime);
		
		Quaternion r = Quaternion.Euler(0.0f, 0.0f, _currLeanAngle);
		

		Vector3 steadyCamPos = newPos;
if (!debug)
		{
			if (originOfLean)
			{
				var delta = originOfLean.transform.position - transform.position;
				delta = Quaternion.Inverse(qRot) * delta;
				newPos = newPos + delta - r * delta;
			}
			
			transform.position = newPos;//Vector3.Lerp(transform.position, newPos, BulletTime.deltaTime * 5.0f);
			transform.rotation = qRot * r;
		}

		if (leanAngle < -leanAngleToTriggerFx)
			if (rightFx && !rightFx.IsAlive())
			{
				rightFx.Stop(true); rightFx.Play(true);
				var ra = BulletTime.RandomFromArray(rightAudio) as AudioSource;
				if( ra && ra.enabled == true )
					ra.Play();
			}
		if (leanAngle > leanAngleToTriggerFx)
			if (leftFx && !leftFx.IsAlive())
			{
				leftFx.Stop(true); leftFx.Play(true);
				var la = BulletTime.RandomFromArray(leftAudio) as AudioSource;
				if( la && la.enabled == true )
					la.Play();
			}

		if (steadyCamRoot && !debug)
		{
			steadyCamRoot.transform.position = steadyCamPos;
			steadyCamRoot.transform.rotation = qRot;
		}
		
		if (onboardCamRoot)
		{
			if (legacyOnboardCamera)
			{
				onboardCamRoot.transform.position = newPos;
				onboardCamRoot.transform.rotation = qRot * r;
			}
			else
			{
				var frameXform = (originOfLean) ? originOfLean.transform : transform;
				onboardCamRoot.transform.position = frameXform.position;
				onboardCamRoot.transform.rotation = frameXform.rotation;
			}
		}
	}
	
	private float SwayNoise(float freq)
	{
		return Mathf.Sin(BulletTime.time * freq) + Mathf.Sin(BulletTime.time * freq * 0.5f * 3.0f) + Mathf.Sin(BulletTime.time * freq * 0.3f * 15.0f);
	}
	
	private float cachedTimeLength = -1.0f;
	private float prevSpeed = 0;
	private float prevSeed = 0;
	private float prevAmountOfNoise = 0;
	private bool prevUseVelocityCurve = false;
	
	public void Invalidate ()
	{
		cachedTimeLength = -1.0f;
	}
	
	static float NeverStop(float v)
	{
		var VelocityEpsilon = 1e-4f;
		if (Mathf.Abs(v) < VelocityEpsilon)
			v = VelocityEpsilon * Mathf.Sign(v);
		return v;
	}

	public float EvaluateTimeLength()
	{
		if (cachedTimeLength >= 0.0f && speed == prevSpeed && _noiseSeed == prevSeed && velocityNoise == prevAmountOfNoise && prevUseVelocityCurve == useVelocityCurve)
			return cachedTimeLength;

		var MaxFrames = 60*60*(int)BulletTime.frameRate;
		int cachedFrameNumber = 0; double cachedTime = 0.0;
		
		var lastT = 0.0f;
		int frameIndex = 0;
		for (; frameIndex < MaxFrames; ++frameIndex)
		{
			var t = ConvertFrameToWarpedTime(frameIndex, ref cachedFrameNumber, ref cachedTime);
			if ((speed > 0) && (t > 1.0 || t < lastT))
				break;
			if ((speed < 0) && (t < -1.0 || t > lastT))
				break;
			lastT = t;
		}
		
		//	Keffo: meh.
		//Debug.Log("Spline Time Length: " + frameIndex + " frames");
		
		cachedTimeLength = (float)frameIndex * BulletTime.invFrameRate;
		prevSpeed = speed;
		prevSeed = _noiseSeed;
		prevAmountOfNoise = velocityNoise;
		prevUseVelocityCurve = useVelocityCurve;

		return cachedTimeLength;
	}
	
	public float ConvertFrameToWarpedTime(int frame, ref int cachedFrameNumber, ref double cachedTime)
	{
		if (cachedFrameNumber > frame)
		{
			cachedFrameNumber = 0;
			cachedTime = 0.0;
		}
				
		double dWarpedTime = cachedTime;
		//float invSplineLength = 1.0f/spline.Length;
		for (; cachedFrameNumber < frame; ++cachedFrameNumber)
		{
/*			float time = BulletTime.MakeFrameTime(cachedFrameNumber, BulletTime.frameRate);
			float noise = BulletTime.Noise1D(0.1f*time + _noiseSeed);
			float animatedSpeed = (useVelocityCurve)? velocityCurve.Evaluate(time)*invSplineLength: 0f;
			float noisieSpeed = (speed + animatedSpeed) * Mathf.Lerp(1f - velocityNoise/2f, 1f + velocityNoise/2f, noise);
			float finalSpeed = NeverStop(noisieSpeed);*/
			float finalSpeed = NeverStop(speed);
			dWarpedTime += BulletTime.invFrameRate * finalSpeed;
		}
		cachedTime = dWarpedTime;
		return (float)cachedTime;
	}

	public float ConvertFrameToWarpedTime(int frame, float frameFraction, ref int cachedFrameNumber, ref double cachedTime)
	{
		float dWarpedTime = ConvertFrameToWarpedTime(frame, ref cachedFrameNumber, ref cachedTime);
		int uncachedFrameNumber = cachedFrameNumber; double uncachedTime = cachedTime;
		float dWarpedTime2 = ConvertFrameToWarpedTime(frame + 1, ref uncachedFrameNumber, ref uncachedTime);
		
		return Mathf.Lerp(dWarpedTime, dWarpedTime2, frameFraction);
	}
		
	public float ConvertWarpedTimeToSplineNormalizedParameter(float time)
	{
		return WrapValue(time, 0f, 1f, wrapMode);
	}
	
	public Vector3 GetPositionOnSpline(int frame, ref int cachedFrameNumber, ref double cachedTime)
	{
		var warpedTime = ConvertFrameToWarpedTime(frame, ref cachedFrameNumber, ref cachedTime) + offSet + _additionalOffSet;
		return spline.GetPositionOnSpline(ConvertWarpedTimeToSplineNormalizedParameter(warpedTime));
	}
	
	static float WrapValue( float v, float start, float end, WrapMode wMode )
	{
		switch( wMode )
		{
		case WrapMode.Clamp:
		case WrapMode.ClampForever:
			return Mathf.Clamp( v, start, end );
		case WrapMode.Default:
		case WrapMode.Loop:
			return Mathf.Repeat( v, end - start ) + start;
		case WrapMode.PingPong:
			return Mathf.PingPong( v, end - start ) + start;
		default:
			return v;
		}
	}
}
