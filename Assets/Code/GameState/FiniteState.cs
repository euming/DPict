///////////////////////////////////////////////////////////////////////////////
///
/// FiniteState
/// 
///////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections.Generic;
using CustomExtensions;

//[ExecuteInEditMode]
[System.Serializable] // Required so it shows up in the inspector 
[AddComponentMenu ("FSM/FiniteState")]
public class FiniteState : MonoBehaviour
{
	[SerializeField]	public	FiniteStateMachine				m_MyFSM;	//	the finite state machine that I belong to. Can only be 1!
	[SerializeField] 	public	List<FSTrigger>					m_EntryTriggerList;
	[SerializeField] 	public	List<FSTrigger>					m_ExitTriggerList;
	//	[SerializeField] 	public	List<FSConnection>				m_ConnectionList;	//	don't need to be a component yet
	[SerializeField] 	public	List<FiniteState>				m_ValidStateList;	//	use this until we rally need to be a Connection
	
	private				bool 									m_bInstancesBound;	//	our trigger lists may refer to prefabs. This is false if we may still be bound to prefabs.
	private				bool									m_bStartCalled;
	
	public virtual void Awake()
	{
		Rlplog.Trace("", "FiniteStateMachine::Awake() - " + this.transform.name);
		//	attach myself
		if ((m_MyFSM == null) && (Application.isEditor==true)) {
			if (this.transform.parent != null) {
				m_MyFSM = this.transform.parent.GetComponent<FiniteStateMachine>();
			}
			Rlplog.Error("FiniteState.Awake", "FiniteState " + this.name + " does not have m_MyFSM assigned. Assigning in EditorMode Only.");
		}
		
		if (m_EntryTriggerList == null) {
			m_EntryTriggerList = new List<FSTrigger>();
		}
		if (m_ExitTriggerList == null) {
			m_ExitTriggerList = new List<FSTrigger>();
		}
		/*
		if (m_ConnectionList == null) {
			m_ConnectionList = new List<FSConnection>();
		}
		*/
		if (m_ValidStateList == null) {
			m_ValidStateList = new List<FiniteState>();
		}
		
		m_bInstancesBound = false;
		m_bStartCalled = false;
	}
	
	public bool AttachToBrothers(GameObject instanceRoot)
	{
		bool	bAllPrefabsAreBoundToInstances = true;
		
		//	attach triggers to my instances
		foreach(FSTrigger trig in this.m_EntryTriggerList) {
			if (trig != null) {
				GameObject trigObj = this.gameObject.FindObjectExactlyFromChildren(trig.gameObject);	//	find an exact match. If we found an exact match, we don't need to go any further
				if (trigObj == null) {	//	no match found
					trigObj = this.gameObject.FindObjectFromChildren(trig.name);	//	first, try to find the child under this AnimState
					if (trigObj == null) {	//	if not found, start from the top
						trigObj = instanceRoot.FindObjectFromChildren(trig.name);
					}
					if (trigObj != null) {
						FSTrigger trigInst = trigObj.GetComponent<FSTrigger>();
						if (trigInst != null) {
							int idx = m_EntryTriggerList.IndexOf(trig);
							m_EntryTriggerList[idx] = trigInst;
						}
						else {
							Rlplog.Debug("FiniteState.AttachToBrothers", "Failed to bind trigger " + trig.name + " to " + instanceRoot.name);
							bAllPrefabsAreBoundToInstances = false;
						}
					}
				}
			}
		}
		foreach(FSTrigger trig in this.m_ExitTriggerList) {
			if (trig != null) {
				GameObject trigObj = this.gameObject.FindObjectExactlyFromChildren(trig.gameObject);	//	find an exact match. If we found an exact match, we don't need to go any further
				if (trigObj == null) {
					trigObj = this.gameObject.FindObjectFromChildren(trig.name);
					if (trigObj == null) {
						trigObj = instanceRoot.FindObjectFromChildren(trig.name);
					}
					if (trigObj != null) {
						FSTrigger trigInst = trigObj.GetComponent<FSTrigger>();
						if (trigInst != null) {
							int idx = m_ExitTriggerList.IndexOf(trig);
							m_ExitTriggerList[idx] = trigInst;
						}
						else {
							Rlplog.Debug("FiniteState.AttachToBrothers", "Failed to bind trigger " + trig.name + " to " + instanceRoot.name);
							bAllPrefabsAreBoundToInstances = false;
						}
					}
				}
			}
		}
		return bAllPrefabsAreBoundToInstances;
	}
	
