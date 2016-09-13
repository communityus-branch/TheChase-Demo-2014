using System;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class WorldFeatureDemonstration : FeatureDemonstration
{
	private Spline[] paths;
	private int currentIdx = 0;
	private Spline currentPath = null;
	private bool performPathFadein = true;

	public GameObject Paths = null;
	public float PanDuration = 10.0f;
	public float PanFadeDuration = 1.0f;
	public string activePath = "N/A";

	public AnimationCurve rampIn = new AnimationCurve( new Keyframe[ 2 ] { new Keyframe( 0.0f, 0.0f ), new Keyframe( 1.0f, 1.0f ) } );
	public AnimationCurve rampOut = new AnimationCurve( new Keyframe[ 2 ] { new Keyframe( 0.0f, 1.0f ), new Keyframe( 1.0f, 0.0f ) } );
	
	public override void Start()
	{
		base.Start();
		if( Paths != null )
			paths = Paths.GetComponentsInChildren<Spline>();
	}
	
	public override void Show( float duration, bool performTransition = true )
	{
		base.Show( duration, performTransition );
		performPathFadein = performTransition;
	}

	protected float lerpFov( Spline path, float t )
	{
		float normalizedParam;
		int normalizedIndex;
		path.RecalculateParameter( t, out normalizedIndex, out normalizedParam );

		Debug.Log( "path:" + normalizedIndex + ", " + normalizedParam );

		SplineNode n0 = path.SplineNodes[ normalizedIndex ];
		if( !n0.CheckReferences() )
		{
			Debug.LogError( "n0.CheckReferences() == false" );
			return 0.0f;
		}

		SplineNode n1 = n0.NextNode0;
		Debug.Log( "fov:" + n0.Fov + ", " + n1.Fov );
		return Mathf.Lerp( n0.Fov, n1.Fov, normalizedParam );
	}

	public override void Update()
	{
		if( !Running )
			return;

		if( currentPath != null || paths.Length == 0 )
			return;

		currentPath = paths[ currentIdx ];
		if( currentPath == null )
			return;

		activePath = currentPath.name;

		const string id = "worldFeatureCameraPathTween";
		Misc.animate( gameObject, id, 0.0f, PanDuration, PanDuration, true,
			(Action<object>)( ( x ) =>
			{
				var t = (float)x;
				var unit_t = Mathf.Clamp01( t / PanDuration );

				GetComponent<Camera>().transform.position = currentPath.GetPositionOnSpline( unit_t );
				GetComponent<Camera>().transform.localRotation = currentPath.GetOrientationOnSpline( unit_t );

				if( t >= 0 || t <= PanDuration )
				{
					float v = 1.0f;
					if( t < PanFadeDuration )
					{
						if( performPathFadein == true )
						{
							v = rampIn.Evaluate( t / PanFadeDuration );
							Cinematron.setFade( v );
						}
					}
					else if( t > (PanDuration-PanFadeDuration) )
					{
						if( transitionOut == true )
						{
							t = PanDuration - t;
							v = rampOut.Evaluate( 1.0f - ( t / PanFadeDuration ) );
							Cinematron.setFade( v );
						}
					}
				}
			}),
			(Action<object>)( (x) =>
			{
				currentPath = null;
				activePath = "N/A";
				currentIdx = (currentIdx+1) % paths.Length;

				//	The following is to prevent fading twice, when coming from the main(non-feature) loop.
				if( performPathFadein == false )
					performPathFadein = true;
			})
		);
	}
}
