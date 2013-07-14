using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.IO;

namespace LitJson
{
	// An enumeration of animals. Start at 1 (0 = uninitialized).
	public enum ExportType {
		Default = 0,
		NoExport,
		Reference,
	}
	
	// A custom attribute to allow a target to have a pet.
	public class ExportTypeAttribute : Attribute {
	    // The constructor is called when the attribute is set.
	    public ExportTypeAttribute(ExportType xt) {
	        m_exportType = xt;
	    }
	
	    // Keep a variable internally ...
	    protected ExportType m_exportType;
	
	    // .. and show a copy to the outside world.
	    public ExportType exportType {
	        get { return m_exportType; }
	        set { m_exportType = value; }
	    }
	}
	
	//	this allows us to find the [ExportType.NoExport] attribute that we assign to members of a class
	public static class systemObjectExtension 
	{
		public static ExportType GetExportType(this System.Object obj)
		{
			ExportType exType = ExportType.Default;
			if (obj == null) {
				return ExportType.Default;	//	no information about this. But null can still be exported
			}
			Type myType = obj.GetType();
			//	check to see if we have a special attribute on this field
			object[] attrs = myType.GetCustomAttributes(true);

			foreach(Attribute attr in attrs) {
				if (attr.GetType() == typeof(LitJson.ExportTypeAttribute)) {
					ExportTypeAttribute exAttr = attr as ExportTypeAttribute;
					if (exAttr != null) {
						exType = exAttr.exportType;
						break;
					}
				}
			}
			
			return exType;
		}
		
		public static ExportType GetExportType(this System.Reflection.MemberInfo memberInfo)
		{
			ExportType exType = ExportType.Default;
			//	check to see if we have a special attribute on this field
			object[] attrs = memberInfo.GetCustomAttributes(true);
			foreach(System.Attribute attr in attrs) {
				if (attr.GetType() == typeof(LitJson.ExportTypeAttribute)) {
					ExportTypeAttribute exAttr = attr as ExportTypeAttribute;
					if (exAttr != null) {
						exType = exAttr.exportType;
						break;
					}
				}
			}

			return exType;
		}		
		
	}
	
	public class JsonExtend
    {
		static bool bExtensionsLoaded = false;
        public static void AddExtentds()
        {
			if (bExtensionsLoaded) return;	//	early bail
			
			// UnityEngine.GameObject exporter
            ExporterFunc<UnityEngine.GameObject> gameObjectExporter = new ExporterFunc<UnityEngine.GameObject>(JsonExtend.gameObjexp);
            JsonMapper.RegisterExporter<UnityEngine.GameObject>(gameObjectExporter);
			
			// UnityEngine.MonoBehaviour exporter
            ExporterFunc<UnityEngine.MonoBehaviour> MonoBehaviourExporter = new ExporterFunc<UnityEngine.MonoBehaviour>(JsonExtend.MonoBehaviourexp);
            JsonMapper.RegisterExporter<UnityEngine.MonoBehaviour>(MonoBehaviourExporter);
			
			// UnityEngine.Quaternion exporter
            ExporterFunc<UnityEngine.Quaternion> quaternionExporter = new ExporterFunc<UnityEngine.Quaternion>(JsonExtend.quaternionexp);
            JsonMapper.RegisterExporter<UnityEngine.Quaternion>(quaternionExporter);

            // UnityEngine.Object exporter
            ExporterFunc<UnityEngine.Object> objectExporter = new ExporterFunc<UnityEngine.Object>(JsonExtend.objectexp);
            JsonMapper.RegisterExporter<UnityEngine.Object>(objectExporter);

            // DogNeeds exporter
			//DogNeeds.InitJsonExporter();

            // UnityEngine.Transform exporter
            ExporterFunc<UnityEngine.Transform> TransformExporter = new ExporterFunc<UnityEngine.Transform>(JsonExtend.Transformexp);
            JsonMapper.RegisterExporter<UnityEngine.Transform>(TransformExporter);

            // Vector4 exporter
            ExporterFunc<Vector4> vector4Exporter = new ExporterFunc<Vector4>(JsonExtend.vector4exp);
            JsonMapper.RegisterExporter<Vector4>(vector4Exporter);

            // Vector3 exporter
            ExporterFunc<Vector3> vector3Exporter = new ExporterFunc<Vector3>(JsonExtend.vector3exp);
            JsonMapper.RegisterExporter<Vector3>(vector3Exporter);

            // Vector2 exporter
            ExporterFunc<Vector2> vector2Exporter = new ExporterFunc<Vector2>(JsonExtend.vector2exp);
            JsonMapper.RegisterExporter<Vector2>(vector2Exporter);

            // float to double
            ExporterFunc<float> float2double = new ExporterFunc<float>(JsonExtend.float2double);
            JsonMapper.RegisterExporter<float>(float2double);

            // double to float
            ImporterFunc<double, Single> double2float = new ImporterFunc<double, Single>(JsonExtend.double2float);
            JsonMapper.RegisterImporter<double, Single>(double2float);
			bExtensionsLoaded = true;
        }
		
