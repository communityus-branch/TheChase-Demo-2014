using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ShinyShadingLookupTexture))]
internal class ShinyShadingLookupTextureInspector : Editor
{
	private bool baked = true;
	private bool lowResPreview = false;
	private bool previewRGB = true;
	
	private static string kDirectoryName = "Assets/GeneratedTextures";
	private static string kExtensionName = "png";
	private static string kLookupTexturePropertyName = "_ShinyShadingTex";
	
	private static int kTexturePreviewBorder = 8;
	private static string[] kTextureSizes = { "16", "32", "64", "128", "256", "512" };
	private static int[] kTextureSizesValues = { 16, 32, 64, 128, 256, 512 };
	

	private static Texture2D PersistLookupTexture (string assetName, Texture2D tex)
	{
		if (!System.IO.Directory.Exists (kDirectoryName))
			System.IO.Directory.CreateDirectory (kDirectoryName);	

		string assetPath = System.IO.Path.Combine (kDirectoryName, assetName + "." + kExtensionName);
		bool newAsset = !System.IO.File.Exists (assetPath);
		
		System.IO.File.WriteAllBytes (assetPath, tex.EncodeToPNG());
		AssetDatabase.ImportAsset (assetPath, ImportAssetOptions.ForceUpdate);

		TextureImporter texSettings = AssetImporter.GetAtPath (assetPath) as TextureImporter;
		if (!texSettings)
		{
			// workaround for bug when importing first generated texture in the project
			AssetDatabase.Refresh ();
			AssetDatabase.ImportAsset (assetPath, ImportAssetOptions.ForceUpdate);
			texSettings = AssetImporter.GetAtPath (assetPath) as TextureImporter;
		}
		texSettings.textureFormat = TextureImporterFormat.AutomaticTruecolor;
		texSettings.wrapMode = TextureWrapMode.Clamp;
		texSettings.mipmapEnabled = false; // !
		texSettings.linearTexture = true; // !

		if (newAsset)
			AssetDatabase.ImportAsset (assetPath, ImportAssetOptions.ForceUpdate);
		
		AssetDatabase.Refresh ();
		
		Texture2D newTex = AssetDatabase.LoadAssetAtPath (assetPath, typeof(Texture2D)) as Texture2D;		
		return newTex;
	}
	
	private void PersistLookupTexture ()
	{
		ShinyShadingLookupTexture l = target as ShinyShadingLookupTexture;
		if (!l) return;
				
		string assetName = (l.gameObject.name) + kLookupTexturePropertyName;
		
		// Dr Paranoia, featuring his sidekick, Punk Patch.
		assetName = assetName.Replace( ":", "_" );
		
		Texture2D persistentTexture = PersistLookupTexture (assetName, l.lookupTexture);
		
		foreach (Material m in l.GetComponent<Renderer>().sharedMaterials)
		{
			if(HasLookupTextureSlot(m))
			{
				m.SetTexture (kLookupTexturePropertyName, persistentTexture);
				l.SetParameters (m);
			}
		}
	}
	
	static bool HasLookupTextureSlot (Material m)
	{
		return (m && m.HasProperty (kLookupTexturePropertyName));
	}

	public void OnEnable ()
	{
		SkinShadingLookupTexture l = target as SkinShadingLookupTexture;
		if (!l) return;
		
		string path = AssetDatabase.GetAssetPath (l.lookupTexture);
		if (path == "")
			baked = false;
	}
	
	public void OnDisable ()
	{
		// Access to AssetDatabase from OnDisable/OnDestroy results in a crash
		// otherwise would be nice to bake lookup texture when leaving asset
	}

