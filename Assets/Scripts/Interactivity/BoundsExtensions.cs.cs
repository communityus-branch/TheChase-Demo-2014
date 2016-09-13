using UnityEngine;
using System.Collections;

public static class BoundsExtensions
{
	public static Rect ToScreenSpace( this Bounds bounds, Camera camera )
	{
		var origin = camera.WorldToScreenPoint( new Vector3( bounds.min.x, bounds.min.y, 0.0f ) );
		var extents = camera.WorldToScreenPoint( new Vector3( bounds.max.x, bounds.min.y, 0.0f ) );

		Debug.LogError( origin.ToString() + ", " + extents.ToString() );

		return new Rect( origin.x, Screen.height - origin.y, extents.x - origin.x, origin.y - extents.y );
	}
}