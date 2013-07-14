
// ======================================================================================
// File         : ExtensionMethods.cs
// Author       : Eu-Ming Lee 
// Changelist   :
//	11/7/2011 - First creation
// Description  : 
//	These are methods that extend existing Unity classes that are intended to be used
//	for the Editor only.
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using CustomEditorExtensions;
using System.Reflection;
using CustomExtensions;

namespace CustomEditorExtensions
{
	public static class MonoBehaviourEditorExtension
	{
		
		/*
		 * SetThisAsDefault - create an Asset with the various parameters of this class as a default. When creating a new class
		 * of this type, these defaults will be used. This lets the user customize their development environment to their workflow.
		 */
		/*
		public static void SetThisAsDefault( this MonoBehaviour script )
		{
			string classname = script.GetType().ToString();
			//AssetDatabase.CreateFolder(kRootDir, kDefaultSubdir);	//	TBA: must check to see if folder exists. How do to this?
			string filepath = MonoBehaviourExtension.GetDefaultAssetsSubdirAsPrefab(classname);	//	users must manually move Defaults over to the System directory from the User directory
//			bool bDeleteSuccess = AssetDatabase.DeleteAsset(filepath); // removed due to warning (slc)
			AssetDatabase.DeleteAsset(filepath); // added due to warning (slc)
			GameObject go = script.gameObject;
			Object prefab = PrefabUtility.CreateEmptyPrefab(filepath);
			PrefabUtility.ReplacePrefab(go, prefab);
		}
		*/
		/*
		//	made obsolete by LoadDefault() in the run-time MonoBehaviourExtension
		public static GameObject LoadDefault( string classname )
		{
			Object defaultObject = AssetDatabase.LoadAssetAtPath(GetDefaultAssetsSubdir(classname), typeof(Object));
			GameObject go = defaultObject as GameObject;
			if (!go) {
				go = (GameObject)AssetDatabase.LoadAssetAtPath(GetDefaultAssetsSubdir(classname), typeof(GameObject));
			}
			return go;
		}		
		*/		
	}
	
	public static class ScriptableObjectExtension
	{
		public delegate UnityEngine.Object UnityObjectWithParamFcn(UnityEngine.Object obj, UnityEngine.Object param);		//	delegate definition (i.e. function pointer to you C++ people)
		public delegate UnityEngine.Object UnityObjectFcn(UnityEngine.Object obj);		//	delegate definition (i.e. function pointer to you C++ people)
		
		//	Use this MenuLoop as a standard for creating new GameObjects, Assets, etc.
		//	It will automatically select the newly created objects. Having this consistency helps the artists.
		public static UnityEngine.Object[] DoCreateObjectWithParamLoop(UnityObjectWithParamFcn fcn, UnityEngine.Object param)
		{
			UnityEngine.Object[]		objects = Selection.objects;
			//	make the newlist be selected
			UnityEngine.Object[]		newList = new UnityEngine.Object[objects.Length];
			UnityEngine.Object			newObject;
			int				nNewObjects = 0;
			
	        foreach(UnityEngine.Object obj in objects)
	        {
				newObject = fcn(obj, param);
				//	add to the list of newly created objects
				if (newObject) {
					newList[nNewObjects++] = newObject;
				}
	        }
			
			//	if our new list is shorter than the original, we should chop it up
			if (nNewObjects < objects.Length) {
				//	null out remaining entries
				for(int ii=nNewObjects; ii<objects.Length; ii++) {
					newList[ii] = null;
				}
				System.Array.Resize(ref newList, nNewObjects);
			}			
			//	make the newlist be selected
			Selection.objects = newList;
			UnityEditor.Selection.objects = newList;
			return newList;
		}
		
