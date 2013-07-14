
// ======================================================================================
// File         : GameObjectExtensionMethods.cs
// Author       : Eu-Ming Lee 
// Changelist   :
//	12/6/2011 - First creation
// Description  : 
//	These are methods that extend existing Unity classes that are intended to be used
//	on the live client
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using UnityEngine;
using CustomExtensions;
using LitJson;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;


namespace CustomExtensions
{

	public static class MonoBehaviourExtension
	{
		public const string kRootDir = "Assets/Resources";
		const string kDefaultSystemSubdir = "Defaults/System";
		const string kDefaultUserSubdir = "Defaults/User";
		const string kSuffix = ".prefab";
		
		//	use reflection to save out the values in this script
		public static void Save(this MonoBehaviour script, JsonWriter writer)
		{
			string classname;
			classname = script.GetType().ToString();
			Rlplog.Debug("MonoBehaviourExtension.Save", "parent="+script.name+", class=" + classname);
			writer.WriteObjectStart();
				writer.WritePropertyName(classname);
				writer.Write(script as object);
    		writer.WriteObjectEnd();

			//JsonMapper.ToJson(script, writer);
		}
		
		//	use reflection to save out the values in this script
		public static void SaveReference(this MonoBehaviour script, JsonWriter writer)
		{
			string classname;
			classname = script.GetType().ToString();
			writer.WriteObjectStart();
				writer.WritePropertyName("reference: " + classname);
				writer.Write(script as object);
    		writer.WriteObjectEnd();

			//JsonMapper.ToJson(script, writer);
		}

		public static void ActivateRecursively(this MonoBehaviour script)
		{
			GameObject go = script.gameObject;
			go.SetActiveRecursively(true);
		}		
		public static void DeactivateRecursively(this MonoBehaviour script)
		{
			GameObject go = script.gameObject;
			go.SetActiveRecursively(false);
		}		
		
		public static void Activate(this MonoBehaviour script)
		{
			GameObject go = script.gameObject;
			go.active = true;
		}		
		public static void Deactivate(this MonoBehaviour script)
		{
			GameObject go = script.gameObject;
			go.active = false;
		}
		public static void AwakeThis(this MonoBehaviour script) 
		{
			script.Invoke("Awake", 0.0f);
		}
		//	don't use this. just do it directly if we have access to the variable!
		/*
		public static void Enable(this MonoBehaviour script)
		{
			script.enabled = true;
		}		
		public static void Disable(this MonoBehaviour script)
		{
			script.enabled = false;
		}
		*/
		
		/*
		 * This queries UserPreferences for a specific User Directory. If no user preferences are found, then the system
		 * preferences above take over.
		 */		
		/*
		public static string GetDefaultAssetsSubdir(string classname)
		{
			string filepath = kDefaultSystemSubdir;	//	default system defaults directory		
			
			Object prefsObj = Object.FindObjectOfType(typeof(UserPreferences));
			UserPreferences prefs = prefsObj as UserPreferences;
			
			if (prefs != null) {
				filepath = prefs.GetDefaultsPath();
			}
			
			filepath += "/" + classname;
			return filepath;
		}
		public static string GetDefaultAssetsSubdirAsPrefab(string classname)
		{
			string filepath = kRootDir + "/" + GetDefaultAssetsSubdir( classname ) + kSuffix;	//	default system defaults directory		
			return filepath;
		}
		
		public static GameObject LoadDefault( string classname )
		{
			GameObject defaultObject;
			string path = GetDefaultAssetsSubdir(classname);
			Object obj = Resources.Load(path);
			defaultObject = obj as GameObject;
			
			return defaultObject;
		}
		 
		public static GameObject LoadDefault( this MonoBehaviour script )
		{
			string classname = script.GetType().ToString();
			GameObject go = LoadDefault(classname);
			return go;
		}
		
		//	Use this in the virtual method Reset(). We cannot override the virtual method Reset(), but we make this easily available
		//	to all MonoBehaviours that would like to use it.
		public static void ResetToDefaultGO( this MonoBehaviour script )
		{
			string classname = script.GetType().ToString();
			GameObject defaultGO = script.LoadDefault();
			if (defaultGO == null) {
				Rlplog.Error(classname + "::ResetToDefaultGO", "No Default found at " + MonoBehaviourExtension.GetDefaultAssetsSubdirAsPrefab(classname));
			}
			else {
				script.gameObject.SetDefaultLike(script.GetType());
				//	script.gameObject.SetDefaultLikeComponent(defaultGO, script.GetType());	//	don't do this. Why? If Sprite3D has a Frame on it, it will set the defaults to those in Frame.prefab rather than on the Frame component on Sprite3D.prefab.
			}
		}
		 */
	}
	
	public static class GameObjectExtension
	{
		const string kRootDir = MonoBehaviourExtension.kRootDir;
#if IPHONE_UNITY
		public const string kUserSaveDirectory = "UserSaveFiles";
#else
		public const string kUserSaveDirectory = kRootDir + "/" + "UserSaveFiles";
#endif
		static SendMessageOptions kDefaultOptions = SendMessageOptions.DontRequireReceiver;
		public static string			m_currentMessage;
		
		public delegate void GameObjectFcn(GameObject go, string[] parameters);
		public delegate void GameObjectFcnGO(GameObject go, GameObject targetGO);
		
		public static void TestReceiveMsg(this GameObject go, string[] parameters)
		{
			Rlplog.Debug("", "Test Message Received!");
		}
		
