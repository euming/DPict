// ======================================================================================
// File         : ProbabilityTable.cs
// Author       : Eu-Ming Lee 
// Changelist   :
//	02/06/2013 - First creation
// Description  : 
//	This adds a weighted probability table to a FSRandom class so that random things can
//	be picked more or less often with different weighting
//	
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
[System.Serializable] // Required so it shows up in the inspector 
[AddComponentMenu ("FSM/ProbabilityTable")]
public class ProbabilityTable : MonoBehaviour
{
	
	public	List<float>				m_ProbabilityTable = new List<float>();
	private	float					m_TotalSum;
	private	float					m_LastTotalSum;
	private bool					m_bNormalized = false;
	
	public void Awake()
	{
		//	autobuild
		AutoBuildTable();
		
		m_TotalSum = GetTotalSum();
		m_bNormalized = false;
	}
	
	public void AutoBuildTable()
	{
		if (m_ProbabilityTable.Count == 0) {
			FSRandom fsRand = GetComponent<FSRandom>();
			if (fsRand != null) {
				for(int ii=0; ii<fsRand.m_RandomList.Count; ii++) {
					m_ProbabilityTable.Add(1.0f);
				}
			}
		}
	}
	public int	GetRandomIndex()
	{
		float randFlt = Random.Range(0.0f, m_TotalSum);
		float curSum = 0.0f;
		for(int ii=0; ii<m_ProbabilityTable.Count; ii++) {
			curSum += m_ProbabilityTable[ii];
			if (randFlt <= curSum) {
				return ii;
			}
		}
		return m_ProbabilityTable.Count-1;
	}
	
	public float GetTotalSum()
	{
		float totalSum = 0;
		
		for(int ii=0; ii<m_ProbabilityTable.Count; ii++) {
			totalSum += m_ProbabilityTable[ii];
		}
		return totalSum;
	}
	
	public void Normalize()
	{
		if (m_bNormalized == false) {
			float totalSum = GetTotalSum();
			
			for(int ii=0; ii<m_ProbabilityTable.Count; ii++) {
				m_ProbabilityTable[ii] = m_ProbabilityTable[ii] / totalSum;
			}
			m_LastTotalSum = totalSum;
			m_TotalSum = GetTotalSum();
			m_bNormalized = true;
		}
	}
	
	public void Unnormalize()
	{
		if (m_bNormalized == true) {
			for(int ii=0; ii<m_ProbabilityTable.Count; ii++) {
				m_ProbabilityTable[ii] = m_ProbabilityTable[ii] * m_LastTotalSum;
			}
			m_bNormalized = false;
		}
	}
}