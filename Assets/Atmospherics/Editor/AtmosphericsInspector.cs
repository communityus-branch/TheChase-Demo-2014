using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(Atmospherics))]
class AtmosphericsInspector : Editor 
{
	private bool changed = false;

	private static string kDirectoryName = "Assets/GeneratedTextures";
	private static string kExtensionName = "png";
	private static string kLookupTexturePropertyName = "_FogTexture";

	private static string[] kTextureSizes = { "16", "32", "64", "128", "256" };
	private static int[] kTextureSizesValues = { 16, 32, 64, 128, 256 };

	private static Texture2D PersistLookupTexture(string assetName, Texture2D tex)
	{
		if (!System.IO.Directory.Exists(kDirectoryName))
			System.IO.Directory.CreateDirectory(kDirectoryName);

		string assetPath = System.IO.Path.Combine(kDirectoryName, AssetHelper.CleanFileName(assetName) + "." + kExtensionName);
		bool newAsset = !System.IO.File.Exists(assetPath);

		System.IO.File.WriteAllBytes(assetPath, tex.EncodeToPNG());
		AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

		TextureImporter texSettings = AssetImporter.GetAtPath(assetPath) as TextureImporter;
		if (!texSettings)
		{
			// workaround for bug when importing first generated texture in the project
			AssetDatabase.Refresh();
			AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
			texSettings = AssetImporter.GetAtPath(assetPath) as TextureImporter;
		}
		texSettings.textureFormat = TextureImporterFormat.AutomaticTruecolor;
		texSettings.wrapMode = TextureWrapMode.Clamp;
		texSettings.mipmapEnabled = false; // !
		texSettings.linearTexture = true; // !

		if (newAsset)
			AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

		AssetDatabase.Refresh();

		Texture2D newTex = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture2D)) as Texture2D;
		return newTex;
	}

	public void OnEnable()
	{
		Atmospherics a = target as Atmospherics;
		if (!a) return;

		string path = AssetDatabase.GetAssetPath(a.lookupTexture);
		if (path == "")
			changed = true;
	}

	public void OnDisable()
	{
		// Access to AssetDatabase from OnDisable/OnDestroy results in a crash
		// otherwise would be nice to bake lookup texture when leaving asset
	}

	private void PersistLookupTexture()
	{
		var a = target as Atmospherics;
		if (!a) return;

		var sceneName = System.IO.Path.GetFileNameWithoutExtension(EditorApplication.currentScene);
		string assetName = sceneName + "_" + (a.gameObject.name) + kLookupTexturePropertyName;
		//Texture2D persistentTexture = 
		PersistLookupTexture(assetName, a.lookupTexture);
	}

	public override void OnInspectorGUI () 
	{
		var a = target as Atmospherics;

		EditorGUIUtility.LookLikeInspector ();
		DrawDefaultInspector();

		EditorGUIUtility.LookLikeControls();
		GUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Lookup Texture", "MiniPopup");
		a.lookupTextureWidth = EditorGUILayout.IntPopup(a.lookupTextureWidth, kTextureSizes, kTextureSizesValues, GUILayout.MinWidth(40));
		GUILayout.Label("x");
		a.lookupTextureHeight = EditorGUILayout.IntPopup(a.lookupTextureHeight, kTextureSizes, kTextureSizesValues, GUILayout.MinWidth(40));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		if (GUI.changed)
		{
			Undo.RegisterUndo (a, "Atmospherics Params Change");
			changed = true;
		}

		if (changed || !a.lookupTexture)
		{
			a.Bake();
			PersistLookupTexture();
			changed = false;
		}
	
		// persist lookup-texture on Undo
		if (Event.current.type == EventType.ValidateCommand)
		{
		    switch (Event.current.commandName)
		    {
		        case "UndoRedoPerformed":
					a.Bake();
					PersistLookupTexture();
		            break;
		    }
		}
	}
}