	public override void OnInspectorGUI ()
	{
		ShinyShadingLookupTexture l = target as ShinyShadingLookupTexture;

		EditorGUI.indentLevel++;

		//l.diffuseIntensity = EditorGUILayout.Slider ("Diffuse", l.diffuseIntensity, 0f, 2f);
		{
			EditorGUILayout.Space ();
			GUILayout.Label ("FRESNEL", EditorStyles.miniBoldLabel);
			l.reflectivityAt0 = EditorGUILayout.Slider (" Reflectivity (0 degrees)", l.reflectivityAt0, 0f, 1f);
			l.reflectivityAt90 = EditorGUILayout.Slider (" Reflectivity (90 degrees)", l.reflectivityAt90, 0f, 1f);
			l.reflectivityFalloff = EditorGUILayout.Slider (" Falloff", l.reflectivityFalloff, 0f, 20.0f);
		}
           
		EditorGUI.indentLevel--;

		EditorGUILayout.Space ();

		GUILayout.Label ("SPECULAR", EditorStyles.miniBoldLabel);
		EditorGUI.indentLevel++;

		l.specularIntensity = EditorGUILayout.Slider ("Specular 1", l.specularIntensity, 0f, 10f);
		if (l.specularIntensity > 1e-6)
		{
			EditorGUI.indentLevel++;
			l.specularShininess = EditorGUILayout.Slider (" Roughness", l.specularShininess, 0f, 1f);
			EditorGUI.indentLevel--;
		}
		l.specularIntensity2 = EditorGUILayout.Slider ("Specular 2", l.specularIntensity2, 0f, 10f);
		if (l.specularIntensity2 > 1e-6)
		{
			EditorGUI.indentLevel++;
			l.specularShininess2 = EditorGUILayout.Slider (" Roughness", l.specularShininess2, 0f, 1f);
			EditorGUI.indentLevel--;
		}	
		EditorGUI.indentLevel--;

		EditorGUILayout.Space ();

			GUILayout.Label ("MISC", EditorStyles.miniBoldLabel);

		GUILayout.BeginHorizontal ();
		EditorGUILayout.PrefixLabel ("Lookup Texture", "MiniPopup");
		l.lookupTextureWidth = EditorGUILayout.IntPopup (l.lookupTextureWidth, kTextureSizes, kTextureSizesValues, GUILayout.MinWidth(40));
		GUILayout.Label ("x");
		l.lookupTextureHeight = EditorGUILayout.IntPopup (l.lookupTextureHeight, kTextureSizes, kTextureSizesValues, GUILayout.MinWidth(40));
		GUILayout.FlexibleSpace ();
		GUILayout.EndHorizontal ();

		if (GUI.changed)
		{
			EditorUtility.SetDirty (l);
			Undo.RegisterUndo (l, "BRDFTexture Params Change");
			baked = false;
		}
				
		// preview
		GUILayout.BeginHorizontal();
		lowResPreview = EditorGUILayout.Toggle ("Low Res Preview", lowResPreview);
		GUILayout.FlexibleSpace();
		if (GUILayout.Button (previewRGB? "RGB": "Alpha", "MiniButton", GUILayout.MinWidth(38)))
			previewRGB = !previewRGB;
		GUILayout.EndHorizontal();
		
		if (lowResPreview && !baked)
		{
			GUILayout.BeginHorizontal ();
			GUILayout.FlexibleSpace ();
			if (GUILayout.Button ("Bake", GUILayout.MinWidth (64)))
			{
				l.Bake ();
				baked = true;
			}
			GUILayout.EndHorizontal ();
		}
		
		if (GUI.changed || !l.lookupTexture)
		{
			if (lowResPreview)
			{
				if (!baked)
					l.Preview ();
			}
			else
				l.Bake ();
			
			PersistLookupTexture ();
		}
		
		Rect r = GUILayoutUtility.GetAspectRect (1.0f);
		r.x += kTexturePreviewBorder;
		r.y += kTexturePreviewBorder;
		r.width -= kTexturePreviewBorder * 2;
		r.height -= kTexturePreviewBorder * 2;
		if (previewRGB)
			EditorGUI.DrawPreviewTexture (r, l.lookupTexture);
		else
			EditorGUI.DrawTextureAlpha (r, l.lookupTexture);
			
			// persist lookup-texture on Undo
		if (Event.current.type == EventType.ValidateCommand)
		{
		    switch (Event.current.commandName)
		    {
		        case "UndoRedoPerformed":
					{
						l.Bake ();
						PersistLookupTexture ();
						baked = false;
					}
		            break;
		    }
		}
	}
	
}