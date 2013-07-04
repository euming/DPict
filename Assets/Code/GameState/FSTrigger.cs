///////////////////////////////////////////////////////////////////////////////
///
/// Trigger
/// 
///////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections.Generic;
using CustomExtensions;

//[ExecuteInEditMode]
[System.Serializable] // Required so it shows up in the inspector 
[AddComponentMenu ("FSM/Trigger")]
public class FSTrigger : MonoBehaviour
{
	public		GameObject			m_MsgReceiver;
	public 		bool				m_isBroadcast;			//	send this same message to all of the *components" under this GameObject and to self
	public		bool				m_isRecursive;			//	send this same message and parameters to self and to the children of the transform hierarchy
	public		string				m_ScriptMessage;
	public 		bool				m_bRelayThis = false;
	public		string[]			m_Parameters = new string[1];
	public		bool				m_bBreakInDebugger = false;
	
	/*
	//	non-thead-safe
	static public GameObjectExtension.GameObjectFcn localSendMessageFcn = new GameObjectExtension.GameObjectFcn(GameObjectExtension.SendMessageDelegate);
	static public GameObjectExtension.GameObjectFcn localBroadcastMessageFcn = new GameObjectExtension.GameObjectFcn(GameObjectExtension.BroadcastMessageDelegate);
	static public GameObjectExtension.GameObjectFcn localSendMessageNoRelayFcn = new GameObjectExtension.GameObjectFcn(GameObjectExtension.SendMessageDelegateNoRelay);
	static public GameObjectExtension.GameObjectFcn localBroadcastMessageNoRelayFcn = new GameObjectExtension.GameObjectFcn(GameObjectExtension.BroadcastMessageDelegateNoRelay);
	*/
	static public GameObjectExtension.GameObjectFcn localSendMessageFcn = new GameObjectExtension.GameObjectFcn(GameObjectExtension.SendMessageDelegateMT);
	static public GameObjectExtension.GameObjectFcn localBroadcastMessageFcn = new GameObjectExtension.GameObjectFcn(GameObjectExtension.BroadcastMessageDelegateMT);
	static public GameObjectExtension.GameObjectFcn localSendMessageNoRelayFcn = new GameObjectExtension.GameObjectFcn(GameObjectExtension.SendMessageDelegateNoRelayMT);
	static public GameObjectExtension.GameObjectFcn localBroadcastMessageNoRelayFcn = new GameObjectExtension.GameObjectFcn(GameObjectExtension.BroadcastMessageDelegateNoRelayMT);
	
	public void SetDefaultLike(GameObject defaultObj)
	{
		FSTrigger defaultComponent = defaultObj.GetComponent<FSTrigger>();
		
		if (defaultComponent != null) {
			this.m_MsgReceiver = defaultComponent.m_MsgReceiver;
			this.m_ScriptMessage = defaultComponent.m_ScriptMessage;
			this.m_Parameters = defaultComponent.m_Parameters;
			this.m_isBroadcast = defaultComponent.m_isBroadcast;
		}
	}

	public void Awake()
	{
		if (m_Parameters == null)
			m_Parameters = new string[1];
		/*
		if (localSendMessageFcn == null)
			localSendMessageFcn = new GameObjectExtension.GameObjectFcn(GameObjectExtension.SendMessageDelegate);
		if (localBroadcastMessageFcn == null)
			localBroadcastMessageFcn = new GameObjectExtension.GameObjectFcn(GameObjectExtension.BroadcastMessageDelegate);
		*/
		//	automatically relink myself to the instance
		//	check parent
		if (this.transform.parent) {
			if (m_MsgReceiver == null) {
				string errstring = Rlplog.Error("FSTrigger.Awake", "Trigger " + this.name + " has null MsgReceiver.");
				CommentError error = this.transform.parent.gameObject.QuarantineError(this.gameObject, errstring);
				error.AddReference(this.transform.parent.gameObject);
			}
			else if (this.transform.parent.name == m_MsgReceiver.name) { // added "else" to avoid exception when m_MsgReceiver == null (slc)
				m_MsgReceiver = this.transform.parent.gameObject;
			}
		}
		else if (null != m_MsgReceiver) { // added null reference check (slc)
			MsgReceiver msgReceiverInstance = m_MsgReceiver.GetInstance(typeof(MsgReceiver)) as MsgReceiver;
			if (msgReceiverInstance != null) {
				m_MsgReceiver = msgReceiverInstance.gameObject;
			}
		}
	}
	
	static public void FireTriggerList(List<FSTrigger> triggerList)
	{
		foreach(FSTrigger trigger in triggerList) {
			if (trigger == null) {
				Rlplog.Debug("FSTrigger.FireTriggerList", "NULL Trigger in its list.");
			}
			else {
				trigger.Fire();
			}
		}
	}
	