		//	This fills out the entire class structure of parentClass with the data in dataParent as much as possible
		public static void JsonDataToField(object parentClass, JsonData dataParent)
		{
			string dataParentFieldName = dataParent.GetKey();
			if (parentClass == null) {
				Rlplog.Error("JsonDataToField", "Not expecting null for " + dataParentFieldName);
				return;
			}
			System.Type componentType = parentClass.GetType();
			//System.Type	fieldType = null;

//			JsonType parentFieldType = dataParent.GetJsonType(); // removed due to warning (slc)

			
			switch (dataParent.GetJsonType())
			{
				case JsonType.Object:
					//	an object will have a bunch of fields. Iterate through each of those and fill it out.
//					var newComponent = parentClass; // removed due to warning (slc)
					//	for each Field specified in the JsonData, fill out that public field in the parentClass
					for(int ll=0; ll<dataParent.Count; ll++) {	
						JsonData dataChildField = dataParent[ll];
						if (dataChildField != null) {	//	it's legal to have a null list for a data field
							string dataChildFieldName = dataChildField.GetKey();
							FieldInfo dataChildComponentFieldInfo = componentType.GetField(dataChildFieldName);
//							JsonType childFieldType = dataChildField.GetJsonType(); // removed due to warning (slc)
							//	System.Type dataChildComponentFieldType = dataChildComponentFieldInfo.GetType();
//							var childClassObj = parentClass.GetType().GetField(dataChildFieldName); // removed due to warning (slc)
							if (dataChildComponentFieldInfo == null) {
								Rlplog.Error("JsonDataToField", "Not expecting " + dataChildFieldName + " of type " + componentType.ToString() + " to be null.");
							}
//							System.Type	dataChildFieldType = dataChildComponentFieldInfo.FieldType; // removed due to warning (slc)
							JsonDataToField(parentClass, dataChildFieldName, dataChildField);
						}
					}
				
					//	we may be a dictionary type of collection
				/*
					System.Type genericType = componentType.GetGenericTypeDefinition();
					bool isDictionary = (genericType == typeof(IDictionary<,>));
					if (!isDictionary) {
						isDictionary = (genericType == typeof(System.Collections.Generic.Dictionary<,>));
					}
					if ((dataParent.Count == 0) && (isDictionary)) {
						
					}
					*/
					break;
			}
		}
		/*
		public static void JsonDataToField(object parentClass, System.Reflection.FieldInfo field, JsonData dataParent)
		{
			System.Type componentType = field.FieldType;
			for(int ll=0; ll<dataParent.Count; ll++) {	
				JsonData dataChildField = dataParent[ll];
				string dataChildFieldName = dataChildField.GetKey();
				FieldInfo dataChildComponentFieldInfo = componentType.GetField(dataChildFieldName);
				JsonType childFieldType = dataChildField.GetJsonType();
				//	System.Type dataChildComponentFieldType = dataChildComponentFieldInfo.GetType();
				var childClassObj = parentClass.GetType().GetField(dataChildFieldName);
				System.Type	dataChildFieldType = dataChildComponentFieldInfo.FieldType;
				JsonDataToField(parentClass, dataChildFieldName, dataChildField);
			}
		}
		*/
		
		//	takes a JsonData that is a JsonType.Object assuming it is a class of "parentClass" fills out a particular field of type fieldType in the parentClass with the data
		public static void JsonDataToField(object parentClass, string fieldName, JsonData dataParent)
		{
			string dataParentFieldName = dataParent.GetKey();
			System.Type componentType = parentClass.GetType();
			
			LitJson.ExportType exportType = parentClass.GetExportType();
			if (exportType == LitJson.ExportType.NoExport) {
				return;
			}
			
			FieldInfo componentFieldInfo = null;
			if (fieldName != null)
				componentFieldInfo = componentType.GetField(fieldName);	//	if we're an object, we won't specify a fieldName, so this may be NULL.
			System.Type	fieldType = null;
			if (componentFieldInfo != null) {
				fieldType = componentFieldInfo.FieldType;
			/*
				//	We put this under Int rather than here. Doesn't belong here
				if (fieldType.IsEnum) {
					int		intVal = (int)dataParent;
					object		newEnumValue = System.Enum.ToObject(fieldType, intVal);
					componentFieldInfo.SetValue(parentClass, newEnumValue);
					return;
				}			
			*/
			}
			
//			JsonType parentFieldType = dataParent.GetJsonType(); // removed due to warning (slc)

			
			switch (dataParent.GetJsonType())
			{
				case JsonType.Object:
					if (fieldName == null) {	//	fill each field of this object
						for(int ii=0; ii<dataParent.Count; ii++) {
							string childFieldName = dataParent[ii].GetKey();
							JsonDataToField(parentClass, childFieldName, dataParent[ii]);
						}
					}
					else {	//	a fieldName was specified, so fill that particular fieldName
						var childClassField = componentType.GetField(fieldName);
						if (childClassField == null) {
							Rlplog.Error("JsonDataToField", "Expecting " + fieldName + " of Type " + componentType.ToString() + " to not be null.");
						}
						var childClassObj = childClassField.GetValue(parentClass);
						//JsonDataToField(parentClass, childClassField, dataParent);
						JsonDataToField(childClassObj, dataParent);
						/*
						//	an object will have a bunch of fields. Iterate through each of those and fill it out.
						var newComponent = parentClass;
						for(int ll=0; ll<dataParent.Count; ll++) {
							JsonData dataChildField = dataParent[ll];
							string dataChildFieldName = dataChildField.GetKey();
							FieldInfo dataChildComponentFieldInfo = componentType.GetField(dataChildFieldName);
							JsonType childFieldType = dataChildField.GetJsonType();
							//	System.Type dataChildComponentFieldType = dataChildComponentFieldInfo.GetType();
							var childClassObj = parentClass.GetType().GetField(dataChildFieldName);
							//System.Type	dataChildFieldType = dataChildComponentFieldInfo.FieldType;
							JsonDataToField(childClassObj, dataChildField);
						}
						*/
					}
					break;
				case JsonType.String:
					MemberInfo[] memberInfoArray = componentType.GetMember(fieldName);
					LitJson.ExportType fieldExportType = ExportType.Default;
					MemberInfo		thisMemberInfo = null;
				
					if (memberInfoArray.Length > 0) {
						thisMemberInfo = memberInfoArray[0];
						fieldExportType = thisMemberInfo.GetExportType();
					}
					if (fieldExportType == ExportType.Default) {
						componentFieldInfo.SetValue(parentClass, (System.String)dataParent);					
					}
					else {
						Object[] gameObjects = GameObject.FindObjectsOfType(fieldType);
						foreach(Object obj in gameObjects) {
							System.String valueString = (System.String)dataParent;
							if (obj.name == valueString) {
								componentFieldInfo.SetValue(parentClass, obj);
								break;
							}
						}
					}
					break;
				case JsonType.Int:
					/* enums may be saved as Ints. Check to see if it originally was an enum */
					if (componentType.IsEnum) {
						System.Int32		intVal = (System.Int32)dataParent;
						object		newEnumValue = System.Enum.ToObject(componentType, intVal);
						parentClass = newEnumValue;
					}
					else {
						componentFieldInfo.SetValue(parentClass, (System.Int32)dataParent);
					}
					break;
				case JsonType.Long:
					if (componentType.IsEnum) {
						System.Int32		intVal = (System.Int32)dataParent;
						object		newEnumValue = System.Enum.ToObject(componentType, intVal);
						parentClass = newEnumValue;
					}
					else {
						componentFieldInfo.SetValue(parentClass, (System.Int32)dataParent);
					}
					break;
				case JsonType.Double:
					float fValue = (float)((double)dataParent);
					componentFieldInfo.SetValue(parentClass, (System.Single)fValue);
					break;
				case JsonType.Boolean:
					componentFieldInfo.SetValue(parentClass, (System.Boolean)dataParent);
					break;
				case JsonType.Array:
					if (fieldType.IsArray) {
						System.Type singleElemType = fieldType.GetElementType();
						object		newArray = System.Array.CreateInstance(singleElemType, dataParent.Count);
						System.Array newArrayOfType = newArray as System.Array;
	
						
						for(int mm=0; mm<dataParent.Count; mm++) {
							JsonData dataChildArrayField = dataParent[mm];
							var		elem = System.Activator.CreateInstance(singleElemType);
							if (dataChildArrayField.GetJsonType() == JsonType.Object) {
								JsonDataToField(elem, dataChildArrayField);
							}
							else {
								JsonDataToField(elem, fieldName, dataChildArrayField);
							}
							newArrayOfType.SetValue(elem, mm);
						}
						componentFieldInfo.SetValue(parentClass, newArray);
					}
					else {	//	it's a Systems.Collections.Generic.List<T>. We'll use IList
						//	we could use var, but it's better to create this as a generic list
						System.Collections.IList newList = System.Activator.CreateInstance(fieldType) as System.Collections.IList;
						System.Type singleElemType = newList.GetType().GetProperty("Item").PropertyType;
						
						//	if this is a MonoBehaviour, we will not be able to create an element. Therefore, we need to take a different
						//	approach
						System.Type baseType = singleElemType.BaseType;
//						string expectedName = baseType.AssemblyQualifiedName.ToString(); // removed due to warning (slc)
						const string kMonoBehaviourAssemblyQualifiedName = "UnityEngine.MonoBehaviour, UnityEngine, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
						System.Type	compType = System.Type.GetType(kMonoBehaviourAssemblyQualifiedName);
						if (baseType == compType) {	//	monobehaviours can only be created by using AddComponent to a GameObject.
							Rlplog.Error("JsonDataToField", "Cannot import List of MonoBehaviour of type " + singleElemType.ToString() + " in " + dataParentFieldName + ".\nRecommend not exporting references to MonoBehaviours by using the [LitJson.ExportType(ExportType.NoExport)] property on members of the exported class.");
						}
						else {
							for(int mm=0; mm<dataParent.Count; mm++) {	//	set each element of the array
								JsonData dataChildArrayField = dataParent[mm];
								var		elem = System.Activator.CreateInstance(singleElemType);
								JsonDataToField(elem, dataChildArrayField);	//	fill in the element with the JSON data
								newList.Add(elem);
							}
							componentFieldInfo.SetValue(parentClass, newList);	//	attach this list to the parent
						}
					}
					break;
			}
				//System.Enum.GetUnderlyingType();
				//componentFieldInfo.SetValue(newComponent, System.Convert.ChangeType((System.Int32)dataChildField, childFieldType));
			/*
			*/
		}
		
