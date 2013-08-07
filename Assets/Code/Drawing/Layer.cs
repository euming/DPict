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
	
	public float		m_SpritePersistTime = 1.0f / 30.0f;	//	how long sprites should live
	Color[]				m_pixelLayer;
	public int			m_textureWidth;
	public int			m_textureHeight;
	int					m_maxPoints=128;
	Vector3[]			m_prevPoints = null;
	List<Vector3>		m_prevPointsList = new List<Vector3>();
	int					m_frameCounter;
	bool				m_bIsDrawing;
	float				m_BrushLineDensity = 2.75f;	//	2.75 = best mix of performance and quality
	bool				m_bIsSubscriberOfTouchListener;
	
	List<GameObject>	m_spriteList = new List<GameObject>();
	
	public Camera		m_RenderCamera;
	
	//	use slow render for brushes and other things that need to directly modify the texture
	public bool m_bFastRender = true;	//	uses polygons rather than direct texture access to draw brushes
	public bool m_bStretchSpriteRender = true;	//	stretch a brush across the space between user input points
	public int	m_numStretchSprites = 2;
	public float m_userInputSampleRate = 90.0f;		//	sample user imput this many times per second
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
	
	//	debugging stuff
	public bool			m_bDrawEndPts = true;
	public int			m_numPoints = 0;
	public bool			m_bDebugDontDestroy = false;
	
	static Layer		m_currentLayer;
	static List<Layer>	m_layerList = new List<Layer>();
	
	void Awake()
	{
		Time.fixedDeltaTime = 1.0f/m_userInputSampleRate;	//	user input update time
		
		if (this.tag == "DrawingLayer") {
			m_currentLayer = this;
		}
		m_layerList.Add(this);
		m_bIsDrawing = false;
	
		m_prevPoints = new Vector3[m_maxPoints];
	    renderer.material.mainTexture = InstantiateTexture();
		Clear();
		
		Bake();
		
		//	determine whether we are a subscriber to a touch listener which will be sending us the Mouse messages in lieu of Unity's messages
		m_bIsSubscriberOfTouchListener = TouchListener.isSubscriberOfTouchListener(this.gameObject);
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
		if (this.enabled) {
			m_bIsDrawing = true;
			StartPoint(GetPoint());
		}
	}
	
	public void OnMouseUpListener(int buttonNo)
	{
		if (this.enabled) {
			m_bIsDrawing = false;
			DrawSegments();
			m_prevPointsList.Clear();
		}
	}
	
	public void OnMouseDown()
	{
		if (this.enabled) {
			//	if I'm a subscriber and my publisher is a TouchListener, then ignore this message because I'll already get one from my publisher.
			if (!m_bIsSubscriberOfTouchListener) {
				OnMouseDownListener(0);
			}
		}
	}
	
	public void OnMouseUp()
	{
		if (this.enabled) {
			if (!m_bIsSubscriberOfTouchListener) {
				OnMouseUpListener(0);
			}
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
							ratio = (fRadiusSq/brushWidthRadiusSquared);
							blendValue = Mathf.Sqrt(1.0f-ratio);	//	 * brushBlendValue;
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
		m_prevPointsList.Clear();
		m_prevPointsList.Add(new Vector3(pt.x, pt.y, pt.z));
	}
	
	void AddPreviousPoint(Vector3 pt)	//	add to the end of the list
	{
		for(int ii=0; ii<m_prevPoints.Length-1; ii++) {
			m_prevPoints[ii] = m_prevPoints[ii+1];
		}
		m_prevPoints[m_prevPoints.Length-1] = pt;
		m_prevPointsList.Add(new Vector3(pt.x, pt.y, pt.z));
		m_numPoints = m_prevPointsList.Count;
	}
	
	public struct BoxExtents
	{
		public Vector3 max;
		public Vector3 min;
	}
	
	Vector3 GetSafePoint(int index)
	{
		if (index >= m_prevPointsList.Count) {
			index = m_prevPointsList.Count-1;
		}
		else if (index < 0) {
			index = 0;
		}
		return m_prevPointsList[index];
	}
	void InterpolatePreviousPoints(int idx0, int idx1)
	{
		Vector3 newPt, prevPt;
		float fBrushWidth = (float)m_myBrush.GetBrushSize();
		
		float	interpolationAmount = 0.1f;
		float 	distBetweenPts;
		float	nInterpolations;
		
		if (idx0==idx1) {
			CreateBrushGO(m_myBrush, m_prevPointsList[0], false);
		}
		else {
			for(int ii=idx0; ii<idx1; ii++) {		//	actual points
				if (m_bStretchSpriteRender) {
					Vector3 endPt2 = GetSafePoint(ii+1);
					Vector3 prevEndPt1 = GetSafePoint(ii-1);
					Vector3 prevEndPt2 = GetSafePoint(ii);
					CreateStretchedBrushGO(m_myBrush, m_prevPointsList[ii], endPt2, prevEndPt1, prevEndPt2, m_numStretchSprites);
				}
				else {
					for(float p=0.0f; p<1.0f; p+=interpolationAmount) {	//	interpolation between points
						distBetweenPts = Vector3.Distance(m_prevPointsList[ii], m_prevPointsList[ii+1]);
						nInterpolations = m_BrushLineDensity * distBetweenPts / fBrushWidth;
						if (nInterpolations == 0) {
							nInterpolations = 1;
							interpolationAmount = 1.0f;
						}
						else {
							interpolationAmount = 1.0f / nInterpolations;	//	make sure there's enough to fill in-between spaces.
						}
						newPt = Vector3.Lerp(m_prevPointsList[ii], m_prevPointsList[ii+1], p);
						if (this.m_bFastRender == true) {
							CreateBrushGO(m_myBrush, newPt, false);
						}
						else {
							PaintBrush(newPt, m_myBrush);
						}
						prevPt = newPt;
					}
				}
			}
		}
	}
	
	float m_zoffset = -10.0f;
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
			hitPoint = m_prevPointsList[m_prevPointsList.Count-1];		//	use previous point if we're off screen
		}
		
		hitPoint.z += m_zoffset;
		return hitPoint;
	}
	
	bool m_bDestroyAllSprites = false;
	
	public void DestroyAllSprites(float time)
	{
		if (m_bDebugDontDestroy)
			return;	//	test not destroying stuff
		if (m_bDestroyAllSprites == true) {
			foreach(GameObject go in m_spriteList)
			{
				if (time==0.0f) {
					DestroyImmediate(go);
				}
				else {
					Destroy(go, time);
				}
			}
			m_spriteList.Clear();
		}
		m_bDestroyAllSprites = false;
	}
	
	GameObject CreatePatchTriangle(Brush brush, Vector3 worldPos)
	{
		GameObject spriteGO = null;
		spriteGO = Sprite3D.CreatePatchTriangleSprite3D(brush.GetTexture());
		spriteGO.transform.position = worldPos;
		spriteGO.transform.parent = this.transform;
		spriteGO.layer = this.gameObject.layer;
		m_spriteList.Add(spriteGO);
		return spriteGO;
	}
	
	GameObject CreateBrushGO(Brush brush, Vector3 worldPos, bool bStretchedSprite)
	{
		GameObject spriteGO = null;
		if (bStretchedSprite) {
			spriteGO = Sprite3D.CreateStretchedSprite3D(brush.GetTexture());
		}
		else {
			spriteGO = Sprite3D.CreateSprite3D(brush.GetTexture());
		}
		spriteGO.transform.position = worldPos;
		spriteGO.transform.parent = this.transform;
		spriteGO.layer = this.gameObject.layer;
		m_spriteList.Add(spriteGO);
		return spriteGO;
	}
	
	int spriteNo = 0;
	
	GameObject CreateStretchedBrushGO(Brush brush, Vector3 endPt1, Vector3 endPt2, Vector3 prevEndPt1, Vector3 prevEndpt2, int numStretchedSprites)
	{
//		Vector3 midPt = (endPt1 + endPt2) / 2.0f;
		Vector3 diffPt = endPt2 - endPt1;
		Vector3 prevDiffPt = prevEndpt2 - prevEndPt1;
		float len = diffPt.magnitude;		//	difference between pt1 and pt2. in screen pixel units (and world space)
		//Vector2		uvMin, uvMax;
		
		//uvMin = new Vector2(0.50f,0);	//	use the center of the brush's texture for the stretch
		//uvMax = new Vector2(0.50f,1);
		//Vector3 startPt1 = midPt;
		Vector3 startPt1 = endPt1;
		GameObject spriteGO = null;
		//	angle segment in world space
		float angle = Mathf.Atan2(diffPt.normalized.y, diffPt.normalized.x);
		//	figure out the angle (theta) between the two segments, current and previous
		float prevAngle = Mathf.Atan2(prevDiffPt.normalized.y, prevDiffPt.normalized.x);
		//	difference between previous and current.
		//float theta = angle - prevAngle;
		float theta = Mathf.DeltaAngle(prevAngle*Mathf.Rad2Deg, angle*Mathf.Rad2Deg) * Mathf.Deg2Rad;
		//	need to push the current start point forward a bit according to the angle
		float brushWidth = this.m_myBrush.m_brushWidth;
		float	extraPatchWidth = brushWidth/2.0f;
		if (theta < 0.0f) theta = -theta;	//	no negative angles
		float pushDist = brushWidth * Mathf.Tan(theta / 2.0f);
		//pushDist = 0.0f;
		startPt1 += diffPt.normalized * pushDist;	//	push forward a little bit
			spriteGO = CreateBrushGO(brush, startPt1, true);
			Sprite3D sprite = spriteGO.GetComponent<Sprite3D>();		//	scale of each sprite is 1.0 with xmin=-0.5, xmax=0.5
			//sprite.SetUVs(uvMin, uvMax);
			Transform xform = spriteGO.transform;
			//	figure out the rotation of drawSegment in screen space
			if (angle != 0.0f) {	//	rotate our segment
				Vector3 newEulerAngles = Vector3.zero;
				newEulerAngles.z = angle * Mathf.Rad2Deg;
				spriteGO.transform.localEulerAngles = newEulerAngles;
			}
			
			//	name this sprite
			string newName = spriteNo + "- prev: " + prevAngle + ", cur: " + angle;
			spriteNo++;
			spriteGO.name = newName;
			//	figure out the scale
			Vector3 newScale = spriteGO.transform.localScale;
			float spriteWidth = sprite.m_Texture.width;
			len -= extraPatchWidth + pushDist;		//	leave a bit of room for the patch. Also, subtract the amount we pushed forward so the patch always starts from the same point
			len /= spriteWidth;			//	since our brush is unit size, we need to change our units for the scale accordingly.
			float scale = len;			//	scale should be 1.0, not 0.0 if pt1 and pt2 are the same.
			//scale -= 0.5f;				//	subtract half a brush width for patch triangle(s)
			if (scale < 0.0f)
				scale = 0.01f;

		/*
			//	breakpoint for debugging
			if (scale > 1.0f) {
				newScale.y = 1.0f;
			}
		*/
			newScale.x = scale;
			spriteGO.transform.localScale = newScale;		//	this is the number of "brush widths" long
			m_spriteList.Add(spriteGO);
		
		/*
		//	create a marker sprite
		Sprite3D markerSprite = Instantiate(sprite) as Sprite3D;
		markerSprite.transform.position = endPt1;
		newScale.x = 0.001f;
		markerSprite.transform.localScale = newScale;
		for(int ii=0; ii<numStretchedSprites; ii++) {
			Sprite3D newSprite = Instantiate(sprite) as Sprite3D;
			//newSprite.SetUVs(uvMin, uvMax);
			m_spriteList.Add(newSprite.gameObject);
		}
		*/
		
		//	create patch sprite
		Sprite3D patchSprite = Instantiate(sprite) as Sprite3D;
		Vector3 patchStartPt1 = prevEndpt2 + prevDiffPt.normalized * (-extraPatchWidth);
		patchSprite.transform.position = patchStartPt1;
		if (prevAngle != 0.0f) {	//	rotate our segment
			Vector3 newEulerAngles = Vector3.zero;
			newEulerAngles.z = prevAngle * Mathf.Rad2Deg;
			patchSprite.transform.localEulerAngles = newEulerAngles;
			patchSprite.transform.parent = this.transform;			
		}
		{
			//	figure out the scale
			Vector3 patchScale = Vector3.one;
			//pushDist = brushWidth * Mathf.Tan(theta / 2.0f);
			len = extraPatchWidth - pushDist;
			len /= spriteWidth;			//	since our brush is unit size, we need to change our units for the scale accordingly.
			scale = len;			//	scale should be 1.0, not 0.0 if pt1 and pt2 are the same.
			//scale -= 0.5f;				//	subtract half a brush width for patch triangle(s)
			if (scale < 0.0f)
				scale = 0.01f;
			patchScale.x = len;
			patchSprite.transform.localScale = patchScale;
			patchSprite.name = spriteGO + " patch";
		}
		m_spriteList.Add(patchSprite.gameObject);
		
		
		GameObject patchTriangleGO = CreatePatchTriangle(brush, prevEndpt2);
		Sprite3D patchTriangleSprite = patchTriangleGO.GetComponent<Sprite3D>();
		float averageAngle = (prevAngle + angle)/2.0f;
		if (averageAngle != 0.0f) {	//	rotate our segment
			Vector3 newEulerAngles = Vector3.zero;
			newEulerAngles.z = averageAngle * Mathf.Rad2Deg;
			patchTriangleSprite.transform.localEulerAngles = newEulerAngles;
			patchTriangleSprite.transform.parent = this.transform;
		}
		{
			//	figure out the scale
			Vector3 patchScale = Vector3.one;
			//pushDist = brushWidth * Mathf.Tan(theta / 2.0f);
			len = pushDist * 8.0f;
			len /= spriteWidth;			//	since our brush is unit size, we need to change our units for the scale accordingly.
			scale = len;			//	scale should be 1.0, not 0.0 if pt1 and pt2 are the same.
			//scale -= 0.5f;				//	subtract half a brush width for patch triangle(s)
			if (scale < 0.0f)
				scale = 0.01f;
			patchScale.x = len;
			patchTriangleSprite.transform.localScale = patchScale;
			patchTriangleSprite.name = spriteGO + " patch Triangle";
		}		
		
		for(int ii=0; ii<numStretchedSprites; ii++) {
			Sprite3D newSprite = Instantiate(sprite) as Sprite3D;
			//newSprite.SetUVs(uvMin, uvMax);
			m_spriteList.Add(newSprite.gameObject);
		}
		
		
		//	now create two dots at the endpoints
		if (m_bDrawEndPts) {
			if (numStretchedSprites <= 0)
				numStretchedSprites = 1;
			for(int ii=0; ii<numStretchedSprites; ii++) {
				CreateBrushGO(brush, endPt1, false);
				//CreateBrushGO(brush, endPt2, false);
			}
		}
		return spriteGO;
	}
	
	int		m_bakeEveryNFrames = 1;
	int		m_currentIdxCounter = 0;
	public void DrawSegments()
	{
		int		buffer = 2;
		int 	nSegments = this.m_prevPointsList.Count;
		
		bool	bSlowCPUTextureUpdate = !m_bFastRender;
		
		if (bSlowCPUTextureUpdate && m_myTexture2D) {
			//InterpolatePreviousPoints(m_maxPoints-buffer-nSegments, m_maxPoints-buffer);
			//m_myTexture.SetPixels(0, (int)extents.min.y, 1024, (int)(extents.max.y-extents.min.y), m_pixelLayer);
			//m_myTexture.SetPixels(0, 0, 1024, (int)(extents.max.y-extents.min.y), m_pixelLayer);
			InterpolatePreviousPoints(m_prevPointsList.Count-1, 0);
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
			//InterpolatePreviousPoints(m_prevPointsList.Count-buffer-nSegments, m_prevPointsList.Count-buffer);
			if (m_prevPointsList.Count > 0) {
				InterpolatePreviousPoints(m_currentIdxCounter, m_prevPointsList.Count-1);
				
				Bake();
			}
			//Dirty();
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
		if ( m_prevPointsList.Count > 2) {
			m_prevPointsList.RemoveRange(0, m_prevPointsList.Count-3);
			m_currentIdxCounter = 2;
			/*
			if (m_bIsDrawing) {
				m_prevPointsList.Add(lastPt);
			}
			*/
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
		if (Time.frameCount % 1 == 0) {
			DrawSegments();
		}
	}

	void FixedUpdate()
	{
		//Time.fixedDeltaTime = 1.0f/m_userInputSampleRate;	//	user input update time
		if (m_bIsDrawing && (Input.GetMouseButton(0) == true)) {

			AddPreviousPoint(GetPoint());
		}
	}
}
