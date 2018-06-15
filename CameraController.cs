using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;
using System.Collections;
using System.Collections.Generic;

public class CameraController : MonoBehaviour {

	// Objects
	private Transform myTransform;
	private Camera myCamera;

	// Camera zooming
	private int zoomFactor = 0;
	private int zoomFactorOld = 0;
	private float zoomTarget = 0.0f;
	private float zoomSpeed = 10.0f;
	private List<float> zoom = new List<float>();
	
	// Camera Rotation
	private int rotateFactor = 0;
	private float rotateSpeed = 10.0f;
	private Quaternion rotateTarget = Quaternion.identity;
	private List<float> rotate = new List<float>();

	// Camera tilt
	private float cameraTiltSpeed = 10.0f;
	private Vector3 cameraTiltPosition = Vector3.zero;
	private Vector3 cameraTiltPositionMin = new Vector3(0.0f, 20.0f, -70.0f);
	private Vector3 cameraTiltPositionMax = new Vector3(0.0f, 50.0f, 0.0f);
	private Vector3 cameraTiltPositionDefault = new Vector3(0.0f, 50, -70.0f);
	private Vector3 cameraTiltRotation = Vector3.zero;
	private Vector3 cameraTiltRotationMin = new Vector3(15.0f, 0.0f, 0.0f);
	private Vector3 cameraTiltRotationMax = new Vector3(90.0f, 0.0f, 0.0f);
	private Vector3 cameraTiltRotationDefault = new Vector3(35.0f, 0.0f, 0.0f);

	// Focus on target
	private bool focusing = true;
	private float focusSpeed = 5.0f;

	// Movement
	private Vector3 cameraPosition = new Vector3(0.0f, 0.0f, 0.0f);
	private Vector3 cameraSpeed = new Vector3(20.0f, 0.0f, 20.0f);

	// Camera bounds
	private float cameraMaxZ = 75.0f;
	private float cameraMinZ = -75.0f;
	private float cameraMaxX = 75.0f;
	private float cameraMinX = -75.0f;

	// Default values for map screen
	private float mapMaxZ = 75.0f;
	private float mapMinZ = -75.0f;
	private float mapMaxX = 75.0f;
	private float mapMinX = -75.0f;

	// Effects
	private BlurOptimized blur;
	private float blurValue = 0.0f;
	private float blurMax = 3.0f;
	private bool effects = false;
	private float effectsSpeed = 0.0f;

	// Flash
	private Image flash;
	private Color flashColor = new Color(1.0f, 1.0f, 1.0f, 0.0f);
	private bool flashActive = false;
	private float flashAlpha = 0.0f;
	private float flashSpeed = 0.0f;
	private float flashMax = 0.75f;



	void Start () {
		myTransform = transform;
		myCamera = myTransform.Find("Camera").GetComponent<Camera>();
		blur = myCamera.GetComponent<BlurOptimized>();
		flash = myTransform.parent.Find("GUI/Effects/Flash").GetComponent<Image>();

		// Add list of zooms
		zoom.Add(5.0f);
		zoom.Add(10.0f);
		zoom.Add(20.0f);
		zoom.Add(30.0f);
		zoom.Add(50.0f);

		// Add list of rotates
		rotate.Add(0.0f);
		rotate.Add(120.0f);
		rotate.Add(240.0f);

		// Default tilt
		cameraTiltPosition = cameraTiltPositionDefault;
		cameraTiltRotation = cameraTiltRotationDefault;

		// Default camera bounds
		cameraMaxZ = mapMaxZ;
		cameraMinZ = mapMinZ;
		cameraMaxX = mapMaxX;
		cameraMinX = mapMinX;
	}
	
	void Update () {
		if(!Global.system.isPaused) {

			// Focus on player
			Focus();

			// Camera movement
			Zoom();
			Movement();
			Rotation();

			// Camera effects
			TiltDisplay();
			EffectsDisplay();
			FlashDisplay();
		}
	}


	// CAMERA BOUNDS

	public void Bounds(float top, float bottom, float left, float right) {
		cameraMaxZ = top;
		cameraMinZ = bottom;
		cameraMaxX = right;
		cameraMinX = left;
	}
	

	// CAMERA FOCUS ON TARGET

	void Focus() {
		if(Input.GetKeyDown(KeyCode.F)) {
			if(Global.system.focusTarget != null) {
				focusing = true;
				Vector3 newTarget = new Vector3(Global.system.focusTarget.position.x, myTransform.position.y, Global.system.focusTarget.position.z);
				float dist = Vector3.Distance(myTransform.position, newTarget);				
				if(dist > 10.0f) {
					EffectsActivate(focusSpeed);
				}
			}
		}
		if(focusing) {
			if(Global.system.focusTarget != null) {
				Vector3 newTarget = new Vector3(Global.system.focusTarget.position.x, myTransform.position.y, Global.system.focusTarget.position.z);
				float dist = Vector3.Distance(myTransform.position, newTarget);
				if(dist < 0.05f) {
					myTransform.position = newTarget;
					focusing = false;
				}
				else {
					myTransform.position = Vector3.Lerp(myTransform.position, newTarget, focusSpeed * Time.deltaTime);
				}
			}
		}
	}


	// CAMERA ZOOM
	
	void Zoom() {
		if(Input.GetAxis("Mouse ScrollWheel") < 0) {
			ZoomChange(false);
		}
		else if(Input.GetAxis("Mouse ScrollWheel") > 0) {
			ZoomChange(true);
		}
		zoomTarget = Mathf.Lerp(myCamera.orthographicSize, zoom[zoomFactor], zoomSpeed * Time.deltaTime);
		myCamera.orthographicSize = zoomTarget;
	}

