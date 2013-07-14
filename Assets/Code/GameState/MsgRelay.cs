///////////////////////////////////////////////////////////////////////////////
///
/// MsgRelay
/// 
/// A MsgRelay is like a pointer. It allows a large group of objects to point
/// to the MsgRelay rather than directly to an object.
/// 
///////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
[System.Serializable] // Required so it shows up in the inspector 
[AddComponentMenu ("FSM/MsgRelay")]
public class MsgRelay : MonoBehaviour
{
	[SerializeField]	public	List<GameObject>				m_MsgReceivers;	//	the list of receivers
	[SerializeField] 	public	List<FSTrigger>					m_EntryTriggerList;
	[SerializeField] 	public	List<FSTrigger>					m_ExitTriggerList;
	
	public void Awake()
	{
		Rlplog.Trace("", "MsgRelay::Awake() - " + this.transform.name);
		if (m_EntryTriggerList == null) {
			m_EntryTriggerList = new List<FSTrigger>();
		}
		if (m_ExitTriggerList == null) {
			m_ExitTriggerList = new List<FSTrigger>();
		}
		if (m_MsgReceivers == null) {
			m_MsgReceivers = new List<GameObject>();
		}
	}
	
	public void AddMsgReceiver(GameObject newRcvr)
	{
		if (!m_MsgReceivers.Find(o => o == newRcvr))
			m_MsgReceivers.Add(newRcvr);
	}
	
	/*
	public GameObject CreateEntryAndExitTriggers(Button3D button)
	{
		//	create entry and exit triggers and attach them
		GameObject newFiniteStateGO = this.gameObject;
		
		GameObject entryTriggerGO = FSTrigger.CreateEntryTrigger(button);
		FSTrigger entryTrigger = entryTriggerGO.GetComponent<FSTrigger>();
		this.AddEntryTrigger(entryTrigger);
		entryTrigger.transform.parent = newFiniteStateGO.transform;	//	attach to the state as a parent
			
		GameObject exitTriggerGO = FSTrigger.CreateExitTrigger(button);
		FSTrigger exitTrigger = exitTriggerGO.GetComponent<FSTrigger>();
		this.AddExitTrigger(exitTrigger);
		exitTrigger.transform.parent = newFiniteStateGO.transform;	//	attach to the state as a parent
		
		return newFiniteStateGO;
	}
	*/
	
	public void AddEntryTrigger(FSTrigger trig)
	{
		m_EntryTriggerList.Add(trig);
	}
	
	public void AddExitTrigger(FSTrigger trig)
	{
		m_ExitTriggerList.Add(trig);
	}
	
	public void EnterThis()
	{
		foreach(FSTrigger trigger in m_EntryTriggerList) {
			trigger.Fire();
		}
	}
	
	public void LeaveThis()
	{
		foreach(FSTrigger trigger in m_ExitTriggerList) {
			trigger.Fire();
		}
	}	
	
	public string DebugParamList(string[] parameters)
	{
		string paramlist = "(";
		foreach(string s in parameters) {
			paramlist += s + ", ";
		}
		paramlist += ")";
		return paramlist;
	}
	
	public void RelaySendMessage(string msg, string[] parameters, SendMessageOptions smOptions)
	{
		foreach( GameObject go in m_MsgReceivers )
		{
			string paramlist = DebugParamList(parameters);
			Rlplog.Trace("MsgRelay.RelaySendMessage", "GameObject="+go.name + ", msg="+msg+", params="+paramlist);
			smOptions = SendMessageOptions.RequireReceiver;	//	hack: for testing to see if we have a valid receiver
			
			go.SendMessage(msg, parameters, smOptions);
		}
	}
	
	public void RelayBroadcastMessage(string msg, string[] parameters, SendMessageOptions smOptions)
	{
		foreach( GameObject go in m_MsgReceivers )
		{
			string paramlist = DebugParamList(parameters);
			Rlplog.Trace("", "RelayBroadcastMessage() - " + go.name + ", msg="+msg+", params="+paramlist);
			go.BroadcastMessage(msg, parameters, smOptions);
		}
	}
}