		public static GameObject Load(this GameObject go, JsonData json_data)
		{
			GameObject newGameObject = go;
			//GameObject importGO = JsonMapper.ToObject<GameObject>(reader);	//	would like this to work, but I don't think it can due to MonoBehaviours requiring AddComponent() in order to instantiate
			//	ReadJsonData(json);		//	this is just to debug that our changes to the parser worked okay.
			int nFields = json_data.Count;
			for (int ii=0; ii<nFields; ii++) {
				JsonData child = json_data[ii];
				var type = child.GetJsonType();
				string childKey = child.GetKey();
				if (childKey == "GameObjectName") {
					//newGameObject = new GameObject(json_data[childKey].ToString());
					newGameObject.name = json_data[childKey].ToString();
				}

				if ((type == JsonType.Array) && (child != null)) {
					for(int jj=0; jj<json_data[childKey].Count; jj++) {
						GameObject childGO = null;
						string childName = null;
						//	see if we have an existing GameObject child that has the same GameObjectName as this one
						if (json_data[childKey].GetKey() == "GameObject") {
							JsonData dataChild = json_data[childKey][jj];
							JsonData gameObjectNameChild = dataChild["GameObjectName"];
							childName = gameObjectNameChild.ToString();
							if (gameObjectNameChild != null) {
								childGO = newGameObject.FindObjectFromChildren(childName);
								//	we didn't find an existing child, so create one
								if (childGO == null) {
									childGO = new GameObject(childName);
									childGO.transform.parent = newGameObject.transform;	//	connect to parent
								}
							}
							
							//	now load our json data into the gameobject we've found or created
							//string child_json_text = dataChild.ToJson();
							//childGO = childGO.Load(child_json_text);
							childGO = childGO.Load (dataChild);
						}
						else if (json_data[childKey].GetKey() == "MonoBehaviour") {
							JsonData dataChild = json_data[childKey][jj];
							string classname = dataChild.GetKey();
							if (dataChild.GetJsonType() == JsonType.Object) {
								for(int kk=0; kk<dataChild.Count; kk++) {
									classname = dataChild[kk].GetKey();
									ExportType exportType = ExportType.Default;
									var newComponent = newGameObject.GetComponent(classname);
									if (newComponent == null) {
										newComponent = newGameObject.AddComponent(classname);	//	add the script to the gameObject
										exportType = newComponent.GetExportType();
										if (exportType == ExportType.NoExport)
											Object.DestroyImmediate(newComponent);
									}
									exportType = newComponent.GetExportType();
									if (exportType == ExportType.NoExport) {
										continue;
									}
									//	fill out the field details of the new component
//									System.Type componentType = newComponent.GetType(); // removed due to warning (slc)
									if (dataChild[kk].GetJsonType() == JsonType.Object) {
										//	JsonDataToField(newComponent, ""/* NULL means we want to fill out the entire object class. componentType.ToString()*/, dataChild[kk]);
										JsonDataToField(newComponent, dataChild[kk]);
									}
									MonoBehaviour mb = newComponent as MonoBehaviour;
									if (mb != null) {
										mb.Invoke("Start", 0.0f);
									}
								}
							}
						}
					}
				}
			}
			
			return newGameObject;
		}
		
		//	loads into an existing GameObject, overwriting any existing MonoBehaviours and/or GameObject children
		public static GameObject Load(this GameObject go, string json_text)
		{
			//	test this thing
			JsonData json_data = JsonMapper.ToObject(json_text);
			
			GameObject newGameObject = go.Load (json_data);
			return newGameObject;
		}
		
