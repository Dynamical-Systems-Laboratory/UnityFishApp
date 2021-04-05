using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class textfader : MonoBehaviour { // null

	Color col;
	float a = 1;
	float b = 0;
	float step = 0;
	float time;


	// Use this for initialization
	void Start () {
	}

	// Update is called once per frame
	void Update() {

		fadein ();

		if (step == 1) {
			time += Time.deltaTime;
		}

		if (time > 2) {
			fadeout ();
		}

	}

	void fadein(){
		if (step == 0) {
			b += 0.005f;	
			gameObject.GetComponent<Text> ().color = new Color (1f, 1f, 1f, b);
			col = gameObject.GetComponent<Text> ().color;
			if (col.a > 1f) {
				step = 1;
			}
		}
	}

	void fadeout(){
		if (step == 1) {
			a -= 0.005f;
			gameObject.GetComponent<Text> ().color = new Color (1f, 1f, 1f, a);
		}
	}
}
