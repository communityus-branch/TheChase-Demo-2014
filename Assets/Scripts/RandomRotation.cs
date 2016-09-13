using UnityEngine;
using System.Collections;


public class RandomRotation : MonoBehaviour {

	public float maxAngle = 10f;
	public float frequency = 1f;
	public float pullToRest = 0.0f;
	public Vector3 aroundAxis = Vector3.up;
	private float _noiseSeed = 0.5317f;
	private Quaternion _initialLocalRotation;

	// Use this for initialization
	void Start () {
		_noiseSeed = (gameObject.GetInstanceID() % 1024) / 1024f;
		_initialLocalRotation = transform.localRotation;
	
	}
	
	// Update is called once per frame
	void Update () {
		var r = BulletTime.Noise1D(BulletTime.time * frequency + _noiseSeed)*2f-1f;
		var a = ((Mathf.Abs(r) > 0.25)?Mathf.Pow(r, 1f + pullToRest) * Mathf.Sign(r): r) * maxAngle;
		var rot = (Mathf.Abs(a) > Mathf.Epsilon)? Quaternion.AngleAxis(a, aroundAxis): Quaternion.identity;
		transform.localRotation = _initialLocalRotation * rot	;
	
	}
}
