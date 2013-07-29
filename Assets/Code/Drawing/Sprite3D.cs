// ======================================================================================
// File         : Sprite3D.cs
// Author       : Eu-Ming Lee 
// Changelist   :
//	10/24/2011 - First creation
// Description  : 
//	A Sprite3D is a 2D Texture that is placed onto a simple quad such that the aspect
//	ratio of the original texture is preserved. 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Reflection;
using CustomExtensions;

///////////////////////////////////////////////////////////////////////////////
///
/// Sprite3D
///
///////////////////////////////////////////////////////////////////////////////
//	[RequireComponent(typeof(Frame))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[LitJson.ExportType(LitJson.ExportType.NoExport)]	//	use this to prevent specific fields from being exported by LitJson library
[ExecuteInEditMode]
[System.Serializable] // Required so it shows up in the inspector 
[AddComponentMenu ("GUI/Sprite3D")]
public class Sprite3D : MonoBehaviour 
{
	public class Quad
	{
		public Vector3[] kQuadVertices;
		public Vector2[] kQuadUVs;
		public int[] kQuadTriangles;
		public bool			m_bFlipped = false;
		public Quad()
		{
			kQuadVertices = new Vector3[4];
			kQuadVertices[0] = new Vector3(-.5f, -.5f, 0.0f);
			kQuadVertices[1] = new Vector3(.5f, -.5f, 0.0f);
			kQuadVertices[2] = new Vector3(.5f, .5f, 0.0f);
			kQuadVertices[3] = new Vector3(-.5f, .5f, 0.0f);
			kQuadUVs = new Vector2[4];
			if (m_bFlipped) {
				kQuadUVs[0] = new Vector2(1, 0);
				kQuadUVs[1] = new Vector2(0, 0);
				kQuadUVs[2] = new Vector2(0, 1);
				kQuadUVs[3] = new Vector2(1, 1);
				kQuadTriangles = new int[] {0, 1, 2, 0, 2, 3};
			}
			else {
				kQuadUVs[0] = new Vector2(0, 0);
				kQuadUVs[1] = new Vector2(1, 0);
				kQuadUVs[2] = new Vector2(1, 1);
				kQuadUVs[3] = new Vector2(0, 1);				
				kQuadTriangles = new int[] {0, 2, 1, 0, 3, 2};
			}
		}
	}
	
						static 		Quad			m_DefaultQuad = new Quad();	//	only use this as a guideline for creating new meshes that are quads
	
	[SerializeField] 	public		Texture			m_Texture;			//	relative to previous window
	[SerializeField] 	public 		AnchorPoint		m_Pivot;
						public		bool			m_bFitWidth=false;		//	Scale this to fit the width of our parent.
						private		Mesh			m_Quad;				//	generate this according to the Sprite given
									bool			m_bUsingSharedQuad = false;
	[SerializeField] 	public
	//private 		
	Rect			m_FrameRect;	//	upon creation, this Sprite will be the size of this FrameRect
	static Material	s_mobileShader = null;
	static Mesh		s_sharedQuad = null;
	static Mesh		s_sharedStretchedQuad = null;
	static Mesh		s_creationMesh = null;
	Mesh			m_creationMesh = null;
	
	static public GameObject CreateSprite3D(Texture tex2D)
	{
		GameObject newSprite3D = CreateSprite3D(tex2D, s_sharedQuad);
		return newSprite3D;
	}
	
	static public GameObject CreateSprite3D(Texture tex2D, Mesh sharedMesh)
	{
		if (tex2D == null) return null;	//	fail bail
		
		s_creationMesh = sharedMesh;
		
		GameObject newSprite3D = new GameObject(tex2D.name+"_Sprite3D");
//		Frame	frame = newSprite3D.AddComponent<Frame>(); // removed due to warning (slc)

		Sprite3D sprite3Dcomponent = newSprite3D.AddComponent<Sprite3D>();
		newSprite3D.AddComponent<Frame>(); // added due to warning - leak? (slc)
		//	sprite3Dcomponent = newSprite3D.GetComponent<Sprite3D>();	//	probably unnecessary
		sprite3Dcomponent.SetTexture(tex2D);
		return newSprite3D;
	}
	
	static public GameObject CreateStretchedSprite3D(Texture tex2D)
	{
		GameObject newSprite3D = CreateSprite3D(tex2D, s_sharedStretchedQuad);
		return newSprite3D;
	}
	
