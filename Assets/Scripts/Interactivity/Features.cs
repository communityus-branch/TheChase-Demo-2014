using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;

[ExecuteInEditMode]
public class Features : MonoBehaviour
{
	public AnimationCurve visibility = new AnimationCurve( new Keyframe[ 4 ] { new Keyframe( 0.0f, 0.0f ), new Keyframe( 0.1f, 1.0f ), new Keyframe( 0.9f, 1.0f ), new Keyframe( 1.0f, 0.0f ) } );
	public bool animateOnlyFlagged = true;

	public FeatureSlot[] items = new FeatureSlot[] {
		new FeatureSlot ("Unity Presents:", 5.2f, 7.5f, true, false),
		new FeatureSlot ("The Chase, a mobile demo", 7.5f, 12.2f),

		new FeatureSlot ("Grand vistas", 12.2f, 15f),
		new FeatureSlot ("Amazing character detail", 15f, 17.5f),
		new FeatureSlot ("40,000 vertices per character", 17.5f, 20f),
		new FeatureSlot ("Anamorphic lens flare", 20f, 24.3f),
		new FeatureSlot ("250,000 polygons per frame", 24.3f, 27.7f),
		new FeatureSlot ("Hair simulation", 27.7f, 32f),
		new FeatureSlot ("Linear space lighting approximation", 32f, 37f),
		new FeatureSlot ("Metallic car paint shader", 37f, 40f),
		new FeatureSlot ("Physically-based shading model", 40f, 44f),
		new FeatureSlot ("Screen space diffusion for skin", 44f, 49f),
		new FeatureSlot ("Atmospheric scattering", 49f, 52f),
		new FeatureSlot ("Subsurface scattering for skin", 52f, 59f),

		new FeatureSlot ("Powered by off-the-shelf Unity 4.2", 59f, 61f, false, true)
	};

	[System.Serializable]
	public class FeatureSlot
	{
		public float from;
		public float to;

		public string text;

		public bool animateIn = false;
		public bool animateOut = false;

		public FeatureSlot (string txt, float f, float t)
		{
			text = txt;
			from = f;
			to = t;
		}
		public FeatureSlot (string txt, float f, float t, bool ain, bool aout)
		{
			text = txt;
			from = f;
			to = t;
			animateIn = ain;
			animateOut = aout;
		}
	}

	private FeatureSlot findSlot( float t )
	{
		if( items == null || items.Length <= 0 )
			return null;

		FeatureSlot activeSlot = null;
		foreach( var se in items )
		{
			if( t >= se.from && t <= se.to && se.text != "")
				activeSlot = se;
		}

		return activeSlot;
	}

	public SpikeSlides spikeSlides;

	public float tmp = 0;

	void Start()
	{
		BulletTime.OnRestart += () =>
		{
			if( spikeSlides != null )
				spikeSlides.visibility = 0;
		};

		if( spikeSlides != null )
			spikeSlides.visibility = 0;
	}

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
		var t = BulletTime.playbackTime;
		var slot = findSlot( t );
		if( slot != null )
		{
			if( spikeSlides != null )
			{
				var x = Mathf.Clamp01( ( t - slot.from ) / ( slot.to - slot.from ) );				
				var f = visibility.Evaluate( x );
				if (animateOnlyFlagged)
					if ((!slot.animateIn && x < 0.5f) || (!slot.animateOut && x >= 0.5f))
						f = 1;
				if( Cinematron.featureMode == true )
					f = 0;

				spikeSlides.visibility = f;
				spikeSlides.text = slot.text;
			}
		}
	}
}
