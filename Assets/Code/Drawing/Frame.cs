
// ======================================================================================
// File         : Frame.cs
// Author       : Eu-Ming Lee 
// Changelist   :
//	10/24/2011 - First creation
// Description  : 
//	A frame is hierarchical window system that automatically scales its contents according 
//	to its parent frames. In this way, we hope to abstract away the differences in aspect
//	ratio and pixel widths/heights among various hardware skews and use a single cohesive
//	relative system.
//
//	Frame only cares about relative position to its parent Frame. It places the center of the
//	current Frame at one of 9 positions like a tic-tac-toe or Brady Bunch grid. For further alignment,
//	the Sprite3D should have its local center adjusted by adjusting that Sprite3D's Pivot.
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;

[System.Serializable] // Required so it shows up in the inspector 
public class AnchorPoint
{
	public static readonly AnchorPoint kTopLeft = new AnchorPoint(AnchorPoint.AnchorH.left, AnchorPoint.AnchorV.top);
	
	/*	AnchorPoint is one of 9 positions in a grid like the Brady Bunch
	 */
	[System.Serializable] // Required so it shows up in the inspector 
	public enum AnchorH
	{
		left, center, right
	};
		
	[System.Serializable] // Required so it shows up in the inspector 
	public enum AnchorV
	{
		//bottom, middle, top
		top, middle, bottom
	};
	
	public AnchorH		horiz;
	public AnchorV		vert;

	public AnchorPoint()
	{
		SetAnchor(AnchorH.center, AnchorV.middle);
	}
	
	public AnchorPoint(AnchorH h, AnchorV v)
	{
		horiz = h;
		vert = v;
	}
	
	public void SetAnchor(AnchorH h, AnchorV v)
	{
		horiz = h;
		vert = v;
	}
	
	public void SetAnchor(AnchorPoint pv)
	{
		SetAnchor(pv.horiz, pv.vert);
	}
	
	//	Find the center of this Rect
	public static Vector2 GetRectCenter(Rect frameRect)
	{
		Vector2		result = new Vector2(frameRect.xMin+frameRect.xMax, frameRect.yMin+frameRect.yMax);
		result *= 0.5f;
		return result;
	}
	/*
	public Vector2 GetRectCenter(Rect frameRect)
	{
		Vector2		result = new Vector2(frameRect.xMin, frameRect.yMin);
		Vector2		relOffset = GetRelativeOffset();
	
		frameRect.
		relOffset.x = relOffset.x + 0.5f;
		relOffset.y = relOffset.y + 0.5f;
		
		relOffset.x = relOffset.x * frameRect.width;
		relOffset.y = relOffset.y * frameRect.height;
		
		result = result + relOffset;
		return result;
	}
	*/
	
	//	find the unitized offset as a ratio of the width/height of the frame from the center of the frame.
	//	This is called the Brady position because it's one of 9 fixed spots in a grid like the Brady Bunch intro.
	public Vector2 GetUnitizedRelativeOffset()
	{
		float xoff, yoff;
		Vector2		result;
		
		switch(horiz)
		{
			default:
				xoff = 0.0f;
			break;
			case AnchorPoint.AnchorH.left:	//	girls
				xoff = -0.5f;
				break;
			case AnchorPoint.AnchorH.center:	//	adults
				xoff = +0.0f;
				break;
			case AnchorPoint.AnchorH.right:		//	boys
				xoff = +0.5f;
				break;
		}
		
		switch (vert)
		{
			default:
				yoff = 0.0f;
			break;
			case AnchorPoint.AnchorV.top:		//	eldest
				yoff = +0.5f;
				break;
			case AnchorPoint.AnchorV.middle:	//	middle
				yoff = +0.0f;
				break;
			case AnchorPoint.AnchorV.bottom:	//	youngest
				yoff = -0.5f;
				break;
		}
		
		result = new Vector2(xoff, yoff);
		return result;
	}
};

///////////////////////////////////////////////////////////////////////////////
///
/// Frame
///
///////////////////////////////////////////////////////////////////////////////
[ExecuteInEditMode]
[System.Serializable] // Required so it shows up in the inspector 
[AddComponentMenu ("GUI/Frame")]
public class Frame : MonoBehaviour 
{
	[SerializeField] public Vector2			m_relativeOffset;			//	relative to previous Frame's FrameRect from the given anchor point using the previous Frame's width and height as the relative scale
	//	[SerializeField] public bool			m_byPixels;				//	Instead of relative offsets, use precise pixel offsets. not needed for now. We'll figure this out later when we need it. (maybe never)
	[SerializeField] public AnchorPoint		m_fromAnchor;
	
