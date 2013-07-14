// ======================================================================================
// File         : CommentError.cs
// Author       : Eu-Ming Lee 
// Changelist   :
//	10/23/2012 - First creation
// Description  : 
//	This allows a quarantine of errors so that we can visit them later and trace them back
//	to their origins in real-time
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CustomExtensions;

//[ExecuteInEditMode]
[System.Serializable] // Required so it shows up in the inspector 
public class CommentError : Comment
{
	public GameObject					m_OriginalParent;
	public List<GameObject>				m_OtherReferences = new List<GameObject>();
	
	const string QuarantineErrorName = "Errors";
	
	static public CommentError AddError(GameObject parentGO, GameObject errorGO, string errmsg)
	{
		GameObject errorChild = parentGO.FindObject(QuarantineErrorName);
		if (errorChild == null) {
			errorChild = new GameObject(QuarantineErrorName);
			errorChild.transform.parent = parentGO.transform;	//	attach it to the hierarchy
		}
		
		CommentError commentError = errorGO.AddComponent<CommentError>();
		if (errorChild.transform.parent != null) {
			commentError.m_OriginalParent = errorChild.transform.parent.gameObject;
		}
		else {
			commentError.m_OriginalParent = null;
		}
		errorGO.transform.parent = errorChild.transform;
		commentError.m_comment = errmsg;
		return commentError;
	}
	
	public void AddReference(GameObject go)
	{
		m_OtherReferences.Add(go);
	}
}