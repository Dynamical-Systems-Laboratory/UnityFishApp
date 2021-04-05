using UnityEngine;
using System.Collections;

public class singlefish_loading : MonoBehaviour {

	// empty variable to hold angle value of last frame 
	float prevangle;

	// variable k for variable timet, timet for time rate change per frame
	float variablek = 0;
	float timet = 0;

	// public GameObject variable fish, assign [name of this gameobject] to the variable from the scene tab 
	public GameObject fish;

	// constant variable values velocity and deltat
	public float velocity = 5;
	public float deltat = 0.01f;

	public float rotspeed = 1;

	// integer variable vcount to set the value of vertices on the matrix
	public int vcount = 100;

	// mesh variables
	private Vector3[] pval; // the x and y positions of the mesh vertices in relation to the game screen 
	private Vector2[] UV; // the x and y positions of the mesh vertices in relation to the image/texture
	private int[] Triangles; // the ordering of the vertices on the mesh
	public Vector3[] Vertices; // array of changing vertice values to move mesh
	Mesh stuff; // name of mesh
	float[] meshmover; // new points on mesh based off of carangiform equation

	// set vertice position and order on the game screen
	void Meshsetup(){

		// set Vertices to initial mesh values
		Vertices = pval; 

		// determine the length of the x bounds of the image being used
		// divide this value into intervals to space out the vertices evenly on the x axis
		float space = (.75f - .2f) / (vcount/2);

		// set up the vertice values for the image being used
		float ux = .75f; // start value at the right

		// establish x and y positions for the top vertices
		// determine what the upper y bounds for the image should be
		for (int i = 0; i < vcount; i = i + 2) { 
			UV [i] = new Vector2 (ux, .7f); // establish the x and y point for this vertice
			ux = ux - (space); // move to the left by an interval
		}

		// reset start value
		ux = .75f;
		// establish x and y position for the bottom vertices
		// determine what the lower y bounds for the image should be
		for (int i = 1; i < vcount; i = i + 2) {
			UV [i] = new Vector2 (ux, .4f); // ""
			ux = ux - (space); // ""
		}

		// set the three digit orders of the vertices for each triangle in the mesh
		// set initial values
		int newj = 0;
		int j;

		// if there are two rows of vertices, the top vertices run even from right to left starting from 0
		// the bottom vertices run odd from right to left starting from 1
		//  
		//  6    4    2    0
		//  .    .    .    .    
		//                 
		//
		//  .    .    .    .     
		//  7    5    3    1
		//
// ** see developers notes for more information on how the mesh is visualized
		//
		// start with evens and move counter clockwise
		// [ 012 234 456 ... ]
		for (int i = 0; i < vcount/2 - 1; ++i) {
			// set new integer variable to be double the current i value
			// this is the starting even number value in the current three digit order
			int newi = i + i;
			for (j = newj; j < newj + 3; ++j) { // the integer j will take the value of newj and run for 3 loops
				// set up the three digit order
				Triangles [j] = newi; 
				// increase the value by one to match the order of the even numbers
				newi += 1;
			}
			// save the previous j value which
			// this continues indexing Triangles from where it left off instead of starting over
			newj = j;
		}

		// continue with odds and move counter clockwise
		// [ 321 543 765 ... ]
		for (int i = 0; i < vcount/2 - 1; ++i) {
			// set new integer variable to be double plus 3 the current i value
			// this is the starting odd number value in the current three digit order
			int newi = 2*i + 3;
			for (j = newj; j < newj + 3; ++j) { // ""
				// set up the three digit order
				Triangles [j] = newi;
				// decrease the value by one to match the order of odd numbers
				newi -= 1;
			}
			// ""
			newj = j;

		}
	}

	// variable for frequency of tail beat
	float variablef = 0;
	// variable for the length of the fish
	float lengthf;
	// velocity value used to determine frequency
	float velocity2 = 5;
	// bool variable to determine whether mesh needs to be established
	// initial value is true
	bool setmesh = true;

	// variables for function Push to move gameobject 
	float startangle; // start angle of gameobject
	float endangle = 192; // end angle of gameobject, manually determined
	float thisangle; // current angle of gameobject
	// variables for function Push to change opacity of gameobject
	float a = 1f; // starting alpha value (100% opacity)
	public Material mat; // the material of the mesh, select through inspector
	float time; // time value to measure time change per frame