	//	uncommenting allows us to see the framerect for debugging purposes
	[SerializeField] public
	//private  
			Rect							m_FrameRect;				//	this is where the actual Frame resides relative to the actual Transform.position
	private bool							m_bAwakened = false;
	
	public void Reset()
	{
		if (!isAwakened())
			Awake();	//	somehow, Reset() may be called before Awake(). So we need to do this to be sure everything is created before we attempt to set defaults
		//this.ResetToDefaultGO();
	}
	
	public bool isAwakened()
	{
		return m_bAwakened;
	}
	
	public void SetDefaultLike(GameObject defaultObj)
	{
		Frame defaultComponent = defaultObj.GetComponent<Frame>();
		
		//	SetAutomaticDefaultLike(defaultSpriteObj, defaultSpriteComponent.GetType());
		this.m_relativeOffset = defaultComponent.m_relativeOffset;
		this.SetAnchor(defaultComponent.m_fromAnchor);
		//	this.SetFrameRect(defaultComponent.m_FrameRect);	//	this should not be done. The FrameRect should be set properly by the component that uses this Frame
	}
	float min(float a, float b)
	{
		float retval = a;
		if (b < a)
			retval = b;
		return retval;
	}
	
	float max(float a, float b)
	{
		float retval = a;
		if (b > a)
			retval = b;
		return retval;
	}
		
	public void Awake()
	{
		Rlplog.Trace("", "Frame::Awake() - " + this.transform.name);
		if (m_fromAnchor == null)
			m_fromAnchor = new AnchorPoint();
		
		Camera camera = GetComponent<Camera>();
		if (camera) {
			camera.ResetAspect();
			float size = camera.orthographicSize;
			if (!camera.isOrthoGraphic) {
				Debug.LogWarning("Camera: " + camera.name + "is not orthographic: Frame component is meant for Orthographic cameras\n");
			}
//			Vector3 pos = camera.transform.position; // removed due to warning (slc)
			Rect camRect = camera.rect;
			camRect.height = size * 2;	//	orthographicSize is half the vertical size of the viewing volume
			camRect.width = camRect.height * camera.aspect;
			camRect.x -= camRect.width/2;
			camRect.y -= camRect.height/2;
			SetFrameRect(camRect);
			//Debug.LogWarning("Camera Rect("+camera.aspect+") = (x=" + camRect.x + ", y=" + camRect.y + ", w=" + camRect.width + ", h=" + camRect.height + ")\n");
		}
		
		MeshRenderer	mr = GetComponent<MeshRenderer>();
		if (mr) {
			if (mr.isVisible) {
				Rect mrRect = new Rect();
				
				mrRect.height = mr.bounds.max.y - mr.bounds.min.y;
				mrRect.width = mr.bounds.max.x - mr.bounds.min.x;
				mrRect.x -= mrRect.width/2;
				mrRect.y -= mrRect.height/2;
				SetFrameRect(mrRect);
			}
		}
		
		TextMesh textMesh = GetComponent<TextMesh>();
		if (textMesh != null) {
			this.SetAnchor(textMesh.anchor);	//	match the textMesh's same anchor
		}
		//	hmmm... as it turns out, it's not wise to put the initialization for FrameRect here.
		//	the reason is, we can get nested components like the following structure:
		//	Button - Frame - Autobuild(MeshCollider(Sprite0))
		//		Sprite0 - Frame - MeshCollider(Sprite0)
		//	When the game runs, this Awake() will be called. Although the Button-Frame already contains the correct FrameRect,
		//		the autobuild of the MeshCollider of Sprite0 will cause a MeshCollider to be built that is one level hierarchy above
		//		the original MeshCollider on Sprite0. This causes a different translation to be assigned that MeshCollider.
		//		As a result, the incorrect FrameRect is forced over the correct FrameRect for the Sprite0-Frame.
		//	tldr; it's bad to set the FrameRect here. Set the FrameRect correctly from the Editor and bake it into the Asset rather
		//	than try to dynamically set it. Dynamically set FrameRects should only be appropriate for dynamic Assets which should be
		//	rather rare.
		/*
		MeshCollider mc = GetComponent<MeshCollider>();
		if (mc) {
			Rect collisRect = Rect.MinMaxRect(min(mc.bounds.min.x, mc.bounds.max.x), 
			                                  max(mc.bounds.min.y, mc.bounds.max.y), 
			                                  max(mc.bounds.max.x, mc.bounds.min.x), 
			                                  min(mc.bounds.max.y, mc.bounds.min.y));
			Rect collisRect = new Rect(min(mc.bounds.min.x, mc.bounds.max.x), 
			                                  max(mc.bounds.min.y, mc.bounds.max.y), 
			                                  max(mc.bounds.max.x, mc.bounds.min.x), 
			                                  min(mc.bounds.max.y, mc.bounds.min.y));
			SetFrameRect(collisRect);
		}
		*/
		m_bAwakened = true;
	}
	