		public static void Save(this GameObject go, JsonWriter writer, bool bSaveAllChildren)
		{
			Rlplog.Debug("GameObject.Save", go.name + ", recursive="+bSaveAllChildren.ToString());
			writer.WriteObjectStart();	//	start GameObject write
				writer.WritePropertyName("GameObjectName");
				writer.Write(go.name);
				
				//	GameObject children need to be exported first so that when imported, MonoBehaviours that compile from the
				//	GameObject hierarchy may have that hierarchy on hand
				//*****************************************************************
					//	save out each gameObject in the hierarchy
					//	export each game object
				if (bSaveAllChildren == true) {
					if (go.transform.childCount > 0) {
						writer.WritePropertyName("GameObject");
						writer.WriteArrayStart();
							//	export each script
							foreach (Transform xform in go.transform)
							{
								xform.gameObject.Save(writer, bSaveAllChildren);	//	recurse into this method to export this child GameObject
							}
						writer.WriteArrayEnd();
					}
				}
					
				//*****************************************************************
				//	save out each of the scripts
				MonoBehaviour[] scripts;
				scripts = go.GetComponents<MonoBehaviour>();
				writer.WritePropertyName("MonoBehaviour");
				writer.WriteArrayStart();
				
					//	export each script
					foreach (MonoBehaviour script in scripts)
					{
						//	check to see if we have a special attribute on this field
						bool bDontExport = false;
						ExportType exType = script.GetExportType();
						if (exType == ExportType.NoExport) {
							bDontExport = true;
						}

						if (exType == ExportType.Reference) {	//	tba: change this to do something special for references
							bDontExport = true;
							script.SaveReference(writer);
						}
						
						if (bDontExport==false) {
							script.Save(writer);	//	probably need to JsonMapper.RegisterExporter an exporter function for your specific class for this to work right.
						}
					}
			
				writer.WriteArrayEnd();
			
       		writer.WriteObjectEnd();	//	end GameObject write
		}
		
		public static GameObject LoadInto(this GameObject go, string filename)
		{
			string json_text = null;
			string savedir_filename = GetSaveFilePath(go, filename); //	kUserSaveDirectory + "/" + filename;
			
			//JsonWriter writer = new JsonWriter();
			//writer.PrettyPrint = true;	//	make the output look nice
			StreamReader stream = File.OpenText(savedir_filename);
			json_text = stream.ReadToEnd();
			stream.Close();
			GameObject newGameObject = go.Load(json_text);
			return newGameObject;
		}
		
		public static string GetSaveFilePath(this GameObject go, string filename)
		{
			string savedir_filename = Application.persistentDataPath +"/"+ kUserSaveDirectory + "/" + filename;
			return savedir_filename;
		}
		
		public static GameObject LoadAs(this GameObject go, string filename)
		{
			LitJson.JsonExtend.AddExtentds();		//	make sure our extensions are loaded
			string json_text = null;
			if (go == null) {
				Rlplog.DbgFlag = true;
				Rlplog.Error("LoadAs()", "GameObject is null");
				return new GameObject("Load Failed GameObject");
			}
			if (filename == null) {
				Rlplog.DbgFlag = true;
				Rlplog.Error("LoadAs(null)", "filename is null");
				return new GameObject("Load Failed GameObject");
			}
			string savedir_filename = go.GetSaveFilePath(filename);
			//Rlplog.TrcFlag = true;
			Rlplog.Trace("LoadAs("+savedir_filename+")", json_text);
			
			//JsonWriter writer = new JsonWriter();
			//writer.PrettyPrint = true;	//	make the output look nice
			StreamReader stream = File.OpenText(savedir_filename);
			json_text = stream.ReadToEnd();
			stream.Close();
			GameObject newGameObject = go.Load(json_text);
			return newGameObject;
		}
		
		/*
		 * This allows us to save the current gameObject
		 */
		public static string SaveAs(this GameObject go, string filename, bool bHumanReadableOutput, bool bSaveAllChildren)
		{
			LitJson.JsonExtend.AddExtentds();		//	make sure our extensions are loaded
			string json_text = null;
			string savedir_filename = null;
			
			if (filename != null)
				savedir_filename = go.GetSaveFilePath(filename);
			
			JsonWriter writer = new JsonWriter();
			writer.PrettyPrint = bHumanReadableOutput;	//	make the output look nice
			
			if (true) {
				go.Save(writer, bSaveAllChildren);
			} /* removed due to warning (slc)
			else {
				writer.WriteObjectStart();
				writer.WritePropertyName("GameObject");
				writer.Write(go.name);
	
				//*****************************************************************
					//	save out each of the scripts
					MonoBehaviour[] scripts;
					scripts = go.GetComponents<MonoBehaviour>();
					writer.WritePropertyName("MonoBehaviour");
					writer.WriteArrayStart();
					
					
					//	export each script
					foreach (MonoBehaviour script in scripts)
					{
						//	check to see if we have a special attribute on this field
						bool bDontExport = false;
						ExportType exType = script.GetExportType();
						if (exType == ExportType.NoExport) {
							bDontExport = true;
						}
						if (exType == ExportType.Reference) {	//	tba: change this to do something special for references
							script.SaveReference(writer);
						}
						
						if (bDontExport==false) {
							script.Save(writer);	//	probably need to JsonMapper.RegisterExporter an exporter function for your specific class for this to work right.
						}
					}
					writer.WriteArrayEnd();
				//*****************************************************************
					if (bSaveAllChildren == true) {
						//	save out each gameObject in the hierarchy
						//	export each game object
						writer.WritePropertyName("GameObject");
						writer.WriteArrayStart();
						
						
						//	export each script
						foreach (Transform xform in go.transform)
						{
							xform.gameObject.Save(writer, bSaveAllChildren);
						}
						writer.WriteArrayEnd();
					}				
	       			writer.WriteObjectEnd();
			} */
			
			json_text = writer.ToString();
			
			//	output this thing
			if (savedir_filename != null) {
				TextWriter tw = new StreamWriter(savedir_filename);
				//Rlplog.TrcFlag = true;
				Rlplog.Trace("SaveAs("+savedir_filename+")", json_text);
				tw.Write(json_text);
				tw.Flush();
				tw.Close();
			}
			
			/*
			foreach (Transform xform in go.transform)
			{
				xform.gameObject.SaveAs(filename);
			}
			*/
			
			return json_text;
			//	validate that the save worked
			//Load(go, json_text);
		}
		
