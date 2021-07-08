using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class RandomPatrol : MonoBehaviour
{

   //Functions to get U (flow profile) which varies depending on the fish's y position
    public static float getU (float yk, float a, float b, float flowVariable) {
        float u;
        u = -flowVariable * (a * Mathf.Pow(yk, 2) + b); 
        return u;
    }
    
    //derivative of flow profile
    public static float getdU (float yk, float a, float flowVariable) {
        float du;
        du = -flowVariable * 2 * a * yk;
        return du;
    }

    public static RandomPatrol instance;

    Vector2 playerPos;

    //object spawner
    public rockSpawner spawner;

    //variable manager
    public Manager manager;

    //Predator
    public predatorBehaviour predator;

   //Camera position
    public Transform cameraTrans;

    //Lay Egg Button
    public Button layEggButton;


    //UI gameobjects
    public GameObject ingameO;
    public GameObject endgameO;
    //animators
    public Animator anim; //animator for the fish, set at start
    public Animator eggAnimator; //animator for the eggspot that the fish is currently touching, set whenever the fish touches a new spot

   //food bar variables
    public foodBar foodBar;
    public float maxFood = 100f; //max amount of food, constant, loads on start
   
    public float foodPenaltyAmount = 15f; //amount of food reduced everytime we hit a predator
    public float foodRewardAmount = 7f; //amount of food rewarded when you eat food
    public float eggRewardAmount = 10f; //amount of food rewarded when you lay eggs
    public float food; //current amount of food, variable

   //variables
    public float xo;
    public float yo;
    public float theta0 = 0f; //heading direction
    public float uc = 0.0f; //user controlled for turning, between 20 and -20 USER CONTROLLED THROUGH HORIZONTAL AXIS
    public float vk; //velocity of the fish, slower in the middle, faster by the sides.
    public float u_flow;

   //Constants
    
    
    //a = -b / yMax
    //a = b / ymax^2

    //uniformly set with manager at start()
    private float a; //constant of the flow parabola
    private float b; //maximum flow (at the center of the parabola), positive for water flow to the right, negative for water flow to the left
    private float y_boundary; //river bank  range is between y = -y_boundary and y = y_boundary 
    

    private float l = 0.5f; //thickness of the zebrafish in cm
    //private float v_max = 5f; //max speed of the fish

    //These four are set from manager at start()
    public float flowVariable; 
    public float foodDecreaseSpeed; //how fast food is dropping
    public float controlSens; //sensitivity, how reactive the fish is to uc
    public float speedSens; //speed sensitivity
    public float eggSuccessRate; //enter as a percentage, e.g. 30 would mean 30%



    //facilitary variables
    private float xk_1;
    private float yk_1;
    private float thetak_1;
    private float u_k1;
    private float u_k = 0.0f;
    private float a_u = 0.5f;
    private float u_v;
    private float dUfl;

   

    //tail frequency    
    private float tf = 1.0f; //on default, the tail goes one cycle for second
    
    //rock avoidance
    private bool avoidFlagStrong = false;
    private bool avoidFlag = false;
    private Vector2 currObstacleVector;

    //toggles
    public int layingEgg = 0;
    private bool canLayEgg = false;
    
    //FOR MOBILE, NOT IN USE FOR PC VERSION
    private bool turnLeft = false; 
    private bool turnRight = false; 
    private bool swimToggle = false; 

    //FOR TUTORIAL PURPOSE
   public bool isTutorial;
   //System Setup
   // GameTime.deltaTime * target = 1 is ideal
    //private float dt = 0.033f; //this is assuming 30 frames per sec
    public int target = 30; //desired framerate



     
     void Awake()
     {
         instance = this; 
         QualitySettings.vSyncCount = 0;
         Application.targetFrameRate = target;

        foodPenaltyAmount = 15f; //amount of food reduced everytime we hit a predator
        foodRewardAmount = 6f; //amount of food rewarded when you eat food
        eggRewardAmount = 13f; //amount of food rewarded when you lay eggs
     }


   // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        playerPos = transform.position;

        //Foodbar setup  
        foodBar.SetMaxFood(maxFood);
        food = maxFood;

        
        //Manager setup
        a = manager.a;
        b = manager.b;
        y_boundary = manager.y_boundary; 

        controlSens = manager.rotate_sensitivity;
        speedSens = manager.speed_sensitivity;
        foodDecreaseSpeed = manager.food_decreaseSpeed;
        flowVariable = manager.flow_variable;
        eggSuccessRate = manager.egg_successChance;

        timerController.instance.BeginTimer(); 


        
    }

   // Update is called once per frame
    void Update()
    {
        if (GameTime.isPaused){return;}

        //keep theta0 between PI and -PI
        if (theta0 > Mathf.PI){
            theta0 = -(Mathf.PI*2 - theta0); 
        } else if (theta0 < -Mathf.PI){
            theta0 = (Mathf.PI*2 + theta0);
        }

        if (food < 8)
        {
            layingEgg = 1; //if no more food, pause fish motion
            timerController.instance.EndTimer();
            ingameO.SetActive(false);
            endgameO.SetActive(true);
        }
        
// user control for fish's turning MOBILE
       /*
       if (turnLeft){
           uc = 10;
       } else if (turnRight) {
           uc = -10;
       } else {
           uc = 0;
       }

       
        if (swimToggle && vk <= v_max) // Increase speed
        {
          u_v = speedSens;
        }
         else
        {
          u_v = 0.0f;
        } 
        */

// user control for fish's turning COMPUTER
        if (isTutorial && !TutorialScript.instance.canTurn){
            //if tutorial is on and fish cannot turn yet
        }
        else if (Input.GetAxis("Horizontal") < -0.1f) //turning left
        {
            uc = Input.GetAxis("Horizontal") * -20f;
        }
        else if (Input.GetAxis("Horizontal") > 0.1f) //turning right
        {
            uc = Input.GetAxis("Horizontal") * -20f;
        } 


        if (isTutorial && !TutorialScript.instance.canSwim){
            //if tutorial is on and fish cannot swim yet
        } 
        else if ( ( Input.GetKey("space") || Input.GetKey("up") ) && vk <= speedSens) // Increase speed
        {
          u_v = speedSens;
        }
         else
        {
          u_v = 0.0f;
        } 

        if (canLayEgg && Input.GetKey(KeyCode.Return)){
            LayEggs();
        }
        

        //fish speed calculation according to flow position
        //first order linear filter, a_u is damping constant
        u_k1 = u_k + (-a_u*u_k + u_v)*GameTime.deltaTime;
        vk = u_k;
        u_k = u_k1;
/*
        //fish turn calculation
        //also first order linear filter
        u_q1 = u_q + (-controlSens*u_q + uc/2)*GameTime.deltaTime;
        rk = u_q;
        u_q = u_q1;
*/

//fish undulation stuff
        //2 is half of fish body length as only half undulates
        
        tf = ( (vk/4f) + 1.39f * Mathf.Pow(2f,(2/3)) ) / (0.98f*(2f));
        anim.SetFloat("speedVar",tf);


       //get position at curr frame
        xo = transform.position.x;
        yo = transform.position.y;
        
       //Updating the next frame player position

        if (avoidFlagStrong){
            Vector2 backoffVector = new Vector2 (transform.position.x - currObstacleVector.x, transform.position.y - currObstacleVector.y);
            backoffVector.Normalize();
            Vector2 Nudge = backoffVector * 2f;
            //Debug.Log("Strong Nudging");
            yk_1 = yo + Nudge.y * GameTime.deltaTime;
            xk_1 = xo + Nudge.x * GameTime.deltaTime;
            thetak_1 = theta0;
            //thetak_1 = theta0 + (-1f * getdU(yo, a, flowVariable) * Mathf.Pow(Mathf.Cos(theta0), 2)) * GameTime.deltaTime + ((uc*controlSens)/(2*Mathf.PI*Mathf.Pow(l,2))) * GameTime.deltaTime;
        }
        else if (avoidFlag){
            Vector2 backoffVector = new Vector2 (transform.position.x - currObstacleVector.x, transform.position.y - currObstacleVector.y);
            backoffVector.Normalize();
            Vector2 lightNudge = backoffVector * 0.2f;
            //Debug.Log("Nudging");
            yk_1 = yo + lightNudge.y * GameTime.deltaTime;
            xk_1 = xo + lightNudge.x * GameTime.deltaTime;
            thetak_1 = theta0 + (-1f * getdU(yo, a, flowVariable) * Mathf.Pow(Mathf.Cos(theta0), 2)) * GameTime.deltaTime + ((uc*controlSens)/(2*Mathf.PI*Mathf.Pow(l,2))) * GameTime.deltaTime;
            //yk_1 = yo + (-vk)*0.1f * Mathf.Sin(theta0) * GameTime.deltaTime;
            //xk_1 = xo + (-vk)*0.1f * Mathf.Cos(theta0)  * GameTime.deltaTime;
        } else {
            yk_1 = yo + vk * Mathf.Sin(theta0) * GameTime.deltaTime;
            xk_1 = xo + vk * Mathf.Cos(theta0) * GameTime.deltaTime + getU(yo, a, b, flowVariable) * GameTime.deltaTime;
            thetak_1 = theta0 + (-1f * getdU(yo, a, flowVariable) * Mathf.Pow(Mathf.Cos(theta0), 2)) * GameTime.deltaTime + ((uc*controlSens)/(2*Mathf.PI*Mathf.Pow(l,2))) * GameTime.deltaTime;
        }

        u_flow = getU(yo, a, b, flowVariable);

        /*
       //Updating the next frame player angle
       if (avoidFlag){


        Vector2 horizontalVect = new Vector2 (0,0);
        //this is the vector from the fish center to the fish head
        Vector2 headVect = new Vector2 (transform.GetChild(0).position.x - transform.position.x, transform.GetChild(0).position.y - transform.position.y);
        float headAngle = Vector2.Angle (horizontalVect, headVect);
        //this is the vector from the fish center to the rock center
        Vector2 rockVect = new Vector2 (currObstacleVector.x - transform.position.x, currObstacleVector.y - transform.position.y);
        float rockAngle = Vector2.Angle (horizontalVect, rockVect);

        if (headAngle >= rockAngle){ //if head angle is bigger (this is unsigned, look up unity Vecto2.Angle for more info), then headAngle need to go towards pi
            if (headVect.y <= 0 ){ //fish cannot turn left 
                if (uc > 0) {
                    thetak_1 = theta0;
                } else {
                    thetak_1 = theta0 + (-1f * getdU(yo, a, flowVariable) * Mathf.Pow(Mathf.Cos(theta0), 2)) * GameTime.deltaTime + ((uc*controlSens)/(2*Mathf.PI*Mathf.Pow(l,2))) * GameTime.deltaTime;
                }
            } else { //fish cannot turn right
                if (uc < 0) {
                    thetak_1 = theta0;
                } else {
                    thetak_1 = theta0 + (-1f * getdU(yo, a, flowVariable) * Mathf.Pow(Mathf.Cos(theta0), 2)) * GameTime.deltaTime + ((uc*controlSens)/(2*Mathf.PI*Mathf.Pow(l,2))) * GameTime.deltaTime;
                }
            }

        } else { //if head angle is smaller, then headAngle need to go towards zero
            if (headVect.y <= 0 ){ //fish cannot turn right 
                if (uc < 0) {
                    thetak_1 = theta0;
                } else {
                    thetak_1 = theta0 + (-1f * getdU(yo, a, flowVariable) * Mathf.Pow(Mathf.Cos(theta0), 2)) * GameTime.deltaTime + ((uc*controlSens)/(2*Mathf.PI*Mathf.Pow(l,2))) * GameTime.deltaTime;
                }
            } else { //fish cannot turn left
                if (uc > 0) {
                    thetak_1 = theta0;
                } else {
                    thetak_1 = theta0 + (-1f * getdU(yo, a, flowVariable) * Mathf.Pow(Mathf.Cos(theta0), 2)) * GameTime.deltaTime + ((uc*controlSens)/(2*Mathf.PI*Mathf.Pow(l,2))) * GameTime.deltaTime;
                }
            }           
        }
       } else {
           thetak_1 = theta0 + (-1f * getdU(yo, a, flowVariable) * Mathf.Pow(Mathf.Cos(theta0), 2)) * GameTime.deltaTime + ((uc*controlSens)/(2*Mathf.PI*Mathf.Pow(l,2))) * GameTime.deltaTime; 
       }
        
        Debug.Log(transform.GetChild(0).position);
        */




        //Updating fish's velocity
        //vk = (v_max / y_boundary) * Mathf.Abs(yo) + v_max;



        
        //horizontal bound with camera, fish can never go back left past the camera. 
        if (cameraTrans.position.x - xk_1 > (Camera.main.orthographicSize * Screen.width / Screen.height)-1f)
        {
            xk_1 = cameraTrans.position.x - (Camera.main.orthographicSize * Screen.width / Screen.height) + 1f;
        }



        //apply horizontal bound with camera on left size as well IF we are in tutorial mode
        if (isTutorial){
            if (xk_1 - cameraTrans.position.x > (Camera.main.orthographicSize * Screen.width / Screen.height)-1f){
                xk_1 = cameraTrans.position.x + (Camera.main.orthographicSize * Screen.width / Screen.height) - 1f;
            }
        } else { //THIS IS VERY IMPORTANT IF NOT TUTORIAL, LIMITS CAMERA SO THE FISH IS ALWAYS AT MOST 8 UNITS LEFT OF CENTER OF CAMERA. 
            if (xk_1 - cameraTrans.position.x > -8f)
            {
                cameraTrans.position = new Vector3(xk_1 + 8f, 0, -10);
            }
        }
        

        



        //vertical boundary, fish have a hard barrier when they go into the riverbanks
        if (yk_1 >= y_boundary && theta0 > 0)
        {
            yk_1 = y_boundary;
            /*
            if (Mathf.Cos(theta0)>0)
            {
                theta0 = -0.785398163f;
                thetak_1 = -0.785398163f;
            } else
            {
                theta0 = -2.35619449f;
                thetak_1 = -2.35619449f;
            }*/
        }
        else if (yk_1 <= -y_boundary && theta0 < 0)
        {
            yk_1 = -y_boundary;
            /*
            if (Mathf.Cos(theta0) > 0)
            {
                theta0 = 0.785398163f;
                thetak_1 = 0.785398163f;
            }
            else
            {
                theta0 = 2.35619449f;
                thetak_1 = 2.35619449f;
            }*/
        }

        //if the fish is laying eggs, it should not move, if it is not laying eggs, make it able to move.
        if (layingEgg == 0)//fish not laying eggs
        {
            //translating and rotating the fish according to curr frame
            playerPos = new Vector2(xk_1, yk_1);
            transform.position = playerPos;
            transform.eulerAngles = new Vector3(0f, 0f, thetak_1 * Mathf.Rad2Deg);

            //updating for the next frame;
            theta0 = thetak_1;
            
            //food only decreases when moving
            food -= foodDecreaseSpeed * (vk/2f + 3f) * GameTime.deltaTime;
            if (food > maxFood){
                food = maxFood;
            }
            foodBar.SetFood(food);
        }

        uc = 0; //reset user control in case no button is pressed for next frame




        //Debug.Log("Fish yo: " + yo + "Fish flowVariable: " + flowVariable + "Fish a: " + a + "Fish b: " + b);
        //Debug.Log("Fish flow: " + u_flow);

    }

    public void leftDown() {
        turnLeft = true;
    }
    public void leftUp() {
        turnLeft = false;
    }
    public void rightDown() {
        turnRight = true;
    }
    public void rightUp() {
        turnRight = false; 
    }

    public void swimDown() {
        swimToggle = true;
    }
    public void swimUp() {
        swimToggle = false;
    }

    //THIS FUNCTION SHOULD ONLY BE USED UNDER TUTORIAL MODE TO SET FLOW VARIABLE TO 1
    public void setFlowOne(){
        flowVariable = 1;
    }







