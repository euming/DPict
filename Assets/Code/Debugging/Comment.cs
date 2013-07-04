// ======================================================================================
// File         : Comment.cs
// Author       : Eu-Ming Lee 
// Changelist   :
//	8/22/2012 - First creation
// Description  : 
//	This allows people to attach comments to various GameObjects
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//[ExecuteInEditMode]
[System.Serializable] // Required so it shows up in the inspector 
[AddComponentMenu ("Debugging/Comment")]
public class Comment : MonoBehaviour
{
	public string 						m_comment;
}