		/*
		 * Set this gameObject like the specified defaultGameObject.
		 */
		public static void SetDefaultLike( this GameObject thisGO, GameObject defaultGO )
		{
			if (defaultGO == null) return;	//	fail bail
			//	stuff I care about
			thisGO.layer = defaultGO.layer;
			
			thisGO.transform.localScale = defaultGO.transform.localScale;
			thisGO.transform.localRotation = defaultGO.transform.localRotation;
			thisGO.transform.localPosition = defaultGO.transform.localPosition;
			thisGO.tag = defaultGO.tag;
			
			//	stuff I probably don't care about, but added for future compatibility
			thisGO.isStatic = defaultGO.isStatic;
			thisGO.hideFlags = defaultGO.hideFlags;
			
			//	stuff that is read only and would require a special copy (like transform above)
			//thisGO.animation = defaultGO.animation;
			//thisGO.audio = defaultGO.audio;
			//thisGO.camera = defaultGO.camera;
			//thisGO.collider = defaultGO.collider;
			//thisGO.constantForce = defaultGO.constantForce;
			//thisGO.light = defaultGO.light;
			//thisGO.networkView = defaultGO.networkView;
			//thisGO.particleEmitter = defaultGO.particleEmitter;
			//thisGO.renderer = defaultGO.renderer;
			//thisGO.rigidbody = defaultGO.rigidbody;
			//thisGO.guiText = defaultGO.guiText;
			//thisGO.guiTexture = defaultGO.guiTexture;
			//thisGO.hingeJoint = defaultGO.hingeJoint;
				
			//	stuff I exclude on purpose			
			//	thisGO.name = defaultGO.name;
			//	thisGO.active = defaultGO.active;
			
			
		}
		
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
		
		public static List<GameObject> GetAllChildren(this GameObject go, ref List<GameObject> curList)
		{
			if (curList == null) {
				curList = new List<GameObject>();
			}
			int numChildren = go.transform.childCount;
			GameObject childGO;
			
			for (int ii = 0; ii < numChildren; ++ii)
			{
				childGO = go.transform.GetChild(ii).gameObject;
				curList.Add(childGO);
				curList = childGO.GetAllChildren(ref curList);
			}
			return curList;
		}
		
		public static GameObject FindChildWithTag(this GameObject go, string tagName)
		{
			GameObject foundIt = null;
			
			Rlplog.Trace("GameObjectExtensions.FindChildWithTag", "Searching in " + go.name + " for tag of " + tagName + "...");
			if ((go == null) || (tagName == null)) return null;	//	early bail
			
			GameObject child;
			int numChildren = go.transform.childCount;
			for (int ii = 0; ii < numChildren; ++ii)
			{
				child = go.transform.GetChild(ii).gameObject;
				if (child.tag == tagName) {
					return child;
				}
			}
			return foundIt;
		}

		public static GameObject FindChildWithTagRecursive(this GameObject go, string tagName)
		{
			GameObject foundIt = null;
			foundIt = FindChildWithTag(go, tagName);	//	first search all of our children

			//	if none of our children have the tag, then search each of our children's children, breadth first
			if (foundIt == null) {
				GameObject child;
				int numChildren = go.transform.childCount;
				for (int ii = 0; ii < numChildren; ++ii)
				{
					child = go.transform.GetChild(ii).gameObject;
					foundIt = child.FindChildWithTagRecursive(tagName);
					if (foundIt != null) {
						return foundIt;
					}
				}
			}
			return foundIt;
		}
		
		//	Single search depth Find
		public static GameObject FindObjectOfType(this GameObject go, string typeName)
		{
			GameObject foundIt = null;
			var comp = go.GetComponent(typeName);
			
			if (comp != null)
				return comp.gameObject;
			
			Rlplog.Trace("GameObjectExtensions.FindObjectOfType", "Searching in " + go.name + " for type of " + typeName + "...");
			if ((go == null) || (typeName == null)) return null;	//	early bail
			comp = go.GetComponent(typeName);
			if (comp != null) {
				Rlplog.Trace("", "Found " + comp.name + "!\n");
				return comp.gameObject;
			}
			
			int numChildren = go.transform.childCount;
			
			GameObject child;
			for (int ii = 0; ii < numChildren; ++ii)
			{
				child = go.transform.GetChild(ii).gameObject;
				comp = child.GetComponent(typeName);
				if (comp != null) {
					return child;
				}				
			}
			return foundIt;
		}
		
		public static System.Object CacheFindObject(this GameObject go, GameObject exactMatch)
		{
			System.Object		foundIt = null;
			//	see if we have a cache. If so, use the cache since it's faster and we must be using it for a good reason
			HashCache cache = go.GetComponent<HashCache>();
			if (cache) {
				foundIt = cache.Find(exactMatch.name);
				GameObject foundItGO = foundIt as GameObject;
				if (foundItGO != exactMatch)
					foundIt = null;
			}
			return foundIt;
		}
		
		public static System.Object CacheFindObject(this GameObject go, string searchName)
		{
			System.Object		foundIt = null;
			//	see if we have a cache. If so, use the cache since it's faster and we must be using it for a good reason
			HashCache cache = go.GetComponent<HashCache>();
			if (cache) {
				foundIt = cache.Find(searchName);
			}
			return foundIt;
		}
		
		//	Single search depth Find of this GameObject's children or cache
		public static GameObject FindObject(this GameObject go, string searchName)
		{
			GameObject foundIt = null;
			
			Rlplog.Trace("GameObjectExtensions.FindObject", "Searching in " + go.name + " for " + searchName + "...");
			if ((go == null) || (searchName == null)) return null;	//	early bail
			if (go.name == searchName) {
				Rlplog.Trace("", "Found " + go.name + "!\n");
				return go;
			}
			
			//	see if we have a cache. If so, use the cache since it's faster and we must be using it for a good reason
			var comp = CacheFindObject(go, searchName);
			if (comp != null) {
				foundIt = comp as GameObject;
				if (foundIt == null) {
					MonoBehaviour script = comp as MonoBehaviour;
					if (script != null) {
						foundIt = script.gameObject;
					}
				}
			}
			else {					
				int numChildren = go.transform.childCount;
				
				GameObject child;
				for (int ii = 0; ii < numChildren; ++ii)
				{
					child = go.transform.GetChild(ii).gameObject;
					if (child.name == searchName) {
						return child;
					}				
				}
			}
			return foundIt;
		}
		
		//	recursive exact match Find. This is helpful for knowing if the instance is already bound or if it's still pointing to the prefab rather than the instance
		public static GameObject FindObjectExactlyFromChildren(this GameObject go, GameObject exactMatch)
		{
			GameObject foundIt = null;
			//	too much spam from this was slowing down Editor playback. Enable only when necessary.
			//	Rlplog.Trace("GameObjectExtensions.FindObjectFromChildren", "Searching in " + go.name + " for " + searchName + "...");
			if ((go == null) || (exactMatch == null)) return null;	//	early bail
			if (exactMatch == go) {
				Rlplog.Trace("GameObjectExtensions.FindObjectFromChildren", "Found " + go.name + "!\n");
				return go;
			}
			
			//	see if we have a cache. If so, use the cache since it's faster and we must be using it for a good reason
			var comp = CacheFindObject(go, exactMatch);
			if (comp != null) {
				foundIt = comp as GameObject;
				if (foundIt == null) {
					MonoBehaviour script = comp as MonoBehaviour;
					if (script != null) {
						foundIt = script.gameObject;
					}
				}
			}
			
			if (foundIt != null)	//	early exit due to cache find
				return foundIt;
			
			int numChildren = go.transform.childCount;
			
			for (int ii = 0; ii < numChildren; ++ii)
			{
			 	foundIt = go.transform.GetChild(ii).gameObject.FindObjectExactlyFromChildren(exactMatch);
				if (foundIt != null) return foundIt;	//	early exit due to successful hit
			}
			return foundIt;
		}
		
