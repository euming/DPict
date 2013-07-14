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
	public Texture		m_BakeTo;
	bool				m_bBake;
	public bool			m_bFastBake = true;
	
	public void Dirty()
	{
		m_bBake = true;
	}
	
	void BakeSlow()
	{
        if (m_bBake) {
			Texture2D bakeTo2D = m_BakeTo as Texture2D;
			if (bakeTo2D != null) {
				//	this is too slow. Need a way to do this copy of FrameBuffer to BakeTo in hardware.
	            bakeTo2D.ReadPixels(new Rect(0, 0, m_BakeTo.width, m_BakeTo.height), 0, 0);	//	Reads the rectangle from the camera's RenderTexture into this texture.
	            bakeTo2D.Apply();
	            m_bBake = false;
			}
        }
	}
	
	void BakeFast(RenderTexture src, RenderTexture dst)
	{
        if (m_bBake) {
			RenderTexture activeRT = RenderTexture.active;	//	current render target
			//	int nRenderTargets = SystemInfo.supportedRenderTargetCount;
			if (dst != null) {
				bool bTestFullScreenBlit = true;
				if (bTestFullScreenBlit) {
					//	src, dest
					//camera.targetTexture = null;
					Graphics.Blit(src, dst);	//	very slow. 322.84 ms on iPad 3
					//camera.targetTexture = activeRT;
				}
				else {
					//	I still don't know how this works
					Rect screenRect = new Rect(0, 0, 256, 256);
				
					Graphics.DrawTexture(screenRect, activeRT);
				}
	            m_bBake = false;
			}
		}
	}
	
	/*
	void UpdateTextureCPP( int nativeTexID )
	{
		glPixelStorei(GL_UNPACK_ALIGNMENT, 4);
		glBindTexture(GL_TEXTURE_2D, nativeTexID);
		glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, width, height, 0, GL_BGRA, GL_UNSIGNED_BYTE, im->data());
	}
	*/
#if UNITY_IPHONE
	[DllImport ("__Internal")]
	public static extern void UpdateTextureCPP(int nativeTexID);
#endif
	
	void OnPostRender()
	//void OnRenderImage(RenderTexture src, RenderTexture dst)
	{
		if (m_bFastBake) {
			//	m_bBake = true;
			RenderTexture bakeToRT = m_BakeTo as RenderTexture;
			RenderTexture activeRT = RenderTexture.active; 
			BakeFast(activeRT, bakeToRT);
		}
		else {
			BakeSlow();
		}
	}
}