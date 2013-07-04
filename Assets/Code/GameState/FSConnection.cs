
///////////////////////////////////////////////////////////////////////////////
///
/// Connection - this is one way only. If you want a two way connection, make two of them!
/// 
///////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
[System.Serializable] // Required so it shows up in the inspector 
[AddComponentMenu ("FSM/Connection")]
public class FSConnection : MonoBehaviour
{
	public FiniteState			m_NextState;
}

