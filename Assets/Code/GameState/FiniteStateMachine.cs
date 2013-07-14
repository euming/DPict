
// ======================================================================================
// File         : FiniteStatemachine.cs
// Author       : Eu-Ming Lee 
// Changelist   :
//	12/6/2011 - First creation
// Description  : 
//	FSM stands for Finite State Machine, not Flying Spaghetti Monster.
//	This just means that you can only be in one state at a time, called the current state.
//	Creating individual states and relating them to each other via connections is how you get things to
//	happen in the game. You can only enter or leave states via these connections in a transition.
//	Entering and leaving states via transitions allows you to trigger various messages. 
//	Triggering various messages makes an infinite variety of other things work.
// ======================================================================================

using UnityEngine;
using System.Collections.Generic;
using CustomExtensions;

///////////////////////////////////////////////////////////////////////////////
///
/// FiniteStateMachine
/// 
///////////////////////////////////////////////////////////////////////////////
//[ExecuteInEditMode]
[System.Serializable] // Required so it shows up in the inspector 
[AddComponentMenu ("FSM/FiniteStateMachine")]
public class FiniteStateMachine : MonoBehaviour
{
	[SerializeField]	public FiniteState					m_InitialState;
	[SerializeField]	public List<FiniteState>			m_ValidStateList;
	[SerializeField]	public List<GameObject>				m_DependencyList = new List<GameObject>();		//	Activate/Deactivate these things when this FSM is activated/deactivated
	
	public FiniteState					m_LastState;
	[SerializeField] private FiniteState					m_CurrentState;
	public List<FiniteState>			m_PreviousStateList;		//	makes debugging easier
	public FiniteStateMachine			m_ParentFSM;
	public bool							m_isDebugging;
	private bool						m_wasStart;
	private Publisher					m_Publisher = null;
//	private bool						m_bInitialEnabledState; // removed due to warning (slc)
	
	public virtual void Awake()
	{
//		m_bInitialEnabledState = enabled; // removed due to warning (slc)
		m_wasStart = false;
		Rlplog.Trace("FiniteStateMachine.Awake", this.name);
		if (m_ValidStateList == null) {
			m_ValidStateList = new List<FiniteState>();
		}
		if (Application.isEditor==true) {
			m_ValidStateList.Clear();
			foreach(Transform child in transform) {
				FiniteState childState = child.GetComponent<FiniteState>();
				if (childState != null) {
					m_ValidStateList.Add(childState);
				}
			}
		}
		
		if (m_PreviousStateList == null) {
			m_PreviousStateList = new List<FiniteState>();
		}

		if ((Application.isEditor==true) && (m_DependencyList.Count==0)) {//	only if this is the first time creating this thing
			foreach(Transform child in transform) {
				FiniteState childState = child.GetComponent<FiniteState>();
				if (childState != null) {
					m_DependencyList.Add(childState.gameObject);
				}
			}
		}
		m_Publisher = GetComponent<Publisher>();
		
		CleanupNulls();

		m_CurrentState = null;
	}
	
	public void CleanupNulls()
	{
		for(int ii=m_DependencyList.Count-1; ii>=0; ii--) {
			if (m_DependencyList[ii] == null) {
				m_DependencyList.RemoveAt(ii);
			}
		}
	}
	
	public void SetInitialState(FiniteState fs)
	{
		if (bIsValidState(fs))
			m_InitialState = fs;
	}
	
	public FiniteState GetInitialState()
	{
		return m_InitialState;
	}
	
	public virtual void Start()
	{
		Rlplog.Trace("FiniteStateMachine.Start", this.name);

		if (m_InitialState != null) {
			if (m_CurrentState != m_InitialState)
				EnterState(m_InitialState);
		}
		
		m_wasStart = true;
	}
	
	public virtual void OnEnable()
	{
		Rlplog.Trace("FiniteStateMachine.OnEnable", this.name);
		
		foreach(GameObject go in m_DependencyList) {
			if (go != null) {
				go.ActivateRecursively(null);
			}
		}
		//this.Activate();
		
		if (m_wasStart) {	//	sometimes, there may be an attempt to enable this before all dependencies have been enabled. Ignore this request.
			if (m_InitialState != null) {
				if (m_CurrentState != m_InitialState)
					EnterState(m_InitialState);
			}
		}
	}
	
	public void LeaveCurrentState()
	{
		if (m_CurrentState) {
			m_CurrentState.LeaveThis();
			m_LastState = m_CurrentState;
			//	subtle bug: In between states, we need to set the current state to null otherwise entry/exit triggers may erroneously call the current_state again!
			m_CurrentState = null;
		}
	}
	
	//	this comes before OnDisable and OnDestroy
	public void OnApplicationQuit()
	{
		this.OnDisable();	//	Same thing as OnDisable, but we must remove our dependencies since they may be destroyed after this
		m_DependencyList.Clear();	
	}
	
