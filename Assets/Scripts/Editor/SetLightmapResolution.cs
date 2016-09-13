using UnityEngine;
using UnityEditor;
using System.Collections;

public class SetLightmapResolution : MonoBehaviour {


	[MenuItem ("Custom/SetLightmapResolution 2048")]
	static void SetLightmapResolution2048 () 
	{
		SetLightmapAtlasResolution (2048, 2048);
	}

	[MenuItem ("Custom/SetLightmapResolution 4096x2048")]
	static void SetLightmapResolution4096x2048 () 
	{
		SetLightmapAtlasResolution (4096, 2048);
	}

	[MenuItem ("Custom/SetLightmapResolution 4096")]
	static void SetLightmapResolution4096 () 
	{
		SetLightmapAtlasResolution (4096, 4096);
	}

	[MenuItem ("Custom/SetLightmapResolution 1024")]
	static void SetLightmapResolution1024 () 
	{
		SetLightmapAtlasResolution (1024, 1024);
	}
	
	static void SetLightmapAtlasResolution (int w, int h) 
	{
		LightmapEditorSettings.maxAtlasWidth = w;
		LightmapEditorSettings.maxAtlasHeight = h;
	}
}