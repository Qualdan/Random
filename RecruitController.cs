using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class RecruitController : MonoBehaviour {

	[Header("Panels")]
	public Transform mapPanel;
	public Transform characterPanel;
	public Transform systemPanel;
	public Transform confirmQuest;
	public Transform weightPanel;

	[Header("View Objects")]
	public Transform recruitListView;
	public Transform armourListView;
	public Transform weaponListView;
	public Transform playerView;
	public Transform recruitView1;
	public Transform recruitView2;

	[Header("Prefabs")]
	public Transform recruitPrefab;
	public Transform armourPrefab;
	public Transform weaponPrefab;

	[Header("Recruits")]
	public Character recruit1;
	public Character recruit2;
	public List<RectTransform> recruitList = new List<RectTransform>();
	public int changeRecruit = 0;

	[Header("Inventory")]
	public int changeArmour = 0;
	public List<RectTransform> armourList = new List<RectTransform>();
	public int changeWeapon = 0;
	public List<RectTransform> weaponList = new List<RectTransform>();

	// Quests
	public int bossSeed0 = 0;
	public int bossSeed1 = 0;
	public int bossSeed2 = 0;
	public int currentQuest = 0;


	// RECRUIT CHANGING
	public void ChooseRecruit(int index) {
		changeRecruit = index;
		for(int l = 0; l < recruitList.Count; l++) {
			Destroy(recruitList[l].gameObject);
		}
		recruitList.Clear();
		int recruitIndex = 0;
		for(int r = 0; r < Global.system.recruits.Count; r++) {
			if(!Global.system.recruits[r].combatant) {
				RectTransform rc = Instantiate(recruitPrefab, Vector3.zero, Quaternion.identity) as RectTransform;
				rc.SetParent(recruitListView.Find("Scroll/Viewport/Content"));
				rc.anchoredPosition = new Vector2(20.0f, -10.0f + recruitIndex * -150.0f);
				Vector2 size = recruitListView.Find("Scroll/Viewport/Content").GetComponent<RectTransform>().sizeDelta;
				recruitListView.Find("Scroll/Viewport/Content").GetComponent<RectTransform>().sizeDelta = new Vector2(size.x, 160.0f + recruitIndex * 150.0f);
				int buttonIndex = r;
				rc.Find("Change").GetComponent<Button>().onClick.AddListener(() => ChangeRecruit(buttonIndex));
				ChangeInfo(Global.system.recruits[r], rc);
				recruitList.Add(rc);
				recruitIndex++;
			}
		}
		recruitListView.gameObject.SetActive(true);
	}
	public void ChangeRecruit(int index) {
		if(changeRecruit == 1) {
			if(recruit1 != null) {
				recruit1.combatant = false;
			}
			recruit1 = Global.system.recruits[index];
			recruit1.combatant = true;
			recruit1.recruitID = 1;
			ChangeInfo(recruit1, recruitView1);
		}
		else if(changeRecruit == 2) {
			if(recruit2 != null) {
				recruit2.combatant = false;
			}
			recruit2 = Global.system.recruits[index];
			recruit2.combatant = true;
			recruit2.recruitID = 2;
			ChangeInfo(recruit2, recruitView2);
		}
		recruitListView.gameObject.SetActive(false);
	}
	public void CancelRecruit() {
		recruitListView.gameObject.SetActive(false);
	}

	// ARMOUR CHANGING
	public void ChooseArmour(int index) {
		changeArmour = index;
		for(int l = 0; l < armourList.Count; l++) {
			Destroy(armourList[l].gameObject);
		}
		armourList.Clear();
		int armourIndex = 0;
		for(int a = 0; a < Global.system.armours.Count; a++) {
			if(!Global.system.armours[a].inUse) {
				RectTransform ar = Instantiate(armourPrefab, Vector3.zero, Quaternion.identity) as RectTransform;
				ar.SetParent(armourListView.Find("Scroll/Viewport/Content"));
				ar.anchoredPosition = new Vector2(20.0f, -10.0f + armourIndex * -150.0f);
				Vector2 size = armourListView.Find("Scroll/Viewport/Content").GetComponent<RectTransform>().sizeDelta;
				armourListView.Find("Scroll/Viewport/Content").GetComponent<RectTransform>().sizeDelta = new Vector2(size.x, 160.0f + armourIndex * 150.0f);
				int buttonIndex = a;
				ar.Find("Change").GetComponent<Button>().onClick.AddListener(() => ChangeArmour(buttonIndex));
				ChangeInfoArmour(Global.system.armours[a], ar);
				armourList.Add(ar);
				armourIndex++;
			}
		}
		armourListView.gameObject.SetActive(true);
	}
	public void ChangeArmour(int index) {
		int weight = Global.system.armours[index].weight + Global.system.player.weapon.weight;
		int strength = Global.system.player.strength + 10;
		if(weight <= strength) {
			if(changeArmour == 0) {
				Global.system.player.armour.inUse = false;
				Global.system.player.armour = Global.system.armours[index];
				Global.system.player.armour.inUse = true;
				ChangeInfo(Global.system.player, playerView);
			}
			else if(changeArmour == 1) {
				recruit1.armour.inUse = false;
				recruit1.armour = Global.system.armours[index];
				recruit1.armour.inUse = true;
				ChangeInfo(recruit1, recruitView1);
			}
			else if(changeArmour == 2) {
				recruit2.armour.inUse = false;
				recruit2.armour = Global.system.armours[index];
				recruit2.armour.inUse = true;
				ChangeInfo(recruit2, recruitView2);
			}
			armourListView.gameObject.SetActive(false);
		}
		else {
			ShowWeight(true);
		}
	}
	public void CancelArmour() {
		armourListView.gameObject.SetActive(false);
	}

	// WEAPON CHANGING
	public void ChooseWeapon(int index) {
		changeWeapon = index;
		for(int l = 0; l < weaponList.Count; l++) {
			Destroy(weaponList[l].gameObject);
		}
		weaponList.Clear();
		int weaponIndex = 0;
		for(int w = 0; w < Global.system.weapons.Count; w++) {
			if(!Global.system.weapons[w].inUse) {
				RectTransform wp = Instantiate(weaponPrefab, Vector3.zero, Quaternion.identity) as RectTransform;
				wp.SetParent(weaponListView.Find("Scroll/Viewport/Content"));
				wp.anchoredPosition = new Vector2(20.0f, -10.0f + weaponIndex * -150.0f);
				Vector2 size = weaponListView.Find("Scroll/Viewport/Content").GetComponent<RectTransform>().sizeDelta;
				weaponListView.Find("Scroll/Viewport/Content").GetComponent<RectTransform>().sizeDelta = new Vector2(size.x, 160.0f + weaponIndex * 150.0f);
				int buttonIndex = w;
				wp.Find("Change").GetComponent<Button>().onClick.AddListener(() => ChangeWeapon(buttonIndex));
				weaponList.Add(wp);
				ChangeInfoWeapon(Global.system.weapons[w], wp);
				weaponIndex++;
			}
		}
		weaponListView.gameObject.SetActive(true);
	}
	public void ChangeWeapon(int index) {
		int weight = Global.system.weapons[index].weight + Global.system.player.armour.weight;
		int strength = Global.system.player.strength + 10;
		if(weight <= strength) {
			if(changeWeapon == 0) {
				Global.system.player.weapon.inUse = false;
				Global.system.player.weapon = Global.system.weapons[index];
				Global.system.player.weapon.inUse = true;
				ChangeInfo(Global.system.player, playerView);
			}
			if(changeWeapon == 1) {
				recruit1.weapon.inUse = false;
				recruit1.weapon = Global.system.weapons[index];
				recruit1.weapon.inUse = true;
				ChangeInfo(recruit1, recruitView1);
			}
			else if(changeWeapon == 2) {
				recruit2.weapon.inUse = false;
				recruit2.weapon = Global.system.weapons[index];
				recruit2.weapon.inUse = true;
				ChangeInfo(recruit2, recruitView2);
			}
			weaponListView.gameObject.SetActive(false);
		}
		else {
			ShowWeight(true);
		}
	}
	public void CancelWeapon() {
		weaponListView.gameObject.SetActive(false);
	}


	// CHANGE CHARACTER INFO
	public void ChangeInfo(Character character, Transform view) {

		// Name
		string name = character.name;
		if(name == null || name == "" || name == " ") { name = "Noname McNoname"; }
		view.Find("Name").GetComponent<Text>().text = name;

		// Stats
		//string values0 = "Type: " + character.type.ToString() + "\nHealth: " + character.healthMax + "\nStrength: " + character.strength + "\nEndurance: " + character.endurance;
		string values0 = "Health: " + character.healthMax + "\nStrength: " + character.strength + "\nEndurance: " + character.endurance;
		string values1 = "Focus: " + character.focus + "\nFirearms: " + character.firearms + "\nInitiative: " + character.initiative;
		view.Find("Values0").GetComponent<Text>().text = values0;
		view.Find("Values1").GetComponent<Text>().text = values1;

		// Armour/weapon
		ChangeInfoArmour(character.armour, view);
		ChangeInfoWeapon(character.weapon, view);
	}
	void ChangeInfoArmour(Armour armour, Transform view) {
		string valuesAR = "Defense: " + armour.defense + "\nWeight: " + armour.weight;
		view.Find("ValuesAR").GetComponent<Text>().text = valuesAR;
		int arType = 0;
		if (armour.type == Armour.armourTypes.medium) {
			arType = 1;
		}
		else if(armour.type == Armour.armourTypes.heavy) {
			arType = 2;
		}
		Sprite arSprite = Resources.Load<Sprite>("Armours/" + arType.ToString());
		view.Find("ImageAR").GetComponent<Image>().sprite = arSprite;
	}
	void ChangeInfoWeapon(Weapon weapon, Transform view) { 
		string valuesWP0 = "Damage: " + weapon.damage + " (x" + weapon.firerate + ")" + "\nFirerate: " + weapon.firerate + "\nAccurary: " + weapon.accuracy + "\nRange: " + weapon.range;
		string valuesWP1 = "Magazine: " + weapon.magazineMax + "\nWeight: " + weapon.weight + "\nAP shoot: " + weapon.apShooting + "\nAP reload: " + weapon.apReload;
		view.Find("NameWP").GetComponent<Text>().text = "Weapon: " + weapon.type.ToString();
			view.Find("ValuesWP0").GetComponent<Text>().text = valuesWP0;
			view.Find("ValuesWP1").GetComponent<Text>().text = valuesWP1;

		// Change weapon type
		List<Sprite> wp = new List<Sprite>();
		Weapon.weaponTypes type = weapon.type;
		Transform wpType = view.Find("Weapons/" + type.ToString());
		int barrel = weapon.barrel;
		int body = weapon.body;
		int handle = weapon.handle;
		int scope = weapon.scope;
		if (type == Weapon.weaponTypes.pistol) {
			wp = Global.system.pistol;
		}
		else if(type == Weapon.weaponTypes.smg) {
			wp = Global.system.smg;
		}
		else if(type == Weapon.weaponTypes.shotgun) {
			wp = Global.system.shotgun;
		}
		else if(type == Weapon.weaponTypes.rifle) {
			wp = Global.system.rifle;
		}

		// Lists
		List<Sprite> barrels = new List<Sprite>();
		List<Sprite> bodies = new List<Sprite>();
		List<Sprite> handles = new List<Sprite>();
		List<Sprite> scopes = new List<Sprite>();
		for(int w = 0; w<wp.Count; w++) {
			if(wp[w].name.Contains("barrels")) {
				barrels.Add(wp[w]);
			}
			if(wp[w].name.Contains("bodies")) {
				bodies.Add(wp[w]);
			}
			if(wp[w].name.Contains("handles")) {
				handles.Add(wp[w]);
			}
			if(wp[w].name.Contains("scopes")) {
				scopes.Add(wp[w]);
			}
		}

		// Error check
		if(barrel > barrels.Count) { barrel = 0; }
		if(body > bodies.Count) { body = 0; }
		if(handle > handles.Count) { handle = 0; }
		if(scope > scopes.Count) { scope = 0; }

		// Set sprites
		view.Find("Weapons/pistol").gameObject.SetActive(false);
		view.Find("Weapons/smg").gameObject.SetActive(false);
		view.Find("Weapons/shotgun").gameObject.SetActive(false);
		view.Find("Weapons/rifle").gameObject.SetActive(false);
		if(barrels.Count > 0 && bodies.Count > 0 && handles.Count > 0) {
			wpType.Find("Barrel").GetComponent<Image>().sprite = barrels[barrel];
			wpType.Find("Body").GetComponent<Image>().sprite = bodies[body];
			wpType.Find("Handle").GetComponent<Image>().sprite = handles[handle];
			if(type != Weapon.weaponTypes.pistol) {
				wpType.Find("Scope").GetComponent<Image>().sprite = scopes[scope];
			}
			wpType.gameObject.SetActive(true);
		}
	}


	public void ShowWeight(bool show) {
		weightPanel.gameObject.SetActive(show);
	}
}