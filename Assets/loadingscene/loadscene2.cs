using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class loadscene2 : MonoBehaviour {

	// empty GameObject variable for the DSL logo
	GameObject logo;

	// Use this for initialization
	void Start () {

		// initialize by finding the GameObject with the proper logo name and assign it to the variable logo
		logo = GameObject.Find ("4004.tmp");
	
	}
	
	// Update is called once per frame
	void Update () {

		// if the alpha value of the logo is less than 0 (0 opacity)...
		if (logo.GetComponent<SpriteRenderer> ().color.a < 0f) {
			// load next scene (name: scene1_mesh)
			SceneManager.LoadScene ("scene1_mesh");
		}
	
	}
}