	static public GameObject CreateSprite3D(Texture tex2D, Rect fitInThisRect)
	{
		if (tex2D == null) return null;	//	fail bail
		
		GameObject newSprite3D = CreateSprite3D(tex2D);
		Sprite3D sprite3Dcomponent = newSprite3D.GetComponent<Sprite3D>();
		sprite3Dcomponent.SetFrameRect(fitInThisRect);
		
		return newSprite3D;
	}
	
	public void InitFrameRect()
	{
		if (m_Texture) {
			m_FrameRect = new Rect(0, 0, m_Texture.width, m_Texture.height);
		}
		else {
			m_FrameRect = new Rect(0, 0, 256, 256);
		}
	}
	
	public void Reset()
	{
		Awake();	//	somehow, Reset() may be called before Awake(). So we need to do this to be sure everything is created before we attempt to set defaults
		//this.ResetToDefaultGO();
		/*
		newSprite3D.SetDefaultLike(typeof(Sprite3D));	//	GameObject defaults
		newSprite3D.SetDefaultLikeComponent(null, typeof(Sprite3D));	//	Sprite3D defaults
		newSprite3D.SetDefaultLikeComponent(null, typeof(Frame));		//	Frame defaults
		*/
	}
	
	//	this sets a LOT of things. I don't recommend it at this point because it copies too much from the default sprite.
	//	however, this code serves as an example of how to use reflection to discover all of the properties in a component and set them.
	//	TBA: make this recursive
	//	example: SetAutomaticDefaultLike(defaultSpriteObj, defaultSpriteComponent.GetType());
	public void SetAutomaticDefaultLike(GameObject defaultSpriteObj, System.Type defaultSpriteComponentType)
	{
		GameObject mySpriteObj = this.gameObject;
		var	mySpriteComponent = 	mySpriteObj.GetComponent(defaultSpriteComponentType.ToString());
		var defaultSpriteComponent = defaultSpriteObj.GetComponent(defaultSpriteComponentType.ToString());

		//	find the propertyInfo of this type
		System.Type myType = defaultSpriteComponentType;
		PropertyInfo[] properties;
		properties = myType.GetProperties();
		
		foreach(PropertyInfo prop in properties) {
			if (prop.CanRead && prop.CanWrite) {
				var defaultValue = prop.GetValue(defaultSpriteComponent, null);
				prop.SetValue(mySpriteComponent, defaultValue, null);
				Rlplog.Trace("", "Property "+prop.Name+"="+defaultValue);
			}
			else if (prop.CanRead) {
				Rlplog.Trace("", "Property "+prop.Name);
				System.Type propType = prop.PropertyType;
				var defaultSpriteComponentPropValue = prop.GetValue(defaultSpriteComponent, null);
				var mySpriteComponentPropValue = prop.GetValue(mySpriteComponent, null);
				if ((!defaultSpriteComponentPropValue.Equals(null)) && (!mySpriteComponentPropValue.Equals(null))) {
					PropertyInfo[] propProperties = propType.GetProperties();
					foreach(PropertyInfo propProp in propProperties) {
						if (propProp.CanRead && propProp.CanWrite) {
							var defaultSpriteComponentPropValueActual = propProp.GetValue(defaultSpriteComponentPropValue, null);
							propProp.SetValue(mySpriteComponentPropValue, defaultSpriteComponentPropValueActual, null);
							Rlplog.Trace("", "Property "+prop.Name + "." +propProp.Name+"="+defaultSpriteComponentPropValueActual);
						}
						else {
							Rlplog.Trace("", "Property "+prop.Name + "." + propProp.Name);
						}
					}
				}
				else {
					Rlplog.Trace("", "Property "+prop.Name + "=null");
				}
			}
		}
	}
	
	/*
	 * make this just like the defaultSprite in every way except these:
	 	* m_Texture
	 	* m_Quad		//	automatically created
	 */
	public void SetDefaultLike(GameObject defaultSpriteObj)
	{
		Sprite3D defaultSpriteComponent = defaultSpriteObj.GetComponent<Sprite3D>();
		
		//	SetAutomaticDefaultLike(defaultSpriteObj, defaultSpriteComponent.GetType());

		//this.SetPivot(defaultSpriteComponent.m_Pivot);
	}

	

