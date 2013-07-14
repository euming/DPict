// ======================================================================================
// File         : HashCache.cs
// Author       : Eu-Ming Lee 
// Changelist   :
//	10/23/2012 - First creation
// Description  : 
//	HashCache - allows quick search of objects by name
// ======================================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CustomExtensions;

///////////////////////////////////////////////////////////////////////////////
///
/// AnimState
/// 
///////////////////////////////////////////////////////////////////////////////
//[ExecuteInEditMode]
//[System.Serializable] // Required so it shows up in the inspector 
//[AddComponentMenu ("Search/HashCache")]
public class HashCache : MonoBehaviour
{
	private Hashtable		m_HashTable = new Hashtable();
	
	public void AddChildrenOfType(GameObject parentObject, string componentTypeName)
	{
		foreach(Transform child in parentObject.transform) {
			var comp = child.GetComponent(componentTypeName);
			if (comp != null) {
				Add(child.name, comp);
			}
		}
	}
	
	public void Add(string name, System.Object obj)
	{
		var foundIt = Find(name);
		if (foundIt != null) {
			Rlplog.Error("HashCache.Add", name + ": Duplicate hash key. Overwriting old key.");
			m_HashTable.Remove(name);
		}
		m_HashTable.Add(name, obj);
	}
	
	public void Remove(string name, System.Object obj)
	{
		var foundIt = Find(name);
		if (foundIt == null) {
			Rlplog.Trace("HashCache.Remove", name + ": Hash key not found.");
		}
		else {
			m_HashTable.Remove(name);
		}
	}
	
	public System.Object Find(string name)
	{
		var foundIt = m_HashTable[name];
		return foundIt;
	}
	
	static public HashCache GetCache(GameObject cacheGO)
	{
		HashCache cache = cacheGO.GetComponent<HashCache>();
		if (cache == null) {
			cache = cacheGO.AddComponent<HashCache>();
		}
		return cache;
	}
}