		//	same as above, but without a parameter
		public static UnityEngine.Object[] DoCreateObjectLoop(UnityObjectFcn fcn)
		{
			UnityEngine.Object[]		objects = Selection.objects;
			//	make the newlist be selected
			UnityEngine.Object[]		newList = new UnityEngine.Object[objects.Length];
			UnityEngine.Object			newObject;
			int				nNewObjects = 0;
			
	        foreach(UnityEngine.Object obj in objects)
	        {
				newObject = fcn(obj);
				//	add to the list of newly created objects
				if (newObject) {
					newList[nNewObjects++] = newObject;
				}
	        }
			
			//	if our new list is shorter than the original, we should chop it up
			if (nNewObjects < objects.Length) {
				//	null out remaining entries
				for(int ii=nNewObjects; ii<objects.Length; ii++) {
					newList[ii] = null;
				}
				System.Array.Resize(ref newList, nNewObjects);
			}			
			//	make the newlist be selected
			Selection.objects = newList;
			UnityEditor.Selection.objects = newList;
			return newList;
		}
	}
	

	
	public static class GameObjectEditorExtension
	{
		/*
		public static GameObject GetDefaultGameObject(this GameObject go, System.Type componentType)
		{
			GameObject defaultGameObject = null;
			var component = go.GetComponent(componentType.ToString());
			MonoBehaviour mb = component as MonoBehaviour;
			if (mb) {
				if (defaultGameObject == null) {
					defaultGameObject = mb.LoadDefault() as GameObject;	//	load the default prefab for this component
				}
			}
			return defaultGameObject;
		}
		
		public static void SetThisAsDefaultAsset(this GameObject go, System.Type componentType)
		{
			GameObject defaultGO = go.GetDefaultGameObject(componentType);
			SetThisAsDefaultAsset(defaultGO);
		}
		
		public static GameObject GetDefaultAsset(this GameObject go, System.Type componentType)
		{
			GameObject defaultGO = go.GetDefaultGameObject(componentType);
			return defaultGO;
		}
		
		public static GameObject GetDefaultAsset(MonoBehaviour mb)
		{
			GameObject defaultGameObject = null;
			if (mb) {
				if (defaultGameObject == null) {
					defaultGameObject = mb.LoadDefault() as GameObject;	//	load the default prefab for this component
				}
			}			
			return defaultGameObject;
		}
		*/
		/*
		 * This allows us to use the current GameObject's component(s) as the default component(s) for the next time the object is created.
		 */
		/*
		public static void SetThisAsDefaultAsset(this GameObject go)
		{
			MonoBehaviour[] scripts;
			scripts = go.GetComponents<MonoBehaviour>();
			foreach (MonoBehaviour script in scripts)
			{
				string classname;
				classname = script.GetType().ToString();
				bool bSetDefault = EditorUtility.DisplayDialog(classname + ": Default", "Are you sure you want to set the defaults for " + classname + "?", "OK", "Cancel");
				if (bSetDefault) {
					script.SetThisAsDefault();
				}
			}
		}
		 */
		/*
		 * Given a gameObject and a component type, attempt to set the component of that gameObject to the defaults of that component
		 * as defined in the Assets/Defaults directory.
		 * Also sets the GameObject defaults.
		 */
		//	this is now in the run-time GameObject extensions
		/*
		public static void SetDefaultLikeComponent(this GameObject gameObject, GameObject defaultGameObject, System.Type componentType)
		{
			CustomExtensions.GameObjectExtension.SetDefaultLikeComponent(gameObject, defaultGameObject, componentType);
		}
		public static GameObject SetDefaultLike( this GameObject thisGO, System.Type componentType )
		{
			GameObject go = CustomExtensions.GameObjectExtension.SetDefaultLike(thisGO, componentType);
			return go;
		}
		*/		
		/*
		public static void SetDefaultLikeComponent(this GameObject gameObject, GameObject defaultGameObject, System.Type componentType)
		{
			var component = gameObject.GetComponent(componentType.ToString());
			MonoBehaviour mb = component as MonoBehaviour;
			if (mb) {
				if (defaultGameObject == null) {
					defaultGameObject = mb.LoadDefault() as GameObject;	//	load the default prefab for this component
				}
				if (defaultGameObject) {
					
					//	now set the component defaults
					Object[] args = new Object[1];
					
					args[0] = defaultGameObject;
					componentType.InvokeMember("SetDefaultLike", BindingFlags.Default | BindingFlags.InvokeMethod | BindingFlags.Instance, null, component, args);
				}
			}
		}
		
		public static GameObject SetDefaultLike( this GameObject thisGO, System.Type componentType )
		{
			GameObject defaultGO = thisGO.GetDefaultGameObject(componentType);
			
			if (!defaultGO) return null;	//	fail bail
			
			thisGO.SetDefaultLike(defaultGO);
			
			//	for each component on the defaultGO, set to those defaults
			MonoBehaviour[] scripts;
			scripts = defaultGO.GetComponents<MonoBehaviour>();
			foreach (MonoBehaviour script in scripts)
			{
				thisGO.SetDefaultLikeComponent(defaultGO, script.GetType());
			}
			
			return defaultGO;
		}
		*/

	}
}
