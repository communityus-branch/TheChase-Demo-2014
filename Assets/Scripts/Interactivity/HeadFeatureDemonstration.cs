using UnityEngine;
using System.Collections;

public class HeadFeatureDemonstration : FeatureDemonstration
{
	//	Wind.
	public WindTracker windTracker;
	public float Strength { get; set; }
	public float Responsiveness { get; set; }

	//	Hair.
	public GameObject hair;
	public Color HairColor = Color.white;
	public float Speed { get; set; }
	public float Scale { get; set; }

	//	Skin.
	public GameObject skin;
	public Color SkinColor = Color.white;

	public override void Start()
	{
		base.Start();

		if( windTracker != null )
		{
			Strength = windTracker.Strength;
			Responsiveness = windTracker.Responsiveness;
		}

		if( hair != null )
		{
			var mat = hair.GetComponent<Renderer>().sharedMaterial;
			if( mat != null )
			{
				HairColor = mat.GetColor( "_Color" );
				Speed = mat.GetFloat( "_AnimationSpeed" );
				Scale = mat.GetFloat( "_NoiseScale" );
			}
		}
	}

	public float ducTimeScale = 0.25f;
	public override void Show( float duration, bool performTransition = true )
	{
		Cinematron.ducTime( ducTimeScale, duration, iTween.EaseType.easeOutCubic, true );
		base.Show( duration, performTransition );

		if( windTracker != null )
			windTracker.ignoreTimescale = true;
	}

	public override void Hide( float duration, bool performTransition = true )
	{
		Cinematron.ducTime( 1.0f, duration, iTween.EaseType.easeOutCubic, true );
		base.Hide( duration, performTransition );

		if( windTracker != null )
			windTracker.ignoreTimescale = false;
	}

	public override void Update()
	{
		if( !Running )
			return;

		//	Wind.
		if( windTracker != null )
		{
			windTracker.Strength = Strength;
			windTracker.Responsiveness = Responsiveness;
		}

		//	Hair.
		if( hair != null )
		{
			var mat = hair.GetComponent<Renderer>().material;
			if( mat != null )
			{
				mat.SetColor( "_Color", HairColor );
				mat.SetFloat( "_AnimationSpeed", Speed );
				mat.SetFloat( "_NoiseScale", Scale );
			}
		}

		if( skin != null )
		{
		}
	}
}