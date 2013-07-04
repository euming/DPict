
// ======================================================================================
// File         : Subscriber.cs
// Author       : Eu-Ming Lee 
// Changelist   :
//	1/4/2012 - First creation
// Description  : 
//	A Subscriber listens to messages sent from a Publisher
//	
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections.Generic;
using CustomExtensions;

//[LitJson.ExportType(LitJson.ExportType.NoExport)]	//	use this to prevent specific fields from being exported by LitJson library
//[ExecuteInEditMode]
[System.Serializable] // Required so it shows up in the inspector 
[AddComponentMenu ("DesignPatterns/Subscriber")]
public class Subscriber : MonoBehaviour
{
	[SerializeField] 	private		Publisher		m_myPublisher;
	[SerializeField] 	public		string			m_MessageFilter=null;
	[SerializeField] 	public		string			m_MethodFilter=null;
	
	//	automatically subscribe to my parent if the parent is a publisher
	public void Start()
	{
		if (m_myPublisher == null) {
			Publisher ifParentIsPublisher = null;
			if (this.transform.parent != null) {
				ifParentIsPublisher = this.transform.parent.GetComponent<Publisher>();
				if (ifParentIsPublisher != null) {
					SubscribeTo(ifParentIsPublisher, m_MessageFilter);
				}
			}
		}
		BindToInstances();
	}
	
	public void SetPublisher(Publisher p)
	{
		m_myPublisher = p;
	}
	
	public void BindToInstances()
	{
		if (m_myPublisher != null) {
			Publisher publisherInstance = m_myPublisher.gameObject.GetInstance(typeof(Publisher)) as Publisher;
			if (publisherInstance != null) {
				m_myPublisher = publisherInstance;
			}
		}
	}
	
	public void SubscribeTo(Publisher publisher, string msg)
	{
		m_myPublisher = publisher;
		publisher.AddSubscriber(this);
		m_MessageFilter = msg;
	}
	
	public void Subscribe()
	{
		SubscribeTo(m_myPublisher, m_MessageFilter);
	}
	
	public void Unsubscribe()
	{
		m_myPublisher.RemoveSubscriber(this);
		//	m_myPublisher = null;	//	not necessary to forget our publisher. We may want to remember it in case we Subscribe() again. This allows us to efficiently stop receiving messages when we are inactive.
	}
	
	public virtual void ReceivePublisherMessage(string methodName, string publisherName, string msg)
	{
		Rlplog.Trace("Subscriber.ReceivePublisherMessage", "( method="+methodName+", to="+this.name+", from="+publisherName+", msg="+msg+" )");
		bool bSendMessage = true;
		
		//	we have a message filter to consider and our message does not have a match
		if (!string.IsNullOrEmpty(m_MessageFilter) && !msg.Contains(m_MessageFilter)) {
			bSendMessage = false;
		}
		
		if (!string.IsNullOrEmpty(m_MethodFilter) && !methodName.Contains(m_MethodFilter)) {
			bSendMessage = false;
		}
		
		if (bSendMessage == true) {
			//	if debugging, turn this on to require receiver
			//SendMessageOptions smOptions = SendMessageOptions.RequireReceiver;	//	for testing to see if we have a valid receiver
			if (this.gameObject.active == false) {
				Rlplog.Debug("Subscriber.ReceivePublisherMessage", "Warning: " + this.name + " is not active, and thus will not receive msg="+msg+" method="+methodName+" from publisher="+publisherName);
			}
			SendMessageOptions smOptions = SendMessageOptions.DontRequireReceiver;
			SendMessage(methodName, msg, smOptions);		//	send a message to myself
		}
	}
}
