using UnityEngine;
using System.Collections;

[RequireComponent(typeof(FieldSet))]
public class InteractiveColor : MonoBehaviour
{
	[HideInInspector]
	public FieldSet fieldSet;

	public string	_description = "?";
	
	void Awake()
	{
		fieldSet = GetComponent<FieldSet>();
	}
}
