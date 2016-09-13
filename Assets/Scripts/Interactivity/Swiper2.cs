using UnityEngine;
using System.Collections;

public class TouchInfo
{
	public Vector2 touchPosition;
	public bool swipeComplete;
	public float timeSwipeStarted;
}

public class Swiper2 : MonoBehaviour
{
	public event System.Action OnSwipeLeft;
	public event System.Action OnSwipeRight;
	public event System.Action<Vector2> OnSwipeDrag;
	
	public int swipeLength = 25;
	public int swipeVariance = 10;
	public float timeToSwipe = 0;
	
	private TouchInfo[] touchInfoArray;
	private int activeTouch = -1;
	
	void Start()
	{
		touchInfoArray = new TouchInfo[5];
	}   
	
	void Update()
	{
		if( Input.touchCount > 1 && Input.touchCount < 6 )
		{
			foreach( Touch touch in Input.touches )
			{
				TouchInfo ti = touchInfoArray[ touch.fingerId ];
				if( ti == null)
				{
					ti = new TouchInfo();
					touchInfoArray[ touch.fingerId ] = ti;
				}

				if( touch.phase == TouchPhase.Began )
				{
					ti.touchPosition = touch.position;
					ti.timeSwipeStarted = Time.realtimeSinceStartup;
				}

				if( touch.phase == TouchPhase.Moved && touch.fingerId == activeTouch )
				{
					Vector2 drag = touch.position - ti.touchPosition;
					//float dist = drag.magnitude;
					if( OnSwipeDrag != null )
						OnSwipeDrag( drag );
				}

				//	Check if withing swipe variance.
				if( touch.position.y > (ti.touchPosition.y + swipeVariance))
					ti.touchPosition = touch.position;
				if(touch.position.y < (ti.touchPosition.y - swipeVariance))
					ti.touchPosition = touch.position;

				if( timeToSwipe == 0.0f || (timeToSwipe > 0.0f && (Time.realtimeSinceStartup - touchInfoArray[ touch.fingerId ].timeSwipeStarted) <= timeToSwipe) )
				{
					if( (touch.position.x < ti.touchPosition.x - swipeLength) && !ti.swipeComplete && activeTouch == -1 )
					{
						Reset( touch );
						if( OnSwipeLeft != null )
							OnSwipeLeft();
					}
					
					if( (touch.position.x > ti.touchPosition.x + swipeLength) && !ti.swipeComplete && activeTouch == -1 )
					{
						Reset( touch );
						if( OnSwipeRight != null )
							OnSwipeRight();
					}
				}
				
				//	When the touch has ended we can start accepting swipes again.
				if( touch.fingerId == activeTouch && touch.phase == TouchPhase.Ended )
				{
					//Debug.Log("Ending " + touch.fingerId);
					//if more than one finger has swiped then reset the other fingers so
					//you do not get a double/triple etc. swipe
					foreach( Touch touchReset in Input.touches )
						ti.touchPosition = touchReset.position;
					
					ti.swipeComplete = false;
					activeTouch = -1;
				}
			}           
		}   
	}

	void Reset( Touch touch )
	{
		activeTouch = touch.fingerId;
		touchInfoArray[ touch.fingerId ].swipeComplete = true;
	}
}