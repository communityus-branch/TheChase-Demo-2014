using UnityEngine;
using System.Collections;
using System;

public partial class Interactivity
{
	public event System.Action<Vector2> OnInputDown;
	public event System.Action<Vector2> OnInputMove;
	public event System.Action<Vector2> OnInputUp;

	#region Event logic
	private void handleDown()
	{
		if( Input.mousePresent && Input.GetMouseButtonDown(0) )
		{
			if( OnInputDown != null )
			{
				OnInputDown( Input.mousePosition );
				return;
			}
		}

		if( Input.touchCount > 0 )
		{
			Touch it = Input.GetTouch(0);
			if( it.phase == TouchPhase.Began )
			{
				if( OnInputDown != null )
					OnInputDown( it.position );
			}
		}
	}

	private void handleMove()
	{
		if( Input.mousePresent && Input.GetMouseButton(0) )
		{
			if( OnInputMove != null )
			{
				OnInputMove( Input.mousePosition );
				return;
			}
		}

		if( Input.touchCount > 0 )
		{
			Touch it = Input.GetTouch(0);
			if( it.phase == TouchPhase.Moved )
			{
				if( OnInputMove != null )
					OnInputMove( it.position );
			}
		}
	}

	private void handleUp()
	{
		if( Input.mousePresent && Input.GetMouseButtonUp(0) )
		{
			if( OnInputUp != null )
			{
				OnInputUp( Input.mousePosition );
				return;
			}
		}

		if( Input.touchCount > 0 )
		{
			Touch it = Input.GetTouch(0);
			if( it.phase == TouchPhase.Ended || it.phase == TouchPhase.Canceled )
			{
				if( OnInputUp != null )
					OnInputUp( it.position );
			}
		}
	}
	#endregion

	private void setupInput()
	{
		Input.simulateMouseWithTouches = false;

		//OnInputDown += (p) => { Debug.Log( "Input DOWN: " + p.ToString() ); };
		//OnInputMove += (p) => { Debug.Log( "Input MOVE: " + p.ToString() ); };
		//OnInputUp += (p) => { Debug.Log( "Input UP: " + p.ToString() ); };
	}

	private void updateInput()
	{
		handleDown();
		handleMove();
		handleUp();
	}
}