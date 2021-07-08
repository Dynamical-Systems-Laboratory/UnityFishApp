using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class foodBehaviour : MonoBehaviour
{
    private Rigidbody2D rb;

    //Functions to get U (flow profile) which varies depending on the fish's y position
    public static float getU(float yk, float a, float b, float flowVariable)
    {
        float u;
        u = -flowVariable * (a * Mathf.Pow(yk, 2) + b);
        return u;
    }

    //derivative of flow profile
    public static float getdU(float yk, float a, float flowVariable)
    {
        float du;
        du = -flowVariable * 2 * a * yk;
        return du;
    }

    Vector2 playerPos;
    Vector2 playerPosini;



    //variables
    public float xo;
    public float yo;
    public float x_limit;
    private float UR_x;
    private float UR_y;
    private float x1;
    private float y1;

    //Constants
    //These will all get set by Manager when it gets initialized.
    public float a; //constant of the flow parabola
    public float b; //maximum flow (at the center of the parabola), positive for water flow to the right, negative for water flow to the left
    public float y_boundary; //river bank  range is between y = -y_boundary and y = y_boundary 
    public float flowVariable;

    private float rockRadius = 2.2f;
    public float xa = -5f;
    public float ya = 0f;
    private float t2;
    private float t3;
    private float tt3;


    //debug
    public float velocityx;
    public float velocityy;

    //System Setup
    // dt * target = 1 is ideal
    //private float dt = 0.033f; //this is assuming 30 frames per sec
    public int target = 30; //desired framerate

    void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = target;
    }


    // Start is called before the first frame update
    void Start()
    {
        /*
        rb = GetComponent<Rigidbody2D>();
        rb.AddForce(new Vector2(-100.0f, 0f));
        */
    }

    // Update is called once per frame
    void Update()
    {


        if (GameTime.isPaused){
            rb.velocity = new Vector2(0,0);
            return;
        }
        x_limit = Camera.main.transform.position.x - (Camera.main.orthographicSize * Screen.width / Screen.height);
        if (transform.position.x < x_limit - 5f)
        {
            Destroy(this.gameObject);
        }

        //get position at curr frame
        xo = transform.position.x;
        yo = transform.position.y;

//Testing for alternative rock implementation

        //xa, ya are positions of the rock, r is radius of the rock
        UR_x =  -1*(Mathf.Pow(rockRadius,2))*(Mathf.Pow((xo-xa),2)-Mathf.Pow((yo-ya),2)) / Mathf.Pow((Mathf.Pow((xo-xa),2)+Mathf.Pow((yo-ya),2)),2);
        UR_y =  2*(Mathf.Pow(rockRadius,2))*((xo-xa)*(yo-ya)) / Mathf.Pow((Mathf.Pow((xo-xa),2)+Mathf.Pow((yo-ya),2)),2);

        t2 = getU(yo,Manager.instance.a, Manager.instance.b, 1.5f*Manager.instance.flow_variable);
        t3 = t2*UR_x;
        tt3 = t2*UR_y;

        x1 = xo + (t2 + t3)* GameTime.deltaTime; 
        y1 = yo - tt3*GameTime.deltaTime; 

        
        transform.position = new Vector2(x1,y1);
        /*
        //vertical boundary, fish flows horizontally when they go into the riverbanks
        if (yo >= y_boundary - 2)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
        }
        else if (yo <= -y_boundary + 2)
        {
            rb.velocity = new Vector2(rb.velocity.x,0);
        } 


        rb.AddForce(new Vector2(-1.0f, 0f));
        if (rb.velocity.x < getU(yo, a, b, flowVariable))
        {
            rb.AddForce(new Vector2(1f, 0f));
        }
        
        velocityx = rb.velocity.x;
        velocityy = rb.velocity.y;
        if (rb.velocity.x == 0 && rb.velocity.y == 0){
            rb.AddForce(new Vector2(1.0f,1.0f));
        }

        */


    }

    
    private void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.tag == "ObstacleProximity"){ //if item is within proximity with a rock, record down the location of that rock
            xa = other.transform.position.x;
            ya = other.transform.position.y;
        }

        Debug.Log("Hit Detected by pellet.");
        


    }
}
