///////////////////////////////////////////////////////////////////////////////
///
/// FSTimer
/// 
/// FSTimer is a Timer for Finite States. This allows wait states and triggers
/// and such
/// 
///////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections.Generic;
/*
Another subtle note on dependency on FiniteState:

You might wonder why the default doesn't turn itself off when it reaches the end of the timer?

The reason is because usually the FSTimer is also attached to a FiniteState. FiniteStates also set enabled=false on the GameObject when the FiniteStateMachine is not in that State. This prevents the Update() method from wasting cycles on States that are not active. Thus, when a FSTimer that is attached to a FiniteState leaves that state due to the timer running out, it is automatically deactivated when the GameObject is deactivated automatically by the action of changing states.

Did that make sense? Basically, FSTimer is usually attached to a FiniteState which automatically turns itself on/off when appropriate, thus allowing FSTimer to piggyback it's enabled/disabled status from the same GameObject.
*/

[ExecuteInEditMode]
[System.Serializable] // Required so it shows up in the inspector 
[AddComponentMenu ("FSM/FSTimer")]
public class FSTimer : MonoBehaviour
{
	private	float							m_StartTime;
	public	float							m_ElapsedTime;
	[SerializeField] 	public	List<FSTrigger>					m_EntryTriggerList = new List<FSTrigger>();
	[SerializeField] 	public	List<FSTrigger>					m_ExitTriggerList = new List<FSTrigger>();
	[SerializeField]	public	float							m_EndTime;
	
	[SerializeField]	private bool							m_TimerLocked = false;
	private bool	m_bPaused = false;		//	if we want to pause the timer
	
	virtual public void Awake()
	{
		Rlplog.Trace("", "FSTimer::Awake() - " + this.transform.name);
		/*
		if (m_EntryTriggerList == null) {
			m_EntryTriggerList = new List<FSTrigger>();
		}
		if (m_ExitTriggerList == null) {
			m_ExitTriggerList = new List<FSTrigger>();
		}
		*/
		m_StartTime = Time.time;
	}
	
	public void Start()
	{
		bool bSuccess = FSTrigger.BindTriggerListToInstances(this.gameObject, ref m_EntryTriggerList);
		bSuccess = FSTrigger.BindTriggerListToInstances(this.gameObject, ref m_ExitTriggerList);
	}
	
	public void RefreshTimer()
	{
		m_StartTime = Time.time;
	}
	
	virtual public void OnEnable()
	{
		Reset();
		foreach(FSTrigger trigger in m_EntryTriggerList) {
			trigger.Fire();
		}
	}
	
	public void Pause()
	{
		m_bPaused = true;
	}
	
	public void Unpause()
	{
		m_bPaused = false;
	}
	
	public void OnTimeReached()
	{
		foreach(FSTrigger trigger in m_ExitTriggerList) {
			trigger.Fire();
		}
		this.enabled = false;	//	disable myself so I don't fire more triggers next frame
	}
	
	virtual public void Reset()
	{
		m_StartTime = Time.time;
		m_ElapsedTime = 0.0f;
	}
	
	virtual public void Update()
	{
		if (this.enabled == true && !m_TimerLocked) {
			if (!m_bPaused) {
				m_ElapsedTime = Time.time - m_StartTime;
			}
			else {
				m_StartTime = Time.time - m_ElapsedTime;
			}
			
			if (m_ElapsedTime >= m_EndTime) {
				OnTimeReached();
			}
		}
	}

	public void TimerAddEntryTrigger(FSTrigger trig)
	{
		m_EntryTriggerList.Add(trig);
	}
	
	public void TimerAddExitTrigger(FSTrigger trig)
	{
		m_ExitTriggerList.Add(trig);
	}
	
	public void LockTimer(bool newLockedState)
	{
		//Reset timer if unlocking
		if(!newLockedState)
			Reset ();
		
		m_TimerLocked = newLockedState;
		
	}
}
