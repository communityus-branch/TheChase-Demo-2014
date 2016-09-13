using UnityEngine;
using System.Collections;
[ExecuteInEditMode()]
public class NewBehaviourScript : MonoBehaviour {
	public Vector3 pos;
	public Vector3 dir;
	public int lines = 10;
	
	// Update is called once per frame
	void Update () {
		Random.seed = 100;
		for (int i = 0; i < lines; i++)
		{
			Vector3 d = Random.onUnitSphere;
 			GetLength (d, pos);
		}
	}

	float GetLength (Vector3 d, Vector3 viewPos)
	{
			Vector3 vpc = -viewPos;

			Vector3 pc = Vector3.Project (vpc, d) + viewPos;
			float pcSqrMag = Vector3.Dot(pc,pc);
			float dist = Mathf.Sqrt (1 - pcSqrMag);
			float len = (viewPos - (pc + d * dist)).magnitude;
			Debug.DrawLine (viewPos, pc + d * dist, new Color (len,len,len,1));
			return len;
	}
}