	public virtual void OnDisable()
	{
		Rlplog.Trace("FiniteStateMachine.OnDisable", this.name);
		LeaveCurrentState();
		foreach(GameObject go in m_DependencyList) {
			if (go != null) {		//	destruction of objects when closing a scene may cause go to be null
				go.DeactivateRecursively(null);
			}
		}
		//this.Deactivate();
	}
	
	//	unfortunately, this comes after OnDisable which means some things in my dependency list may have been destroyed already
	public void OnDestroy()
	{
		this.OnDisable();	//	Same thing as OnDisable, but we must remove our dependencies since they may be destroyed after this
		m_DependencyList.Clear();	
	}
	
	public void AddState(FiniteState newState)
	{
		FiniteState foundState = null;
		
		if (m_ValidStateList != null) {
			foundState = m_ValidStateList.Find(delegate(FiniteState state) {return state == newState ? state : null;});
		}
		if (foundState == null) {
			m_ValidStateList.Add(newState);
			newState.m_MyFSM = this;
			newState.gameObject.transform.parent = this.transform;
		}
	}
	
	public bool bIsValidState(FiniteState state)
	{
		bool bIsValidState = false;
		if (state.m_MyFSM != this) {
			Debug.LogError("FSM '" + this.name + "' entering invalid FiniteState '" + state.name + "'\n");
		}
		else {
			bIsValidState = true;
		}
		return bIsValidState;
	}
	
	public FiniteState GetCurrentState()
	{
		return m_CurrentState;
	}
	
	public void SetCurrentState(FiniteState state)
	{
		m_CurrentState = state;
		if (m_Publisher != null) {
			if (state != null) {
				m_Publisher.SendSubscriberMessage("SetCurrentState", this.name, state.name);
			}
			else {
				m_Publisher.SendSubscriberMessage("SetCurrentState", this.name, "null");
			}
		}
	}
	
	public void EnterState(FiniteState state)
	{
		LeaveCurrentState();
		
		if (state != null) {
			state.EnterThis();
			if (m_ParentFSM != null) {
				SetCurrentState(state);	//	this may override the current state since sometimes state could be from a different FSM than this one if our parentFSM is non-null
				m_ParentFSM.SetCurrentState(state);	//	this may seem redundant with the above, but it's not because we may be entering a state that is NOT within our own FSM because of animation transitions. This may have other side-effects for non-animation FiniteStates, so beware here.
			}
			if (state.m_MyFSM.m_ParentFSM == this) {
				this.SetCurrentState(state);
			}
			if (m_isDebugging) {
				m_PreviousStateList.Add(state);
			}
		}
		else {
			SetCurrentState(null);
		}
	}
	
	public FiniteState FindState(string statename)
	{
		FiniteState foundState = m_ValidStateList.Find(delegate(FiniteState state) {return state.name == statename ? state : null;});
		return foundState;
	}
	
	

	public void EnterState(string statename)
	{
		FiniteState foundState = FindState(statename);
		if (foundState) {
			EnterState(foundState);
		}
		else {
			Debug.LogError("FSM '" + this.name + "' could not find FiniteState '" + statename + "'\n");
		}
	}
	
	
	public void EnterState(string[] parameters)
	{
		EnterState(parameters[0]);
	}
	
	//convenience function, does not allow for repeated state entry
	public void EnterStateUnique(string statename)
	{
		if(statename != m_CurrentState.name){	
			FiniteState foundState = FindState(statename);
			if (foundState) {
				EnterState(foundState);
			}
			else {
				Debug.LogError("FSM '" + this.name + "' could not find FiniteState '" + statename + "'\n");
			}
		}	
	}
	
	public void EnterStateUnique(string[] parameters)
	{
		EnterStateUnique(parameters[0]);	
	}
	
	
	public void Toggle()
	{
		/*
		//	take this out. It may be dangerous and unnecessary. FiniteState.m_myFSM==null was the cause of the bug here.
		if (m_wasStart == false) {	//	sometimes, due to timing, something may fail. We can try again here to start.
			Start();
		}
		*/
		if (m_CurrentState != null) {
			m_CurrentState.GotoNextState();
		}
	}
	
	public void NextState()
	{
		FiniteState chosenState = null;
		if (m_CurrentState == null) {
			chosenState = m_ValidStateList[0];
		}
		else {
			int idx = m_ValidStateList.FindIndex(o => (o==m_CurrentState));
			if (idx >= 0) {
				if (idx + 1 < m_ValidStateList.Count) {
					chosenState = m_ValidStateList[idx+1];
				}
			}
		}
		EnterState(chosenState);
	}
	
	//	script methods
	public void Activate()
	{
		this.enabled = true;
	}		
	public void Deactivate()
	{
		this.enabled = false;
	}
	
	public void ActivateRecursively()
	{
		Activate();
	}		
	public void DeactivateRecursively()
	{
		Deactivate();
	}
	
	/*
	public void OnApplicationQuit()
	{
		this.enabled = m_bInitialEnabledState;
	}
	*/
}