	private void InitializeMeshRenderer()
	{
		//MeshRenderer mr = GetComponent<MeshRenderer>();
		//Shader shader = Shader.Find("Unlit/Texture");
		if (this.renderer.sharedMaterial == null) {	//	maintain previous material if possible. This allows us to add the Sprite3D component to existing geometry
			/*
			Shader mobileShader = Shader.Find("Mobile/Transparent/Brush");
			if (mobileShader == null) {
				mobileShader = Shader.Find("Unlit/Transparent");	//	fallback default shader
			}
			*/
			//	this.renderer.sharedMaterial = new Material(mobileShader);	//	this creates a memory leak. Don't do this.
			this.renderer.sharedMaterial = s_mobileShader;
		}
		
		//	NOTE: Should use "Unlit/Texture" for non-transparent textures for maximum efficiency
		this.renderer.castShadows = false;
		this.renderer.receiveShadows = false;
		
	}

	//	optional, but not required, is the Frame. One is created on the Sprite3D by default when creating a Sprite3D. But this may be removed by the user later for precise positioning.
	public void AddFrameComponent()
	{
		Frame frameComponent = GetComponent<Frame>();
		if (!frameComponent) {
			frameComponent = this.gameObject.AddComponent<Frame>();
		}
	}
	
	public void Awake()
	{
		//Rlplog.Debug("", "Sprite3D::Awake()" + this.transform.name);
		/*
		if (m_DefaultQuad == null) {
			m_DefaultQuad = new Quad();
		}
		*/
		//	static stuff
		if (s_mobileShader == null) {
			Shader mobileShader = Shader.Find("Mobile/Transparent/Brush");
			if (mobileShader == null) {
				mobileShader = Shader.Find("Unlit/Transparent");	//	fallback default shader
			}

			s_mobileShader = new Material(mobileShader);
		}
		
		if (s_sharedQuad == null) {
			//	initialize the mesh
			s_sharedQuad = CreateNewMesh();
			Vector3[] newVerts = new Vector3[4];
			float xoff = -0.5f;
			float yoff = -0.5f;
			newVerts[0] = new Vector3(xoff+0.0f, yoff+0.0f, 0.0f);
			newVerts[1] = new Vector3(xoff+1.0f, yoff+0.0f, 0.0f);
			newVerts[2] = new Vector3(xoff+1.0f, yoff+1.0f, 0.0f);
			newVerts[3] = new Vector3(xoff+0.0f, yoff+1.0f, 0.0f);
			s_sharedQuad.vertices = newVerts;
			this.ScaleMesh(s_sharedQuad, s_sharedQuad.vertices, 128, 128);
		}
		
		if (s_sharedStretchedQuad == null) {
			s_sharedStretchedQuad = CreateNewMesh();
			Vector2		uvMin, uvMax;
			
			uvMin = new Vector2(0.50f,0);	//	use the center of the brush's texture for the stretch
			uvMax = new Vector2(0.50f,1);
			s_sharedStretchedQuad.uv = GetUVs(uvMin, uvMax);
			
			Vector3[] newVerts = new Vector3[4];
			float xoff = 0.0f;
			float yoff = -0.5f;
			newVerts[0] = new Vector3(xoff+0.0f, yoff+0.0f, 0.0f);
			newVerts[1] = new Vector3(xoff+1.0f, yoff+0.0f, 0.0f);
			newVerts[2] = new Vector3(xoff+1.0f, yoff+1.0f, 0.0f);
			newVerts[3] = new Vector3(xoff+0.0f, yoff+1.0f, 0.0f);
			s_sharedStretchedQuad.vertices = newVerts;
			this.ScaleMesh(s_sharedStretchedQuad, s_sharedStretchedQuad.vertices, 128, 128);
		}
		
		if (m_Pivot == null)
			m_Pivot = new AnchorPoint();
		
		m_bUsingSharedQuad = true;
		CreateMesh();
		InitializeMeshRenderer();
		SetTexture(m_Texture);
	}
	
	Mesh CreateNewMesh()
	{
		Mesh mesh = new Mesh();
		mesh.vertices = m_DefaultQuad.kQuadVertices;
		mesh.uv = m_DefaultQuad.kQuadUVs;
		mesh.triangles = m_DefaultQuad.kQuadTriangles;
		return mesh;
	}
	
