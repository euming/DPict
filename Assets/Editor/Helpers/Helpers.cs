using UnityEngine;
using UnityEditor;
using CustomEditorExtensions;
using CustomExtensions;

public class HelperNonPowerOfTwoTexture : ScriptableObject
{
    [MenuItem ("Helpers/Texture/NonPowerOfTwoTexture")]
    static void Helpers_Texture_NonPowerOfTwoTextures()
    {
		Object[]		objects = Selection.objects;

        foreach(Object obj in objects)
        {
			Texture tex2D = obj as Texture;
			if (tex2D) {
				Helpers_Texture_NonPowerOfTwoTexture(tex2D);
			}
        }
	}
	
	static void Helpers_Texture_NonPowerOfTwoTexture(Texture tex2D)
	{
		if (tex2D == null) return;	//	fail bail
		
		string path = AssetDatabase.GetAssetPath(tex2D);

		TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
		textureImporter.npotScale = TextureImporterNPOTScale.None;
		AssetDatabase.ImportAsset(path);		
	}
}

public class ApplyTransformToMesh : ScriptableObject
{
    [MenuItem ("Helpers/Mesh/ApplyTransformToMesh")]
    static void Helpers_Mesh_ApplyTransformToMeshes()
    {
		Object[]		objects = Selection.objects;

        foreach(Object obj in objects)
        {
			GameObject go = obj as GameObject;
			if (go) {
				Helpers_Mesh_ApplyTransformToMesh(go, go.transform);
			}
        }
	}
	
	static void Helpers_Mesh_ApplyTransformToMesh(GameObject go, Transform xform)
	{
		Utility.ApplyTransformToMesh(go, xform);
		/*
		if ((go == null) || (xform == null)) return;	//	fail bail
		
		MeshFilter mf = go.GetComponent<MeshFilter>();
		if (mf == null) return;	//	fail bail
		
        Mesh mesh = mf.sharedMesh;
		if (mesh == null) return;	//	fail bail
		
		Vector3[] vertices = mf.sharedMesh.vertices;
        int p = 0;
        while (p < vertices.Length) {
			vertices[p] = xform.transform.TransformPoint(mf.sharedMesh.vertices[p]);
            p++;
        }
		mesh.vertices = vertices;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		
		//	reset 
		go.transform.localScale = new Vector3(1, 1, 1);
		go.transform.localRotation = new Quaternion(0, 0, 0, 1);
		go.transform.localPosition = new Vector3(0, 0, 0);
		*/
	}
}

/*
public class SetThisAsDefaultAssetObject : ScriptableObject
{
	[MenuItem ("Helpers/SetThisAsDefaultAsset &d")]
	
	static void Helpers_SetThisAsDefaultAsset()
	{
		
		Object[]		objects = Selection.objects;
		GameObject		go;
		
        foreach(Object obj in objects)
        {
			go = obj as GameObject;
			if (go) {
				go.SetThisAsDefaultAsset();
			}
        }		
	}
}
*/
public class SaveAsObject : ScriptableObject
{
	[MenuItem ("Helpers/Save As &1")]
	
	static public void Helpers_SaveAsObject()
	{
		LitJson.JsonExtend.AddExtentds();
		
		Object[]		objects = Selection.objects;
		GameObject		go;
		
        foreach(Object obj in objects)
        {
			go = obj as GameObject;
			if (go) {
				string json_text = go.SaveAs("testUserSave_" + obj.name + ".sav", false, true);
				Debug.Log(json_text);
			}
        }		
	}
}

public class HideAllObjectsRecursive : ScriptableObject
{
	
	[MenuItem ("Helpers/Hierarchical Hide-Unhide Toggle &h")]
	static public void Helpers_HideAllObjectsRecursive()
	{
		
		Object[]		objects = Selection.objects;
		GameObject		go;
		
        foreach(Object obj in objects)
        {
			go = obj as GameObject;
			if (go) {
				HideObjectRecursive(go, true);
			}
        }		
	}
	
	static public bool HideObject(GameObject go, bool bIsActive)
	{
		
		MeshRenderer  mr = go.GetComponent<MeshRenderer>();
		if (mr) {	//	toggles
			mr.enabled = !mr.enabled;
			bIsActive = mr.enabled;
			EditorUtility.SetDirty(go);
		}
		return bIsActive;
	}
	
	static public bool HideObjectRecursive(GameObject go, bool bIsActive)
	{
		bIsActive = HideObject(go, bIsActive);
		foreach(Transform child in go.transform) {
			HideObjectRecursive(child.gameObject, bIsActive);
		}
		return bIsActive;
	}
}

public class FrameMakeRelativeOffset : ScriptableObject
{
	[MenuItem ("Helpers/MakeRelativeOffset(Frame) &r")]
	
	static void Helpers_MakeRelativeOffset()
	{
		
		Object[]		objects = Selection.objects;
		GameObject		go;
		Frame			frame;
        foreach(Object obj in objects)
        {
			go = obj as GameObject;
			if (go) {
				frame = go.GetComponent<Frame>();
				if (frame) {
					frame.BakeIntoRelativeOffset();
				}
			}
        }		
	}
}