		public static void gameObjexp(UnityEngine.GameObject value, JsonWriter writer)
        {
			writer.Write(null);
        }
		
		public static void MonoBehaviourexp(UnityEngine.MonoBehaviour value, JsonWriter writer)
        {
			System.Type obj_type = value.GetType();
			
			if (obj_type.GetExportType() == ExportType.NoExport) {	//	don't export
				writer.Write(null);
				return;
			}
			
			if (value is UnityEngine.MonoBehaviour) {
				string classname = value.GetType().ToString();
				
				//Rlplog.TrcFlag = true;
				Rlplog.Trace("LitJson.MonoBehaviourexp", "script<"+classname+">="+value.name);

				writer.WriteObjectStart();
				
				/*
				//	get everything
				MemberInfo [] memberInfoArray = obj_type.GetMembers (
					BindingFlags.Public |
					BindingFlags.Static |
					BindingFlags.NonPublic |
					BindingFlags.Instance |
					BindingFlags.DeclaredOnly
				);          
				*/
				
				//	get only the stuff we want to export
				MemberFilter myFilter = Type.FilterAttribute;
				//	for testing
				/*
				MemberInfo [] allMemberInfo = obj_type.GetMembers();
				foreach(MemberInfo memInfo in allMemberInfo) {
					string info = "The member " + memInfo.Name + " of " + obj_type + " is a " + memInfo.MemberType.ToString();
					Rlplog.Trace("LitJson.MonoBehaviourexp", info);
				}
				*/
				MemberInfo [] memberInfoArray = obj_type.FindMembers(MemberTypes.Field, 
					BindingFlags.Public | BindingFlags.Instance,
					myFilter, MethodAttributes.Public);
				//	for all fields we want to export, write it out with our JsonWriter
				foreach (MemberInfo memberInfo in memberInfoArray) {
					FieldInfo fieldInfo = memberInfo as FieldInfo;
					if (fieldInfo == null) {	//	early bail. This member is not really a field
						continue;
					}
					
					//	check to see if we have a special attribute on this field
					bool bDontExport = false;
					bool bExportAsReference = false;
					ExportType memberExportType = memberInfo.GetExportType();
					if (memberExportType == ExportType.NoExport) {
						bDontExport = true;
					}
					if (memberExportType == ExportType.Reference) {	//	tba: change this to do something special for references
						bExportAsReference = true;
					}					
					
					//	sometimes, an entire class may have a NoExport MethodAttribute
					var thisFieldsValue = fieldInfo.GetValue(value);
					ExportType valueExportType = ExportType.Default;
					
					if (thisFieldsValue != null)
						valueExportType = thisFieldsValue.GetExportType();
					if (valueExportType == ExportType.NoExport) {
						bDontExport = true;
					}
					if (valueExportType == ExportType.Reference) {	//	tba: change this to do something special for references
						bExportAsReference = true;
					}		
					
					//	determine whether this particular field is serialized or not
					bool bNotSerialized = ((fieldInfo.Attributes & FieldAttributes.NotSerialized) == FieldAttributes.NotSerialized);
					
					/*
					ExportType exType = memberInfo.GetType().GetExportType();
					if (exType == ExportType.NoExport) {
						bDontExport = true;
					}
					if (exType == ExportType.Reference) {	//	tba: change this to do something special for references
						bDontExport = true;
					}
					*/
					/*
					object[] attrs = memberInfo.GetCustomAttributes(true);
					foreach(Attribute attr in attrs) {
						if (attr.GetType() == typeof(LitJson.ExportTypeAttribute)) {
							ExportTypeAttribute exAttr = attr as ExportTypeAttribute;
							if (exAttr != null) {
								if (exAttr.exportType == ExportType.NoExport) {
									bDontExport = true;
								}
								if (exAttr.exportType == ExportType.Reference) {	//	tba: change this to do something special for references
									bExportAsReference = true;
								}
							}
						}
					}
					*/
					// only export what's necessary: Public and non-serialized fields. Some members explicitly are ExportType.NoExport and should not be exported.
					if ((memberInfo.MemberType == MemberTypes.Field) 
					&& (bNotSerialized == false)
					&& (bDontExport == false)
					){
						//Rlplog.TrcFlag = true;
						//	export the property name
						writer.WritePropertyName(memberInfo.Name);
						if (bExportAsReference) {
							//	export the reference
							writer.WriteAsReference(thisFieldsValue, memberInfo);
						}
						else {
							//	export the value of this field
							writer.Write(thisFieldsValue);
						}
						if (thisFieldsValue != null)
							Rlplog.Trace("LitJson.MonoBehaviourexp", "Property("+memberInfo.Name+")="+thisFieldsValue.ToString());
					}
				}
				
				//	PropertyInfo[] properties = obj_type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
				
		        writer.WriteObjectEnd();
				return;
			}
        }
		
