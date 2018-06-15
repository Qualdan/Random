
using UnityEngine;
using System.Collections;

public class Weather : MonoBehaviour {

	// Objects
	private Transform myTransform;
	private Transform globalLight;
	private MeshRenderer clouds;
	private MeshRenderer clouds2;

	// Colors	
	[HideInInspector]
	public Color ambientColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
	private Color ambientColorMorning = new Color(0.5f, 0.3f, 0.1f, 1.0f);
	private Color ambientColorNoon = new Color(1.0f, 1.0f, 0.7f, 1.0f);
	private Color ambientColorEvening = new Color(0.5f, 0.15f, 0.1f, 1.0f);
	private Color ambientColorNight = new Color(0.0f, 0.1f, 0.5f, 1.0f);

	// Time
	public bool timeOn = true;
	public float timeCurrent = 0.0f;
	private float timeOld = -1.0f;
	private float dayLength = 12.0f;

	// Clouds
	private bool cloudsOn = false;
	private float cloudsAlpha = 0.0f;
	private float cloudsMax = 0.5f;
	private float cloudsSpeed = 2.0f;
	private float cloudsMove = 0.01f;
	private Vector2 cloudsOffset = Vector2.zero;
	private float cloudsMove2 = 0.005f;
	private Vector2 cloudsOffset2 = Vector2.zero;

	// Sea
	private Transform sea;
	private MeshRenderer seaNormal;
	private float seaMove = 0.05f;
	private Vector2 seaOffset = Vector2.zero;
	private MeshRenderer seaFoam;
	private float seaMoveFoam = 0.03f;
	private Vector2 seaOffsetFoam = Vector2.zero;



	void Start() {
		myTransform = transform;
		globalLight = myTransform.Find("Weather/Lights/Light");
		clouds = myTransform.Find("Weather/Effects/Clouds/Clouds").GetComponent<MeshRenderer>();
		clouds2 = myTransform.Find("Weather/Effects/Clouds/Clouds2").GetComponent<MeshRenderer>();
		sea = myTransform.Find("Weather/Effects/Sea");
		seaNormal = sea.Find("SeaNormal").GetComponent<MeshRenderer>();
		seaFoam = sea.Find("SeaFoam").GetComponent<MeshRenderer>();

		// Startup setup
		Global.system.isDay = false;
		timeCurrent = 12;
		TimeCycle();
		Lights();
	}

	void Update() {
		if(!Global.system.isPaused) {
			TimeCycle();
			Lights();
			CloudsAlpha();
		}
		CloudsMove();
		SeaMove();
	}


	// TIME

	void TimeCycle() {
		if(timeOn && Global.system.playerMove) {
			timeCurrent = dayLength * (Global.system.timePercentage / 100.0f);
		}
		else {
			timeCurrent = Mathf.Round(timeCurrent);
		}
		timeCurrent = Mathf.Clamp(timeCurrent, 0.0f, dayLength);
	}


	public void TimeOn(bool value) {
		timeOn = value;
	}


	// LIGHTS

