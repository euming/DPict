using UnityEngine;
using System.Collections;


[System.Serializable] // Required so it shows up in the inspector 
[AddComponentMenu ("DPict/ColorPick")]
public class ColorPick : MonoBehaviour 
{
	public ColorPick	m_Gradient;
	public Color 		m_myColor;
	public float		m_holdTime = 1.0f;		//	hold long to hold before m_Gradient slides out
	public float		m_releaseTime = 0.5f;	//	how long between multi-finger touch before selection changes
	Texture2D			m_myTexture;
	public bool			m_bDeactivateWhenUnselected = false;
	
	//static Color		s_Accumulator;
	//static int			s_ColorsPicked;
	
	Camera				m_myCamera;
	bool				m_bIsMouseDown = false;	//	is the mouse pressed on this?
	bool				m_bIsMouseInside = false;	//	is the mouse iside of this ColorPick object?
	bool				m_bSelecting = false;	//	is this choice selected? There needs to be some time after the mouse up where a selection remains selected. This variable distinguishes between mouseUp and selection.
	bool				m_bIsInvertedColor = false;
	float				m_timeHeld = 0.0f;
	float				m_unselectTime;			//	the time at which we stop selecting this.
	bool				m_bIsSubscriberOfTouchListener;	//	mostly used to distinguish between mouse and touch mode
	int					m_nTouchesOnThis;		//	how many fingers are touching me?
	
	//	cache
	ColorPick			m_ColorPickParent;
	public ColorAccumulator	m_ColorAccumulator;
	
	public void Awake()
	{
		//	pointer cache
		if (m_ColorAccumulator==null) {
			m_ColorAccumulator = FindObjectOfType(typeof(ColorAccumulator)) as ColorAccumulator;
		}
		
		Texture2D tex = renderer.material.mainTexture as Texture2D;
		if (tex != null) {
			m_myTexture = tex;
			m_myColor = tex.GetPixel(0,0);
		}
		GameObject camGO = GameObject.FindGameObjectWithTag("OrthoCamera");
		if (camGO)
			m_myCamera = camGO.camera;
		DeactivateGradient();
//		s_ColorsPicked = 0;
		m_nTouchesOnThis = 0;
		
		//	find cache pointers
		if (this.transform.parent != null) {
			m_ColorPickParent = this.transform.parent.GetComponent<ColorPick>();
		}
		
		//	determine whether we are a subscriber to a touch listener which will be sending us the Mouse messages in lieu of Unity's messages
		m_bIsSubscriberOfTouchListener = TouchListener.isSubscriberOfTouchListener(this.gameObject);
		
		Rlplog.DbgFlag = true;
	}
	
	void OnEnable()
	{
		TouchUnselectTimer();
	}
	
	Color InvertColor(Color color)
	{
		Color outColor = color;
		outColor.r = 1.0f - color.r;
		outColor.g = 1.0f - color.g;
		outColor.b = 1.0f - color.b;
		return outColor;
	}
	
	public void OnMouseEnterListener(int buttonNo)
	{
		Rlplog.Debug("ColorPick.OnMouseEnterListener", this.name +": buttonNo="+buttonNo.ToString()+", nTouches="+m_nTouchesOnThis.ToString() + ", nColors="+m_ColorAccumulator.m_nColorSources);
		m_bIsMouseInside = true;
	}
	
	public void OnMouseExitListener(int buttonNo)
	{
		Rlplog.Debug("ColorPick.OnMouseExitListener", this.name +": buttonNo="+buttonNo.ToString()+", nTouches="+m_nTouchesOnThis.ToString() + ", nColors="+m_ColorAccumulator.m_nColorSources);
		m_bIsMouseInside = false;
	}
	
	public void OnMouseDownListener(int buttonNo)
	{
		m_bIsMouseDown = true;
		if (this.enabled) {
			m_nTouchesOnThis++;
			OnTouchDown();
			/*
			if (m_nTouchesOnThis == 1) {
			}
			else {
				InvertColorToggle();
			}
			*/
			Rlplog.Debug("ColorPick.OnMouseDownListener", this.name +": buttonNo="+buttonNo.ToString()+", nTouches="+m_nTouchesOnThis + ", nColors="+m_ColorAccumulator.m_nColorSources);
			//	send my mouse controls to my parent
			if (m_ColorPickParent != null) {
				//m_ColorPickParent.OnMouseUpListener(buttonNo);
			}
		}
	}
	