	public void Start()
	{
		Rlplog.Trace("", "Frame::Start() - " + this.transform.name);
		
		ChildFirstRefresh();
	}
	
	public void ChildFirstRefresh()
	{
		Frame		frame;
		//	starting with my children, do the Refresh()
		//Frame[] frames = this.transform.gameObject.GetComponents<Frame>();
		//foreach(Frame frame in frames)
		foreach(Transform childXform in this.transform)
		{
			frame = childXform.GetComponent<Frame>();
			if (frame != null)
				frame.ChildFirstRefresh();
		}
		
		if (this.m_bAwakened == false) {
			this.Awake();
		}
		this.Refresh(false, 0.0f);
	}

	public void RefreshAllChildren()
	{
		Frame		frame;
		this.Refresh(false, 0.0f);
		foreach(Transform childXform in this.transform)
		{
			frame = childXform.GetComponent<Frame>();
			if (frame != null)
				frame.RefreshAllChildren();
		}
		
	}
	
	public void OnEnable()
	{
		Rlplog.Trace("", "Frame::OnEnable() - " + this.transform.name);

	}
	
	public void AlignChildrenPosition()
	{
		Frame[] frames = this.transform.gameObject.GetComponentsInChildren<Frame>();
		foreach(Frame frame in frames)
		{
			if (frame != this)
				frame.AlignPosition();
		}
	}
	
	public void AlignParentPosition()
	{
		if (this.transform.parent) {
			Frame parentFrame = this.transform.parent.gameObject.GetComponent<Frame>();
			if (parentFrame) {
				parentFrame.AlignPosition();
			}
		}
	}
	
	public void SetAnchor(AnchorPoint anchor)
	{
		m_fromAnchor = anchor;
		AlignPosition();
	}
	
	public void SetAnchor(TextAnchor unityAnchor)
	{
		AnchorPoint anchor = new AnchorPoint();;
		switch (unityAnchor)
		{
			default:
			case TextAnchor.UpperLeft:
				anchor.vert = AnchorPoint.AnchorV.top;
				anchor.horiz = AnchorPoint.AnchorH.left;
				break;
			case TextAnchor.UpperCenter:
				anchor.vert = AnchorPoint.AnchorV.top;
				anchor.horiz = AnchorPoint.AnchorH.center;
				break;
			case TextAnchor.UpperRight:
				anchor.vert = AnchorPoint.AnchorV.top;
				anchor.horiz = AnchorPoint.AnchorH.right;
				break;

			case TextAnchor.MiddleLeft:
				anchor.vert = AnchorPoint.AnchorV.middle;
				anchor.horiz = AnchorPoint.AnchorH.left;
				break;
			case TextAnchor.MiddleCenter:
				anchor.vert = AnchorPoint.AnchorV.middle;
				anchor.horiz = AnchorPoint.AnchorH.center;
				break;
			case TextAnchor.MiddleRight:
				anchor.vert = AnchorPoint.AnchorV.middle;
				anchor.horiz = AnchorPoint.AnchorH.right;
				break;

			case TextAnchor.LowerLeft:
				anchor.vert = AnchorPoint.AnchorV.bottom;
				anchor.horiz = AnchorPoint.AnchorH.left;
				break;
			case TextAnchor.LowerCenter:
				anchor.vert = AnchorPoint.AnchorV.bottom;
				anchor.horiz = AnchorPoint.AnchorH.center;
				break;
			case TextAnchor.LowerRight:
				anchor.vert = AnchorPoint.AnchorV.bottom;
				anchor.horiz = AnchorPoint.AnchorH.right;
				break;

		}
		SetAnchor(anchor);
	}
	//	returns the absolute position of this FrameRect's center
	public Vector2 GetFrameRectCenter()
	{
		Vector2 pos = AnchorPoint.GetRectCenter(m_FrameRect);
		pos.x += transform.position.x;
		pos.y += transform.position.y;
		
		return pos;
	}

