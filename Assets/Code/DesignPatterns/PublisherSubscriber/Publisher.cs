
// ======================================================================================
// File         : Publisher.cs
// Author       : Eu-Ming Lee 
// Changelist   :
//	1/4/2012 - First creation
// Description  : 
//	A Publisher has a list of Subscribers. It sends messages to these Subscribers.
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
[AddComponentMenu ("DesignPatterns/Publisher")]
public class Publisher : MonoBehaviour
{
	[LitJson.ExportType(LitJson.ExportType.Reference)]	//	use this to prevent specific fields from being exported by LitJson library
	[SerializeField] 	public	List<Subscriber>					m_SubscriberList;
	
	public void Awake()
	{
		Rlplog.Trace("Publisher.Awake()", "Transform=" + this.transform.name);
		if (m_SubscriberList == null) {
			m_SubscriberList = new List<Subscriber>();
		}
	}
	
	public void Start()
	{
		BindToInstances();
	}
	
	public void BindToInstances()
	{
		for(int ii=0; ii<m_SubscriberList.Count; ii++) {
			Subscriber sub = m_SubscriberList[ii];
			if (sub != null) {
				Subscriber subscriberInstance = sub.gameObject.GetInstance(typeof(Subscriber)) as Subscriber;
				if (subscriberInstance != null) {
					m_SubscriberList[ii] = subscriberInstance;
					m_SubscriberList[ii].SetPublisher(this);
				}
			}
		}
	}
	
	public void SendSubscriberMessage(string methodName, string publisherName, string msg)
	{
		foreach(Subscriber sb in m_SubscriberList)
		{
			if (sb != null)	//	we'll let this be null for now. Maybe we'll want to clean this up later.
				sb.ReceivePublisherMessage(methodName, publisherName, msg);
		}
	}
	
	public void AddSubscriber(Subscriber sb)
	{
		if (!isSubscriber(sb))
			m_SubscriberList.Add(sb);
	}
	
	public void RemoveSubscriber(Subscriber sb)
	{
		m_SubscriberList.Remove(sb);
		m_SubscriberList.TrimExcess();
	}
	
	public bool isSubscriber(Subscriber sb)
	{
		bool bIsSub = m_SubscriberList.Find(o => o == sb);
		return bIsSub;
	}
	
	public bool isSubscriber(GameObject go)
	{
		bool bIsSub = false;
		Subscriber sub = go.GetComponent<Subscriber>();
		if (sub != null) {
			if (isSubscriber(sub)) {
				bIsSub = true;
			}
		}
		return bIsSub;
	}
}