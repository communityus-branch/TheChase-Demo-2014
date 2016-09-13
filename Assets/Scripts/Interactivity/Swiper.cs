using UnityEngine;
using System.Collections;

public class Swiper : MonoBehaviour
{
	public event System.Action<Vector2> OnSwipe;

	private float	touch_t = 0.0f;
	private Vector2 touch_p = Vector2.zero;

	public float minimumSwipeDistance = 8.0f;
	public float maximumSwipeTime = 0.1f;
	public float comfortZone = 70.0f;

	public bool isSwipe = false;
	
	public string debug_time = "";
	public string debug_dist = "";
	
	public Hud	hud;

	void Update()
	{
		if( Input.touchCount > 1 && OnSwipe != null )
		{
			var touch = Input.touches[0];
			
			switch( touch.phase )
			{
				case TouchPhase.Began:
					isSwipe = true;
					touch_t = Time.realtimeSinceStartup;
					touch_p = touch.position;
					break;

				case TouchPhase.Moved:
					float d = (touch.position - touch_p).magnitude;
					if( d > comfortZone )
					{
						isSwipe = false;
						Debug.Log( "Touch moved out of comfortzone" );
					}
					break;

				case TouchPhase.Canceled:
					isSwipe = false;
					Debug.Log( "Touch canceled" );
					break;

				/*case TouchPhase.Stationary:
					isSwipe = false;
					Debug.Log( "Touch stationary" );
					break;*/

				case TouchPhase.Ended:
					if( !isSwipe )
					{
						Debug.Log( "Touch ended, but no swipe" );
						break;
					}

					float dur = Time.realtimeSinceStartup - touch_t;
					if( dur < maximumSwipeTime )
					{
						Debug.Log( string.Format( "Touch duration({0}) less than swipetime({1})", dur, maximumSwipeTime ) );
						break;
					}

					Vector2 dir = touch.position - touch_p;
					float mag = dir.magnitude;
					if( mag < minimumSwipeDistance )
						Debug.Log( string.Format( "Touch mag({0}) less than mininum({1})", mag, minimumSwipeDistance ) );
					else
						OnSwipe( dir );
						
					break;
			}
		}
		
		/*if( Input.touchCount != 2 )
		{
			isSwiping = false;
			return;
		}

		foreach( Touch touch in Input.touches )
		{
			switch( touch.phase )
			{
				case TouchPhase.Began:
					isSwiping = true;
					touch_t = Time.timeSinceLevelLoad;
					touch_p = touch.position;
					break;

				case TouchPhase.Canceled:
					isSwiping = false;
					break;

				case TouchPhase.Ended:
					if( !isSwiping )
						continue;

					float dur = Mathf.Abs( Time.time - touch_t );
					if( dur < maximumSwipeTime )
						continue;

					Vector2 dir = touch.position - touch_p;
					float dist = Mathf.Abs( dir.magnitude );
					if( dist > minimumSwipeDistance )
					{
						if( OnSwipe != null )
						{
							var tmp = Vector2.zero;
	
	                        if( Mathf.Abs(dir.x) > Mathf.Abs( dir.y ) )
	                            tmp = Vector2.right * Mathf.Sign( dir.x );
	                        else
	                            tmp = Vector2.up * Mathf.Sign( dir.y );
						
							OnSwipe( tmp );
						}
					}
					break;
			}
		}*/
	}
}