		//	recursive Find
		public static GameObject FindObjectFromChildren(this GameObject go, string searchName)
		{
			GameObject foundIt = null;
			//	too much spam from this was slowing down Editor playback. Enable only when necessary.
			//	Rlplog.Trace("GameObjectExtensions.FindObjectFromChildren", "Searching in " + go.name + " for " + searchName + "...");
			if ((go == null) || (searchName == null)) return null;	//	early bail
			if (go.name == searchName) {
				Rlplog.Trace("GameObjectExtensions.FindObjectFromChildren", "Found " + go.name + "!\n");
				return go;
			}
			
			//	see if we have a cache. If so, use the cache since it's faster and we must be using it for a good reason
			var comp = CacheFindObject(go, searchName);
			if (comp != null) {
				foundIt = comp as GameObject;
				if (foundIt == null) {
					MonoBehaviour script = comp as MonoBehaviour;
					if (script != null) {
						foundIt = script.gameObject;
					}
				}
			}
			
			if (foundIt != null)	//	early exit due to cache find
				return foundIt;
			
			int numChildren = go.transform.childCount;
			
			for (int ii = 0; ii < numChildren; ++ii)
			{
			 	foundIt = go.transform.GetChild(ii).gameObject.FindObjectFromChildren(searchName);
				if (foundIt != null) return foundIt;	//	early exit due to successful hit
			}
			return foundIt;
		}
		
		//	sometimes, this is actually a prefab, but we want the instance of it in the game hierarchy
		//	this occurs when prefabs link to other prefabs, but we ultimately want the instance and
		//	not the prefab
		public static object GetInstance(this GameObject go, System.Type componentType)
		{
			object			returnInstance = null;
			GameObject foundIt = null;
			
			//	do I have a parent that's an instance?
			if (go.transform.parent != null) {
				GameObject parentInstance = go.transform.parent.gameObject.GetInstance((System.Type)null) as GameObject;
				if (parentInstance == null)
					parentInstance = GameObject.Find(go.transform.parent.name);	//	we have parent instance
				
				if (parentInstance != null) {
					foundIt = parentInstance.FindObject(go.name);
				}
			}
			
			//	do a regular search of the scene
			if (foundIt == null) {
				foundIt = GameObject.Find(go.name);
			}
			if (foundIt != null) {
				if (componentType != null) {
					returnInstance = foundIt.GetComponent(componentType.ToString());
					if (returnInstance == null) {
						Rlplog.Debug("GameObjectExtensions.GetInstance", "Found " + foundIt.name + " but it did not have component type of " + componentType.ToString());
						returnInstance = foundIt;	//	sometimes, we may use a Transform or other Unity class as the Type
					}
				}
			}
			return returnInstance;
		}
		
		//	get an instance that was specifically instantiated by ReferenceHolder
		public static GameObject GetInstance(this GameObject go, string instanceName)
		{
			GameObject result = null;
			ReferenceHolder rh = go.GetComponent<ReferenceHolder>();
			if (rh) {
				result = rh.GetInstance(instanceName);
			}
			return result;
		}
		
		//	recursively search for this instance through the childrens' hierarchies.
		public static GameObject GetInstanceFromChildren(this GameObject go, string instanceName)
		{
			GameObject result = null;
			ReferenceHolder rh = go.GetComponent<ReferenceHolder>();
			if (rh) {
				result = rh.GetInstanceFromChildren(instanceName);		
			}
			else {
				result = FindObjectFromChildren(go, instanceName);
			}
			
			return result;
		}
		public static void ForEachChildDo(this GameObject go, GameObjectFcn fcn, string[] parameters)
	    {
	       fcn(go, parameters);
	
	       int numChildren = go.transform.childCount;
	
	       for (int ii = 0; ii < numChildren; ++ii)
	       {
				if (go.transform.GetChild(ii).gameObject != null) {	//	When an object is being recursively destroyed, the gameObject here may be null legitimately.
	         		go.transform.GetChild(ii).gameObject.ForEachChildDo(fcn, parameters);
				}
	       }
	    }
		
		public static void ForEachChildDoGO(this GameObject go, GameObjectFcnGO fcnGO, GameObject targetGO)
	    {
	       fcnGO(go, targetGO);
	
	       int numChildren = go.transform.childCount;
	
	       for (int ii = 0; ii < numChildren; ++ii)
	       {
				if (go.transform.GetChild(ii).gameObject != null) {	//	When an object is being recursively destroyed, the gameObject here may be null legitimately.
					go.transform.GetChild(ii).gameObject.ForEachChildDoGO(fcnGO, targetGO);
				}
	       }
	    }
		
		public static void SetMessage(this GameObject go, string msg)
		{
			m_currentMessage = msg;
		}
		
		//	ChainInvokeScriptMethod
		//	- Uses the result of the first invoke as an argument for the method of the second invoke
		//	use the selected object to Invoke a method from a component,
		//	then use the result of that Invoke to invoke a method from a component on a named GameObject using the result of the first Invoke as its argument
		//	arg0 - scriptName on selected object
		//	arg1 - methodCall on selected object
		//	arg2 - name of GameObject to receive new Invoke
		//	arg3 - script name on target GameObject
		//	arg4 - method call name on target GameObject
		//			-	argument for targetGameObject.targetMethodCall is result of first Invoke of selected Object
		static public object ChainInvokeScriptMethod(this GameObject selectedObj, string[] paramlist)
		{
			object result = null;
			if (selectedObj != null) {
				string scriptName = paramlist[0];
				string methodCall = paramlist[1];
				result = selectedObj.InvokeScriptMethod(scriptName, methodCall, null);
				
				string targetGameObjectName = paramlist[2];
				GameObject		targetObj = GameObject.Find(targetGameObjectName);
				
				if (targetObj != null) {
					string targetScriptName = paramlist[3];
					string targetMethodCall = paramlist[4];
					System.Object[] args;
					args = new System.Object[1];
					args[0] = result;
					//	InvokeScriptMethod(this GameObject destGO, string scriptName, string methodCall, System.Object[] args)
					result = targetObj.InvokeScriptMethod(targetScriptName, targetMethodCall, args);
				}			
			}
			return result;
		}
		
