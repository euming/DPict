using UnityEngine;

public class Utility : ScriptableObject
{
	static public void ApplyTransformToMesh(GameObject go, Transform xform)
	{
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
		mesh = Instantiate(mesh) as Mesh;
		mesh.vertices = vertices;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		mf.sharedMesh = null;
		mf.sharedMesh = mesh;
		
		//	reset 
		go.transform.localScale = new Vector3(1, 1, 1);
		go.transform.localRotation = new Quaternion(0, 0, 0, 1);
		go.transform.localPosition = new Vector3(0, 0, 0);
	}
}