	void Lights() {

		// If time has changed (player is moving)
		if(timeOld != timeCurrent) {
			float rotY = 0.0f;
			float rotX = 50.0f;
			float rotZ = 0.0f;
			float lightIntensity = 0.0f;
			float shadowIntensity = 0.0f;
			if(!Global.system.isDay) {

				// Noon -> Evening
				if(timeCurrent < dayLength / 2.0f) {
					lightIntensity = 1.0f - (((timeCurrent * 2.0f) / dayLength) / 2.0f);
					shadowIntensity = 1.0f - ((timeCurrent * 2.0f) / dayLength);
					ambientColor = Color.Lerp(ambientColorNoon, ambientColorEvening, (timeCurrent * 2.0f) / dayLength);
				}

				// Evening -> Night
				else {
					lightIntensity = 0.5f;
					shadowIntensity = 0.0f;
					ambientColor = Color.Lerp(ambientColorNight, ambientColorEvening, 2.0f - ((timeCurrent * 2.0f) / dayLength));
				}

				// Rotate light
				rotY = 0.0f + (180.0f * (timeCurrent / dayLength));
			}
			else {

				// Night -> Morning
				if(timeCurrent < dayLength / 2.0f) {
					lightIntensity = 0.5f;
					shadowIntensity = 0.0f;
					ambientColor = Color.Lerp(ambientColorNight, ambientColorMorning, (timeCurrent * 2.0f) / dayLength);
				}

				// Morning -> Noon
				else {
					lightIntensity = 0.5f + ((timeCurrent - (dayLength / 2.0f)) / (dayLength / 2.0f) / 2.0f);
					shadowIntensity = 0.0f + (timeCurrent - (dayLength / 2.0f)) / (dayLength / 2.0f);
					ambientColor = Color.Lerp(ambientColorNoon, ambientColorMorning, 2.0f - ((timeCurrent * 2.0f) / dayLength));
				}

				// Rotate light
				rotY = 180.0f + (180.0f * (timeCurrent / dayLength));
			}

			// Set lights
			globalLight.rotation = Quaternion.Euler(rotX, rotY, rotZ);
			globalLight.GetComponent<Light>().intensity = lightIntensity;
			globalLight.GetComponent<Light>().shadowStrength = shadowIntensity;
			RenderSettings.ambientLight = ambientColor;

			// Time has changed
			timeOld = timeCurrent;
		}
	}







	// CLOUDS

	public void CloudsActivate(bool enable) {
		clouds.gameObject.SetActive(enable);
		clouds2.gameObject.SetActive(enable);
		cloudsOn = enable;
		cloudsAlpha = 0.0f;
	}

	void CloudsAlpha() {
		if(cloudsOn) {
			if(cloudsAlpha >= cloudsMax) {
				cloudsAlpha = cloudsMax;
			}
			else {
				cloudsAlpha += cloudsSpeed * Time.deltaTime;
			}
			Color cloudsColor = clouds.material.color;
			cloudsColor.a = cloudsAlpha;
			clouds.material.color = cloudsColor;
			clouds2.material.color = cloudsColor;
		}
	}

	void CloudsMove() {
		if(cloudsOn) {

			// 1
			cloudsOffset.x += cloudsMove * Time.deltaTime;
			cloudsOffset.y += cloudsMove * Time.deltaTime;
			if(cloudsOffset.x > 1.0f) {
				cloudsOffset.x = 0.0f;
			}
			if(cloudsOffset.y > 1.0f) {
				cloudsOffset.y = 0.0f;
			}
			clouds.material.SetTextureOffset("_MainTex", cloudsOffset);

			// 2
			cloudsOffset2.x += cloudsMove2 * Time.deltaTime;
			cloudsOffset2.y += 0.00005f + cloudsMove2 * Time.deltaTime;
			if(cloudsOffset2.x > 1.0f) {
				cloudsOffset2.x = 0.0f;
			}
			if(cloudsOffset2.y > 1.0f) {
				cloudsOffset2.y = 0.0f;
			}
			clouds2.material.SetTextureOffset("_MainTex", cloudsOffset2);
		}
	}

	void SeaMove() {
		sea.gameObject.SetActive(!Global.system.battle);
		if(!Global.system.battle) {

			// Normal
			seaOffset.x -= seaMove * Time.deltaTime;
			seaOffset.y -= seaMove * Time.deltaTime;
			if(seaOffset.x < 0.0f) {
				seaOffset.x = 1.0f;
			}
			if(seaOffset.y < 0.0f) {
				seaOffset.y = 1.0f;
			}
			seaNormal.material.SetTextureOffset("_MainTex", seaOffset);

			// Foam
			seaOffsetFoam.x -= seaMoveFoam * Time.deltaTime;
			seaOffsetFoam.y -= seaMoveFoam * Time.deltaTime;
			if(seaOffsetFoam.x < 0.0f) {
				seaOffsetFoam.x = 1.0f;
			}
			if(seaOffsetFoam.y < 0.0f) {
				seaOffsetFoam.y = 1.0f;
			}
			seaFoam.material.SetTextureOffset("_MainTex", seaOffsetFoam);
		}
	}
}