	public bool BindBrotherInstances()
	{
		bool	bAllPrefabsAreBoundToInstances = false;
		
		//	do a bind of prefabs to instances
		bool bSuccess = FSTrigger.BindTriggerListToInstances(this.gameObject, ref m_EntryTriggerList);
		bAllPrefabsAreBoundToInstances = bSuccess;
		/*
		for(int ii=0; ii<m_EntryTriggerList.Count; ii++) {
			FSTrigger trig = m_EntryTriggerList[ii];
			if (trig != null) {
				GameObject trigObj = this.gameObject.FindObjectExactlyFromChildren(trig.gameObject);	//	find an exact match. If we found an exact match, we don't need to go any further
				if (trigObj == null) {
					FSTrigger trigInstance = trig.gameObject.GetInstance(typeof(FSTrigger)) as FSTrigger;
					if (trigInstance != null) {
						m_EntryTriggerList[ii] = trigInstance;
					}
				}
			}
		}
		*/
		
		bSuccess = FSTrigger.BindTriggerListToInstances(this.gameObject, ref m_ExitTriggerList);
		bAllPrefabsAreBoundToInstances = bSuccess && bAllPrefabsAreBoundToInstances;
		/*
		for(int ii=0; ii<m_ExitTriggerList.Count; ii++) {
			FSTrigger trig = m_ExitTriggerList[ii];
			if (trig != null) {
				GameObject trigObj = this.gameObject.FindObjectExactlyFromChildren(trig.gameObject);	//	find an exact match. If we found an exact match, we don't need to go any further
				if (trigObj == null) {
					FSTrigger trigInstance = trig.gameObject.GetInstance(typeof(FSTrigger)) as FSTrigger;
					if (trigInstance != null) {
						m_ExitTriggerList[ii] = trigInstance;
					}
				}
			}
		}
		*/
		return bAllPrefabsAreBoundToInstances;
	}
	
	public void OnEnable()
	{
		if (m_bStartCalled==true) {
			if (m_bInstancesBound == false) {
				m_bInstancesBound = BindBrotherInstances();		//	this should probably not occur here since we're not ready upon Instantiation to do this anyway.
			}
		}
	}
	
	virtual public void Start()
	{
		/*
		GameObject parentInstance = null;
		if (this.gameObject.transform.parent != null) {
			parentInstance = this.gameObject.transform.parent.gameObject.GetInstance((System.Type)null) as GameObject;
		}
		AttachToBrothers(parentInstance);
		*/
		
		//	this is more accurate and less reliant on naming uniquely
		m_bInstancesBound = BindBrotherInstances();
		m_bStartCalled = true;
	}
	
	static public FiniteState CreateWaitState(float waitTime)
	{
		//	create the gameObject
		GameObject waitStateGO = new GameObject();
		waitStateGO.name = "WaitState";
		FiniteState waitState = waitStateGO.AddComponent<FiniteState>();
		FSTimer timer = waitStateGO.AddComponent<FSTimer>();
		timer.m_EndTime = waitTime;
		
		//	create a trigger to start the timer when entering this state
		GameObject	startTriggerGO = FSTrigger.CreateTrigger(null, "Reset", null);
		FSTrigger 	startTrigger = startTriggerGO.GetComponent<FSTrigger>();
		startTrigger.m_MsgReceiver = waitStateGO;
		waitState.AddEntryTrigger(startTrigger);
		startTriggerGO.transform.parent = waitStateGO.transform;
		
		//	create a trigger to leave this state when the timer reaches its end
		GameObject	endTriggerGO = FSTrigger.CreateTrigger(null, "GotoNextState", null);
		FSTrigger 	endTrigger = endTriggerGO.GetComponent<FSTrigger>();
		endTrigger.m_MsgReceiver = waitStateGO;
		timer.TimerAddExitTrigger(endTrigger);
		endTriggerGO.transform.parent = waitStateGO.transform;

		return waitState;
	}
	
