using UnityEngine;
using System.Collections;

public class Targeting : MonoBehaviour {

	GameController game;
	private Transform myTransform;
	private LineRenderer myLine;
	private LineRenderer myTarget;
	Material myEmission;

	// Colors
	public Color colorShoot = new Color(0.2f, 0.2f, 0.8f, 1.0f);
	public Color colorRange = new Color(1.0f, 0.1f, 0.1f, 1.0f);

	// Variables
	private Vector3 myPosition = Vector3.zero;
	private float radius = 1.5f;
	private float radiusTemp = 1.5f;
	private float radiusSpeed = 5.0f;
	private int resolution = 120;
	
	// Use this for initialization
	void Start () {
		game = Global.system.game;
		myTransform = transform;
		myLine = myTransform.Find("Line").GetComponent<LineRenderer>();
		myTarget = myTransform.Find("Target").GetComponent<LineRenderer>();
		myEmission = myLine.material;
		Circle(false);
	}

	void Update() {
		CircleSize();
	}


	// DEFICE CIRCLE POSITION
	public void Circle(bool on) {
		if(on) {
			myPosition = Global.system.game.actionTarget.model.transform.position;
			myPosition.z = myPosition.y - radius;
			myTransform.position = myPosition;
		}
		myLine.gameObject.SetActive(on);
		myTarget.gameObject.SetActive(on);
	}

	// SET CIRCLE SIZE
	void CircleSize() {
		if(game.combatants.Count > 0) {
			float chance = game.ShootingChance(game.combatants[game.combatant].model.transform.position, myTransform.position);
			radiusTemp = game.targetRadius;
			radius = Mathf.Lerp(radius, radiusTemp, radiusSpeed * Time.deltaTime);

			bool los = true;
			if(Global.system.game.actionTarget != null) {
				los = Global.system.grid.LoSCheck(Global.system.game.combatants[Global.system.game.combatant].coordinates, Global.system.game.actionTarget.coordinates, false);
			}
			CircleRange(chance, los);

			// Set circle
			myLine.SetVertexCount(resolution + 1);
			for(var i = 0; i < resolution + 1; i++) {
				int angle = (360 / resolution) * i;
				myLine.SetPosition(i, myTransform.position + radius * new Vector3(Mathf.Sin(Mathf.Deg2Rad * angle), Mathf.Cos(Mathf.Deg2Rad * angle) / 2.0f, 0.0f));
			}

			// Set target circle
			myTarget.SetVertexCount(resolution + 1);
			for(var i = 0; i < resolution + 1; i++) {
				int angle = (360 / resolution) * i;
				myTarget.SetPosition(i, myTransform.position + 1.5f * new Vector3(Mathf.Sin(Mathf.Deg2Rad * angle), Mathf.Cos(Mathf.Deg2Rad * angle) / 2.0f, 0.0f));
			}
		}
	}

	// SET CIRCLE COLOR
	void CircleRange(float inRange, bool los) {
		if(inRange > 0 && los) {
			myEmission.SetColor("_EmissionColor", colorShoot);
		}
		else {
			myEmission.SetColor("_EmissionColor", colorRange);
		}
	}
}