		//	call a particular script's method by name while passing real object arguments
		static public object InvokeScriptMethod(this GameObject destGO, string scriptName, string methodCall, System.Object[] args)
		{
			object result = null;
			if (destGO != null) {
				var component = destGO.GetComponent(scriptName);
				if (component == null) {
					Rlplog.Error("GameObject.InvokeScriptMethod", "GameObject " + destGO.name + " has no script named " + scriptName);
					return null;
				}
				var componentType = component.GetType();

				
				result = componentType.InvokeMember(methodCall, BindingFlags.Default | BindingFlags.InvokeMethod | BindingFlags.Instance, null, component, args);
			}
			return result;
		}
		
		public static void AwakeAllScripts(this GameObject go)
		{
			MonoBehaviour[] scripts;
			scripts = go.GetComponents<MonoBehaviour>();
			foreach(MonoBehaviour script in scripts) {
				if (script.enabled==true) {	//	this prevents ReferenceHolder from making recursive duplicates
					string methodName = "Awake";
					var componentType = script.GetType();
					if (componentType.GetMethod(methodName) != null) {
						script.Invoke(methodName, 0.0f);
						//componentType.InvokeMember(methodName, BindingFlags.Default | BindingFlags.InvokeMethod | BindingFlags.Instance, null, script, null);
					}
				}
			}
		}

		//	delegate GameObjectFcn compatible methods
		public static void ActivateRecursively(this GameObject go, string[] parameters)
		{
			//	special hack to strip method name argument
			if ((parameters!=null) && (parameters[0] == "ActivateRecursively") && (parameters.Length==1)) {
				go.ForEachChildDo(new GameObjectFcn(Activate), null);
			}
			else {
				go.ForEachChildDo(new GameObjectFcn(Activate), parameters);
			}
		}		
		public static void DeactivateRecursively(this GameObject go, string[] parameters)
		{
			//	special hack to strip method name argument
			if ((parameters!=null) && (parameters[0] == "DeactivateRecursively") && (parameters.Length==1)) {
				go.ForEachChildDo(new GameObjectFcn(Deactivate), null);
			}
			else {
				go.ForEachChildDo(new GameObjectFcn(Deactivate), parameters);
			}
		}
		
		//	Bug fix: Calling convention for message system caused issues with Activate/Deactivate 
		//	which uses 0 arguments to specify the current GameObject 
		//	or multiple arguments to specify the component(s) to Activate/Deactivate. 
		//	This method should be split into two. Enable()/Disable() should take the functionality 
		//	of doing the GameObject.active while Activate/Deactivate should work on components only. 
		//	However, it's kept this way for backwards compatibility for now.
		public static void Activate(this GameObject go, string[] parameters)
		{
			//	special hack to strip method name argument
			if ((parameters!=null) && (parameters[0] == "Activate") && (parameters.Length==1)) {
				parameters = null;
			}

			if ((parameters==null) || (parameters.Length == 0)) {	//	new thread-safe trigger calls require that the first parameter is the method to be called.
				go.active = true;
			}
			else {	//
				foreach(string componentName in parameters) {
					/*
					UnityEngine.Component[] components = go.GetComponents(componentName));
					foreach(UnityEngine.Component component in components) {
						if (component != null) {
							component.enabled = true;
						}
					}
					*/
					UnityEngine.MonoBehaviour component = go.GetComponent(componentName) as MonoBehaviour;
					if (component != null) {
						component.enabled = true;
					}
					UnityEngine.Behaviour behaviour = go.GetComponent(componentName) as UnityEngine.Behaviour;
					if (behaviour != null) {
						behaviour.enabled = true;
					}
				}
			}
			//go.SendMessage("Activate", kDefaultOptions);
		}
		//	parameters contains the Components that we want to disable
		public static void Deactivate(this GameObject go, string[] parameters)
		{
			//go.SendMessage("Deactivate", kDefaultOptions);
			//	special hack to strip method name argument
			if ((parameters!=null) && (parameters[0] == "Deactivate") && (parameters.Length==1)) {
				parameters = null;
			}
			
			if ((parameters==null) || (parameters.Length == 0)) {	//	new thread-safe trigger calls require that the first parameter is the method to be called.
				go.active = false;
			}
			else {	//
				foreach(string componentName in parameters) {
					/*
					UnityEngine.Component[] components = go.GetComponents(componentName);
					foreach(UnityEngine.Component component in components) {
						if (component != null) {
							component.enabled = false;
						}
					}
					*/
					UnityEngine.MonoBehaviour component = go.GetComponent(componentName) as MonoBehaviour;
					if (component != null) {
						component.enabled = false;
					}
					UnityEngine.Behaviour behaviour = go.GetComponent(componentName) as UnityEngine.Behaviour;
					if (behaviour != null) {
						behaviour.enabled = false;
					}
				}
			}
		}
		
		public static GameObject GetRootParent(this GameObject go)
		{
			if (go.transform.parent == null) {
				return go;
			}
			return go.transform.parent.gameObject.GetRootParent();	//	recurse
		}
		
		public static void SetParent(this GameObject go, GameObject parentGO)
		{
			go.transform.parent = parentGO.transform;
		}
		public static void SetAllChildrensParent(this GameObject go, GameObject parentGO)
		{
			go.ForEachChildDoGO(new GameObjectFcnGO(SetParent), parentGO);
		}
		//	this sets the message that allows the SendMessage and BroadcastMessage delegates to work.
		//	This is done for speed reasons due to many recursive calls which would have to pass this parameter
		//	However, it is not thread-safe. A new solution would need to be written if multi-threaded issues arise later.
		//	6/5/2013 - alas, multi-threaded issues did arise. So this SetMessage should be made obsolete and deprecated.
		//	The m_currentMessage is appended to the list of parameters now.
		public static void SetMessage(this GameObject go, string[] parameters)
		{
			if ((parameters != null) && (parameters.Length > 0)) {
				m_currentMessage = parameters[0];
			}
		}
		public static void Hide(this GameObject go, string[] parameters)
		{
			if (go.renderer != null)
				go.renderer.enabled = false;
		}
		public static void HideRecursively(this GameObject go, string[] parameters)
		{
			go.ForEachChildDo(new GameObjectFcn(Hide), parameters);
		}
		
		public static void Unhide(this GameObject go, string[] parameters)
		{
			if (go.renderer != null)
				go.renderer.enabled = true;
		}
		public static void UnhideRecursively(this GameObject go, string[] parameters)
		{
			go.ForEachChildDo(new GameObjectFcn(Unhide), parameters);
		}
		
		//	error handling
		//	go = parent from which to hang the error message
		//	errorObject = quarantined problem object
		public static CommentError QuarantineError(this GameObject go, GameObject errorObject, string errmsg)
		{
			CommentError error = CommentError.AddError(go, errorObject, errmsg);
			return error;
		}
		
