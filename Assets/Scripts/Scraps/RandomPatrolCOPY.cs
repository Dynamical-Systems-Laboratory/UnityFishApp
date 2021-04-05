//THIS VERSION OF RANDOM PATROL IS NOT USED
//IT IS VERY MESSY AND ITS PURPOSE IS TO MAINTAIN ALL THE EDITED OUT PIECES 
//IN CASE WE NEED TO CHANGE BEHAVIOUR BACK TO A PREVIOUS MODEL SO THAT I CAN HAVE A REFERENCE 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class RandomPatrolCOPY : MonoBehaviour
{
    /*
   //Gaussian Noise Functions
    public static float NextGaussian() {
        float v1, v2, s;
        do {
            v1 = 2.0f * Random.Range(0f,1f) - 1.0f;
            v2 = 2.0f * Random.Range(0f,1f) - 1.0f;
            s = v1 * v1 + v2 * v2;
        } while (s >= 1.0f || s == 0f);
 
        s = Mathf.Sqrt((-2.0f * Mathf.Log(s)) / s);
 
        return v1 * s;
    }

    public static float NextGaussian2(float mean, float standard_deviation)
    {
        return mean + NextGaussian() * standard_deviation;
    }

    public static float NextGaussian3 (float meanb, float standard_deviationb, float min, float max) {
	    float x;
	    do {
		    x = NextGaussian2(meanb, standard_deviationb);
	    } while (x < min || x > max);
	    return x;
    }
    */


   //Functions to get U (flow profile) which varies depending on the fish's y position
    public static float getU (float yk, float a, float b, float flowVariable) {
        float u;
        u = flowVariable * (a * Mathf.Pow(yk, 2) + b); 
        return u;
    }
    
    //derivative of flow profile
    public static float getdU (float yk, float a, float flowVariable) {
        float du;
        du = flowVariable * 2 * a * yk;
        return du;
    }

    Vector2 playerPos;
    Vector2 playerPosini;


   //Camera position
    public Transform cameraTrans;

   //food bar variables
    public foodBar foodBar;
    public float maxFood = 100f; //max amount of food, constant, loads on start
    public float foodDecreaseSpeed = 0.05f; //how fast food is dropping
    public float food; //current amount of food, variable

   //variables
    public float xo;
    public float yo;
    public float uh;
    public float zk;
    public float theta0 = 0f; //heading direction
    public float wk0 = 0.0f; //turn rate 
    public float uc = 0.0f; //user controlled for turning, between 40 and -40 USER CONTROLLED THROUGH HORIZONTAL AXIS
    public float beta = 0.5f; //average fish speed, between 0 and 32 (at 32 the fish can go against strongest current) USER CONTROLLED THROUGH VERTICAL AXIS
    public float gammaLK = 1f; //left circulation of the fish
    public float gammaRK = 1f; //right circulation of the fish
    public float flowVariable = 1;
   
   //Constants
    //When you change y_boundary, you also need to change a and b to be appropriate values to have a nice curve for U, ask Daniel how to do that!
    public float a = -0.2f; //constant of the flow parabola
    public float b = -2f; //maximum flow (at the center of the parabola), positive for water flow to the right, negative for water flow to the left
    public float y_boundary = 10f; //river bank  range is between y = -y_boundary and y = y_boundary 

    public float alpha = 0.2f; //relaxation parameter: the rate at which the fish comes back to original position (relaxes), also affects how much the fish randomly turns
    public float sigma = 1.4f; //strength of variation of turning, affects how much the fish randomly turns
    public float kappa = 4f; //gain between vortex's strength
    
    public float kr = 1f; //hydrodynamic feedback gain AKA how fast the fish turns against the current
    public float r = 0.5f; //radius of hydrodynamic sensing 
    
    public float l = 0.5f; //thickness of the zebrafish in cm
    public float vk = 1.0f; //speed of the fish, constant
    ////public float user_control_turn_speed = 0.3f; //when key is pressed down, how fast uc will go up
    public float user_control_acceleration = 0.2f; //when key is pressed down, how fast

    public float controlSens = 0.35f; //sensitivity, how reactive the fish is to uc

    //facilitary variables
    private float xk_1;
    public float yk_1;
    private float thetak_1;
    private float wk_1;
    public float gammaLK1;
    public float gammaRK1;


    ////private float randomAngle; 
   
   //System Setup
   // dt * target = 1 is ideal
    private float dt = 0.033f; //this is assuming 30 frames per sec
    public int target = 30; //desired framerate
     
     void Awake()
     {
         QualitySettings.vSyncCount = 0;
         Application.targetFrameRate = target;
     }


   // Start is called before the first frame update
    void Start()
    {
       playerPos = transform.position;

      //Foodbar setup  
       foodBar.SetMaxFood(maxFood);
       food = maxFood;
       if (STATIC.pollLevel == "h")
        {
            controlSens = 0.1f;
        } 
        else
        {
            controlSens = 0.35f;
        }
    }

   // Update is called once per frame
    void Update()
    {   
        /*
        //usercontrol for fish's turning
        if ( (uc < 40 && (Input.GetAxis("Horizontal") < 0f)) || (uc > -40 && (Input.GetAxis("Horizontal") > 0f)) ) 
        {
            uc = uc - user_control_turn_speed * (Input.GetAxis("Horizontal")); //the axis 'Horizontal' gives -1 when'a', and 1 when 'd' 
        }*/

        
       //alternative user control for fish's turning
        if (Input.GetAxis("Horizontal") < -0.1f) //turning left
        {
            uc = Input.GetAxis("Horizontal") * -20f;
        }
        else if (Input.GetAxis("Horizontal") > 0.1f) //turning right
        {
            uc = Input.GetAxis("Horizontal") * -20f;
        } 
        else //user not influencing turning
        {
            uc = 0f;
        }

       //usercontrol for fish's speed
        if ((beta < 32f && (Input.GetAxis("Vertical") > 0f)) || (beta > 0f && (Input.GetAxis("Vertical") < 0f)))
        {
            beta = beta + user_control_acceleration * (Input.GetAxis("Vertical")); //the axis 'Vertical' gives -1 when 's', and 1 when 'w'
        }
        if (beta < 0.5f)
        {
            beta = 0.5f;
        }

   
       //get position at curr frame
        xo = playerPos.x;
        yo = playerPos.y;
        playerPosini = playerPos;

        /*
        //get vk for current frame from gammaLK and gammaRK
        vk = (gammaLK + gammaRK) / (4 * Mathf.PI * l);
        */
        
        /*
       //update the gammaLK and gammaRK for next frame
        uh =  -((kr) * Mathf.Pow(Mathf.PI, 2) * Mathf.Pow(r, 2) * getdU(yo, a, flowVariable)); //hydrodynamic feedback
        zk = kappa * (gammaRK - gammaLK) + uh + uc; //uc is user control with uh 
        ////zk = kappa * (gammaRK - gammaLK) + uc; //uc is only user control
        gammaLK1 = gammaLK + (alpha * (beta - gammaLK) + zk) * dt + sigma * Mathf.Sqrt(gammaLK) * Mathf.Sqrt(dt) * NextGaussian3(0.0f, 1.0f, -20.0f, 20.0f); ;
        gammaRK1 = gammaRK + (alpha * (beta - gammaRK) - zk) * dt + sigma * Mathf.Sqrt(gammaRK) * Mathf.Sqrt(dt) * NextGaussian3(0.0f, 1.0f, -20.0f, 20.0f); ;
        */
        
       //Updating the next frame player position
        ////xk_1 = xo + V * Mathf.Cos(theta0) * dt + getU(yo, a, b) * dt;
        ////yk_1 = yo + V * Mathf.Sin (theta0) * dt;
        xk_1 = xo + vk * Mathf.Cos(theta0) * dt + getU(yo, a, b, flowVariable) * dt;
        yk_1 = yo + vk * Mathf.Sin(theta0) * dt;

       //Updating the next frame player angle
        ////wk_1 = wk0 - alpha * wk0 * dt + wc*dt + sigma * Mathf.Sqrt(dt) * randomAngle;
        thetak_1 = theta0 + (-1f * getdU(yo, a, flowVariable) * Mathf.Pow(Mathf.Cos(theta0), 2)) * dt + ((uc*controlSens)/(2*Mathf.PI*Mathf.Pow(l,2))) * dt;
        ////thetak_1 = theta0 + 2*Mathf.Sqrt(dt)*randomAngle + (-1f* getdU(yo,a)*Mathf.Pow(Mathf.Cos(theta0),2)) * dt;
        ////Wk_1 = Wk - alpha * Wk * dt + sigma * sqrt(dt);

        //fish translation for next frame
        playerPos = new Vector2(xk_1, yk_1);


        //horizontal boundary, fish cannot go past this border
        if (playerPos.x >= 500f)
        {
            playerPos.x = 500f;
        } else if (playerPos.x <=-14f)
        {
            playerPos.x = -13.9f;
        }

        
        //horizontal bound with camera, fish can never go back left past the camera. 
        if (cameraTrans.position.x - playerPos.x > (Camera.main.orthographicSize * Screen.width / Screen.height)-1f)
        {
            playerPos.x = cameraTrans.position.x - (Camera.main.orthographicSize * Screen.width / Screen.height) + 1f;
        }


        //vertical boundary, fish 'bounces off' when they go into the riverbanks
        if (playerPos.y >= y_boundary)
        {
            //playerPos.y = -10.4f;
            if (Mathf.Cos(theta0)>0)
            {
                theta0 = -0.785398163f;
                thetak_1 = -0.785398163f;
            } else
            {
                theta0 = -2.35619449f;
                thetak_1 = -2.35619449f;
            }
        }
        else if (playerPos.y <= -y_boundary)
        {
            if (Mathf.Cos(theta0) > 0)
            {
                theta0 = 0.785398163f;
                thetak_1 = 0.785398163f;
            }
            else
            {
                theta0 = 2.35619449f;
                thetak_1 = 2.35619449f;
            }
            //playerPos.y = 10.4f;
        } 
        
        //translating and rotating the fish according to curr frame
        transform.position = playerPos;
        transform.eulerAngles = new Vector3(0f, 0f, theta0 * Mathf.Rad2Deg);

        //updating for the next frame;
        wk0 = wk_1;
        theta0 = thetak_1;
        //gammaLK = Mathf.Abs(gammaLK1);
        //gammaRK = Mathf.Abs(gammaRK1);



        //food testing
        foodDecreaseSpeed = beta * (1f / (4f * 32f)); 
        food -= foodDecreaseSpeed;
        foodBar.SetFood(food);

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        
        Debug.Log("Hit Detected With: ");

        food -= 10f;
        foodBar.SetFood(food);
        /*
        Vector2 normal = Vector2.Perpendicular(other.ClosestPoint(transform.position));
        Vector2 directionVector = new Vector2(Mathf.Cos(theta0), Mathf.Sin(theta0));
        Vector2 resultVector = new Vector2();

        resultVector = Vector2.Reflect(directionVector, normal);
        if (resultVector.x < 0)
        {
            theta0 = 360f - (Mathf.Atan2(resultVector.x, resultVector.y) * -1f);
        } else
        {
            theta0 = Mathf.Atan2(resultVector.x, resultVector.y);
        }
        */

    }
}
