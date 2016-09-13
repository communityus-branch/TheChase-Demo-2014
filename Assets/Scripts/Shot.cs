using UnityEngine;
using System.Collections;

public class Shot : MonoBehaviour
{
	[HideInInspector]
	public iTweenPath cameraPosition = null;
	[HideInInspector]
	public iTweenPath cameraTarget = null;

	public AnimationCurve visibility = new AnimationCurve( new Keyframe[ 4 ] { new Keyframe( 0.0f, 0.0f ), new Keyframe( 0.2f, 1.0f ), new Keyframe( 0.8f, 1.0f ), new Keyframe( 1.0f, 0.0f ) } );
	public AnimationCurve fov = new AnimationCurve( new Keyframe[ 2 ] { new Keyframe( 0.0f, 60.0f ), new Keyframe( 1.0f, 60.0f ) } );
	public AnimationCurve roll = new AnimationCurve( new Keyframe[ 2 ] { new Keyframe( 0.0f, 0.0f ), new Keyframe( 1.0f, 0.0f ) } );

	public iTween.EaseType easing = iTween.EaseType.linear;
	private iTween.LoopType looping = iTween.LoopType.none;

	public float duration = 5;
	private string scratchName = "";

	static int count = 0;

	//	Stupid way of doing things!
	private void findPaths()
	{
		var paths = gameObject.GetComponentsInChildren<iTweenPath>();
		foreach( var item in paths )
		{
			var txt = item.pathName.ToLower();
			if( txt.EndsWith( "camera" ) )	cameraPosition = item;
			if( txt.EndsWith( "target" ) )	cameraTarget = item;
		}

		if( cameraPosition == null )
			Debug.LogError( "no cameraPosition" );

		if( cameraTarget == null )
			Debug.LogError( "no cameraTarget" );
	}

	void Start()
	{
	//	scratchName = string.IsNullOrEmpty( name ) ? System.Guid.NewGuid().ToString() : name;
	//	scratchName = string.Format( "{0}-{1}", scratchName, System.Guid.NewGuid().ToString() );
		scratchName = name + " " + count++;
		findPaths();
	}

	public void play( Camera cam )
	{
		var camPath = iTweenPath.rawPath( cameraPosition, false );
		var targetPath = iTweenPath.rawPath( cameraTarget, false );

		string id = scratchName + "_auxTween";
		Debug.Log( "Shot: " + id + ", duration: " + duration );

		iTween.StopByName( gameObject, id );
		iTween.ValueTo( gameObject, iTween.Hash( "from", 0.0f, "to", 1.0f, "time", duration, "easetype", easing, "looptype", looping, "ignoretimescale", true,
			"onUpdate", (System.Action<object>)( ( x ) =>
			{
				var f = (float)x;
				cam.fieldOfView = fov.Evaluate( f );

				var p = transform.parent.TransformPoint( iTween.PointOnPath( camPath, f ) );
				var t = transform.parent.TransformPoint( iTween.PointOnPath( targetPath, f ) );
				var dir = t - p;

				cam.transform.position = p;
				cam.transform.rotation = Quaternion.LookRotation( dir.normalized, Vector3.up );
				cam.transform.Rotate( new Vector3( 0, 0, roll.Evaluate( f ) ), Space.Self );
			} ) ) );
	}
}