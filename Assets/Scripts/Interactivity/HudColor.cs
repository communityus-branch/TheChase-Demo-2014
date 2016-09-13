using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class HudColor : HudThing
{
	public event System.Action<Color> OnColorChanged;
	private Color cachedColor = new Color( 0, 0, 0, 0 );

	public void setValue( Color c )
	{
		if( c != cachedColor )
		{
			cachedColor = c;
			if( OnColorChanged != null )
				OnColorChanged( cachedColor );
		}
	}

	void Start()
	{
		OnHit += ( o ) =>
		{
			RaycastHit hit = (RaycastHit)o;
			Texture2D tex = hit.collider.GetComponent<Renderer>().material.mainTexture as Texture2D;
			setValue( tex.GetPixelBilinear( hit.textureCoord.x, hit.textureCoord.y ) );
		};
	}
}