//This function is called when the button is pressed.
    public void LayEggs()
    {
        //Debug.Log("Will begin laying eggs now");
        layingEgg = 1; //stop the motion of the fish
        eggAnimator.GetComponent<eggLaying>().laying();

        //Determine whether this egg laying will be success or failure and play the associated animation

        if (Random.Range(0,100) < eggSuccessRate)
        {
            eggAnimator.Play("layingEgg_Good"); 
        } else
        {
            eggAnimator.Play("layingEgg_Bad"); 
        }

        
    }






//ON TRIGGER HANDLING

    //The user controlled fish is a trigger for rigibody2d collisions
    private void OnTriggerEnter2D(Collider2D other)
    {
        
        //Debug.Log("Hit Detected With: " + other.tag);

        //this is for tutorial only
        if (isTutorial && (other.tag == "Area"))
        {
            TutorialScript.instance.nextStage();
        }
        //if hits a predator - Minus food, no changes to 
        if (other.tag == "Predator")
        {
            if (predator.dealtDmg == 0)
            {
                food -= foodPenaltyAmount;
                foodBar.SetFood(food);
                anim.Play("blink",1); //play the blink animation on layer 1, does not interfere with fish swimming layer
                predator.toggleDmg();
                if (isTutorial){
                    TutorialScript.instance.gotHit = true;
                }
            }
        }

        if (other.tag == "ObstacleInside")
        {
            avoidFlagStrong = true;
            
        }
        if (other.tag == "Obstacle")
        {
            avoidFlag = true;
            currObstacleVector = other.transform.position;
            /*
            Vector2 normal = Vector2.Perpendicular(other.ClosestPoint(transform.position));
            Vector2 directionVector = new Vector2(Mathf.Cos(theta0), Mathf.Sin(theta0));
            Vector2 resultVector = new Vector2();

            resultVector = Vector2.Reflect(directionVector, normal);
            if (resultVector.x < 0)
            {
                theta0 = 360f - (Mathf.Atan2(resultVector.x, resultVector.y) * -1f);
                predator.theta0 = theta0;
            } else
            {
                theta0 = Mathf.Atan2(resultVector.x, resultVector.y);
                predator.theta0 = theta0;
            }
            */
        }
        

        if (other.tag == "Food")
        {
            food += foodRewardAmount;
            foodBar.SetFood(food);
            Destroy(other.gameObject);
        }

        if (other.tag == "Egg Spot")
        {
            //Debug.Log("Hitting Egg Spot");
            eggAnimator = other.gameObject.GetComponent<Animator>();

            eggAnimator.Play("eggSpot_highlight");
            layEggButton.interactable = true;
            canLayEgg = true; //this boolean is only for the enter key animation alternative


        }

    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag == "Egg Spot")
        {
            other.gameObject.GetComponent<Animator>().Play("eggSpot_highlight");
            
        }

        if (other.tag == "Egg Spot_Full")
        {
            layEggButton.interactable = false;
            canLayEgg = false;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "ObstacleInside")
        {
            avoidFlagStrong = false;
        }
        if (other.tag == "Obstacle")
        {
            avoidFlag = false;
        }
        if (other.tag == "Egg Spot")
        {
            layEggButton.interactable = false;
            canLayEgg = false;
            other.gameObject.GetComponent<Animator>().Play("Empty");
        }
    }



}
