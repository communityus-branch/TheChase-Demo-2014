using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

[ CustomEditor( typeof( ColliderButton ) ) ]
public class ColliderButtonEditor : Editor {
		
	//-----------------------------------------------------------
	// Private Variables
	//-----------------------------------------------------------
	
	ColliderButton _target;
	
	//-----------------------------------------------------------
	// Init
	//-----------------------------------------------------------
	
	void Awake(){
		_target = (ColliderButton)target;
	}
	
	//-----------------------------------------------------------
	// Inspector UI
	//-----------------------------------------------------------
	
	public override void OnInspectorGUI(){
		
		//draw toggle section for handling setting of rendered camera:
		_target.findRenderingCamera = EditorGUILayout.Toggle( "Detect Rendering Camera?", _target.findRenderingCamera );
		if ( !_target.findRenderingCamera ) {
			EditorGUI.indentLevel = 2;
			_target.renderingCamera = (Camera)EditorGUILayout.ObjectField( "Rendering Camera", _target.renderingCamera, typeof( Camera ), true );
			EditorGUI.indentLevel = 0;
		}
		
		//draw booleans:
		_target.useEvent = EditorGUILayout.Toggle( "Use Event?", _target.useEvent );
		_target.debug = EditorGUILayout.Toggle( "Debug Messages?", _target.debug );
		_target.drawConnections = EditorGUILayout.Toggle( "Draw Connections?", _target.drawConnections );
	}
}
