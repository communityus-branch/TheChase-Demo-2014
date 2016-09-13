using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PropSet))]
public class InteractiveValue : MonoBehaviour
{
	[HideInInspector]
	public PropSet propSet;

	public string	_description = "?";
	public float	_min = 0.0f;
	public float	_max = 1.0f;
	
	void Awake()
	{
		propSet = GetComponent<PropSet>();
	}
}
