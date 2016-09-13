using UnityEngine;
using System.Collections;

[AddComponentMenu("Interactivity-Controls/Input Orbit")]
public class InputOrbit : MonoBehaviour
{
    public Transform target;

    public float distance = 50.0f;
    public float xSpeed = 120.0f;
    public float ySpeed = 120.0f;
	public float ScrollSpeed = 20.0f;
 
    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;
 
    public float distanceMin = 5f;
    public float distanceMax = 100f;
 
    float x = 0.0f;
    float y = 0.0f;

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
		if( target )
		{
			x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
			y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
			y = ClampAngle(y, yMinLimit, yMaxLimit);

			Quaternion rotation = Quaternion.Euler(y, x, 0);
			distance = Mathf.Clamp(distance - Input.GetAxis( "Mouse ScrollWheel" ) * ScrollSpeed, distanceMin, distanceMax );
			//Vector3 negDistance = new Vector3( 0.0f, 0.0f, -distance );
			//Vector3 position = rotation * negDistance + target.position;

			transform.localRotation = rotation;
			//transform.position = position;
		}
 
	}
}