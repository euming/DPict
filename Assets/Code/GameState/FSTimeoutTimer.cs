using UnityEngine;
using System.Collections;

public class FSTimeoutTimer : MonoBehaviour
{
	private FiniteStateMachine m_FSM = null;
	
	private FiniteState m_CurrentState = null;
	
	public FiniteState m_TimedState = null;
	public FiniteState m_TimeoutTarget = null;
	
	public float m_TimeoutDelay = 10.0f;
	[SerializeField] private float m_Timer = 0;

	// Use this for initialization
	void Start ()
	{
		if (m_FSM == null)
			m_FSM = GetComponent<FiniteStateMachine>();
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (m_FSM == null || m_TimedState == null || m_TimeoutTarget == null)
			return;
		
		if (m_FSM.GetCurrentState() == m_TimedState)
		{
			if (m_CurrentState == null || m_CurrentState != m_FSM.GetCurrentState())
			{
				m_CurrentState = m_FSM.GetCurrentState();
				m_Timer = m_TimeoutDelay;
			}
			
			m_Timer -= Time.deltaTime;
			if (m_Timer <= 0)
			{
				m_FSM.EnterState(m_TimeoutTarget);
				m_CurrentState = m_TimeoutTarget;
			}
		}
		
		if (m_FSM.GetCurrentState() != m_TimedState)
		{
			m_Timer = m_TimeoutDelay;
			m_CurrentState = m_FSM.GetCurrentState();
		}
	}
}