	void ZoomChange(bool lower) {
		if(lower && zoomFactor > 0) {
			zoomFactor--;
			EffectsActivate(zoomSpeed);
		}
		else if(!lower && zoomFactor < zoom.Count - 1) {
			zoomFactor++;
			EffectsActivate(zoomSpeed);
		}
		zoomFactor = Mathf.Clamp(zoomFactor, 0, zoom.Count - 1);
		if(zoomFactor == zoom.Count - 1) {
			if(zoomFactorOld != zoomFactor) {
				Global.system.weather.CloudsActivate(true);
			}
		}
		else {
			Global.system.weather.CloudsActivate(false);
		}
		FlashActivate(zoomSpeed);
		TiltActivate();
		zoomFactorOld = zoomFactor;
	}


	// CAMERA MOVEMENT

	void Movement() {
		cameraPosition.x = 0.0f;
		cameraPosition.z = 0.0f;

		if(Input.GetKey(KeyCode.W)) {
			cameraPosition.z += cameraSpeed.x * Time.deltaTime;
			focusing = false;
			Speed();
		}
		if(Input.GetKey(KeyCode.S)) {
			cameraPosition.z -= cameraSpeed.x * Time.deltaTime;
			focusing = false;
			Speed();
		}
		if(Input.GetKey(KeyCode.A)) {
			cameraPosition.x -= cameraSpeed.x * Time.deltaTime;
			focusing = false;
			Speed();
		}
		if(Input.GetKey(KeyCode.D)) {
			cameraPosition.x += cameraSpeed.x * Time.deltaTime;
			focusing = false;
			Speed();
		}


		// Move with mouse button and mouse movement
		//if(Input.GetMouseButton(2)) {
		//	cameraPosition.x = -cameraSpeed.x * Input.GetAxis("Mouse X");
		//	cameraPosition.z = -cameraSpeed.z * Input.GetAxis("Mouse Y");
		//	myTransform.Translate(cameraPosition);
		//	CameraCheck();
		//}
	}


	void Speed() {
		if(cameraPosition.x != 0 && cameraPosition.z != 0) {
			cameraPosition *= 0.7071f;
		}
		Check();
		myTransform.Translate(cameraPosition);
	}

	void Check() {
		Vector3 myPos = myTransform.localPosition;
		myPos.z = Mathf.Clamp(myPos.z, cameraMinZ, cameraMaxZ);
		myPos.x = Mathf.Clamp(myPos.x, cameraMinX, cameraMaxX);
		myTransform.localPosition = myPos;
	}


	// CAMERA ROTATION

	void Rotation() {
		if(Input.GetKeyDown(KeyCode.Q)) {
			rotateFactor++;
			EffectsActivate(rotateSpeed);
			FlashActivate(rotateSpeed);
		}
		if(Input.GetKeyDown(KeyCode.E)) {
			rotateFactor--;
			EffectsActivate(rotateSpeed);
			FlashActivate(rotateSpeed);
		}
		if(rotateFactor < 0) {
			rotateFactor = rotate.Count - 1;
		}
		else if(rotateFactor > rotate.Count - 1) {
			rotateFactor = 0;
		}
		rotateTarget = Quaternion.Lerp(myTransform.rotation, Quaternion.Euler(0, rotate[rotateFactor], 0), rotateSpeed * Time.deltaTime);
		myTransform.rotation = rotateTarget;
	}


	// CAMERA TILT

	void TiltActivate() {
		if(zoomFactor == 0) {
			cameraTiltRotation = cameraTiltRotationMin;
			cameraTiltPosition = cameraTiltPositionMin;
		}
		else if(zoomFactor == zoom.Count - 1) {
			cameraTiltRotation = cameraTiltRotationMax;
			cameraTiltPosition = cameraTiltPositionMax;
		}
		else {
			cameraTiltRotation = cameraTiltRotationDefault;
			cameraTiltPosition = cameraTiltPositionDefault;
		}
	}

	void TiltDisplay() {
		myCamera.transform.localEulerAngles = Vector3.Lerp(myCamera.transform.localEulerAngles, cameraTiltRotation, cameraTiltSpeed * Time.deltaTime);
		myCamera.transform.localPosition = Vector3.Lerp(myCamera.transform.localPosition, cameraTiltPosition, cameraTiltSpeed * Time.deltaTime);
	}



	// CAMERA EFFECTS

	void EffectsActivate(float speed) {
		effectsSpeed = Mathf.Abs(speed) / 1.5f;
		blurValue = blurMax;
		blur.enabled = true;
		effects = true;
	}

	void EffectsDisplay() {
		if(effects) {
			blurValue -= effectsSpeed * Time.deltaTime;
			blurValue = Mathf.Clamp(blurValue, 0.0f, blurMax);
			blur.blurSize = blurValue;
			if(blurValue <= 0.0f) {
				effects = false;
				blur.enabled = false;
			}
		}
	}
	
	void FlashActivate(float speed) {
		flashActive = true;
		flashAlpha = flashMax;
		flashSpeed = Mathf.Abs(speed) / 10.0f;
		flashColor = Global.system.weather.ambientColor;
	}

	void FlashDisplay() {
		if(flashActive) {
			if(flashAlpha <= 0.0f) {
				flashAlpha = 0.0f;
				flashActive = false;
			}
			else {
				flashAlpha -= flashSpeed * Time.deltaTime;
			}
			flashColor.a = flashAlpha;
			flash.color = flashColor;
		}
	}
}