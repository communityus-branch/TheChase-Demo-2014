using UnityEngine;
using System.Collections;

public class ShotSequence : MonoBehaviour
{
	public Camera targetCamera;
	private Shot[] shots;

	private int shotIndex = 0;
	private bool running = false;

	void Start()
	{
		shots = gameObject.GetComponentsInChildren<Shot>();
		Debug.Log( string.Format( "'{0}' has {1} shots", name, shots.Length ) );
		Play();
	}

	public void Play()
	{
		running = true;
		shotIndex = 0;
		StartCoroutine( sequence() );
	}

	public void Stop()
	{
		running = false;
	}

	private Shot nextShot()
	{
		var path = shots[ shotIndex ];
		shotIndex = ( shotIndex + 1 ) % shots.Length;
		return path;
	}

	private IEnumerator sequence()
	{
		while( running == true && shots.Length > 0 )
		{
			Shot shot = nextShot();
			shot.play( targetCamera );

			float pauseEndTime = Time.realtimeSinceStartup + shot.duration;
			while( Time.realtimeSinceStartup < pauseEndTime || running == false )
				yield return 0;
		}

		yield return null;
	}
}