	static public bool BindTriggerListToInstancesOrNull(GameObject parentGO, ref List<FSTrigger> triggerList)
	{
		bool	bAllPrefabsAreBoundToInstances = true;
		
		for(int ii=0; ii<triggerList.Count; ii++) {
			FSTrigger trig = triggerList[ii];
			if (trig != null) {
				GameObject trigObj = null;
				if (parentGO != null)
					trigObj = parentGO.FindObjectExactlyFromChildren(trig.gameObject);	//	find an exact match. If we found an exact match, we don't need to go any further
				if (trigObj == null) {
					FSTrigger trigInstance = trig.gameObject.GetInstance(typeof(FSTrigger)) as FSTrigger;
					triggerList[ii] = trigInstance;
					if (trigInstance == null) {
						bAllPrefabsAreBoundToInstances = false;
						string trigname = trig.name;
						if (trig.transform.parent != null) {
							trigname = trig.transform.parent.name + "." + trig.name;
						}
						Rlplog.Debug("FSTrigger.BindTriggerListToInstancesOrNull", "Trigger on " + parentGO.name + " is not instanced: " + trigname);
					}
				}
			}
		}
		return bAllPrefabsAreBoundToInstances;
	}
	
	static public bool BindTriggerListToInstances(GameObject parentGO, ref List<FSTrigger> triggerList)
	{
		bool	bAllPrefabsAreBoundToInstances = true;
		
		for(int ii=0; ii<triggerList.Count; ii++) {
			FSTrigger trig = triggerList[ii];
			if (trig != null) {
				GameObject trigObj = null;
				if (parentGO != null)
					trigObj = parentGO.FindObjectExactlyFromChildren(trig.gameObject);	//	find an exact match. If we found an exact match, we don't need to go any further
				if (trigObj == null) {
					FSTrigger trigInstance = trig.gameObject.GetInstance(typeof(FSTrigger)) as FSTrigger;
					if (trigInstance != null) {
						triggerList[ii] = trigInstance;
					}
					else {
						bAllPrefabsAreBoundToInstances = false;
						string trigname = trig.name;
						if (trig.transform.parent != null) {
							trigname = trig.transform.parent.name + "." + trig.name;
						}
						Rlplog.Debug("FSTrigger.BindTriggerListToInstances", "Trigger on " + parentGO.name + " is not instanced: " + trigname);
					}
				}
			}
		}
		return bAllPrefabsAreBoundToInstances;
	}
	
	public string GetCaller(int upCallStackNumber)
	{
		System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace();
		
		System.Diagnostics.StackFrame frame = trace.GetFrame(upCallStackNumber);
		
		return frame.GetMethod().Name;
	}
	
	//	do this thing
	public void Fire()
	{
		if (Application.isEditor == true) {
			if (this.m_bBreakInDebugger == true){				//	allow the debugger to pause here so it's easier for us to debug
				string parentName = "None";
				if (this.transform.parent != null) {
					parentName = this.transform.parent.name;
				}
				string callerMethodName = GetCaller(2);
				string msg = this.m_ScriptMessage;
				msg += "(";
				foreach(string param in this.m_Parameters) {
					msg += param + ", ";
				}
				msg += ")";
				Rlplog.Debug("FSTrigger.Fire", "Caller="+callerMethodName + ", Trigger="+this.name + ", Msg="+parentName+"."+msg);
				//Rlplog.Debug("FSTrigger.Fire", "DebugBreak called by " + this.name + " child of " + parentName + ". Fired from " + callerMethodName + ". Message="+msg);
				Debug.Break();
			}
		}
		
		GameObjectExtension.GameObjectFcn	localMessageFcn;
		if (localSendMessageFcn == null) {
			Rlplog.Error("FSTrigger.Fire","Trying to Fire() without Awake() being called for this trigger " + this.name + "\nYou may need to clear the Current State in the FSM that this trigger belongs to.\n");
		}
		
		if (m_MsgReceiver == null) {
			Rlplog.Error("FSTrigger.Fire", "Trigger " + this.name + " has null MsgReceive.");
			return;
		}
		m_MsgReceiver.gameObject.SetMessage(m_ScriptMessage);
		string paramlist = "(";
		foreach(string s in m_Parameters) {
			paramlist += s + ", ";
		}
		paramlist += ")";
		Rlplog.Trace("FSTrigger.Fire()", m_MsgReceiver.gameObject.name + " -> " + this.transform.name + ", msg="+m_ScriptMessage+", params="+paramlist);
		
		if (true==m_bRelayThis) {
			//	decide whether we want to send or broadcast
			if (m_isBroadcast == true) {
				localMessageFcn = localBroadcastMessageFcn;
			}
			else {
				localMessageFcn = localSendMessageFcn;
			}
		}
		else {
			if (m_isBroadcast == true) {
				localMessageFcn = localBroadcastMessageNoRelayFcn;
			}
			else {
				localMessageFcn = localSendMessageNoRelayFcn;
			}
		}
		
		if (m_ScriptMessage == "Activate" || m_ScriptMessage == "Deactivate")
		{
			if (m_Parameters.Length > 0)
			{
				if (m_Parameters[0] == "Collider")
				{
					if (m_MsgReceiver.gameObject.collider != null)
					{
						if (m_ScriptMessage == "Activate")
							m_MsgReceiver.gameObject.collider.enabled = true;
						else
							m_MsgReceiver.gameObject.collider.enabled = false;
					}
				}
				else if (m_Parameters[0] == "BoxCollider")
				{
					BoxCollider boxObj = m_MsgReceiver.gameObject.GetComponent<BoxCollider>();
					if (boxObj != null)
					{
						if (m_ScriptMessage == "Activate")
							boxObj.enabled = true;
						else
							boxObj.enabled = false;
					}
				}
				else if (m_Parameters[0] == "MeshCollider")
				{
					MeshCollider meshObj = m_MsgReceiver.gameObject.GetComponent<MeshCollider>();
					if (meshObj != null)
					{
						if (m_ScriptMessage == "Activate")
							meshObj.enabled = true;
						else
							meshObj.enabled = false;
					}
				}
			}
		}
		
		string[] threadSafeParameters = BuildParameters(m_Parameters, m_ScriptMessage);
		
		localMessageFcn(m_MsgReceiver.gameObject, threadSafeParameters);	//	first send to this object
		
		//	then, send to its children and children's children, etc.
		if (m_isRecursive == true) {
			m_MsgReceiver.gameObject.ForEachChildDo(localMessageFcn, threadSafeParameters);
		}
	}
	
