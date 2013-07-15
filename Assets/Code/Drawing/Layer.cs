using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable] // Required so it shows up in the inspector 
[AddComponentMenu ("DPict/Layer")]
public class Layer : MonoBehaviour 
{
	public Texture		m_myTexture;
	public Texture2D	m_myBackground;			//	used when clearing
	Texture2D			m_myTexture2D;
	RenderTexture		m_myRenderTexture;
	public Camera		m_myCamera = null;		//	used for collisions
	Color[]				m_pixelLayer;
	public int			m_textureWidth;
	public int			m_textureHeight;
	int					m_maxPoints=6;
	Vector3[]			m_prevPoints = null;
	int					m_frameCounter;
	bool				m_bIsDrawing;
	float				m_BrushLineDensity = 3.5f;
	bool				m_bIsSubscriberOfTouchListener;
	
	List<GameObject>	m_spriteList = new List<GameObject>();
	
	public Camera		m_RenderCamera;
	
	//	use slow render for brushes and other things that need to directly modify the texture
	public bool m_bFastRender = true;	//	uses polygons rather than direct texture access to draw brushes
	public bool m_bStretchSpriteRender = true;	//	stretch a brush across the space between user input points
	
	/*
	//	brush stuff
	float				m_blendValue = 0.333f;
	int					m_brushWidth = 8;
	Color 				m_brushColor = Color.blue;
	Color[] 			color_palette;
	int					m_curColorIndex = 0;
	int 				m_maxColors = 7;
	*/
	
	public Brush		m_myBrush;
	
	static Layer		m_currentLayer;
	static List<Layer>	m_layerList = new List<Layer>();
	
	void Awake()
	{
		/*
		color_palette = new Color[m_maxColors];
		color_palette[0] = Color.blue;
		color_palette[1] = Color.red;
		color_palette[2] = Color.green;
		color_palette[3] = Color.yellow;
		color_palette[4] = Color.white;
		color_palette[5] = Color.black;
		color_palette[6] = Color.gray;
		*/
		
		if (this.tag == "DrawingLayer") {
			m_currentLayer = this;
		}
		m_layerList.Add(this);
		m_bIsDrawing = false;
	
		m_prevPoints = new Vector3[m_maxPoints];
	    renderer.material.mainTexture = InstantiateTexture();
		Clear();
		
		//	determine whether we are a subscriber to a touch listener which will be sending us the Mouse messages in lieu of Unity's messages
		m_bIsSubscriberOfTouchListener = false;
		Subscriber sub = this.GetComponent<Subscriber>();
		if (sub != null) {
			Publisher pub = sub.GetPublisher();
			TouchListener tl = pub.GetComponent<TouchListener>();
			if (tl != null) {
				if (!tl.isTouchEnabled()) {
					m_bIsSubscriberOfTouchListener = true;
				}
			}
		}
	}
	
	Texture InstantiateTexture()
	{
		Texture texture = Instantiate(renderer.material.mainTexture) as Texture;
	    
		if (texture != null) {
			m_myTexture2D = texture as Texture2D;
			
			m_myRenderTexture = texture as RenderTexture;
			
			m_myTexture = texture;
			m_textureWidth = texture.width;
			m_textureHeight = texture.height;
			
			if (m_myTexture2D != null) {
				Clear(m_myTexture2D, 0, Color.white);
				m_pixelLayer = m_myTexture2D.GetPixels(0);
			}
			
			if (m_myRenderTexture != null) {
				if (m_RenderCamera != null) {
					m_RenderCamera.targetTexture = m_myRenderTexture;
					m_RenderCamera.isOrthoGraphic = true;
					m_RenderCamera.aspect = (float)m_myRenderTexture.width/(float)m_myRenderTexture.height;
					m_RenderCamera.orthographicSize = m_myRenderTexture.height / 2;
					m_RenderCamera.ResetAspect();
				}
			}
		}
		
		return texture;
	}
	
	// Use this for initialization
	void Start () 
	{
	
		//m_prevPoints = new Vector3[m_maxPoints];
		m_frameCounter = 0;
	}
	
	static public Brush GetBrush()
	{
		return m_currentLayer.m_myBrush;
	}
	
	static public void SetBrush(Brush brush)
	{
		m_currentLayer.m_myBrush = brush;
	}
	
	static public void SetBrushColor(Color brushColor)
	{
		//m_currentLayer.m_brushColor = brushColor;
		m_currentLayer.m_myBrush.SetBrushColor(brushColor);
	}
	
	static public void SetBrushSize(int sz)
	{
		//m_currentLayer.m_brushWidth = sz;
		m_currentLayer.m_myBrush.SetBrushSize(sz);
	}
	
