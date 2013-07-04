
// ======================================================================================
// File         : Dispatcher.cs
// Author       : Eu-Ming Lee 
// Changelist   :
//	1/4/2012 - First creation
// Description  : 
//	A Dispatcher is a type of Subscriber that receives a message and relays that message
//	to a different GameObject depending upon its ReceiverTag. This allows the the Dispatcher
//	to filter incoming messages and direct only a single message to its intended receipient.
//	Otherwise, too many messages may be sent to inappropriate receivers who must ignore those
//	extraneous messages.
//	
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections.Generic;

//[LitJson.ExportType(LitJson.ExportType.NoExport)]	//	use this to prevent specific fields from being exported by LitJson library
[ExecuteInEditMode]
[System.Serializable] // Required so it shows up in the inspector 
[AddComponentMenu ("DesignPatterns/Dispatcher")]
public class Dispatcher : Subscriber
{
	[System.Serializable] // Required so it shows up in the inspector 
	public class RedirectAddress
	{
		public string					m_MailboxName;				//	what we're receiving messages as
		[LitJson.ExportType(LitJson.ExportType.Reference)]	//	use this to prevent specific fields from being exported by LitJson library
		public GameObject				m_Receiver;					//	who is actually receiving the messages
		
		public RedirectAddress(GameObject rcvr, string mbxName)
		{
			m_Receiver = rcvr;
			m_MailboxName = mbxName;
		}
	}
	
	[SerializeField] 	private	List<RedirectAddress>					m_ForwardingList;
	
	public void Awake()
	{
		Rlplog.Trace("Dispatcher.Awake()", "Transform=" + this.transform.name);
		if (m_ForwardingList == null) {
			m_ForwardingList = new List<RedirectAddress>();
		}
	}
	
	//	publisherName - is who is sending out the message. Example: When an animation ends, it (the publisher) may send an EndOfAnimationTrigger to its subscribers
	public override void ReceivePublisherMessage(string methodName, string publisherName, string msg)
	{
		Rlplog.Trace("Dispatcher.ReceivePublisherMessage", "(method="+methodName+", publisher="+publisherName+", disp="+this.name+", msg="+msg+" )");
		//RedirectAddress foundIt = m_ForwardingList.Find(delegate(RedirectAddress addr) {return addr.m_MailboxName == receiverName;});
		RedirectAddress foundIt = m_ForwardingList.Find( o => o.m_MailboxName == msg);	//	same as above, but with crazy lambda syntax.
		if (foundIt != null) {
			Rlplog.Trace("Dispatcher.ReceivePublisherMessage", foundIt.m_Receiver.name + ".SendMessage("+methodName+")");
			foundIt.m_Receiver.SendMessage(methodName, msg);
		}
	}
	
	public void AddReceiver(GameObject rcvr, string mailboxName)
	{
		RedirectAddress oldAddr = m_ForwardingList.Find(
			(a) => 
			{
				return((a.m_MailboxName==mailboxName) && (a.m_Receiver==rcvr));
			}
		);

		if (oldAddr == null) {
			RedirectAddress addr = new RedirectAddress(rcvr, mailboxName);
			m_ForwardingList.Add(addr);
		}
	}
}