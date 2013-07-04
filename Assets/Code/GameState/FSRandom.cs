///////////////////////////////////////////////////////////////////////////////
///
/// FSRandom
/// 
/// FSRandom allows some random FiniteState generation
/// 
/// 
///////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
[System.Serializable] // Required so it shows up in the inspector 
[AddComponentMenu ("FSM/FSRandom")]
public class FSRandom : MonoBehaviour
{
	[SerializeField] 	public	List<GameObject>					m_RandomList;
	public				GameObject									m_CurrentRandomPick;
	private				ProbabilityTable							m_ProbabilityTable;
	
	public void Awake()
	{
		Rlplog.Trace("", "FSRandom::Awake() - " + this.transform.name);
		if (m_RandomList==null) {
			m_RandomList = new List<GameObject>();
		}
		m_ProbabilityTable = GetComponent<ProbabilityTable>();
		
		SanityCheck();
	}
	
	public void ClearList()
	{
		m_RandomList.Clear();
	}
	
	public void SanityCheck()
	{
		if (m_ProbabilityTable != null) {
			if (m_ProbabilityTable.m_ProbabilityTable.Count != m_RandomList.Count) {
				Rlplog.Error("FSRandom.SanityCheck", this.name + " RandomList and ProbabilityTable have different number of entries. They should be the same!");
			}
		}
	}
	
	/*
	 * PickNewRandom - pick a random state that is not the current state
	 */
	public GameObject PickNewRandom()
	{
		int randInt = GetRandomIndex();
		if (m_RandomList.Count == 1) {	//	we only have one choice. Go with it.
			randInt = 0;
		}
		else if (m_RandomList.Count > 1) {
			int infiniteLoopCounter = 0;
			GameObject prevRandom = m_CurrentRandomPick;
			while (m_RandomList[randInt] == prevRandom)
			{
				randInt = GetRandomIndex();
				infiniteLoopCounter++;
				if (infiniteLoopCounter > 25) {		//	extremely unlikely to have picked the same thing 25x
					Rlplog.Error("FSRandom.PickNewRandom", this.name + " is unable to randomly select a New Random that is different than " + prevRandom.name);
					m_CurrentRandomPick = m_RandomList[0];
					return m_CurrentRandomPick;
				}
			}
		}
		m_CurrentRandomPick = m_RandomList[randInt];
		return m_CurrentRandomPick;
	}
	
	public int			GetRandomIndex()
	{
		int randInt = 0;
		if (m_ProbabilityTable == null) {
			randInt = Random.Range(0, m_RandomList.Count);	//	for ints, the last is exclusive according to unity docs!
		}
		else {
			randInt = m_ProbabilityTable.GetRandomIndex();
		}
		
		if (randInt >= m_RandomList.Count)		// prevent out of range errors (me)
			randInt = m_RandomList.Count - 1;
		
		return randInt;
	}
	
	/*
	 * PickRandom - pick a random state from list of available states
	 */
	public GameObject PickRandom()
	{
		int randInt = GetRandomIndex();
		
		m_CurrentRandomPick = m_RandomList[randInt];
		return m_CurrentRandomPick;
	}
	
	/*
	 * EnterRandomState - If the random selection is a FiniteState, then enter it.
	 */
	public void EnterRandomState()
	{
		if (m_CurrentRandomPick != null) {
			FiniteState state = m_CurrentRandomPick.GetComponent<FiniteState>();
			if (state != null)
				state.m_MyFSM.EnterState(state);
		}
	}
	
	public void AddRandomObject(GameObject go)
	{
		m_RandomList.Add(go);	//	duplicates are allowed
	}
	
}