	public void OnMouseDownListener(int buttonNo)
	{
		m_bIsDrawing = true;
		StartPoint(GetPoint());
	}
	
	public void OnMouseUpListener(int buttonNo)
	{
		DrawSegments();
		m_bIsDrawing = false;
	}
	
	public void OnMouseDown()
	{
		//	if I'm a subscriber and my publisher is a TouchListener, then ignore this message because I'll already get one from my publisher.
		if (!m_bIsSubscriberOfTouchListener) {
			OnMouseDownListener(0);
		}
	}
	
	public void OnMouseUp()
	{
		if (!m_bIsSubscriberOfTouchListener) {
			OnMouseUpListener(0);
		}
	}
	
	/*
	public void OnMouseEnterListener(int buttonNo)
	{
		m_bIsDrawing = true;
		StartPoint(GetPoint());
	}

	public void OnMouseExitListener(int buttonNo)
	{
		DrawSegments();
		m_bIsDrawing = false;
	}
	*/
	
	public void PaintBrush(Vector3 pos, Brush brush)
	{
		Color color = brush.GetBrushColor();
		int brushWidth = brush.GetBrushSize();
		float blendValue = brush.GetBlendValue();
		bool bSoftEdges = true;
		if (brush.m_BrushStyle == Brush.BrushStyle.BrushStyle_HardEdges) {
			bSoftEdges = false;
		}
		PaintBrush(pos, color, brushWidth, blendValue, bSoftEdges);
	}
	
	void PaintBrush(Vector3 pos, Color color, int brushWidth, float brushBlendValue, bool bSoftEdges)
	{
		int xStart = (int)pos.x;
		int yStart = (int)pos.y;
		float brushWidthRadiusSquared = (float)(brushWidth*brushWidth);
		float ratio;
		float fRadiusSq;
		float blendValue;
		
		for(int yy=-brushWidth; yy<=+brushWidth; yy++) {
			for(int xx=-brushWidth; xx<=+brushWidth; xx++) {
				if (xx*xx+yy*yy<=brushWidth*brushWidth) {
					if ((xx+xStart>=0) && (yy+yStart>=0) && (xx+xStart<m_textureWidth) && (yy+yStart<m_textureHeight)) {
						if (bSoftEdges == true) {
							fRadiusSq = (float)(xx*xx+yy*yy);
							ratio = fRadiusSq/brushWidthRadiusSquared;
							blendValue = (1.0f-ratio) * brushBlendValue;
						}
						else {
							blendValue = 1.0f * brushBlendValue;
						}
						color.a = blendValue;	//	use hardware blending
						m_pixelLayer[xx+xStart + (yy+yStart)*m_textureWidth] = Color.Lerp(m_pixelLayer[xx+xStart + (yy+yStart)*m_textureWidth], color, blendValue);
					}
				}
			}
		}
	}
	
	public void StartPoint(Vector3 pt)
	{
		for(int ii=0; ii<m_prevPoints.Length; ii++) {
			m_prevPoints[ii] = pt;
		}
	}
	
	void AddPreviousPoint(Vector3 pt)
	{
		for(int ii=0; ii<m_prevPoints.Length-1; ii++) {
			m_prevPoints[ii] = m_prevPoints[ii+1];
		}
		m_prevPoints[m_prevPoints.Length-1] = pt;
	}
	
	public struct BoxExtents
	{
		public Vector3 max;
		public Vector3 min;
	}
	
	void InterpolatePreviousPoints(int idx0, int idx1)
	{
		Vector3 newPt, prevPt;
		float fBrushWidth = (float)m_myBrush.GetBrushSize();
		
		float	interpolationAmount = 0.1f;
		float 	distBetweenPts;
		float	nInterpolations;
		
		for(int ii=idx0; ii<idx1; ii++) {		//	actual points
			if (m_bStretchSpriteRender) {
				CreateStretchedBrushGO(m_myBrush, m_prevPoints[ii], m_prevPoints[ii+1]);
			}
			else {
				for(float p=0.0f; p<1.0f; p+=interpolationAmount) {	//	interpolation between points
					distBetweenPts = Vector3.Distance(m_prevPoints[ii], m_prevPoints[ii+1]);
					nInterpolations = m_BrushLineDensity * distBetweenPts / fBrushWidth;
					if (nInterpolations == 0) {
						nInterpolations = 1;
						interpolationAmount = 1.0f;
					}
					else {
						interpolationAmount = 1.0f / nInterpolations;	//	make sure there's enough to fill in-between spaces.
					}
					newPt = Vector3.Lerp(m_prevPoints[ii], m_prevPoints[ii+1], p);
					if (this.m_bFastRender == true) {
						CreateBrushGO(m_myBrush, newPt);
					}
					else {
						PaintBrush(newPt, m_myBrush);
					}
					prevPt = newPt;
				}
			}
		}
	}
	
