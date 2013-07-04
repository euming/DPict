using UnityEngine;
using System.Collections;


[System.Serializable] // Required so it shows up in the inspector 
[AddComponentMenu ("DPict/BrushSizePick")]
public class BrushSizePick : MonoBehaviour 
{
	public int m_BrushSize;
	
	/*
	public void Awake()
	{
		Texture2D tex = renderer.material.mainTexture as Texture2D;
		if (tex != null) {
			m_myColor = tex.GetPixel(0,0);
		}
	}
	*/
	
	public void OnMouseDown()
	{
		Layer.SetBrushSize(GetBrushSize());
	}
	
	public int GetBrushSize()
	{
		return m_BrushSize;
	}
}
