using UnityEngine;
using System.Collections;

public class BikeFeatureDemonstration : FeatureDemonstration
{
	public float ducTimeScale = 0.25f;

	public override void Show( float duration, bool performTransition = true )
	{
		Cinematron.ducTime( ducTimeScale, duration, iTween.EaseType.easeOutCubic, true );
		base.Show( duration, performTransition );
	}

	public override void Hide( float duration, bool performTransition = true )
	{
		Cinematron.ducTime( 1.0f, duration, iTween.EaseType.easeOutCubic, true );
		base.Hide( duration, performTransition );
	}

	public override void Start()
	{
		base.Start();
	}
}