	public void CreateMesh()
	{
		if (m_Quad == null) {
			//	initialize the mesh
			if (m_bUsingSharedQuad) {
				m_Quad = s_creationMesh;	//	use prototype quad if possible
				this.m_creationMesh = s_creationMesh;
			}
			else {
				m_Quad = CreateNewMesh();
			}
			GetComponent<MeshFilter>().sharedMesh = m_Quad;
		}
	}
	
	void CleanupMemory()
	{
		//	cleanup memory
		m_Quad = null;
		GetComponent<MeshFilter>().sharedMesh = null;
		this.renderer.sharedMaterial.mainTexture = null;
		this.renderer.sharedMaterial = null;
	}
	
	void OnDestroy()
	{
		CleanupMemory();
	}
	
	//	if we have a collider, force it to be the same as our mesh.
	//	we do this whenever the mesh has changed (probably via SetPivot)
	public void AdjustMeshCollider()
	{
		MeshFilter 		mf = GetComponent<MeshFilter>();
		if (mf) {
			MeshCollider mc = mf.GetComponent<MeshCollider>();
			if (mc) {
				mc.sharedMesh = mf.sharedMesh;
				mc.sharedMesh.RecalculateBounds();
			}
		}
	}

	/*
				kQuadUVs[0] = new Vector2(1, 0);
				kQuadUVs[1] = new Vector2(0, 0);
				kQuadUVs[2] = new Vector2(0, 1);
				kQuadUVs[3] = new Vector2(1, 1);
	*/
	public Vector2[] GetUVs(Vector2 uvmin, Vector2 uvmax)
	{
		Vector2[] newUVs = new Vector2[4];
		newUVs[0] = new Vector2(uvmax.x, uvmin.y);
		newUVs[1] = new Vector2(uvmin.x, uvmin.y);
		newUVs[2] = new Vector2(uvmin.x, uvmax.y);
		newUVs[3] = new Vector2(uvmax.x, uvmax.y);
		return newUVs;
	}
	
	public void SetUVs(Vector2 uvmin, Vector2 uvmax)
	{
		
		if (m_bUsingSharedQuad) {
			m_Quad = CreateNewMesh();
			m_bUsingSharedQuad = false;
		}
		
		m_Quad.uv = GetUVs(uvmin, uvmax);
	}
	
	/*
	public void AdjustUVs()
	{
		RectUVMapper uvmapper = GetComponent<RectUVMapper>();
		if (uvmapper) {		//	if we have a custom UV mapper, use those UVs
			m_Quad.uv = uvmapper.GetUVs();
		}
		else {
			m_Quad.uv = m_DefaultQuad.kQuadUVs;	//	otherwise, use the default UVs
		}
	}
	*/
	public void Start()
	{
		Rlplog.Trace("", "Sprite3D::Start(): " + this.transform.name);
		if (m_Texture) {
			SetTexture(m_Texture);
		}
	}
	
	public void SetTexture(Texture tex)
	{
		m_Texture = tex;
		if (this.renderer && this.renderer.sharedMaterial) {
			this.renderer.sharedMaterial.mainTexture = m_Texture;
		}
		InitFrameRect();
		//SetPivot(m_Pivot);
		//MeshRenderer mr = GetComponent<MeshRenderer>();
		//mr.material.SetTexture("_MainTex", m_Texture);
		//name = m_Texture.name + "_Sprite3D";
	}	
	
	//	remember to Refresh() after doing this
	public void SetFrameRect(Rect rect)
	{
		this.m_FrameRect = rect;
	}
	
	public Texture GetTexture()
	{
		return m_Texture;
	}
	
	public void Refresh()
	{
		//SetPivot(m_Pivot);
	}

	//	script methods
	public void ActivateRecursively()
	{
		this.gameObject.ActivateRecursively(null);
	}		
	public void DeactivateRecursively()
	{
		this.gameObject.DeactivateRecursively(null);
	}		
	public void Activate()
	{
		this.gameObject.Activate(null);
	}		
	public void Deactivate()
	{
		this.gameObject.Deactivate(null);
	}	
	
	public Rect GetFrameRect()
	{
		return m_FrameRect;
	}
	
	public void ScaleMesh(Mesh mesh, Vector3[] unitVerts, float width, float height)
	{
		Vector3[] verts = new Vector3[4];
		for(int ii=0; ii<4; ii++) {
			verts[ii].x = unitVerts[ii].x * width;
			verts[ii].y = unitVerts[ii].y * height;
		}
		mesh.vertices = verts;
	}
	
