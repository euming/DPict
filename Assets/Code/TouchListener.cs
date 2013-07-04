
// ======================================================================================
// File         : TouchListener.cs
// Author       : Eu-Ming Lee 
// Changelist   :
//	7/18/2012 - First creation
// Description  : 
//	Expected to be attached to a Camera to receive Input commands from Mobile devices
//	in order to simulate Mouse controls.
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
//using CustomExtensions;

///////////////////////////////////////////////////////////////////////////////
///
/// TouchListener
/// 
///////////////////////////////////////////////////////////////////////////////
//[RequireComponent(typeof(Publisher))]
[System.Serializable] // Required so it shows up in the inspector 
[AddComponentMenu ("GUI/TouchListener (Camera)")]
public class TouchListener : MonoBehaviour
{
	private Camera			m_camera;
	private GameObject[]	m_currentlyTouchedGO = {null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null};	//	16 slots. 7 wasn't enough!
	//private	Publisher		m_myPublisher;
	
	void Awake()
	{
		m_camera = GetComponent<Camera>();
		//m_myPublisher = GetComponent<Publisher>();
	}
	
	/*
	 * 	tries to simulate a mouse press. Should handle the case where you press on the button, but then move your finger off the button.
	 * 	Like the mouse controls, it activates when you release, not when you press.
	 */
	void TouchTapSelect()
	{
		Camera cam = m_camera;
		GameObject hitGO = null;
		
		int TouchNum = 0;
		
		foreach (Touch touch in Input.touches) {
			bool		bForgetButton = false;
			string		buttonEnterExitMsg;
			string		buttonMsg;
			int			fingerID = touch.fingerId;
			
			TouchNum++;
			
			if (fingerID >= m_currentlyTouchedGO.Length) {	//	check bounds. ignore input beyond 7, send out an error message and continue as normal.
				continue;
			}
			
			Ray ray = cam.ScreenPointToRay(touch.position);
			RaycastHit hit;
			hitGO = null;
			
			if (Physics.Raycast(ray, out hit)) {
				hitGO = hit.transform.gameObject;	//	if we touched something
			}
			buttonMsg = null;
			buttonEnterExitMsg = null;
			switch (touch.phase)
			{
				default:
					break;
				case TouchPhase.Began:
					buttonMsg = "OnMouseDown";
					if (hitGO != null) {
						buttonEnterExitMsg = "OnMouseEnter";
						m_currentlyTouchedGO[fingerID] = hitGO;
					}
					break;
				case TouchPhase.Moved:
				case TouchPhase.Stationary:
					if (hitGO != null) {
						buttonEnterExitMsg = "OnMouseOver";
						m_currentlyTouchedGO[fingerID] = hitGO;
					}
					break;
				case TouchPhase.Canceled:
				case TouchPhase.Ended:
					buttonMsg = "OnMouseUp";
					if (m_currentlyTouchedGO[fingerID] != hitGO) {
						buttonEnterExitMsg = "OnMouseExit";
					}
					bForgetButton = true;
					break;
			}
			
			if (m_currentlyTouchedGO[fingerID] != null) {
				if (buttonEnterExitMsg != null) {
					m_currentlyTouchedGO[fingerID].SendMessage(buttonEnterExitMsg);
				}
				if (buttonMsg != null) {
					m_currentlyTouchedGO[fingerID].SendMessage(buttonMsg);
				}
				if (bForgetButton) {
					m_currentlyTouchedGO[fingerID] = null;
				}
			}
		}
	}
	
	void Update()
	{
		TouchTapSelect();
	}
}