	Vector3 GetPoint()
	{
		Vector3 hitPoint = Vector3.zero;
		if (m_myCamera == null) 	//	early bail
			return hitPoint;
		
		//	there seems to be a bug where the ViewPort of the editor camera is used rather than the camera I've specified here.
		//	Ray ray = m_myCamera.ScreenPointToRay(Input.mousePosition);		//	for perspective camera
		Vector3 mousePos = Input.mousePosition;	//	this is the mouse position on the screen. this seems okay. However, it is in actual pixels. So the editor window size affects this value. Also, the camera we render to may not be the same one as we used for the Mouse Position!
		//	we need to figure out the mouse position if it were in this camera's viewport. The above mouse position is for the screen camera's viewport
		Camera mainCamera = Camera.main;	//	main camera is the one we use for mouse pick stuff
		/*
		Vector3 mouseViewportPos = mainCamera.ScreenToViewportPoint(mousePos);	//	but this is not the viewport of the Editor window camera.
		Ray ray = mainCamera.ViewportPointToRay(mouseViewportPos);		//	for perspective camera
		*/
		//	in general, mouseViewportPos = mousePos / texture(width, height)
		Ray ray = mainCamera.ScreenPointToRay(mousePos);
		RaycastHit hit;
		bool		bUseRelative2DCoords = !m_bFastRender;
		if (Physics.Raycast(ray, out hit, m_myCamera.farClipPlane, m_myCamera.cullingMask)) {
			hitPoint = hit.point;
			float rayLen = this.transform.position.z - ray.origin.z;
			//	Debug.DrawRay(ray.origin, ray.direction * rayLen, Color.red);
			float xoffset = 0.0f;
			float yoffset = 0.0f;
			int width = m_myTexture.width;
			int height = m_myTexture.height;
			if (bUseRelative2DCoords) {
				xoffset = (float)width/2.0f;
				yoffset = (float)height/2.0f;
			}
			//	old way
			bool bReverseX = false;
			bool bReverseY = !m_bFastRender;
			hitPoint.x = (hit.point.x + xoffset);	//	in the shader for this gameObject, use tiling x=-1
			hitPoint.y = (hit.point.y + yoffset);
			if (bReverseX) {
				hitPoint.x = (float)width - hitPoint.x;
			}
			if (bReverseY) {
				hitPoint.y = (float)height - hitPoint.y;
			}
		}
		else {
			hitPoint = m_prevPoints[m_prevPoints.Length-1];		//	use previous point if we're off screen
		}
		return hitPoint;
	}
	
	bool m_bDestroyAllSprites = false;
	
	void DestroyAllSprites()
	{
		if (m_bDestroyAllSprites == true) {
			foreach(GameObject go in m_spriteList)
			{
				Destroy(go, 1.0f/ 10.0f);
			}
			m_spriteList.Clear();
		}
		m_bDestroyAllSprites = false;
	}
	
	GameObject CreateBrushGO(Brush brush, Vector3 worldPos)
	{
		GameObject spriteGO = Sprite3D.CreateSprite3D(brush.GetTexture());
		spriteGO.transform.position = worldPos;
		spriteGO.transform.parent = this.transform;
		spriteGO.layer = this.gameObject.layer;
		m_spriteList.Add(spriteGO);
		return spriteGO;
	}
	
	GameObject CreateStretchedBrushGO(Brush brush, Vector3 endPt1, Vector3 endPt2)
	{
		Vector3 midPt = (endPt1 + endPt2) / 2.0f;
		Vector3 diffPt = endPt2 - endPt1;
		float len = diffPt.magnitude;		//	difference between pt1 and pt2.
		GameObject spriteGO = CreateBrushGO(brush, midPt);
		Sprite3D sprite = spriteGO.GetComponent<Sprite3D>();		//	scale of each sprite is 1.0 with xmin=-0.5, xmax=0.5
		Vector2		uvMin, uvMax;
		
		uvMin = new Vector2(0.49f,0);	//	use the center of the brush for the stretch
		uvMax = new Vector2(0.51f,1);
		sprite.SetUVs(uvMin, uvMax);
		Transform xform = spriteGO.transform;
		//	figure out the rotation
		float angle = Mathf.Atan2(diffPt.normalized.y, diffPt.normalized.x);
		if (angle != 0.0f) {
			Vector3 newEulerAngles = Vector3.zero;
			newEulerAngles.z = angle * Mathf.Rad2Deg;
			spriteGO.transform.localEulerAngles = newEulerAngles;
		}
		
		//	figure out the scale
		Vector3 newScale = spriteGO.transform.localScale;
		float spriteWidth = sprite.m_Texture.width;
		len -= 1;
		len /= spriteWidth;
		float scale = len;			//	scale should be 1.0, not 0.0 if pt1 and pt2 are the same.
		if (scale < 0.0f)
			scale = 0.0f;
		newScale.x = scale;
		spriteGO.transform.localScale = newScale;
		
		//	now create two dots at the endpoints
		CreateBrushGO(brush, endPt1);
		CreateBrushGO(brush, endPt2);
		return spriteGO;
	}
	
