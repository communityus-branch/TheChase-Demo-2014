using UnityEngine;
using System.Collections;

public static class Misc
{
	static public void FitUnitPlaneInFrustum( Transform tx, Camera cam, float nearFarRatio, float offset )
	{
		float pos = Mathf.Lerp( cam.nearClipPlane, cam.farClipPlane, nearFarRatio );
		tx.position = cam.transform.position + cam.transform.forward * pos;
		float h = (Mathf.Tan( cam.fieldOfView * Mathf.Deg2Rad * 0.5f) * pos) * offset;
		tx.localScale = new Vector3( h * cam.aspect, h, 0 );
	}
	
	static public void animate( GameObject target, string id, float from, float to, float duration, bool ignoreTimeScale, System.Action<object> onUpdate, System.Action<object> onComplete )
	{
		if( iTween.tweens.Contains( id ) )
			iTween.StopByName( target, id );

		var ht = new Hashtable();

		ht.Add( "name", id );
		ht.Add( "from", from );
		ht.Add( "to", to );
		ht.Add( "time", duration );
		ht.Add( "ignoretimescale", ignoreTimeScale );
		
		if( onUpdate != null )
			ht.Add( "onUpdate", onUpdate );
			
		if( onComplete != null )
			ht.Add( "onComplete", onComplete );

		iTween.ValueTo( target, ht );
	}
}