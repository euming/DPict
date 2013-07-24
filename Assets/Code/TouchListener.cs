
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
	public 	bool			m_bOnlySendToSubscribers = false;	//	if true, only send to Subscribers
	public 	int				m_nButtons = 1;						//	how many mouse buttons to check for.
	private Camera			m_camera;
	public 	bool			m_bCheckAllHits = false;			//	ray normally stops at the first hit. This allows the ray to penetrate and return all hits
	public  GameObject[]	m_currentlyTouchedGO = {null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null};	//	16 slots. 7 wasn't enough! If mouse is hovering over this object, then it will be stored here.
	//private	Publisher		m_myPublisher;
	
	bool	m_bTouchEnabled;
	
	void Awake()
	{
		m_camera = GetComponent<Camera>();
		//m_myPublisher = GetComponent<Publisher>();
		m_bTouchEnabled = Input.multiTouchEnabled;
		//	Rlplog.DbgFlag = true;
		m_currentlyTouchedGO = new GameObject[16];
		for (int ii=0; ii<m_nButtons; ii++) {
			m_currentlyTouchedGO[ii] = null;
		}
	}
	
	public bool isTouchEnabled()
	{
		return m_bTouchEnabled;
	}
	
	bool isSubscriber(GameObject go)
	{
		bool	bIsSub = false;
		Publisher pub = this.GetComponent<Publisher>();
		if (pub != null) {
			if (pub.isSubscriber(go)) {
				bIsSub = true;
			}
		}
		return bIsSub;
	}
	
	//	determine whether we are a subscriber to a touch listener which will be sending us the Mouse messages in lieu of Unity's messages
	public static bool isSubscriberOfTouchListener(GameObject queriedGO)
	{
		bool bIsSubscriberOfTouchListener = false;
		Subscriber sub = queriedGO.GetComponent<Subscriber>();
		if (sub != null) {
			Publisher pub = sub.GetPublisher();
			TouchListener tl = pub.GetComponent<TouchListener>();
			if (tl != null) {
				if (!tl.isTouchEnabled()) {
					bIsSubscriberOfTouchListener = true;
				}
			}
		}
		return bIsSubscriberOfTouchListener;
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
				bool bSendMessage = true;	//	default is to send to everybody
				if (m_bOnlySendToSubscribers) {	//	sometimes, we only want to send this message to subscribers
					bSendMessage = false;	//	if we send only to subscribers, the default is that we don't send unless we know for sure our target is a subscriber
					if (isSubscriber(m_currentlyTouchedGO[fingerID])) {
						bSendMessage = true;
					}
				}
				if (bSendMessage) {
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
	}
	
	/*
	 * 	Allows different buttons to be differentiated
	 */
	void MouseTapSelect()
	{
		string		buttonMsg;
		
		Camera cam = m_camera;
		GameObject hitGO = null;
		
		int nButtons = m_nButtons;	//	how many buttons does our mouse have? No idea
		
		for(int ii=0; ii<nButtons; ii++) {
			bool bForgetButton = false;	//	used for m_currentlyTouchedGO[ii] = null; so that m_currentlyTouchedGO may be valid for us to do other things with it 
			bool bButtonDown = Input.GetMouseButtonDown(ii);	//	pressed since last check?
			bool bButtonUp = Input.GetMouseButtonUp(ii);		//	released since last check?
			bool bButtonHeld = Input.GetMouseButton(ii);		//	current mouse button status
			bool bButtonAnyEdge = bButtonDown || bButtonUp;
			Vector3 mousePos = Input.mousePosition;
			
			Ray ray = cam.ScreenPointToRay(mousePos);
			RaycastHit[] hits;
			hits = new RaycastHit[1];	//	put one hit in the array
			bool bSendMessage = true;	//	default is to send to everybody
			if (m_bCheckAllHits == false) {
				RaycastHit hit;
				hitGO = null;
				//public static bool Raycast (Ray ray, out RaycastHit hitInfo, float distance, int layerMask)

				if (Physics.Raycast(ray, out hit, cam.far/*, cam.cullingMask*/)) {
					hits[0] = hit;
				}
				else {
					hits = new RaycastHit[0];
				}
			}
			else {			
	        	hits = Physics.RaycastAll(ray.origin, ray.direction, cam.far);
			}
			
			foreach(RaycastHit hit in hits) {
				hitGO = null;
				
				bSendMessage = true;	//	default is to send to everybody
				hitGO = hit.transform.gameObject;	//	if we touched something
				if (m_bOnlySendToSubscribers) {	//	sometimes, we only want to send this message to subscribers
					bSendMessage = false;	//	if we send only to subscribers, the default is that we don't send unless we know for sure our target is a subscriber
					if (isSubscriber(hitGO)) {
						bSendMessage = true;
					}
				}
				//	something changed from last frame
				if (m_currentlyTouchedGO[ii] != hitGO) {
					if (m_currentlyTouchedGO[ii] != null) {
						m_currentlyTouchedGO[ii].SendMessage("OnMouseExitListener", ii, SendMessageOptions.DontRequireReceiver);
						Rlplog.Debug("TouchListener.MouseTapSelect", "OnMouseExitListener("+ ii+") old=" + m_currentlyTouchedGO[ii].name);
						bForgetButton = true;
					}
					if (hitGO != null) {
						if (bSendMessage) {
							m_currentlyTouchedGO[ii] = hitGO;
							m_currentlyTouchedGO[ii].SendMessage("OnMouseEnterListener", ii, SendMessageOptions.DontRequireReceiver);
							Rlplog.Debug("TouchListener.MouseTapSelect", "OnMouseEnterListener("+ ii+") new=" + m_currentlyTouchedGO[ii].name);
							bForgetButton = false;	//	we can't null out m_currentlyTouchedGO[ii]. we just set it here!
						}
					}
				}
				
				if (bButtonAnyEdge) {
					//	button edge triggers
					buttonMsg = null;
					//	we pressed the button this frame
					if (bButtonDown) {
						if (hitGO != null) {	//	only send this message if the button was hit
							//m_currentlyTouchedGO[ii] = hitGO;
							buttonMsg = "OnMouseDownListener";
							Rlplog.Debug("TouchListener.MouseTapSelect", "OnMouseDownListener("+ ii+")");
						}
					}
					
					//	we released the button this frame
					if (bButtonUp) {
						if (hitGO != null) {	//	only send this message if the button was hit
							buttonMsg = "OnMouseUpListener";
							Rlplog.Debug("TouchListener.MouseTapSelect", "OnMouseUpListener("+ ii+")");
						}
					}
					
					if (m_currentlyTouchedGO[ii] != null) {
						if (buttonMsg != null) {
							if (bSendMessage) {
								hitGO.SendMessage(buttonMsg, ii, SendMessageOptions.DontRequireReceiver);
							}
						}
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
		if (m_bTouchEnabled) {
			TouchTapSelect();
		}
		else {
			MouseTapSelect();
		}
	}
}
