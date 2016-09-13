using UnityEngine;
using System.Collections;

[AddComponentMenu("Interactivity-Controls/Input Orbit(pinch2zoom)")]
public class InputOrbit2 : MonoBehaviour
{
    public Transform target;
	
	public string debug1 = "";
	public string debug2 = "";

    public float distance = 50.0f;
    public float distanceMin = 0.1f;
    public float distanceMax = 5f;

	public float xSpeed = 100.0f;
    public float ySpeed = 100.0f;
	public float pinchSpeed = 1.0f;

    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

	private float curDist = 0;
	private float lastDist = 0;
 
    private float x = 0.0f;
    private float y = 0.0f;
	
    private static float ClampAngle(float angle, float min, float max)
    {
        if( angle < -360f )	angle += 360f;
        if( angle >  360f )	angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }

	void Start()
	{
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
 
        // Make the rigid body not change rotation
        if( GetComponent<Rigidbody>() )
            GetComponent<Rigidbody>().freezeRotation = true;
	}
 
    void LateUpdate()
	{
		if( !target )
			return;

		if( Input.multiTouchEnabled )
		{
			//	One finger touch does orbit.
			if( Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved )
	        {
	            var touch = Input.GetTouch(0);
	            x += touch.deltaPosition.x * xSpeed * 0.02f;
	            y -= touch.deltaPosition.y * ySpeed * 0.02f;
	        }
	
			//	Two finger touch does pinch to zoom.
	        if( Input.touchCount > 1 && (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(1).phase == TouchPhase.Moved) )
	        {
				var touch1 = Input.GetTouch(0);
				var touch2 = Input.GetTouch(1);
				curDist = Vector2.Distance(touch1.position, touch2.position);
				if(curDist > lastDist)
					distance += Vector2.Distance( touch1.deltaPosition, touch2.deltaPosition ) * -pinchSpeed;
				else
					distance -= Vector2.Distance( touch1.deltaPosition, touch2.deltaPosition ) * -pinchSpeed;
				
				lastDist = curDist;
	        }
		}
		else
		{
			x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
			y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
			distance = Mathf.Clamp(distance - Input.GetAxis( "Mouse ScrollWheel" ) * pinchSpeed, distanceMin, distanceMax );
		}
		
		y = ClampAngle( y, yMinLimit, yMaxLimit );
		distance = Mathf.Clamp( distance, distanceMin, distanceMax );
		
        Quaternion rot = Quaternion.Euler( y, x, 0 );
        Vector3 pos = rot * new Vector3(0.0f, 0.0f, -distance ) + target.position;
       
        transform.rotation = rot;
        transform.position = pos;		

		debug1 = rot.ToString();
		debug2 = pos.ToString();
	}
}