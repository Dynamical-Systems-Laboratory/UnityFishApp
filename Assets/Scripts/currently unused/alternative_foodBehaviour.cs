using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class alternative_foodBehaviour : MonoBehaviour
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
/*
    public static float getUR_x(x,xa,y,ya,Q)
    {
        float ur_x;
        ur_x = -Q/(2*Mathf.PI)* Mathf.Pow( (Mathf.Pow(x-xa,2f) - Mathf.Pow(y-ya, 2f)) / (Mathf.Pow(x-xa,2f) + Mathf.Pow(y-ya, 2f)),2f );
        return ur_x;
    }

    public static float getUR_y(x,xa,y,ya,Q)
    {
        float ur_x;
        ur_x = -Q/(Mathf.PI)* Mathf.Pow( ((x-xa) - Mathf.Pow(y-ya, 2f)) / (Mathf.Pow(x-xa,2f) + Mathf.Pow(y-ya, 2f)),2f );
        return ur_x;       
    }
*/
    Vector2 playerPos;
    Vector2 playerPosini;



    //variables
    public float xo;
    public float yo;
    public float x_limit;

    //Constants
    //These will all get set by Manager when it gets initialized.
    public float a; //constant of the flow parabola
    public float b; //maximum flow (at the center of the parabola), positive for water flow to the right, negative for water flow to the left
    public float y_boundary; //river bank  range is between y = -y_boundary and y = y_boundary 
    public float flowVariable;

    //System Setup
    // dt * target = 1 is ideal
    private float dt = 0.033f; //this is assuming 30 frames per sec
    public int target = 30; //desired framerate


    //rock stuff
    public float Q;

    void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = target;
    }


    // Start is called before the first frame update
    void Start()
    {
        //rb = GetComponent<Rigidbody2D>();
        //rb.AddForce(new Vector2(-100.0f, 0f));
        Q = 22f * 2.2f;
    }

    // Update is called once per frame
    void Update()
    {
        x_limit = Camera.main.transform.position.x - (Camera.main.orthographicSize * Screen.width / Screen.height);
        if (transform.position.x < x_limit - 5f)
        {
            Destroy(this.gameObject);
        }

        //get position at curr frame
        xo = rb.position.x;
        yo = rb.position.y;

        //float x1 = xo + (getU(yo,a,b,flowVariable) + )
        
        
        //vertical boundary, fish 'bounces off' when they go into the riverbanks
        if (yo >= y_boundary - 2)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
        }
        else if (yo <= -y_boundary + 2)
        {
            rb.velocity = new Vector2(rb.velocity.x,0);
        } 

        //translating and rotating the fish according to curr frame
        //rb.MovePosition (playerPos);
        rb.AddForce(new Vector2(-1.0f, 0f));
        if (rb.velocity.x < getU(yo, a, b, flowVariable))
        {
            rb.AddForce(new Vector2(2.0f, 0f));
        }




    }

    
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Hit Detected by pellet.");
        


    }
}