		public static void Transformexp(UnityEngine.Transform value, JsonWriter writer)
        {
			writer.Write(null);
        }
		
		public static void quaternionexp(UnityEngine.Quaternion value, JsonWriter writer)
        {
			writer.WriteObjectStart();
			writer.WritePropertyName("x");
            writer.Write(value.x);
            writer.WritePropertyName("y");
            writer.Write(value.y);
            writer.WritePropertyName("z");
            writer.Write(value.z);
            writer.WritePropertyName("w");
            writer.Write(value.w);
            writer.WriteObjectEnd();
        }

        public static void objectexp(UnityEngine.Object value, JsonWriter writer)
        {
			writer.Write(null);
        }

        public static void vector4exp(Vector4 value, JsonWriter writer)
        {
            writer.WriteObjectStart();
            writer.WritePropertyName("x");
            writer.Write(value.x);
            writer.WritePropertyName("y");
            writer.Write(value.y);
            writer.WritePropertyName("z");
            writer.Write(value.z);
            writer.WritePropertyName("w");
            writer.Write(value.w);
            writer.WriteObjectEnd();
        }

        public static void vector3exp(Vector3 value, JsonWriter writer)
        {
            writer.WriteObjectStart();
            writer.WritePropertyName("x");
            writer.Write(value.x);
            writer.WritePropertyName("y");
            writer.Write(value.y);
            writer.WritePropertyName("z");
            writer.Write(value.z);
            writer.WriteObjectEnd();
        }

        public static void vector2exp(Vector2 value, JsonWriter writer)
        {
            writer.WriteObjectStart();
            writer.WritePropertyName("x");
            writer.Write(value.x);
            writer.WritePropertyName("y");
            writer.Write(value.y);
            writer.WriteObjectEnd();
        }

        public static void float2double(float value, JsonWriter writer)
        {
            writer.Write((double)value);
        }

        public static System.Single double2float(double value)
        {
            return (System.Single)value;
        }

        /// <summary>
        /// Load file, parse and return object;
        /// </summary>
        public static T Load<T>(string path)
        {
            try
            {
                if (File.Exists(path) == false)
	            {
                    Debug.LogError("file " + path + " doesn't exist!");
                    return default(T);
	            }
            	
	            string str = string.Empty;
                using (StreamReader sr = new StreamReader(path))
	            {
		            str = sr.ReadToEnd();
		            sr.Dispose();
	            }

                //Debug.LogWarning("   JsonExtend.Load()  = " + str);

	            return JsonMapper.ToObject<T>(str);
            }

            catch (Exception Ex)
            {
                Debug.LogError(Ex.ToString());
                return default(T);
            }
        }
        
        
    }
}