	//	in pixels, get the vector from the origin of this frame (transform.position==0,0,0) to the anchor point
	public Vector2 GetFromFrameRectCenterToAnchorPoint(AnchorPoint anchor)
	{
		Vector2			result = anchor.GetUnitizedRelativeOffset();	//	gets us the unitized fractional position relative to the center of the frame

		//	this gets us to the anchor point spot relative to the center of the frame rect in world units
		result.x *= m_FrameRect.width;
		result.y *= m_FrameRect.height;
	
		return result;
	}
	
	//	a child Frame may be relative to a particular point on this frame. Return that position relative to the center in this method.
	public Vector2 GetFromAnchorPoint(AnchorPoint anchor)
	{
		/*
		Vector2			result = anchor.GetUnitizedRelativeOffset();	//	gets us the unitized fractional position relative to the center of the frame
		
		//	this gets us to the anchor point spot relative to the center of the frame rect in world units
		result.x *= m_FrameRect.width;
		result.y *= m_FrameRect.height;
		*/
		
		Vector2			result = GetFromFrameRectCenterToAnchorPoint(anchor);
		
		//	this gives us the relative frame center
		Vector2 relFrameCenter = AnchorPoint.GetRectCenter(m_FrameRect);
		result += relFrameCenter;
		return result;
	}
	
	//	relative offset is calculated from the parent's FrameRect. This allows the artist to control where the placement
	//	is relative to the parent frame.
	//	ONLY if I have a parent Frame do I need to set my position relative to that Parent. Otherwise, simply ignore the Frame request to
	//		AlignPosition.
	public void AlignPosition()
	{
		Vector2			anchorPos = new Vector2(0, 0);
//		Vector2			relFrameRectCenterParent;					//	relative position - removed due to warning (slc)
		Vector2			userOffset;
		Transform 	xformParent = transform.parent;
		Frame		frameParent = null;
		
		if (xformParent) frameParent = xformParent.GetComponent<Frame>();
		
		//	bail if we don't have a parent
		if ((xformParent == null) || (frameParent == null)) {
			//	propagate down the hierarchy
			AlignChildrenPosition();
			return;
		}
		
		//	directional vector from parent's transform.position to the anchor point requested by this Frame
		Vector2		fromParentAnchorPoint = new Vector2(0, 0);		//	this is the center of the parent	

		fromParentAnchorPoint = frameParent.GetFromAnchorPoint(m_fromAnchor);		//	absolute offset to a Brady in my parent's Frame
//		relFrameRectCenterParent = frameParent.transform.localPosition; // removed due to warning (slc)
		//	we may also have an offset relative to the parent Frame's width/height
		userOffset.x = m_relativeOffset.x * frameParent.m_FrameRect.width;
		userOffset.y = m_relativeOffset.y * frameParent.m_FrameRect.height;
		
		anchorPos += fromParentAnchorPoint;
		//	anchorPos += relFrameRectCenterParent;
		anchorPos += userOffset;
		
		//	set the new position
		this.transform.localPosition = new Vector3(anchorPos.x, anchorPos.y, this.transform.localPosition.z);
		//	Rlplog.Debug("", "AlignPosition: " + this.name + " = " + this.transform.localPosition.ToString());
		
		/*
		//	propagate changes down the hierarchy.
		Frame[] frames = GetComponentsInChildren<Frame>();
		//	but we must exclude the frames that are at the same hiearachy level
		Frame[] brotherFrames = GetComponents<Frame>();
		frames = SubtractArray(frames, brotherFrames);
		foreach (Frame frame in frames) {
			if (frame != this)
				frame.AlignPosition();
		}
		*/
		
		//	propagate up the hierarchy
		//	AlignParentPosition();
		//	propagate down the hierarchy
		AlignChildrenPosition();
	}
	//	when the Sprite3D changes its Pivot, it's important to call this so that the Frame
	// 	knows where the corners of the image are relative to the transform.position.
	public void SetFrameRect(Rect newRect)
	{
		//	Rlplog.Debug("", "SetFrameRect: " + this.name + "("+newRect.ToString()+")");	//	too noisy. Don't print this unless necessary
		m_FrameRect = newRect;
		AlignPosition();
	}
	
