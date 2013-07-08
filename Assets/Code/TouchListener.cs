
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
	
	bool	bTouchEnabled;
	
	void Awake()
	{
		m_camera = GetComponent<Camera>();
		//m_myPublisher = GetComponent<Publisher>();
		bTouchEnabled = Input.multiTouchEnabled;
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
	
	/*
	 * 	Allows different buttons to be differentiated
	 */
	void MouseTapSelect()
	{
		string		buttonMsg;
		
		Camera cam = m_camera;
		GameObject hitGO = null;
		
		int nButtons = 2;	//	how many buttons does our mouse have? No idea
		
		for(int ii=0; ii<nButtons; ii++) {
			bool bForgetButton = false;	//	used for m_currentlyTouchedGO[ii] = null; so that m_currentlyTouchedGO may be valid for us to do other things with it 
			bool bButtonDown = Input.GetMouseButtonDown(ii);
			bool bButtonUp = Input.GetMouseButtonUp(ii);
			bool bButtonHeld = Input.GetMouseButton(ii);
			bool bButtonAny = bButtonDown || bButtonUp || bButtonHeld;
			Vector3 mousePos = Input.mousePosition;
			
			Ray ray = cam.ScreenPointToRay(mousePos);
			RaycastHit hit;
			hitGO = null;
			
			if (Physics.Raycast(ray, out hit)) {
				hitGO = hit.transform.gameObject;	//	if we touched something
			}

			//	something changed from last frame
			if (m_currentlyTouchedGO[ii] != hitGO) {
				if (m_currentlyTouchedGO[ii] != null) {
					m_currentlyTouchedGO[ii].SendMessage("OnMouseExitListener", ii, SendMessageOptions.DontRequireReceiver);
					Rlplog.Debug("TouchListener.MouseTapSelect", "OnMouseExit("+ ii+")" + m_currentlyTouchedGO[ii].name);
				}
				if (hitGO != null) {
					m_currentlyTouchedGO[ii] = hitGO;
					m_currentlyTouchedGO[ii].SendMessage("OnMouseEnterListener", ii, SendMessageOptions.DontRequireReceiver);
					Rlplog.Debug("TouchListener.MouseTapSelect", "OnMouseEnter("+ ii+")" + m_currentlyTouchedGO[ii].name);
				}
				else {
					bForgetButton = true;
				}
			}
			
			if (bButtonAny) {
				buttonMsg = null;
				//	we pressed the button this frame
				if (bButtonDown) {
					if (hitGO != null) {	//	only send this message if the button was hit
						//m_currentlyTouchedGO[ii] = hitGO;
						buttonMsg = "OnMouseDownListener";
					}
				}
				
				//	we released the button this frame
				if (bButtonUp) {
					if (hitGO != null) {	//	only send this message if the button was hit
						buttonMsg = "OnMouseUpListener";
						bForgetButton = true;
					}
				}
				
				if (m_currentlyTouchedGO[ii] != null) {
					if (buttonMsg != null) {
						hitGO.SendMessage(buttonMsg, ii, SendMessageOptions.DontRequireReceiver);
					}
				}
			}
			
			if (bForgetButton) {
				m_currentlyTouchedGO[ii] = null;
			}
			
		}
	}

	void Update()
	{
		if (bTouchEnabled) {
			TouchTapSelect();
		}
		else {
			MouseTapSelect();
		}
	}
}
