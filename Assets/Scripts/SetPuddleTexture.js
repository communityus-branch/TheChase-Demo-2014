#pragma strict

var textures : Texture2D[];

function Start () {

}

private var _index : int = 0;
function Update () 
{
	_index++;
	GetComponent.<Renderer>().sharedMaterial.SetTexture("_Puddle", textures[(Time.frameCount/2)%textures.Length]);
}