using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HUD : MonoBehaviour {

	// Objects
	public RectTransform map;

	// Days
	public Text dayText;
	public Text dayNight;

	// Food and water
	public RectTransform meatBar;
	public RectTransform meatBarPath;
	public Text meatText;
	public RectTransform plantsBar;
	public RectTransform plantsBarPath;
	public Text plantsText;
	public RectTransform waterBar;
	public RectTransform waterBarPath;
	public Text waterText;
	private Color waterColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
		
	private float barMax = 500.0f;
	private Color ColorDefault = new Color(1.0f, 1.0f, 1.0f, 1.0f);
	private Color ColorWarning = new Color(1.0f, 0.0f, 0.0f, 1.0f);

	// Variables
	private bool battle = true;


	void Start () {
		battle = Global.system.battle;
		ShowHUD();
		
	}
	void Update () {
		ShowMap();
	}


	// SHOW OR HIDE HUD DEPENDING ON SCENE (BATTLE OR NOT)
	void ShowHUD() {
		map.gameObject.SetActive(!battle);
	}

	// SHOW MAP OBJECTS IF NOT BATTLE SCENE
	void ShowMap() {
		if(!battle) {
			CalculateResources();
		}
	}

	// CALCULATE RESOURCES
	void CalculateResources() {
		
		// Time
		string time = "";
		string journey = "";
		if(!Global.system.playerMove) {

			// Days
			int timePassed = Global.system.statistics.daysPassed;
			int timeNeed = Global.system.map.timeNeed;
			int timeSpend = timePassed + timeNeed;
			time = timePassed.ToString() + " + " + timeNeed.ToString() + " = " + timeSpend.ToString();

			// Journey
			journey = "Journey ends in ";
			int timeTiles = Global.system.map.pathPlanned.Count + Global.system.map.pathFinal.Count;

			if(Global.system.isDay) {
				if(timeTiles % 2 == 0) {
					journey += "day";
				}
				else {
					journey += "night";
				}
			}
			else {
				if(timeTiles % 2 == 0) {
					journey += "night";
				}
				else {
					journey += "day";
				}
			}
		}
		else {
			time = Global.system.statistics.daysPassed.ToString();
			if(Global.system.isDay) {
				journey = "Day";
			}
			else {
				journey = "Night";
			}
		}
		dayText.text = time;
		dayNight.text = journey;

		// Meat
		float meat = (float)Global.system.tamer.meat;
		string meatUsage = "";
		if(meat > 0) {
			float meatMax = (float)Global.system.tamer.meatMax;
			float meatPercentage = (meat / meatMax) * barMax;
			meatBar.sizeDelta = new Vector2(meatPercentage, meatBar.sizeDelta.y);

			// Meat: usage
			Color meatColor = ColorDefault;
			if(!Global.system.playerMove) {
				float meatPath = (float)Global.system.map.meatNeed;
				float meatPathMax = (float)Global.system.tamer.meat;
				float meatPathPercentage = meatPath / meatPathMax;
				if(meatPathPercentage > 1.0f) {
					meatPathPercentage = 1.0f;
				}
				meatPathPercentage *= meatBar.sizeDelta.x;
				meatBarPath.sizeDelta = new Vector2(meatPathPercentage, meatBar.sizeDelta.y);
				meatBarPath.anchoredPosition = new Vector2(meatBar.anchoredPosition.x + meatBar.sizeDelta.x, meatBarPath.anchoredPosition.y);
				int meatCalculated = Global.system.tamer.meat - Mathf.CeilToInt(Global.system.map.meatNeed);
				meatUsage = Global.system.tamer.meat.ToString() + " - " + Global.system.map.meatNeed.ToString() + " = " + meatCalculated.ToString();
				if(meatCalculated <= 0) {
					meatColor = ColorWarning;
				}
			}
			else {
				meatBarPath.sizeDelta = new Vector2(0.0f, 0.0f);
				meatUsage = Global.system.tamer.meat.ToString();
			}
			meatText.text = meatUsage;
			meatText.color = meatColor;
		}
		else {
			if(!Global.system.playerMove) {
				int meatCalculated = Global.system.tamer.meat - Mathf.CeilToInt(Global.system.map.meatNeed);
				meatUsage = Global.system.tamer.meat.ToString() + " - " + Global.system.map.meatNeed.ToString() + " = " + meatCalculated.ToString();
			}
			else {
				meatUsage = Global.system.tamer.meat.ToString();
			}
			meatBar.sizeDelta = new Vector2(0.0f, 0.0f);
			meatBarPath.sizeDelta = new Vector2(0.0f, 0.0f);
			meatText.text = meatUsage;
			meatText.color = ColorWarning;
		}

		// Plants
		float plants = (float)Global.system.tamer.plants;
		string plantsUsage = "";
		if(plants > 0) {
			float plantsMax = (float)Global.system.tamer.plantsMax;
			float plantsPercentage = (plants / plantsMax) * barMax;
			plantsBar.sizeDelta = new Vector2(plantsPercentage, plantsBar.sizeDelta.y);

			// Plants: usage
			Color plantsColor = ColorDefault;
			if(!Global.system.playerMove) {
				float plantsPath = (float)Global.system.map.plantsNeed;
				float plantsPathMax = (float)Global.system.tamer.plants;
				float plantsPathPercentage = plantsPath / plantsPathMax;
				if(plantsPathPercentage > 1.0f) {
					plantsPathPercentage = 1.0f;
				}
				plantsPathPercentage *= plantsBar.sizeDelta.x;
				plantsBarPath.sizeDelta = new Vector2(plantsPathPercentage, plantsBar.sizeDelta.y);
				plantsBarPath.anchoredPosition = new Vector2(plantsBar.anchoredPosition.x + plantsBar.sizeDelta.x, plantsBarPath.anchoredPosition.y);
				int plantsCalculated = Global.system.tamer.plants - Mathf.CeilToInt(Global.system.map.plantsNeed);
				plantsUsage = Global.system.tamer.plants.ToString() + " - " + Global.system.map.plantsNeed.ToString() + " = " + plantsCalculated.ToString();
				if(plantsCalculated <= 0) {
					plantsColor = ColorWarning;
				}
			}
			else {
				plantsBarPath.sizeDelta = new Vector2(0.0f, 0.0f);
				plantsUsage = Global.system.tamer.plants.ToString();
			}
			plantsText.text = plantsUsage;
			plantsText.color = plantsColor;
		}
		else {
			if(!Global.system.playerMove) {
				int plantsCalculated = Global.system.tamer.plants - Mathf.CeilToInt(Global.system.map.plantsNeed);
				plantsUsage = Global.system.tamer.plants.ToString() + " - " + Global.system.map.plantsNeed.ToString() + " = " + plantsCalculated.ToString();
			}
			else {
				plantsUsage = Global.system.tamer.plants.ToString();
			}
			plantsBar.sizeDelta = new Vector2(0.0f, 0.0f);
			plantsBarPath.sizeDelta = new Vector2(0.0f, 0.0f);
			plantsText.text = "0";
			plantsText.color = ColorWarning;
		}
		
		// Water
		float water = (float)Global.system.tamer.water;
		if(water > 0) {
			float waterMax = (float)Global.system.tamer.waterMax;
			float waterPercentage = (water / waterMax);
			waterPercentage *= barMax;
			waterBar.sizeDelta = new Vector2(waterPercentage, waterBar.sizeDelta.y);

			// Water: usage
			string waterUsage = "";
			Color waterColorCurrent = waterColor;
			if(!Global.system.playerMove) {
				float waterPath = (float)Global.system.map.waterNeed;
				float waterPathMax = waterBar.sizeDelta.x;
				float waterPathPercentage = waterPath / waterPathMax;
				if(waterPathPercentage > 1.0f) {
					waterPathPercentage = 1.0f;
				}
				waterPathPercentage *= waterPathMax;
				waterBarPath.sizeDelta = new Vector2(waterPathPercentage, waterBar.sizeDelta.y);
				waterBarPath.anchoredPosition = new Vector2(waterBar.anchoredPosition.x + waterBar.sizeDelta.x, waterBarPath.anchoredPosition.y);

				int waterCalculated = Global.system.tamer.water - (int)Global.system.map.waterNeed;
				if(waterCalculated < 0) { waterCalculated = 0; }
				waterUsage = Global.system.tamer.water.ToString() + " - " + Global.system.map.waterNeed.ToString() + " = " + waterCalculated.ToString();
				if(waterCalculated <= 0) {
					waterColorCurrent = ColorWarning;
				}
			}
			else {
				waterBarPath.sizeDelta = new Vector2(0.0f, 0.0f);
				waterUsage = Global.system.tamer.water.ToString();
			}
			waterText.text = waterUsage;
			waterText.color = waterColorCurrent;
		}
		else {
			waterBar.sizeDelta = new Vector2(0.0f, 0.0f);
			waterBarPath.sizeDelta = new Vector2(0.0f, 0.0f);
			waterText.text = "0";
			waterText.color = ColorWarning;
		}
	}
}