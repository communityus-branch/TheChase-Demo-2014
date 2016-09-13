using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SkinShadingLookupTexture))]
internal class SkinShadingLookupTextureInspector : Editor
{
	private bool baked = true;
	private bool lowResPreview = false;
	private bool previewRGB = true;
	
	private static string kDirectoryName = "Assets/GeneratedTextures";
	private static string kExtensionName = "png";
	private static string kLookupTexturePropertyName = "_SkinLookupTex";
	
	private static int kTexturePreviewBorder = 8;
	private static string[] kTextureSizes = { "16", "32", "64", "128", "256" };
	private static int[] kTextureSizesValues = { 16, 32, 64, 128, 256 };

	private static Texture2D PersistLookupTexture (string assetName, Texture2D tex)
	{
		if (!System.IO.Directory.Exists (kDirectoryName))
			System.IO.Directory.CreateDirectory (kDirectoryName);

		string assetPath = System.IO.Path.Combine(kDirectoryName, AssetHelper.CleanFileName(assetName) + "." + kExtensionName);
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
		SkinShadingLookupTexture l = target as SkinShadingLookupTexture;
		if (!l) return;
				
		string assetName = (l.gameObject.name) + kLookupTexturePropertyName;

		//	Dr Paranoia, featuring his sidekick, Punk Patch.
		assetName = assetName.Replace( ":", "_" );
		
		Texture2D persistentTexture = PersistLookupTexture (assetName, l.lookupTexture);
		
		
		
		//TODO: fix this name.
		
		foreach (Material m in l.GetComponent<Renderer>().sharedMaterials)
		{
			if (HasLookupTextureSlot(m))
				m.SetTexture (kLookupTexturePropertyName, persistentTexture);
		}
	}
	
	static bool HasLookupTextureSlot (Material m)
	{
		return (m && m.HasProperty (kLookupTexturePropertyName));
	}
	
	static CameraSkinScattering cachedSkinScattering;
	static CameraSkinScattering CacheSkinScattering()
	{
		if (cachedSkinScattering == null && Camera.main)
			cachedSkinScattering = Camera.main.GetComponent(typeof(CameraSkinScattering)) as CameraSkinScattering;
		if (cachedSkinScattering == null)
			cachedSkinScattering = Object.FindObjectOfType(typeof(CameraSkinScattering)) as CameraSkinScattering;
		return cachedSkinScattering;
	}
	static bool HasSupportedMaterial(Renderer r)
	{
		foreach (var m in r.sharedMaterials)
			if (HasLookupTextureSlot(m))
				return true;
				
		if (!CacheSkinScattering())
			return true;

		foreach (var m in r.sharedMaterials)
			if (CacheSkinScattering().SupportedMaterial(m))
				return true;
				
		return false;
	}
	
	static void SetupMaterials(Renderer r)
	{
		if (!CacheSkinScattering())
			return;
		CacheSkinScattering().SetupPass (r, 2, null, true);
	}

	public void OnEnable ()
	{
		SkinShadingLookupTexture l = target as SkinShadingLookupTexture;
		if (!l) return;
		
		string path = AssetDatabase.GetAssetPath (l.lookupTexture);
		if (path == "")
			baked = false;
		l.parametersAreDirty = true;
	}
	
	public void OnDisable ()
	{
		// Access to AssetDatabase from OnDisable/OnDestroy results in a crash
		// otherwise would be nice to bake lookup texture when leaving asset
	}

	public override void OnInspectorGUI ()
	{
		SkinShadingLookupTexture l = target as SkinShadingLookupTexture;

		GUILayout.Label ("SCATTERING", EditorStyles.miniBoldLabel);

		EditorGUI.indentLevel++;
		l.topLayerStrength = EditorGUILayout.Slider ("Top Layer", l.topLayerStrength, 0f, 1.0f);
		l.epiLayerStrength = EditorGUILayout.Slider ("Epi Layer", l.epiLayerStrength, 0f, 1.0f);
		l.subLayerStrength = EditorGUILayout.Slider ("Sub Layer", l.subLayerStrength, 0f, 1.0f);

		EditorGUI.indentLevel++;
		l.isEyes = EditorGUILayout.Toggle (" Eyes?", l.isEyes);

		if(!l.isEyes) {
			l.sssssBlurDistance = EditorGUILayout.Slider ("SSSSS Distance", l.sssssBlurDistance, 0.1f, 4.0f);
			l.epiRelative = EditorGUILayout.Slider (" Epi Softness", l.epiRelative, 0.1f, 0.9f);
			l.fadeWithDistance = EditorGUILayout.Toggle (" Distance Fade", l.fadeWithDistance);
		}
		else {
			GUILayout.Label("  NOTE: Eyes are reusing SSS buffer from the face", EditorStyles.miniBoldLabel);
		}
		EditorGUI.indentLevel--;

		EditorGUILayout.Space ();
		//l.diffuseIntensity = EditorGUILayout.Slider ("Diffuse", l.diffuseIntensity, 0f, 2f);
		{
			l.keyColor = EditorGUILayout.ColorField ("Top Layer Color", l.keyColor);
			l.epiColor = EditorGUILayout.ColorField ("Epi Layer Scatter", l.epiColor);
			l.scatterColor = EditorGUILayout.ColorField ("Sub Layer Scatter", l.scatterColor);

			l.saturation = EditorGUILayout.Slider (" Saturation", l.saturation, 0.0f, 10.0f); 
			//l.backColor = EditorGUILayout.ColorField ("Back Color", l.backColor);
			//l.scatter = EditorGUILayout.Slider (" Scatter", l.scatter, -1f, 1f);
			//l.wrapAround = EditorGUILayout.Slider ("Wrap Around", l.wrapAround, -1f, 1f);

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
			EditorUtility.SetDirty(l);
			Undo.RegisterUndo (l, "SSSSkin Params Change");
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

		if (!HasSupportedMaterial(l.GetComponent<Renderer>()))
			if (GUILayout.Button ("Setup shaders (will modify materials)!!!"))
				SetupMaterials(l.GetComponent<Renderer>());
		
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