	int		m_bakeEveryNFrames = 4;
	public void DrawSegments()
	{
		int		buffer = 2;
		int		nSegments = 3;
		
		bool	bSlowCPUTextureUpdate = !m_bFastRender;
		
		if (bSlowCPUTextureUpdate && m_myTexture2D) {
			InterpolatePreviousPoints(m_maxPoints-buffer-nSegments, m_maxPoints-buffer);
			//m_myTexture.SetPixels(0, (int)extents.min.y, 1024, (int)(extents.max.y-extents.min.y), m_pixelLayer);
			//m_myTexture.SetPixels(0, 0, 1024, (int)(extents.max.y-extents.min.y), m_pixelLayer);
			m_myTexture2D.SetPixels(m_pixelLayer, 0);
			m_myTexture2D.Apply();
		}
		else {
			/*
			GameObject spriteGO = Sprite3D.CreateSprite3D(m_myBrush.GetTexture());
			spriteGO.transform.parent = this.transform;
			Vector3 localpos = m_prevPoints[m_prevPoints.Length-1];
			spriteGO.transform.localPosition = m_prevPoints[m_prevPoints.Length-1];
			spriteGO.layer = this.gameObject.layer;
			*/
			/*
			GameObject spriteGO = CreateBrushGO(m_myBrush, m_prevPoints[m_prevPoints.Length-1]);
			m_spriteList.Add(spriteGO);
			*/
			InterpolatePreviousPoints(m_maxPoints-buffer-nSegments, m_maxPoints-buffer);
			//Bake();
		}
	}
	
	void Bake()
	{
		if (this.m_RenderCamera != null) {
			int curFrameNo = Time.frameCount;
			if (curFrameNo%m_bakeEveryNFrames==0) {
				LayerBake baker = m_RenderCamera.GetComponent<LayerBake>();
				if (baker != null) {
					baker.Dirty();
					m_bDestroyAllSprites = true;
				}
			}
		}
	}
	
	public void Clear()
	{
		if (m_myBackground != null) {
			Copy(m_myBackground, m_myTexture);
		}
		else {
			if (m_myTexture2D != null) {
				Color clearColor = Color.white;
				clearColor.a = 0.0f;
				Clear(this.m_myTexture2D, 0, clearColor);
			}
		}
	}
	
	public void Clear(Texture2D texture, int mip, Color color)
	{
	    // tint each mip level
		Color[] pixelLayer = m_pixelLayer;
		if (m_pixelLayer==null)
			pixelLayer = texture.GetPixels(mip);
        var cols = pixelLayer;
        for( var i = 0; i < cols.Length; ++i ) {
            //	cols[i] = Color.Lerp( cols[i], colors[mip], 0.33f );
			cols[i] = color;
        }
        texture.SetPixels( cols, mip );
	
	    // actually apply all SetPixels, don't recalculate mip levels
	    texture.Apply( false );
	}
	
	void Copy(Texture2D dest, Texture src)
	{
		Texture2D src2D = src as Texture2D;
		if (src2D != null) {
			Copy(dest, src2D);
		}
		else {
			RenderTexture srcRT = src as RenderTexture;
			if (srcRT != null) {
				Copy(dest, srcRT);
			}
		}
	}
	
	void Copy(Texture2D dest, Texture2D src)
	{
		Color[] colors = src.GetPixels();
		dest.SetPixels(colors);
	}
	
	void Copy(Texture2D dest, RenderTexture src)
	{
		RenderTexture.active = src;
		dest.ReadPixels(new Rect(0, 0, src.width, src.height), 0, 0);
		dest.Apply();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (m_bIsDrawing && (Input.GetMouseButton(0) == true)) {

			AddPreviousPoint(GetPoint());

			if (m_frameCounter==3) {
				DrawSegments();
				m_frameCounter = 0;
			}
			m_frameCounter++;
		}
		/*
		//	right clicked
		if (Input.GetMouseButtonDown(1) == true) {
			m_curColorIndex++;
			if (m_curColorIndex>=m_maxColors) {
				m_curColorIndex = 0;
			}
			m_brushColor = color_palette[m_curColorIndex];
		}
		*/
		DestroyAllSprites();
		Bake();
	}	
}