	//	make the first parameter the script message so that we don't need to use
	//	SetMessage which is horriby unsafe for threads.
	public string[] BuildParameters(string[] paramlist, string scriptMessage)
	{
		string[] newList = new string[paramlist.Length+1];

		//	append scriptMessage to the argument list
		newList[paramlist.Length] = scriptMessage;
		for(int ii=0; ii<paramlist.Length; ii++) {
			newList[ii] = paramlist[ii];
		}
		
		return newList;
	}
	
	public void AddParameter(string newParam)
	{
		int idx = m_Parameters.Length;
		System.Array.Resize(ref m_Parameters, idx+1);
		m_Parameters[idx] = newParam;
	}
	
	/*
	static public GameObject CreateEntryTrigger(Button3D button)
	{
		GameObject	newTrig = new GameObject("Trigger Entry " + button.name);
		FSTrigger trig = newTrig.AddComponent<FSTrigger>();
		trig.m_MsgReceiver = button.gameObject;
		trig.m_ScriptMessage = "SetActive";
		trig.m_Parameters = new string[1];
		trig.m_Parameters[0] = "True";
		return newTrig;
	}
	static public GameObject CreateExitTrigger(Button3D button)
	{
		GameObject	newTrig = new GameObject("Trigger Exit " + button.name);
		FSTrigger trig = newTrig.AddComponent<FSTrigger>();
		trig.m_MsgReceiver = button.gameObject;
		trig.m_ScriptMessage = "SetActive";
		trig.m_Parameters = new string[1];
		trig.m_Parameters[0] = "False";
		return newTrig;
	}
	*/
	
	static public GameObject CreateTrigger(GameObject msgRelay, string command, string arg)
	{
		GameObject	newTrig = new GameObject("Trigger " + command + " " + arg);
		FSTrigger trig = newTrig.AddComponent<FSTrigger>();
		trig.m_MsgReceiver = msgRelay;
		trig.m_ScriptMessage = command;
		trig.m_Parameters = new string[1];
		trig.m_Parameters[0] = arg;
		return newTrig;
	}
	
	static public GameObject CreatePlayAnimTrigger(GameObject msgRelay, Animation anim, string playCommand)
	{
		GameObject	newTrig = new GameObject("Trigger Anim Play " + anim.name);
		FSTrigger trig = newTrig.AddComponent<FSTrigger>();
		if (msgRelay == null) {
			trig.m_MsgReceiver = anim.gameObject;
		}
		else {
			trig.m_MsgReceiver = msgRelay;
		}
		if (playCommand != null) {
			trig.m_ScriptMessage = playCommand;
		}
		else {
		//	trig.m_ScriptMessage = "CrossFace";
			trig.m_ScriptMessage = "Play";
		}
		trig.m_Parameters = new string[1];
		trig.m_Parameters[0] = anim.clip.name;
		return newTrig;
	}
	
	static public GameObject CreateStopAnimTrigger(GameObject msgRelay, Animation anim)
	{
		GameObject	newTrig = new GameObject("Trigger Anim Stop " + anim.name);
		FSTrigger trig = newTrig.AddComponent<FSTrigger>();
		if (msgRelay == null) {
			trig.m_MsgReceiver = anim.gameObject;
		}
		else {
			trig.m_MsgReceiver = msgRelay;
		}
		//	trig.m_ScriptMessage = "Stop";
		trig.m_ScriptMessage = "CrossFade";
		trig.m_Parameters = new string[1];
		trig.m_Parameters[0] = anim.clip.name;
		return newTrig;
	}
}
			