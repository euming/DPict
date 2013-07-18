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
	
	static Color		m_Accumulator;
	static int			m_ColorsPicked;
	
	Camera				m_myCamera;
	bool				m_bSelecting = false;
	bool				m_bIsInvertedColor = false;
	float				m_timeHeld = 0.0f;
	float				m_unselectTime;			//	the time at which we stop selecting this.
	bool				m_bIsSubscriberOfTouchListener;
	
	public void Awake()
	{
		Texture2D tex = renderer.material.mainTexture as Texture2D;
		if (tex != null) {
			m_myTexture = tex;
			m_myColor = tex.GetPixel(0,0);
		}
		GameObject camGO = GameObject.FindGameObjectWithTag("OrthoCamera");
		if (camGO)
			m_myCamera = camGO.camera;
		DeactivateGradient();
		m_ColorsPicked = 0;

		//	determine whether we are a subscriber to a touch listener which will be sending us the Mouse messages in lieu of Unity's messages
		m_bIsSubscriberOfTouchListener = TouchListener.isSubscriberOfTouchListener(this.gameObject);
	}
	
	Color InvertColor(Color color)
	{
		Color outColor = color;
		outColor.r = 1.0f - color.r;
		outColor.g = 1.0f - color.g;
		outColor.b = 1.0f - color.b;
		return outColor;
	}
	
	public void OnMouseDownListener(int buttonNo)
	{
		if (this.enabled) {
			OnTouchDown();
		}
	}
	
	public void OnMouseUpListener(int buttonNo)
	{
		if (this.enabled) {
			OnTouchUp();
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
		m_ColorsPicked++;
		m_bSelecting = true;
		m_timeHeld = 0.0f;
		m_unselectTime = Time.time + m_releaseTime;
	}
	
	void OnTouchUp()
	{
		m_ColorsPicked--;
		//Color color = PickColor();
		//Layer.SetBrushColor(m_Accumulator);
		//m_bSelecting = false;
		m_bSelecting = false;
	}
	
	public void Unselect()
	{
		m_bSelecting = false;
		DeactivateGradient();
		m_timeHeld = 0.0f;
		m_bIsInvertedColor = false;
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
	
	void DeactivateGradient()
	{
		if (m_Gradient != null) {
			m_Gradient.gameObject.SetActive(false);;
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
	bool isTouchingThis()
	{
		bool bIsTouching = false;
		if (m_Gradient != null) {
			bIsTouching = m_Gradient.isTouchingThis();
		}
		if (bIsTouching==false) {
	        Ray rayToMouse = Camera.main.ScreenPointToRay (Input.mousePosition);
	        RaycastHit hitInfo;
	        if (collider.Raycast (rayToMouse, out hitInfo, Camera.main.far)) {
				bIsTouching = true;
	        }
		}
		return bIsTouching;
	}
	
	void CheckInvertColorToggle()
	{
		if (Input.GetMouseButtonDown(1)==true) {	//	we've clicked a second time
			if (isTouchingThis()) {
				m_bIsInvertedColor = !m_bIsInvertedColor;
			}
		}
	}
	
	void Update()
	{
		CheckInvertColorToggle();
		
		if (m_bSelecting) {
			m_myColor = PickColor();
			m_Accumulator += m_myColor;
			m_timeHeld += Time.deltaTime;
			
			Layer.SetBrushColor(m_Accumulator);
			m_unselectTime = Time.time + m_releaseTime;
		}
		
		if (m_timeHeld >= m_holdTime) {
			ActivateGradient();
		}
		
		if (Time.time >= m_unselectTime) {
			if (m_ColorsPicked==0) {
				Unselect();
			}
		}
		
	}
	
	void LateUpdate()
	{
		m_Accumulator = Color.black;
	}
}
