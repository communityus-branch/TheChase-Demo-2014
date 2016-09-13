#pragma strict

var target : Transform;
var speed : float = 1.0f;
var zoomSpeed : float = 3.0f;

private var x : float = 0.0f;
private var y : float = 0.0f;
private var offsetR : float;
private var offsetY : float;
private var zoomDist : float = 0.0f;
private var zoomLevel : float = 0.0f;

function Start () 
{
    var angles : Vector3 = transform.eulerAngles;
    x = angles.y;
    y = angles.x;

	var offset : Vector3 = target.position - transform.position;
	offsetY = offset.y; offset.y = 0;
	offsetR = offset.magnitude;
}

function LateUpdate () {
	if(!target)
		return;

	// zoom
	if(Input.touchCount == 2)
	{
		var touch1Dir : Vector3 = Input.touches[0].position;
		var touch2Dir : Vector3 = Input.touches[1].position;

		if(Mathf.Abs(zoomDist)<0.0025f)
		{
			zoomDist = zoomLevel + (touch1Dir-touch2Dir).magnitude;
		}
		zoomLevel = zoomDist - (touch1Dir-touch2Dir).magnitude;
	}
	else
	{	
		zoomDist = 0.0f;

		if(Input.touchCount == 1)
		{
			x += Input.touches[0].deltaPosition.normalized.x * speed;
			y -= Input.touches[0].deltaPosition.normalized.y * speed;
		}
	}
 		       
	var rotation = Quaternion.Euler(y, x, 0);
	var position = Vector3(0,-offsetY,0) + rotation * Vector3(0,0,-offsetR + zoomLevel*zoomSpeed) + target.position;
        
    transform.rotation = rotation;
    transform.position = position;
}

static function ClampAngle (angle : float, min : float, max : float) {
	if (angle < -360.0f)
		angle += 360.0f;
	if (angle > 360.0f)
		angle -= 360.0f;
	return Mathf.Clamp (angle, min, max);
}