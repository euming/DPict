// ======================================================================================
// File         : ReferenceHolder.cs
// Author       : Eu-Ming Lee 
// Changelist   :
//	10/3/2012
// Description  : 
//	This is way to hold Prefabs in a nested Prefab hierarchy without putting them directly
//	into a Prefab. By doing it this way, Prefabs hierarchies can be built from existing Prefabs
//	without worrying about messing up the Prefab holder.
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using CustomExtensions;

//[ExecuteInEditMode]
[System.Serializable] // Required so it shows up in the inspector 
[AddComponentMenu ("Prefabs/ReferenceHolder")]
[LitJson.ExportType(LitJson.ExportType.NoExport)]	//	use this to prevent specific fields from being exported by LitJson library
public class ReferenceHolder : MonoBehaviour
{
	[LitJson.ExportType(LitJson.ExportType.NoExport)]	//	TBA: should be LitJson.ExportType.Reference, but it's not currently supporting lists of References :-(
	[SerializeField] 	public	List<GameObject>	m_ReferenceList;
	private						List<GameObject>	m_InstanceList = null;
	
	public void Awake()
	{
		if (this.enabled == true) {	//	reference holder should only create the instancelist once. After that, if we are instantiated, the instantiation process will automatically clone our children. Leaving this enabled would create unnecessary duplicates.
			InstantiateReferences();
			this.enabled = false;
		}		
	}
	
	public void InstantiateReferences()
	{
		m_InstanceList = new List<GameObject>();
		foreach(GameObject go in m_ReferenceList) {
			if (go == null) {
				Rlplog.Error("ReferenceHolder.InstantiateReferences", "null entry in " + this.name);
			}
			else {
				GameObject childInstance = Instantiate(go) as GameObject;	//	Awake() is called on the childInstance here
				//	use the same name as the reference
				childInstance.name = go.name;
				
				//	attach this to the appropriate place in the hierarchy
				childInstance.transform.parent = this.transform;
				
				//	local transforms are copies of the original prefab's transforms so that we can bake offsets into the prefabs
				childInstance.transform.localPosition = go.transform.localPosition;
				childInstance.transform.localScale = go.transform.localScale;
				childInstance.transform.localRotation = go.transform.localRotation;
				
				m_InstanceList.Add(childInstance);
				
				//	call Awake again after connections have been made. Note that
				//	Awake() is called twice, once without the hierarchy attached and once afterwards.
				//	please make sure that our script can handle 2 calls to Awake().
				childInstance.AwakeAllScripts();
				
				/*
				MonoBehaviour[] scripts;
				scripts = childInstance.GetComponents<MonoBehaviour>();
				foreach(MonoBehaviour script in scripts) {
					if (script.enabled==true) {	//	this prevents ReferenceHolder from making recursive duplicates
						string methodName = "Awake";
						//script.Invoke("Awake", 0.0f);
						var componentType = script.GetType();
						if (componentType.GetMethod(methodName) != null) {
							script.Invoke(methodName, 0.0f);
							//componentType.InvokeMember(methodName, BindingFlags.Default | BindingFlags.InvokeMethod | BindingFlags.Instance, null, script, null);
						}
					}
				}
				*/
			}
		}
	}
	
	public GameObject GetInstance(string name)
	{
		GameObject result = null;
		
		if (m_InstanceList != null)	//	sometimes, we can exist, but not have been Awakened, and therefore not have any instances.
			result = m_InstanceList.Find(o => o.name == name);
		return result;
	}
	
	public GameObject GetInstanceFromChildren(string instanceName)
	{
		GameObject result = GetInstance(instanceName);
		
		if (result)
			return result;	//	early bail. We found
		
		//	search
		if (m_InstanceList != null) {
			foreach(GameObject childInst in m_InstanceList) {		//	this is the instance list
				ReferenceHolder rh = childInst.GetComponent<ReferenceHolder>();
				if (rh) {
					result = childInst.GetInstanceFromChildren(instanceName);
					if (result != null) 		//	bail. we found
						return result;
				}
				else {
					//	perhaps the child we're looking for was instantiated underneath the prefab hierarchy that was instantiated as childInst
					result = childInst.FindObjectFromChildren(instanceName);
					if (result != null) return result;	//	bail. we found
				}
			}
		}
					
		return result;
	}
	
	//	having this allows the inspector to enable/disable this component with a check mark
	void Start()
	{
	}
	
	
}