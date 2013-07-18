using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable] // Required so it shows up in the inspector 
[AddComponentMenu ("DPict/Brush")]
public class Brush : MonoBehaviour 
{
	public enum BrushStyle
	{
		BrushStyle_HardEdges,
		BrushStyle_SoftEdges,		//	anti-alias the edges of this brush
		BrushStyle_End,
	}
	
	//	brush stuff
	float				m_blendValue = 0.6667f;//	0.333f;		//	transparency
	public int			m_brushWidth = 16;
	public Color 		m_brushColor = Color.blue;

	//	brush style
	public BrushStyle 			m_BrushStyle = BrushStyle.BrushStyle_SoftEdges;
	
	//	cache
	Layer				m_myLayer;
	
	void Start()
	{
		m_myLayer = this.GetComponent<Layer>();
		UpdateBrushIcon();
	}
	
	public Texture GetTexture()
	{
		Texture tex = null;
		if (m_myLayer) {
			tex = m_myLayer.m_myTexture;
		}
		return tex;
	}
	public void UpdateBrushIcon()
	{
		m_myLayer.Clear();
		Vector3 center = Vector3.zero;
		center.x = m_myLayer.m_textureWidth/2;
		center.y = m_myLayer.m_textureHeight/2;
		m_myLayer.StartPoint(center);
		m_myLayer.PaintBrush(center, this);
		m_myLayer.DrawSegments();
	}
	
	public void SetBrushColor(Color color)
	{
		m_brushColor = color;
		UpdateBrushIcon();
	}
	
	public Color GetBrushColor()
	{
		return m_brushColor;
	}
	
	public void SetBrushSize(int sz)
	{
		m_brushWidth = sz;
		UpdateBrushIcon();
	}
	
	public int GetBrushSize()
	{
		return m_brushWidth;
	}
	
	public float GetBlendValue()
	{
		return m_blendValue;
	}
	
	public void ToggleBrushStyle()
	{
		m_BrushStyle++;
		if (m_BrushStyle >= BrushStyle.BrushStyle_End) {
			m_BrushStyle = (BrushStyle)(0);
		}
	}
	
	public void OnMouseDown()
	{
		if (Layer.GetBrush() != this) {
			Layer.SetBrush(this);
		}
		else {
			ToggleBrushStyle();
		}
		UpdateBrushIcon();
	}
}