		//	make these obsolete eventually.
		//	NOTE: use SetMessage above to set the message. NOTE: Not thread-safe at the moment!
		public static void SendMessageDelegateNoRelay(this GameObject go, string[] parameters)
		{
			//bool bPrevActiveStatus = go.active;
			go.active = true;	//	hack: before we can receive a message, we need the receiver to be active
			go.SendMessage(m_currentMessage, parameters, kDefaultOptions);
			//go.active = bPrevActiveStatus;	//	unfortunately, this kills the message if we aren't active
		}		
		
		public static void BroadcastMessageDelegateNoRelay(this GameObject go, string[] parameters)
		{
			//bool bPrevActiveStatus = go.active;
			go.active = true;	//	hack: before we can receive a message, we need the receiver to be active
			go.BroadcastMessage(m_currentMessage, parameters, kDefaultOptions);
			//go.active = bPrevActiveStatus;
		}		

		//	NOTE: use SetMessage above to set the message. NOTE: Not thread-safe at the moment!
		public static void SendMessageDelegate(this GameObject go, string[] parameters)
		{
			//bool bPrevActiveStatus = go.active;
			go.active = true;	//	hack: before we can receive a message, we need the receiver to be active
			MsgRelay relay = go.GetComponent<MsgRelay>();
			if (relay != null) {
				relay.RelaySendMessage(m_currentMessage, parameters, kDefaultOptions);
			}
			go.SendMessage(m_currentMessage, parameters, kDefaultOptions);
			//go.active = bPrevActiveStatus;
		}		
		
		public static void BroadcastMessageDelegate(this GameObject go, string[] parameters)
		{
			//bool bPrevActiveStatus = go.active;
			go.active = true;	//	hack: before we can receive a message, we need the receiver to be active
			MsgRelay relay = go.GetComponent<MsgRelay>();
			if (relay != null) {
				relay.RelayBroadcastMessage(m_currentMessage, parameters, kDefaultOptions);
			}
			go.BroadcastMessage(m_currentMessage, parameters, kDefaultOptions);
			//go.active = bPrevActiveStatus;
		}		
		
		//	threadsafe messages
		//	NOTE: use SetMessage above to set the message. NOTE: Not thread-safe at the moment!
		public static void SendMessageDelegateNoRelayMT(this GameObject go, string[] parameters)
		{
			//bool bPrevActiveStatus = go.active;
			if (go.active == false)
				go.active = true;	//	hack: before we can receive a message, we need the receiver to be active
			go.SendMessage(parameters[parameters.Length-1], parameters, kDefaultOptions);
			//go.active = bPrevActiveStatus;	//	unfortunately, this kills the message if we aren't active
		}		
		
		public static void BroadcastMessageDelegateNoRelayMT(this GameObject go, string[] parameters)
		{
			//bool bPrevActiveStatus = go.active;
			go.active = true;	//	hack: before we can receive a message, we need the receiver to be active
			go.BroadcastMessage(parameters[parameters.Length-1], parameters, kDefaultOptions);
			//go.active = bPrevActiveStatus;
		}		

		//	NOTE: use SetMessage above to set the message. NOTE: Not thread-safe at the moment!
		public static void SendMessageDelegateMT(this GameObject go, string[] parameters)
		{
			//bool bPrevActiveStatus = go.active;
			go.active = true;	//	hack: before we can receive a message, we need the receiver to be active
			MsgRelay relay = go.GetComponent<MsgRelay>();
			if (relay != null) {
				relay.RelaySendMessage(parameters[parameters.Length-1], parameters, kDefaultOptions);
			}
			go.SendMessage(m_currentMessage, parameters, kDefaultOptions);
			//go.active = bPrevActiveStatus;
		}		
		
		public static void BroadcastMessageDelegateMT(this GameObject go, string[] parameters)
		{
			//bool bPrevActiveStatus = go.active;
			go.active = true;	//	hack: before we can receive a message, we need the receiver to be active
			MsgRelay relay = go.GetComponent<MsgRelay>();
			if (relay != null) {
				relay.RelayBroadcastMessage(parameters[parameters.Length-1], parameters, kDefaultOptions);
			}
			go.BroadcastMessage(parameters[parameters.Length-1], parameters, kDefaultOptions);
			//go.active = bPrevActiveStatus;
		}		
		
		//	physics stuff
		public static void Freeze(this GameObject go)
		{
			if (go.rigidbody != null) {
				go.rigidbody.Sleep();
				go.rigidbody.velocity = Vector3.zero;
				go.rigidbody.angularVelocity = Vector3.zero;
				//go.rigidbody.freezeRotation = true;
				go.rigidbody.useGravity = false;
				go.rigidbody.detectCollisions = false;
			}
		}
		
		public static void Unfreeze(this GameObject go)
		{
			if (go.rigidbody != null) {
				go.rigidbody.WakeUp();
				go.rigidbody.useGravity = true;
				//go.rigidbody.freezeRotation = false;
				go.rigidbody.detectCollisions = true;
			}
		}
	
		//	physical attaching/detaching
		public static void AttachTo(this GameObject go, GameObject parentGO)
		{
			go.transform.parent = parentGO.transform;
			go.transform.localPosition = Vector3.zero;
			go.transform.localRotation = Quaternion.identity;
			
			go.Freeze();
			if (go.rigidbody != null) {
				go.rigidbody.isKinematic = true;
			}
		}
		
		//	drop this object on the ground with the given velocity
		public static bool Detach(this GameObject go, Vector3 inheritVelocity)
		{
			bool bSomethingDetached = false;
			if (go.transform.parent != null) {
				bSomethingDetached = true;
				go.transform.parent = null;
				go.gameObject.Unfreeze();			
				if (go.rigidbody != null) {
					go.rigidbody.isKinematic = false;
					/*
					PhysicsFrisbee customHack = go.GetComponent<PhysicsFrisbee>();	//	stop the stick from moving around in the intro scene
					if (customHack != null) {					
						if (customHack.m_bInheritMomentumWhenDropped==false) {
							inheritVelocity = Vector3.zero;
						}
					}
					*/
					go.rigidbody.velocity = inheritVelocity;
				}
			}
			return bSomethingDetached;
		}
		
		
		public static float GetAngleTo(this GameObject go, Vector3 myForwardFacingVector, GameObject target)
		{
			Vector3 toTarget = target.transform.position - go.transform.position;
			float dotprod = Vector3.Dot(toTarget.normalized, myForwardFacingVector.normalized);
			float angleBetweenInDegrees = (Mathf.Acos(dotprod)) * Mathf.Rad2Deg;
			return angleBetweenInDegrees;
		}
	}
}

