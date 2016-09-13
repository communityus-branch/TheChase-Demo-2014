var scrollSpeedX:float = .5;
var scrollSpeedY:float = .5;
var offsetX:float;
var offsetY:float;
var texname:String = "_MainTex";
 
function Update()
{
    offsetX += (Time.deltaTime * scrollSpeedX) / 10.0;
    offsetY += (Time.deltaTime * scrollSpeedY) / 10.0;
    GetComponent.<Renderer>().material.SetTextureOffset (texname, Vector2( offsetX, offsetY ));
}