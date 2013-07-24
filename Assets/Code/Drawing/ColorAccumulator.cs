// ======================================================================================
// File         : ColorAccumulator.cs
// Author       : Eu-Ming Lee 
// Changelist   :
//	07/22/2013 - First creation
// Description  : 
//	Color Accumulator allows multiple input sources to accumulate a single color. This
//	allows multi-touch to allow combinations of colors
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
//using System.Collections;
using System.Collections.Generic;


[System.Serializable] // Required so it shows up in the inspector 
[AddComponentMenu ("DPict/ColorAccumulator")]
public class ColorAccumulator : MonoBehaviour 
{
	public Color[] 		m_bufferColor = new Color[2];	//	double buffer
	public int			m_curBufferIdx;	//	double buffer
	public Color		m_accColor;
	public int			m_nColorSources;	//	number of sources of color currently
	public List<ColorPick>	m_SelectSources = new List<ColorPick>();
	public List<Color>		m_ColorSources = new List<Color>();
	
	void Awake()
	{
		m_nColorSources = 0;
		m_curBufferIdx = 0;
		m_bufferColor[0] = Color.black;
		m_bufferColor[1] = Color.black;
	}
	
	public void OnSelect(ColorPick src)
	{
		m_nColorSources++;
		m_SelectSources.Add(src);
		m_ColorSources.Add(src.m_myColor);
		Rlplog.Debug("ColorAccumulator.OnSelect", "Add " + src.name);
	}
	
	public void OnUnselect(ColorPick src)
	{
		m_nColorSources--;
		m_SelectSources.Remove(src);
		m_ColorSources.Remove(src.m_myColor);
		Rlplog.Debug("ColorAccumulator.OnUnselect", "Remove " + src.name);
	}
	
	public void AddColor(Color c)
	{
		m_accColor += c;
	}
	
	public Color GetColor()
	{
		return m_accColor;
	}
	
	void Update()
	{
	}
	
	void LateUpdate()			//	swap buffers
	{
		m_accColor = m_bufferColor[m_curBufferIdx];		//	clear accumulator for next frame. This should be made into a static function and called only once per frame
		m_accColor = Color.black;
		for(int ii=0; ii<m_SelectSources.Count; ii++) {
			ColorPick pick = m_SelectSources[ii];
			m_ColorSources[ii] = pick.m_myColor;
			m_accColor += pick.m_myColor;
			
		}
		//m_curBufferIdx ^=1;		//	xor 1
	}	
}