	public Rect GetFrameRect()
	{
		return m_FrameRect;
	}
	
	public Rect GetLocalRect()
	{
		Rect localRect = m_FrameRect;
		localRect.x += this.gameObject.transform.localPosition.x;
		localRect.y += this.gameObject.transform.localPosition.y;
		return localRect;
	}
	
	//	this takes the Frame's XYZ offset and zeroes them so that they become relative offsets from the parent Frame.
	public void BakeIntoRelativeOffset()
	{
		if (this.transform.parent==null) {
			Debug.LogWarning("Baking a relative offset of an object without a parent will not do anything. Are you sure you meant to do this?");
			return;
		}
		GameObject parentGO = this.transform.parent.gameObject;
		Frame parentFrame = parentGO.GetComponent<Frame>();
		Vector2 fromParentAnchorVector = parentFrame.GetFromAnchorPoint(this.m_fromAnchor);

		if ((parentFrame.m_FrameRect.width == 0) || (parentFrame.m_FrameRect.height == 0))
		{
			Rlplog.Error("BakeIntoRelativeOffset", "Frame " + this.name + " has a parent (" + parentFrame.name + ") whose FrameRect is 0 in either height or width.");									
			bool bAutoFix = false;
			
			if (bAutoFix == true) {
				this.m_relativeOffset.x = this.m_relativeOffset.y = 0;
				Sprite3D	mySprite = this.GetComponent<Sprite3D>();
				if (mySprite != null) {
					Rlplog.Error("BakeIntoRelativeOffset", "Frame " + this.name + " has a parent (" + parentFrame.name + ") whose FrameRect is 0 in either height or width. Attempting autofix by assigning parent's FrameRect to be this Sprite3D's FrameRect.");
					parentFrame.m_FrameRect.width = mySprite.m_FrameRect.width;
					parentFrame.m_FrameRect.height = mySprite.m_FrameRect.height;
				}
			}
		}
		else {
			this.m_relativeOffset.x = (-fromParentAnchorVector.x + this.transform.localPosition.x) / parentFrame.m_FrameRect.width;
			this.m_relativeOffset.y = (-fromParentAnchorVector.y + this.transform.localPosition.y) / parentFrame.m_FrameRect.height;
		}
	}

	public void OnPreRender()
	{
		Camera camera = GetComponent<Camera>();
		if (camera) {
			//Refresh(false, 0.0f);
		}
	}
	
