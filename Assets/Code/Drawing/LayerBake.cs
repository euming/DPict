// ======================================================================================
// File         : LayerBake.cs
// Author       : Eu-Ming Lee 
// Changelist   :
//	7/9/2013 - First creation
// Description  : 
//	This takes a screenshot from the camera and bakes it into the given texture.
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////


using UnityEngine;
using System.Collections;


[System.Serializable] // Required so it shows up in the inspector 
[AddComponentMenu ("DPict/LayerBake")]
public class LayerBake : MonoBehaviour 
{
	public Texture2D	m_BakeTo;
	bool				m_bBake;
	public bool			m_bFastBake = true;
	
	public void Dirty()
	{
		m_bBake = true;
	}
	
	void BakeSlow()
	{
        if (m_bBake) {
			//	this is too slow. Need a way to do this copy of FrameBuffer to BakeTo in hardware.
            m_BakeTo.ReadPixels(new Rect(0, 0, m_BakeTo.width, m_BakeTo.height), 0, 0);	//	Reads the rectangle from the camera's RenderTexture into this texture.
            m_BakeTo.Apply();
            m_bBake = false;
        }
	}
	
	void BakeFast()
	{
        if (m_bBake) {
            m_bBake = false;
		}
	}
	
	void OnPostRender()
	{
		if (m_bFastBake) {
			BakeFast();
		}
		else {
			BakeSlow();
		}
	}
}