	static public FiniteState CreateRandomState()
	{
		//	create the gameObject
		GameObject randomStateGO = new GameObject();
		randomStateGO.name = "RandomState";
		FiniteState randomState = randomStateGO.AddComponent<FiniteState>();
//		FSRandom random = randomStateGO.AddComponent<FSRandom>(); // removed due to warning (slc)
		randomStateGO.AddComponent<FSRandom>(); // added due to warning (slc)
		
		//	create a trigger to start the timer when entering this state
		GameObject	startTriggerGO = FSTrigger.CreateTrigger(null, "PickNewRandom", null);
		FSTrigger 	startTrigger = startTriggerGO.GetComponent<FSTrigger>();
		startTrigger.m_MsgReceiver = randomStateGO;
		randomState.AddEntryTrigger(startTrigger);
		startTriggerGO.transform.parent = randomStateGO.transform;

		//	create a trigger to immediately leave this state when entering this state!
		GameObject	causeExitTriggerGO = FSTrigger.CreateTrigger(null, "GotoNextState", null);
		FSTrigger 	causeExitTrigger = causeExitTriggerGO.GetComponent<FSTrigger>();
		causeExitTrigger.m_MsgReceiver = randomStateGO;
		randomState.AddEntryTrigger(causeExitTrigger);
		causeExitTriggerGO.transform.parent = randomStateGO.transform;
		
		//	create a trigger when leaving this state to cause the Animation FSM to enter a random state
		GameObject	endTriggerGO = FSTrigger.CreateTrigger(null, "EnterRandomState", null);
		FSTrigger 	endTrigger = endTriggerGO.GetComponent<FSTrigger>();
		endTrigger.m_MsgReceiver = randomStateGO;
		endTriggerGO.transform.parent = randomStateGO.transform;
		randomState.AddExitTrigger(endTrigger);

		return randomState;
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
	/*
	 * Given a final state, a previous transition state, and a initial state, produce a backwards transition from end->backwards->initial
	 *
	 * Example:
	 	* Starting with a Idle->Trans1->Anim1, this will create
	 	* Anim1->BackwardTrans1->Idle 
 	 * startState = Anim1
 	 * finalState = Idle
 	 * transitionState = Anim1
 	 * return = BackwardTrans1
	 */
	static public GameObject CreateTriggers_BackwardTransition(GameObject FSMGO, FiniteState finalState, FiniteState startState, FiniteState transitionState, bool bPlayBackwards)
	{
		GameObject backwardTransitionStateGO;
		
		if (bPlayBackwards) {
			backwardTransitionStateGO = new GameObject("FS Backwards Trans" + transitionState.name);
		}
		else {
			backwardTransitionStateGO = new GameObject("FS Trans" + transitionState.name);
		}
		FiniteState state = backwardTransitionStateGO.AddComponent<FiniteState>();
		
		//	connect into the hierarchy
		backwardTransitionStateGO.transform.parent = startState.gameObject.transform;
		
		//	Add AnimationEvent that triggers at end of backwards playing animation to goto next state
		
		//	add State triggers that play the animation backwards on entry
		GameObject entryTriggerGO = FSTrigger.CreateTrigger(FSMGO, "PlayBackwards", "foo");
		FSTrigger entryTrigger = entryTriggerGO.GetComponent<FSTrigger>();
		state.AddEntryTrigger(entryTrigger);
		entryTrigger.transform.parent = backwardTransitionStateGO.transform;	//	attach to the state as a parent
		
		//	add State triggers that stop the animation when leaving this state
		GameObject exitTriggerGO = FSTrigger.CreateTrigger(FSMGO, "Stop", "foo");
		FSTrigger exitTrigger = exitTriggerGO.GetComponent<FSTrigger>();
		state.AddExitTrigger(exitTrigger);
		exitTrigger.transform.parent = backwardTransitionStateGO.transform;	//	attach to the state as a parent
		
		return backwardTransitionStateGO; 
	}
	
	public GameObject CreateTriggers_IdleLoop(GameObject FSMGO, Animation anim, string playCommand)
	{
		//	create entry and exit triggers and attach them
		GameObject newFiniteStateGO = this.gameObject;
		
		GameObject entryTriggerGO = FSTrigger.CreatePlayAnimTrigger(FSMGO, anim, playCommand);
		FSTrigger entryTrigger = entryTriggerGO.GetComponent<FSTrigger>();
		this.AddEntryTrigger(entryTrigger);
		entryTrigger.transform.parent = newFiniteStateGO.transform;	//	attach to the state as a parent
			
		GameObject exitTriggerGO = FSTrigger.CreateStopAnimTrigger(FSMGO, anim);
		FSTrigger exitTrigger = exitTriggerGO.GetComponent<FSTrigger>();
		this.AddExitTrigger(exitTrigger);
		exitTrigger.transform.parent = newFiniteStateGO.transform;	//	attach to the state as a parent
		
		return newFiniteStateGO;
	}
	
	public void AddEntryTrigger(FSTrigger trig)
	{
		m_EntryTriggerList.Add(trig);
	}
	
	public void AddExitTrigger(FSTrigger trig)
	{
		m_ExitTriggerList.Add(trig);
	}
	
	public void AddConnection(FiniteState fs)
	{
		m_ValidStateList.Add(fs);
	}
	
	/*
	public void AttachButton(Button3D button)
	{
		button.m_MsgReceiver = this.m_MyFSM.gameObject;
		button.m_ScriptMessage = "EnterState";
		button.m_Parameters = new string[1];
		button.m_Parameters[0] = this.name;
	}
	*/
	
	virtual public void EnterThis()
	{
		//	sometimes, we can get here without Start() having been called if a system starts out active==false.
		if (this.m_bStartCalled==false) {
			Start();
		}
		if (this == this.m_MyFSM.GetCurrentState()) {
			//	hack: Test to find re-entrant state situations.
			Rlplog.Trace("FiniteState.EnterThis", "FiniteState " + this.name + " is being entered when it is already the current state. Not firing triggers the second time.");
			return;
		}
		
		this.m_MyFSM.SetCurrentState(this);
		
		foreach(FSTrigger trigger in m_EntryTriggerList) {
			if (trigger == null) {
				Rlplog.Error("FiniteState.EnterThis", "FiniteState " + this.name + " has a NULL Entry Trigger in its list.");
			}
			else {
				trigger.Fire();
			}
		}
#if _HutongGamesPlayMaker
		//	enable PlayMaker FSM if any
		PlayMakerFSM playMaker = this.GetComponent<PlayMakerFSM>();
		if (playMaker != null) {
			playMaker.enabled = true;
		}
#endif	//	#if _HutongGamesPlayMaker

	}
	
	virtual public void LeaveThis()
	{
#if _HutongGamesPlayMaker
		//	disable PlayMaker FSM if any
		PlayMakerFSM playMaker = this.GetComponent<PlayMakerFSM>();
		if (playMaker != null) {
			playMaker.enabled = false;
		}
#endif //	#if _HutongGamesPlayMaker

		foreach(FSTrigger trigger in m_ExitTriggerList) {
			if (trigger == null) {
				Rlplog.Error("FiniteState.LeaveThis", "FiniteState " + this.name + " in " + this.m_MyFSM.name + " has a NULL Exit Trigger in its list.");
			}
			else {
				trigger.Fire();
			}
		}
	}
	
	public bool isValidNextState(FiniteState state)
	{
		bool bIsValid = false;
		/*
		foreach(FSConnection conx in m_ConnectionList) {
			if (conx.m_NextState == state) {
				return true;
			}
		}
		*/
		foreach(FiniteState conx in m_ValidStateList) {
			if (conx == state) {
				return true;
			}
		}
		return bIsValid;
	}
	
	public void GotoNextState()
	{
		FiniteState nextState = this;
		if (m_ValidStateList.Count > 0) {
			nextState = m_ValidStateList[0];
		}
		Rlplog.Trace("FiniteState.GotoNextState()", "nextState=" + nextState);
		GotoAnyState(nextState);
	}
	
	public void GotoAnyState(FiniteState nextState)
	{
		m_MyFSM.EnterState(nextState);
	}
	
	public void GotoValidState(FiniteState nextState)
	{
		if (isValidNextState(nextState)) {
			m_MyFSM.EnterState(nextState);
		}
	}
}


