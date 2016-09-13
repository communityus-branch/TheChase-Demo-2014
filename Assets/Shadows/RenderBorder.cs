using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class RenderBorder : MonoBehaviour {

	public int border = 1;
	public Color borderColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);

	void OnPreRender () {
		Camera cam = Camera.current;

		Rect rect = new Rect ();	
		rect.x = 0;
		rect.y = 0;
		float w = Screen.width;
		float h = Screen.height;
		if (cam.targetTexture)
		{
			w = cam.targetTexture.width;
			h = cam.targetTexture.height;
		}

		rect.width = w;
		rect.height = h;
		
		GL.Viewport (rect);
		GL.Clear (false, true, borderColor, 1f);

		rect.x = border;
		rect.y = border;
		rect.width = w - border * 2;
		rect.height = h - border * 2;

		GL.Viewport (rect);
	}

}