	// Initialize
	void Start () {

		// determine the starting angle position of the game object on the z axis
		startangle = gameObject.transform.eulerAngles.z;

		// establish the proper size of the array for the points needed to be moved in the mesh
		meshmover = new float[vcount / 2];

		// establish proper sizes for following variables
		pval = new Vector3[vcount];
		UV = new Vector2[vcount];
		// there are 2 triangles in 4 vertice points
		// there are 6 triangle orders in 4 vertice points
		// size of array = (No.ofvertices - 2) * 3
		Triangles = new int[(vcount - 2) * 3];  

		// establish the x and y positions of the vertices in the mesh on the game screen
		// start x value at the right
		float px = 0f;
		for (int i = 0; i < vcount; i = i + 2) { // loop for top vertices (evens)
			pval [i] = new Vector3 (px, .2f, 0f); // establish coordinate positions, determine top y value manually
			px = px - (.56f / (vcount/2)); // set px for next frame to be one interval to the left, determine length of mesh manually
		}

		// reset start x value
		px = 0f;
		for (int i = 1; i < vcount; i = i + 2) { // loop for bottom vertices (odds)
			pval [i] = new Vector3 (px, -.2f, 0f); // "", determine bottom y value manually
			px = px - (.56f / (vcount/2)); // ""
		}

		// run function Meshsetup to establish vertice positions and order
		Meshsetup ();
		// establish variable stuff as a new mesh
		stuff = new Mesh ();

		// establish the length of the fish
		// use value determined in pval array
		lengthf = (0.56f)/2;

		// save the value of the initial angle
		prevangle = startangle;
	}
		
	// loop the motions of object
	void Update() {

		// assign values for the mesh
		if (setmesh == true) {
			// add a mesh filter, stuff
			gameObject.AddComponent<MeshFilter> ().mesh = stuff;
			// add values to stuff 
			stuff.vertices = pval;
			stuff.triangles = Triangles;
			stuff.uv = UV;
			// mesh has been set, setmesh is false
			setmesh = false;
		}

		// save previous angle value in this frame
		prevangle = transform.rotation.eulerAngles.z;

		// run function Push to move the game object across the screen
			Push ();

		// new timet value, used in variable meshmover 
		timet = deltat * variablek;

		// constant values for carangiform model variables
		float c1 = 0.25f;
		float c2 = -0.15f / 2;
		float k = 4f;
		// reset variablex each time new mesh points are being created in meshmover
		float variablex = 0;

		// frequency value, dependent on velocity2 (speed of gameobject) and length f (length of gameobject)
		variablef = ((velocity2) + 1.39f * Mathf.Pow (lengthf * 6, (2 / 3))) / (0.98f * lengthf* 6);

		// create new values for the mesh points being moved
		for (int d = vcount / 4 -5; d < vcount / 2; ++d) {
			variablex += (0.56f) / (vcount / 4);
			// carangiform model, read: Phamduy 2015 IEEE
			meshmover [d] = (c1 * (variablex) + c2 * Mathf.Pow ((variablex), 2)) * Mathf.Cos (k * (variablex) - (2 * Mathf.PI * variablef * timet));
		}
			
		// change values in Vertices array
		for (int s = 0, r = 0; s < vcount / 2 || r < vcount / 2; s = s + 2, ++r) {
			// meshmover values have y = 0 as start value
			// since mesh is set at .2 and -.2, adjust values to match that
			Vertices [s].y = 1f * meshmover [r] + 0.2f; 
			Vertices [s + 1].y =  1f * meshmover [r] - 0.2f;
		}


		if (transform.position.x > -0.723f && transform.position.y < 0.0866f) { // if the gameobject has not reached the endpoint...
			stuff.vertices = Vertices; // move the mesh vertices 
		} else { // if it has reached the endpoint...
			time += Time.deltaTime; // start counting time per frame
			if (time > 2) { // if time passes 2 seconds...
				a -= 0.005f; // begin decreased alpha variable by 0.005 each loop
				mat.color = new Color (1f, 1f, 1f, a); // start decreasing the alpha value of the texture (opacity)
			}
		}

	}
		
	void Push(){
				
		// increase variablek by one each frame for variable timet
		variablek += 1;

		// continue rotating game object on z axis until it reaches desired value
		if (transform.eulerAngles.z < 193) {
			// determine interval between startangle and endangle manually and add onto current value
			thisangle = ((endangle - startangle) / 100) + prevangle;
		}

		// rotate game object toward the new position (endpoint) with variable rotation speed
		transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0,0,thisangle), rotspeed);

		// new x and y positions changed by manually determine intervals
		float nx = transform.position.x - ((transform.position.x + 1.14f) / 61);
		float ny = transform.position.y - ((transform.position.y - 0.09f) / 61);

		// set a new vector for this new position
		Vector3 logopos = new Vector3 (nx, ny, 0); 

		// move game object to endpoint position vector until it reaches endpoint
		if (transform.position.x > -0.723f && transform.position.y < 0.0866f) {
			transform.position = Vector3.MoveTowards (transform.position, logopos, velocity);
		}

	}
}

