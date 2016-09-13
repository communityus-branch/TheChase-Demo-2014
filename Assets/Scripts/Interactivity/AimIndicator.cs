using UnityEngine;
using System.Collections;

public class AimIndicator : MonoBehaviour
{
	public GameObject circles;
	public GameObject arrows;

	public Color arrowColor = new Color( 1, 1, 1, 1 );
	public Color circleColor = new Color( 1, 1, 1, 1 );

	public float duration = 0.5f;	
	
	#region Curves
	public AnimationCurve	ArrowScaleRamp = new AnimationCurve(
		new Keyframe[3]
		{
			new Keyframe(0.0f, 0.25f),
			new Keyframe(0.5f, 1.0f),
			new Keyframe(1.0f, 4.0f)
		} );

	public AnimationCurve	ArrowOpacityRamp = new AnimationCurve(
		new Keyframe[4]
		{
			new Keyframe(0.0f, 0.0f),
			new Keyframe(0.25f, 1.0f),
			new Keyframe(0.5f, 0.0f),
			new Keyframe(1.0f, 0.0f)
		} );

	public AnimationCurve	CircleScaleRamp = new AnimationCurve(
		new Keyframe[2]
		{
			new Keyframe(0.0f, 0.5f),
			new Keyframe(1.0f, 1.0f)
		} );

	public AnimationCurve	CircleOpacityRamp = new AnimationCurve(
		new Keyframe[3]
		{
			new Keyframe(0.0f, 0.0f),
			new Keyframe(0.5f, 0.5f),
			new Keyframe(1.0f, 0.1f)
		} );
	#endregion

	private float factor = 0.0f;

	void Start()
	{
		if (!circles || !arrows)
		{
			Debug.LogError("Circles or Arrows are missing");
			return;
		}

		circles.SetActive( false );
		arrows.SetActive( false );

		Interactivity tmp = Interactivity.instance;
		if (!tmp)
		{
			Debug.LogError("__Interactivity is missing from the scene");
			return;
		}
		
		if (!tmp.allowAiming)
			return;
			
		tmp.OnAim += (state) =>
		{
			const string id = "aimIndicatorTween";
			iTween.StopByName( gameObject, id );
			iTween.ValueTo( gameObject, iTween.Hash(
				"from", factor,
				"to", state==true?1.0f:0.0f,
				"time", duration * (state==true?1.0f:0.25f),
				"easetype", iTween.EaseType.easeOutCubic,
				"ignoretimescale", true,
				"onUpdate", (System.Action<object>)( ( x ) =>
				{
					factor = (float)x;
				} ) ) );
		};
	}

	void Update()
	{
		bool state = false;
		if( factor > 0.0f || factor > 1.0f )
			state = true;

		circles.SetActive( state );
		arrows.SetActive( state );
		
		var f = Mathf.Clamp01( factor );
		{ // circles
			circleColor.a = CircleOpacityRamp.Evaluate( f );
			circles.GetComponent<Renderer>().material.SetColor( "_Color", circleColor );
			var s = CircleScaleRamp.Evaluate( factor );
			circles.transform.localScale = new Vector3( s,s,1.0f);
		}

		{ // arrows
			arrowColor.a = ArrowOpacityRamp.Evaluate( f );
			arrows.GetComponent<Renderer>().material.SetColor( "_Color", arrowColor );
			var s = ArrowScaleRamp.Evaluate( factor );
			arrows.transform.localScale = new Vector3( s,s,1.0f);
		}
	}
}
