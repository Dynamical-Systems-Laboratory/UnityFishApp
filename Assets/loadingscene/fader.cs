using UnityEngine; // This is the Miguel code
using UnityEngine.UI; // use for UI game objects such as text
using System.Collections;

public class fader : MonoBehaviour {

	// a is start alpha value at 100% opacity and b is end alpha value at 0% opacity
	float a = 1;
	float b = 0;

	// step variable between fadein and fadeout
	float step = 0;

	// time variable, starts at 0
	float time;

	// empty bool variable to determine whether game object is text or not
	bool isittxt;

	// Use this for initialization
	void Start () {

		// initialize to determine whether the game object is text
		if (gameObject.GetComponent<Text> () != null){ // if its Text component exists...
			isittxt = true; // the variable is true, game object is text
		} else{ // if the component does not exist
			isittxt = false; // the variable is false, game object is not text
		}

	}
		
	// Update is called once per frame
	void Update() {

		// start fading in the sprite
		fadein ();

		if (step == 1) { // when step equals 1...
			// begin counting the time each loop
			// variable time adds on the change in time per loop to its previous value
			time += Time.deltaTime;
		}

		if (time > 2) { // if time value has passed 2 seconds...
			// start fading out the sprite
			fadeout ();
		}

	}

	void fadein(){ // this function fades the game object into the scene
		if (step == 0) {
			// raise b value by 0.005 every loop
			b += 0.005f; 

			// change the sprite color of this game object
			if (isittxt == false) { // if the game object is not text...
				gameObject.GetComponent<SpriteRenderer> ().color = new Color (1f, 1f, 1f, b); // change color, first three remain the same, only alpha value is changed with variable b
			} else { // if the game object is text...
				gameObject.GetComponent<Text> ().color = new Color (1f, 1f, 1f, b); // change color, ""
			}

			if (b > 1f) { // when the variable b reaches 1 or 100% opacity...
				step = 1; // set step equal to 1
			}
		}
	}

	void fadeout(){ // this function fades the game object out of the scene
			// decrease a value by 0.005 every loop
			a -= 0.005f;

			// change the sprite color of this game object
		if (isittxt == false) { // if the game object is not text...
			gameObject.GetComponent<SpriteRenderer> ().color = new Color (1f, 1f, 1f, a); // change color, first three remain the same, obly alpha value is changed with variable a
		} else { // if the game object is text...
			gameObject.GetComponent<Text> ().color = new Color (1f, 1f, 1f, a); // change color, ""
		}
	}
}
