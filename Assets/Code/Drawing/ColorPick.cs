using UnityEngine;
using System.Collections;


[System.Serializable] // Required so it shows up in the inspector 
[AddComponentMenu ("DPict/ColorPick")]
public class ColorPick : MonoBehaviour 
{
	public ColorPick	m_Gradient;
	public Color 		m_myColor;
	public float		m_holdTime = 1.0f;
	Texture2D			m_myTexture;
	
	Camera				m_myCamera;
	bool				m_bSelecting = false;
	float				m_timeHeld = 0.0f;
	
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
	}
	
	Color InvertColor(Color color)
	{
		Color outColor = color;
		outColor.r = 1.0f - color.r;
		outColor.g = 1.0f - color.g;
		outColor.b = 1.0f - color.b;
		return outColor;
	}
	
	void OnMouseDown()
	{
		m_bSelecting = true;
		m_timeHeld = 0.0f;
	}
	
	public void OnMouseUp()
	{
		Color color = PickColor();
		Layer.SetBrushColor(color);
		m_bSelecting = false;
		DeactivateGradient();
	}
	
	Vector3 lastHit = Vector3.zero;
	Ray		lastRay;
	
	public Vector3 GetPoint()
	{
		
        Ray rayToMouse = Camera.main.ScreenPointToRay (Input.mousePosition);
        RaycastHit hitInfo;
		lastRay = rayToMouse;
        if (collider.Raycast (rayToMouse, out hitInfo, 100.0f)) {
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
			if (m_Gradient.gameObject.active==false) {
				m_Gradient.gameObject.active = true;
			}
		}
	}
	
	void DeactivateGradient()
	{
		if (m_Gradient != null) {
			m_Gradient.gameObject.active = false;
		}
		m_timeHeld = 0.0f;
	}
	
	bool isGradientActive()
	{
		bool bIsActive = false;
		if (m_Gradient != null) {
			bIsActive = m_Gradient.gameObject.active;
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
		
		if (Input.GetMouseButton(1) == true) {
			color = InvertColor(color);
		}
		
		return color;
	}
	
	void Update()
	{
		if (m_bSelecting) {
			Color color = PickColor();
			
			Layer.SetBrushColor(color);
			m_timeHeld += Time.deltaTime;
		}
		
		if (m_timeHeld >= m_holdTime) {
			ActivateGradient();
		}
	}
}