	public void ScaleSprite(float width, float height)
	{
		ScaleMesh(m_Quad, m_creationMesh.vertices, width, height);
		GetComponent<MeshFilter>().sharedMesh = m_Quad;
	}
	
	public void ScaleSpriteToTexture(Texture2D tex2D)
	{
		if (tex2D==null) return;	//	early bail
		float width=tex2D.width, height=tex2D.height;
		ScaleSprite(width, height);
	}

	public void SetPivot(AnchorPoint pv)
	{
		System.String msg = "Sprite3D::SetPivot: " + this.transform.name + " - (" + pv.horiz.ToString() + ", " + pv.vert.ToString() + ")\n";
		Rlplog.Trace("", msg);
		m_Pivot.SetAnchor(pv);
		
		float xoff=0.0f, yoff=0.0f;
		
		Vector2 relOffset = m_Pivot.GetUnitizedRelativeOffset();
		
		xoff = -relOffset.x;
		yoff = -relOffset.y;		

		Vector3[] verts = new Vector3[4];
		
		verts[0] = new Vector3(xoff+this.m_creationMesh.vertices[0].x, yoff+this.m_creationMesh.vertices[0].y, 0.0f);
		verts[1] = new Vector3(xoff+this.m_creationMesh.vertices[1].x, yoff+this.m_creationMesh.vertices[1].y, 0.0f);
		verts[2] = new Vector3(xoff+this.m_creationMesh.vertices[2].x, yoff+this.m_creationMesh.vertices[2].y, 0.0f);
		verts[3] = new Vector3(xoff+this.m_creationMesh.vertices[3].x, yoff+this.m_creationMesh.vertices[3].y, 0.0f);	
		
		float width=256.0f, height=256.0f;
		if (this.m_Texture != null) { // removed due to warning - will always be true (slc)
			width = m_FrameRect.width;
			height = m_FrameRect.height;
		}
		
		if (m_bFitWidth) {
			if (this.gameObject.transform.parent != null) {
				Frame parentFrame = this.gameObject.transform.parent.gameObject.GetComponent<Frame>();
				if (parentFrame != null) {
					width = parentFrame.m_FrameRect.width;
				}
			}
		}
		
		if (this.m_Texture != null) {	//	do this because we offset from m_Quad.vertices above.
			for(int ii=0; ii<4; ii++) {
				verts[ii].x = verts[ii].x * width;
				verts[ii].y = verts[ii].y * height;
			}
		}
		//	CreateMesh();		//	this should be created in Awake()

		m_Quad.vertices = verts;
		m_Quad.RecalculateBounds();
		m_Quad.RecalculateNormals();
		GetComponent<MeshFilter>().sharedMesh = m_Quad;
		AdjustMeshCollider();
//		AdjustUVs();
		
		//	apply transforms - Only the scale is needed because we are relative to the gameObject.transform anyway
		Vector2 	framePos = new Vector2(verts[0].x, verts[0].y);
		//	translation
		//framePos.x += this.transform.localPosition.x;
		//framePos.y += this.transform.localPosition.y;
		//	scale
		//width *= this.transform.lossyScale.x;
		//height *= this.transform.lossyScale.y;
		//	rotation
		//	TBA if we need it
		
		m_FrameRect = new Rect(framePos.x, framePos.y, width, height);
		Frame frame = GetComponent<Frame>();
		if (frame != null) {
			frame.SetFrameRect(m_FrameRect);
			//msg = this.transform.name + ": SetFrameRect(" + m_FrameRect.ToString() + ")\n";
			//Rlplog.Trace("", msg);
		}
	}
	
	/*
	//	hack: for seeing if the sprite is getting collision
    public virtual void OnMouseEnter()
    {
		Rlplog.Trace("", this.name + ": OnMouseEnter\n");
		
		MeshCollider mc = GetComponent<MeshCollider>();
		if (mc) {
			//mc.enabled = false;
			Rlplog.Debug("", this.name + ": MeshCollider enabled=false, layer = " + mc.gameObject.layer.ToString());
			//mc.gameObject.layer = 21;	//	no collis layer
			//Vector3 noscale = new Vector3(0,0,0);
			//mc.isTrigger = true;
		}
    }
	 */

}