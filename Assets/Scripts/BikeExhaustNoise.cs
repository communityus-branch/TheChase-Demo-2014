using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class BikeExhaustNoise : MonoBehaviour
{
	public float mid = 0.5f;
	public float range = 0.5f;
	public float frequency = 0.5f;
	
	public string _rnd = "";
	
	private MaterialPropertyBlock propBlock = null;
	
	void Start()
	{
		propBlock = new MaterialPropertyBlock();
	}

	void Update()
	{
		//float t = BulletTime.time;
		float t = Time.time;
		float rnd = mid + BulletTime.Noise1D( t * frequency ) * range;
		_rnd = rnd.ToString();
		if( propBlock != null )
		{
			propBlock.Clear();
			propBlock.AddFloat( "_Intensity", rnd );
			GetComponent<Renderer>().SetPropertyBlock( propBlock );
		}
	}
}