	public void Refresh(bool bTraverseUpParents, float cameraAspect)
	{	
		Rlplog.Trace("", "Frame::Refresh: " + this.name);
		Camera camera = GetComponent<Camera>();
		if (camera) {
			float size = camera.orthographicSize;
			if (!camera.isOrthoGraphic) {
				Debug.LogWarning("Camera: " + camera.name + "is not orthographic: Frame component is meant for Orthographic cameras\n");
			}
			//Vector3 pos = camera.transform.position;
			Rect camRect = camera.rect;
			camRect.height = size * 2;	//	orthographicSize is half the vertical size of the viewing volume
			
			//	Unity bug! When in the editor, the camera does NOT return the correct aspect ratio. Instead, it returns the inspector window's aspect ratio! Who fucking cares about that???			
			//	therefore, we must choose a special way of determining the aspect ratio when we are in the editor
			/*
			if (Application.isEditor) {
				float projectAspect = (float)PlayerSettings.defaultScreenWidth / (float)PlayerSettings.defaultScreenHeight;
				cameraAspect = projectAspect;
			}
			*/
			if (cameraAspect==0.0f) {
				cameraAspect = camera.aspect;
			}
			camRect.width = camRect.height * cameraAspect;
			camRect.x -= camRect.width/2;
			camRect.y -= camRect.height/2;
			SetFrameRect(camRect);
			Rlplog.Trace("", "Camera Rect("+camera.aspect+") = (x=" + camRect.x + ", y=" + camRect.y + ", w=" + camRect.width + ", h=" + camRect.height + ")\n");
		}
		
		MeshRenderer	mr = GetComponent<MeshRenderer>();
		MeshFilter		mf = GetComponent<MeshFilter>();
//		MeshCollider	mc = GetComponent<MeshCollider>(); // removed due to warning (slc)
		
		if (mf && mf.sharedMesh) {	//	this is for Sprite3D
			Mesh		mesh = mf.sharedMesh;
			Rect mrRect = new Rect();
			
			mrRect.height = mesh.bounds.max.y - mesh.bounds.min.y;
			mrRect.width = mesh.bounds.max.x - mesh.bounds.min.x;
			mrRect.x = mesh.bounds.min.x;
			mrRect.y = mesh.bounds.min.y;
			//mrRect.x -= mrRect.width/2;
			//mrRect.y -= mrRect.height/2;
			SetFrameRect(mrRect);
		}
		else if (mr) {	//	this is for TextMesh
			Rect mrRect = new Rect();
			
			if (mr.isVisible) {
				mrRect.height = mr.bounds.max.y - mr.bounds.min.y;
				mrRect.width = mr.bounds.max.x - mr.bounds.min.x;
				mrRect.x = mr.bounds.min.x;
				mrRect.y = mr.bounds.min.y;
				//mrRect.x -= mrRect.width/2;
				//mrRect.y -= mrRect.height/2;
				SetFrameRect(mrRect);
			}
		}
		/*
		else if (mc) {	//	this is for Button3D or other meshes. May or may not be what the designer intends
			Rect mrRect = new Rect();
			
			mrRect.height = mc.bounds.max.y - mc.bounds.min.y;
			mrRect.width = mc.bounds.max.x - mc.bounds.min.x;
			mrRect.x = mc.bounds.min.x;
			mrRect.y = mc.bounds.min.y;
			//mrRect.x -= mrRect.width/2;
			//mrRect.y -= mrRect.height/2;
			SetFrameRect(mrRect);
		}
		*/
		
		if ((m_FrameRect.width == 0) || (m_FrameRect.height == 0)) {
			Rlplog.Error("Frame.Refresh", "Frame " + this.name + " has zero area FrameRect. This will cause other errors under the hierarchy. Fix this immediately.");
			m_FrameRect.width = 1;
			m_FrameRect.height = 1;
		}
		/*
		Lineup			lineup = GetComponent<Lineup>();
		if (lineup) {
			lineup.Refresh();						//	first refresh is to calculate the right size of FrameRect
			SetFrameRect(lineup.m_FrameRect);
			lineup.Refresh();						//	second refresh is to get the right pivot point
		}
		
		ScaleableSpritePrefab scDialog = GetComponent<ScaleableSpritePrefab>();
		if (scDialog) {
			scDialog.Refresh();
		}
		
		ScaleableSprite scDB = GetComponent<ScaleableSprite>();
		if (scDB) {
			scDB.RefreshFrameRect();
		}
		*/
		
		Sprite3D sprite3D = GetComponent<Sprite3D>();
		if (sprite3D) {
			if (sprite3D.m_bFitWidth == true) {
				sprite3D.Refresh();
			}
		}
		
		this.SetAnchor(this.m_fromAnchor);
		//curAnchor = myFrame.m_fromAnchor;
		
		//	go up the parent chain
		if (bTraverseUpParents == true) {
			if (this.gameObject.transform.parent != null) {
				Frame parentFrame = this.gameObject.transform.parent.GetComponent<Frame>();
				if (parentFrame != null)
					parentFrame.Refresh(true, cameraAspect);
			}
		}
		this.AlignPosition();
	}
	
	//	This should probably be converted to a Template <T> of some type. But I'm not familiar enough with C# to do it now.
	//	so it just works for Frame arrays only for now.
	public static Frame[] SubtractArray(Frame[] bigger, Frame[] smaller)
	{
		int		idx=0;
		bool	bAdd;
		Frame[] newFrames = new Frame[bigger.Length];
		foreach(Frame frame in bigger) {
			bAdd = true;
			foreach(Frame excluder in smaller) {
				if (excluder == frame) {
					bAdd = false;
					break;
				}
			}
			if (bAdd) {
				newFrames[idx] = frame;
				idx++;
			}
		}
		System.Array.Resize<Frame>(ref newFrames, idx);
		return newFrames;
	}
}