	public void OnMouseUpListener(int buttonNo)
	{
		m_bIsMouseDown = false;
		if (this.enabled) {
			OnTouchUp();
			m_nTouchesOnThis--;
			/*
			if (m_nTouchesOnThis == 0) {
			}
			*/
			Rlplog.Debug("ColorPick.OnMouseUpListener", this.name +": buttonNo="+buttonNo.ToString()+", nTouches="+m_nTouchesOnThis.ToString() + ", nColors="+m_ColorAccumulator.m_nColorSources);
			//	send my mouse controls to my parent
			if (m_ColorPickParent != null) {
				//m_ColorPickParent.OnMouseUpListener(buttonNo);
			}
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
	
	void OnTouchDown()
	{
		if (m_bSelecting==false) {
			m_bSelecting = true;
			m_timeHeld = 0.0f;
			TouchUnselectTimer();
			m_ColorAccumulator.OnSelect(this);
		}
	}
	
	void OnTouchUp()
	{
		//Color color = PickColor();
		//Layer.SetBrushColor(m_Accumulator);
		//m_bSelecting = false;
		//Unselect();
		//m_bIsMouseDown = false;
	}
	
	public void TouchUnselectTimer()
	{
		m_unselectTime = Time.time + m_releaseTime;
	}
	
	public void Unselect()
	{
		if (m_bSelecting==true) {
			m_bSelecting = false;
			//	DeactivateGradient();
			m_timeHeld = 0.0f;
			m_bIsInvertedColor = false;
			//m_ColorAccumulator.OnUnselect(this);
			m_bIsMouseDown = false;	//	hack: force this
		}
	}
	
	Vector3 lastHit = Vector3.zero;
	Ray		lastRay;
	
	public Vector3 GetPoint()
	{
		
        Ray rayToMouse = Camera.main.ScreenPointToRay (Input.mousePosition);
        RaycastHit hitInfo;
		lastRay = rayToMouse;
        if (collider.Raycast (rayToMouse, out hitInfo, Camera.main.far)) {
			lastHit = hitInfo.point;
        }
		else {
			hitInfo.point = Vector3.zero;
		}
		Bounds bounds = this.collider.bounds;
		Vector3 relPos = hitInfo.point - bounds.center;
		relPos.x /= bounds.extents.x;
		relPos.y /= bounds.extents.y;
		
		relPos.x *= m_myTexture.width/2;
		relPos.y *= m_myTexture.height/2;
		relPos.x += m_myTexture.width/2;
		relPos.y += m_myTexture.height/2;
		return relPos;
	}
	
	public Color GetColor(Vector3 point)
	{
		Color color = Color.white;
		if (m_myTexture != null) {
			color = m_myTexture.GetPixel((int)point.x, (int)point.y);
			m_myColor = color;
		}
		return color;
	}
	
	public Color GetColor()
	{
		Color color = m_myColor;
		return color;
	}
	
	void ActivateGradient()
	{
		if (m_Gradient != null) {
			if (m_Gradient.gameObject.activeSelf==false) {
				m_Gradient.gameObject.SetActive(true);
			}
		}
	}
	
	public void DeactivateGradient()
	{
		if (m_Gradient != null) {
			if (m_Gradient.gameObject.activeSelf==true) {
				m_Gradient.gameObject.SetActive(false);
			}
		}
	}
	
	bool isGradientActive()
	{
		bool bIsActive = false;
		if (m_Gradient != null) {
			bIsActive = m_Gradient.gameObject.activeSelf;
		}
		return bIsActive;
	}
	
	Color PickColor()
	{
		Color color;
		Vector3 pickPoint;
		
		//	use the gradient's color if it is up
		if (isGradientActive()) {
			pickPoint = m_Gradient.GetPoint();
			color = m_Gradient.GetColor(pickPoint);
		}
		else {
			pickPoint = GetPoint();
			color = GetColor(pickPoint);
		}
		
		if (m_bIsInvertedColor == true) {
			color = InvertColor(color);
		}
		return color;
	}
	
	//	is someone touching this or the gradient?
	bool isTouchingThis(bool andIfMouseIsDown)
	{
		bool bIsTouching = false;
		if (m_Gradient != null) {
			bIsTouching = m_Gradient.isTouchingThis(andIfMouseIsDown);
		}
		if (bIsTouching==false) {
	        Ray rayToMouse = Camera.main.ScreenPointToRay (Input.mousePosition);
	        RaycastHit hitInfo;
	        if (collider.Raycast (rayToMouse, out hitInfo, Camera.main.far)) {
				if (andIfMouseIsDown) {
					if (this.m_bIsMouseDown) {
						bIsTouching = true;
					}
				}
				else {
					bIsTouching = true;
				}
	        }
		}
		return bIsTouching;
	}
	
	void InvertColorToggle()
	{
		if (isTouchingThis(true)) {
			m_bIsInvertedColor = !m_bIsInvertedColor;
		}
	}
	
	void CheckInvertColorToggle()
	{
		if (Input.GetMouseButtonDown(1)==true) {	//	we've clicked a second time
			InvertColorToggle();
		}
	}
	
	bool CheckActivateGradient()
	{
		bool bActivate = false;
		if (m_bIsMouseDown && m_bIsMouseInside) {
			if (m_timeHeld >= m_holdTime) {
				bActivate = true;
			}
		}
		return bActivate;
	}
	
	//	gradients can deactivate themselves
	bool CheckDeactivateSelf()
	{
		bool bDeactivate = false;
		if (m_bDeactivateWhenUnselected) {
			if (Time.time >= m_unselectTime) {
				bDeactivate = true;
			}
		}
		return bDeactivate;
	}
	
	void Update()
	{
		if (!m_bIsSubscriberOfTouchListener) {
			CheckInvertColorToggle();
		}
		
		if (CheckActivateGradient()) {
			ActivateGradient();
		}
		
		if (m_bIsMouseInside) {
			TouchUnselectTimer();
			m_ColorAccumulator.TouchTimer();
		}
		
		if (CheckDeactivateSelf()) {
			//this.gameObject.SetActive(false);
		}
		
		if (m_bIsMouseDown) {
			m_timeHeld += Time.deltaTime;
			//m_ColorAccumulator.AddColor(m_myColor);

			//	is anybody touching me or my gradient?
			if (isTouchingThis(false)) {
				m_myColor = PickColor();
				Layer.SetBrushColor(m_ColorAccumulator.GetColor());
			}
		}
		else {
			m_timeHeld = 0.0f;
			/*
			if (Time.time >= m_unselectTime) {
				Unselect();
			}
			*/
		}
	}
	
	void LateUpdate()
	{
		//s_Accumulator = Color.black;		//	clear accumulator for next frame. This should be made into a static function and called only once per frame
	}
}
