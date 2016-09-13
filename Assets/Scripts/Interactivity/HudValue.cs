using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class HudValue : HudThing
{
	public event System.Action<float> OnValueChanged;
	private float cachedValue = -1.0f;

	public void setValue( float f )
	{
		if( f != cachedValue )
		{
			cachedValue = f;
			if( GetComponent<Renderer>() != null )
				GetComponent<Renderer>().material.SetFloat( "_Value", f );

			if( OnValueChanged != null )
				OnValueChanged( cachedValue );
		}
	}

	void Start()
	{
		OnHit += ( o ) =>
		{
			RaycastHit hit = (RaycastHit)o;
			setValue( Mathf.Clamp01( hit.textureCoord.x ) );
		};
	}
}