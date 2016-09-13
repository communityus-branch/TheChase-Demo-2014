using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent( typeof(Camera) )]
public class FeatureDemonstration : MonoBehaviour
{
	public float startTime = 15;
	public float endTime = 20;
	public string Description = "";
	public Texture2D featureIcon;
	public Hud hud = null;

	[HideInInspector]
	protected bool transitionIn = true;

	[HideInInspector]
	protected bool transitionOut = true;

	private BulletTimeCinematron m_Cinematron = null;
	protected BulletTimeCinematron Cinematron	{
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

	protected Vector3 posToParentSpace( Vector3 pos )
	{
		if( transform.parent )
			return transform.parent.InverseTransformPoint( pos );
		else
			return pos;
	}

	#region Hud values & colors.
	private InteractiveValue[] interactiveValues;
	private List<HudValue> instancedHudValues = new List<HudValue>();

	private InteractiveColor[] interactiveColors;
	private List<HudColor> instancedHudColors= new List<HudColor>();

	protected virtual void registerHud()
	{
		if( interactiveValues.Length <= 0 && interactiveColors.Length <= 0 )
			return;

		float sum = (float)(interactiveValues.Length + interactiveColors.Length);
		for( int i = 0; i < interactiveValues.Length; i++ )
		{
			InteractiveValue v = interactiveValues[ i ] as InteractiveValue;

			HudValue slider = hud.createSlider( (float)i / sum );
			if( slider != null )
			{
				var text = slider.GetComponentInChildren<TextMesh>();
				if( text )
					text.text = v._description;
				
				object obj = v.propSet.Get();
				if( obj != null )
					slider.setValue( (float)obj );

				slider.OnValueChanged += ( val ) =>	{	v.propSet.Set( Mathf.Lerp( v._min, v._max, val ) );	};
				instancedHudValues.Add( slider );
			}
		}

		float tmp = interactiveValues.Length;
		for( int i = 0; i < interactiveColors.Length; i++ )
		{
			InteractiveColor v = interactiveColors[ i ] as InteractiveColor;
			Debug.Log( string.Format( "Creating hsv thing for {0}", v.name ) );

			HudColor slider = hud.createColorPicker( ((float)i+tmp) / (float)sum );
			if( slider != null )
			{
				var text = slider.GetComponentInChildren<TextMesh>();
				if( text )
					text.text = v._description;

				object obj = v.fieldSet.Get();
				if( obj != null )
					slider.setValue( (Color)obj );

				slider.OnColorChanged += ( val ) =>	{	v.fieldSet.Set( val );	};
				instancedHudColors.Add( slider );
			}
		}
	}

	protected virtual void unregisterHud()
	{
		for( int i = 0; i < instancedHudValues.Count; i++ )
		{
			HudValue obj = instancedHudValues[ i ];
			obj.transform.parent = null;
			Debug.Log( string.Format( "Destroying hud value {0}", obj.name ) );
			Destroy( obj.gameObject );
		}

		instancedHudValues.Clear();

		for( int i = 0; i < instancedHudColors.Count; i++ )
		{
			HudColor obj = instancedHudColors[ i ];
			obj.transform.parent = null;
			Debug.Log( string.Format( "Destroying hud color {0}", obj.name ) );
			Destroy( obj.gameObject );
		}

		instancedHudColors.Clear();
	}

	protected void setHudVisibility( float f )
	{
		for( int i = 0; i < instancedHudValues.Count; i++ )
			instancedHudValues[ i ].setVisibility( f );

		for( int i = 0; i < instancedHudColors.Count; i++ )
			instancedHudColors[ i ].setVisibility( f );
	}

	#endregion

	public virtual void Start()
	{
		interactiveValues = gameObject.GetComponentsInChildren<InteractiveValue>();
		interactiveColors = gameObject.GetComponentsInChildren<InteractiveColor>();
		if( interactiveValues.Length > 0 || interactiveColors.Length > 0 )
			Debug.Log( string.Format( "{0} has {1} hud components  ({1} values, {2} colors)", this.name, interactiveValues.Length, interactiveColors.Length ) );
	}

	//private float m_featureVisibility = 0;
	protected virtual void setFeatureVisibility( float x )
	{
		//m_featureVisibility = x;
		setHudVisibility( x );
	}

	private void tweenVisibility( float a, float b, float duration )
	{
		Misc.animate( gameObject, "visibilityTween", a, b, duration, true, (System.Action<object>)( ( x ) => { setFeatureVisibility( Mathf.Clamp01( (float)x ) ); } ), null );
	}

	public virtual void Show( float duration, bool performTransition = true )
	{
		transitionIn = performTransition;
		tweenVisibility( 0.0f, 1.0f, duration );
	}

	public virtual void Hide( float duration, bool performTransition = true )
	{
		transitionOut = performTransition;
		tweenVisibility( 1.0f, 0.0f, duration );
	}

	private bool isRunning = false;
	public bool Running
	{
		get { return isRunning; }
		set
		{
			if( value == false )
			{
				if( GetComponent<Camera>() != null && GetComponent<Camera>().enabled == true )
					GetComponent<Camera>().enabled = false;

				isRunning = false;
				unregisterHud();
			}
			else
			{
				if( GetComponent<Camera>() != null && GetComponent<Camera>().enabled == false )
				{
					if( Camera.main )
						Camera.main.enabled = false;

					GetComponent<Camera>().enabled = true;	//	Enable feature camera.
				}

				registerHud();
				isRunning = true;
			}
		}
	}

	public virtual void Update()
	{
	}
}