using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class predatorBehaviour : MonoBehaviour
{
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

    public static predatorBehaviour instance;

    Vector2 predatorPos;
    Vector2 predatorPosini;

    public Transform targetFish;
    public RandomPatrol targetFishRP;
    public Manager manager;

    //variables
    public float xo;
    public float yo;
    public float theta0 = 0f; //heading direction
    
    public int attackToggle = 0;
    public float attackXRef;
    public float attackYRef;
    public float attackThetaRef;

    //Constants
    
    //uniformly set at manager
    private float a = -0.01389f; //constant of the flow parabola
    private float b = 2f; //maximum flow (at the center of the parabola), positive for water flow to the right, negative for water flow to the left
    private float y_boundary = 12f; //river bank  range is between y = -y_boundary and y = y_boundary 

    public float l = 0.5f; //thickness of the zebrafish in cm NOT IN USE
    public float vk; //speed of the predator, default is declared in vDefault below. 
    private const float vDefault = 6.0f;

    public float tau = 1f; //NOT IN USE
    public float kp = 3f; //coupling strength for diffusive coupling
    public float fishOffset = 9f; //offset between the fishes
    public float fishOffsetDefault = 9f; //how much distance by default
    
    public float speedOfAtk = 5.0f;
    public float speedOfBck = 2.0f;

    //facilitary variables
    private float xk_1;
    public float yk_1;
    public float fishOffset_k1;
    private float thetak_1;
    private float wk_1;
    public float gammaLK1;
    public float gammaRK1;

    //atk sequencing

    private float relaxation_constant = 1.0f; //determines how fast the predator comes back after attacking
    public float convergence_rate = 1.0f; //determines how fast the predator attacks 
    private float randomNoise; 

    private float TargetAngle;
    private float u_atk = 1.0f;
    public float Co_In;

    public float Flag_attack = 0.0f;
    private float Dis;
    public int dealtDmg = 0;

    //retreat flag is when the attack sequence lasts for more than x seconds 
    private IEnumerator coroutine;
    public bool retreat_flag = false;

    //These two are set from manager at the start()
    public float flowVariable;
    public float p_atk;

    //angle of approach
    private float desire_angle;


    //tail frequency
    private float tf = 1.0f;
    ////private float randomAngle; 

    //System Setup
    // dt * target = 1 is ideal
    //private float GameTime.deltaTime = 0.033f; //this is assuming 30 frames per sec
    public int target = 30; //desired framerate

    //obstacle avoidance toggles
    private bool avoidFlagStrong = false;
    private bool avoidFlag = false; 
    private Vector2 currObstacleVector;
    private bool turnDown;
    private bool turnUp;

    //OBSTACLE AVOIDANCE THROUGH MODELING
    private float rockRadius = 2.6f;
    public float xa = -100f;
    public float ya = 0f;
    private float t2;
    private float t3;
    private float tt3;
    private float UR_x;
    private float UR_y;
    private bool kp_vanish = false;
    
    private IEnumerator avoidRockDown(float secs)
    {
        turnDown = true;
        yield return new WaitForSeconds(secs);
        fishOffset = targetFish.position.x - xo;
        turnDown = false;
        avoidFlag = false;
    }
    private IEnumerator avoidRockUp(float secs){
        turnUp = true;
        yield return new WaitForSeconds(secs);
        fishOffset = targetFish.position.x - xo;
        turnUp = false;
        avoidFlag = false;
    }

    private IEnumerator resetBlink(float secs){
        yield return new WaitForSeconds(secs);
        targetFish.GetComponent<Animator>().Play("New State", 1);//stop the fish from blinkin
    }
    private IEnumerator atk_timer(float secs){
        yield return new WaitForSeconds(secs);
        retreat_flag = true;
        Debug.Log("PREDATOR SCRIPT: countdown ended, now retreating");
    }


    void Awake()
    {
        instance = this; 
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = target;

        u_atk = 1f;
    }


    // Start is called before the first frame update
    void Start()
    {
        randomNoise = 0f;
        predatorPos = transform.position;
        attackXRef = targetFish.position.x; //lock in the target fish's x position for attack
        attackYRef = targetFish.position.y;
        attackThetaRef = targetFishRP.theta0;
        /*
        //Foodbar setup  
        foodBar.SetMaxFood(maxFood);
        food = maxFood;
        */

        //Set up variables from manager
        flowVariable = manager.flow_variable;
        p_atk = manager.predator_avoidance; 

        a = manager.a;
        b = manager.b;
        y_boundary = manager.y_boundary;

        vk = vDefault;

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
        

        //get position at curr frame
        xo = predatorPos.x;
        yo = predatorPos.y;

        //angle adjustment prep
        Vector2 pred_2_fish_vect = new Vector2 (targetFish.position.x - xo, targetFish.position.y - yo);
        Vector2 horizontal_vect = new Vector2 (1,0); //unit vector on the horizontal
        float desired_angle = Mathf.Deg2Rad * Vector2.Angle(pred_2_fish_vect,horizontal_vect); //Vector2.Angle gives the smallest angle in degrees between two vectors.
        if (pred_2_fish_vect.y < 0 ){
            desired_angle = -desired_angle;
        }

        //difussive coupling, where kp is the coupling strength. 
        Dis = Mathf.Sqrt( Mathf.Pow(targetFish.position.x - xo,2.0f) + Mathf.Pow(targetFish.position.y - yo,2.0f) );
       
        Co_In = Random.Range(0,1000)/1000.0f;
        
        if (dealtDmg == 1){ //whenever we do damage, reset the blink animation after 2 seconds
            StartCoroutine (resetBlink(2f));
        }

        //handling at which point the predator will backOff
        float backOffDist = 2.78f;
        if (Manager.flow == 1){
            backOffDist = 2.68f;
        }
        if (Manager.instance.isTutorial){
            backOffDist = 2.8f;
        }
        
       if ( (Co_In< p_atk || (targetFishRP.theta0 > (Mathf.PI/2)) || (targetFishRP.theta0 < -(Mathf.PI/2))) && Flag_attack == 0.0f) //attack when 1. random chance 2. fish is swimming to the left
        {
          Debug.Log("Should Attack now");
          retreat_flag = false;
          coroutine = (atk_timer(3.5f)); //4 seconds, if attack sequence doesnt finish, end it manually
          StartCoroutine(coroutine);

          u_atk = 0.0f; // Attack
          Flag_attack = 1.0f;
          randomNoise = NextGaussian3(1f,0.5f,-2f,2f);
        }
        //else if (Flag_attack==1.0f && (fishOffset <= 0.3f || retreat_flag == true)) //predator finishes attacking, now going back
        else if (Flag_attack==1.0f && (pred_2_fish_vect.magnitude <= backOffDist || retreat_flag == true)) //predator finishes attacking, now going back
        {
          StopCoroutine(coroutine);
          u_atk = 1.0f; // no attack
          dealtDmg = 0; //predator can deal dmg again
          retreat_flag = false;
        
        }
       else if (Flag_attack==1.0f && u_atk == 1.0f && fishOffset >= (fishOffsetDefault-3f)) //fish back at default offset
        {
          //targetFish.GetComponent<Animator>().Play("New State", 1);//stop the fish from blinkin
          retreat_flag = false;
          randomNoise = 0f;
          Flag_attack = 0.0f;   
        }
        if ((!avoidFlag)){
            fishOffset_k1 = fishOffset + ( (fishOffsetDefault - relaxation_constant * fishOffset)*u_atk - convergence_rate * fishOffset * (1.0f-u_atk) ) *GameTime.deltaTime;
        }



        UR_x =  -1*(Mathf.Pow(rockRadius,2))*(Mathf.Pow((xo-xa),2)-Mathf.Pow((yo-ya),2)) / Mathf.Pow((Mathf.Pow((xo-xa),2)+Mathf.Pow((yo-ya),2)),2);
        UR_y =  2*(Mathf.Pow(rockRadius,2))*((xo-xa)*(yo-ya)) / Mathf.Pow((Mathf.Pow((xo-xa),2)+Mathf.Pow((yo-ya),2)),2);

        t2 = getU(yo,Manager.instance.a, Manager.instance.b, Manager.instance.flow_variable);
        t3 = -t2* Mathf.Sign(Mathf.Cos(theta0))*UR_x;
        tt3 = t2*UR_y;    

        if (kp_vanish){ //this condition is triggered when predator enters a rock's ObstacleProximity hitbox, THIS MODELS THE BEHAVIOUR AROUND THE 2.6f RADIUS, BUT WE HAVE TO IMPLEMENT SOMETHING TO HARD BOUNDARIES THE 2.6F RADIUS BECAUSE THE MODEL DOESNT WORK WHEN IT TOUCHES.      

            xk_1 = xo + (t2+t3)*GameTime.deltaTime + vk*Mathf.Cos(theta0)*GameTime.deltaTime;
            yk_1 = yo + Mathf.Sign(Mathf.Cos(theta0))*(tt3)*GameTime.deltaTime + vk*Mathf.Sin(theta0)*GameTime.deltaTime;

            //implementation of border avoidance: If (xk_1, yk_1) results in the predator inside the rock, we FORCE PUSH the predator out
            if (Mathf.Pow(xk_1-xa,2) + Mathf.Pow(yk_1-ya,2) <= Mathf.Pow(rockRadius,2)){
                vk = 1.5f*vDefault;
                xk_1 = xo + Mathf.Sign(xo-xa)*vk*GameTime.deltaTime;
                yk_1 = yo + Mathf.Sign(yo-ya)*vk*GameTime.deltaTime;
            }

            //thetak_1 = theta0 + (-1f) * getdU(yo, a, flowVariable) * Mathf.Pow(Mathf.Cos(theta0), 2) * GameTime.deltaTime;
            thetak_1 = theta0;

        } else { //this is normal condition
            /*
            if (-1f*(t2+t3)> vk*Mathf.Cos(theta0) + kp*(targetFish.position.x - fishOffset - xo)) { //predator is stuck on the rock due to high flow
                vk = vk + 0.5f;
            } else {
                vk = vDefault;
            }*/
            vk = vk - (vk - vDefault)*GameTime.deltaTime;
            xk_1 = xo + (t2+t3)*GameTime.deltaTime + vk*Mathf.Cos(theta0)*GameTime.deltaTime + kp * (targetFish.position.x - fishOffset - xo) * GameTime.deltaTime;
            yk_1 = yo + Mathf.Sign(Mathf.Cos(theta0))*(tt3)*GameTime.deltaTime + vk*Mathf.Sin(theta0)*GameTime.deltaTime + ((targetFish.position.y + randomNoise - yo) * GameTime.deltaTime);
            thetak_1 = theta0 + (-1f * getdU(yo, a, flowVariable) * Mathf.Pow(Mathf.Cos(theta0), 2) * GameTime.deltaTime + kp * (desired_angle - theta0) * GameTime.deltaTime);
            
        }
        //predator undulation
       
        //first, determine predator vk 
        float fvk = vk + Mathf.Sqrt(Mathf.Pow(t2+t3+kp*(targetFish.position.x-fishOffset-xo), 2) + Mathf.Pow(Mathf.Sin(Mathf.Cos(theta0))*(tt3) + (targetFish.position.y+randomNoise-yo),2));
        //predator half body length is 1.5
        tf = ( (fvk/3f) + 1.39f * Mathf.Pow(1.5f,(2/3)) ) / (0.98f*(1.5f));
        this.GetComponent<Animator>().SetFloat("speedVar",tf);

        //dx/GameTime.deltaTime = 8-(alpha*x) *input - alpha2 * x * (1-input), input is 0 or 1;

        /*
        if (attackToggle == 0) //if the fish is not mid attack
        {
            if (Random.Range(0, 99) == 0) // 1 in 100 chance we trigger attack sequence
            {
                attackToggle = 1; //for the next few frames we initiate attack sequence
                attackXRef = targetFish.position.x; //lock in the target fish's x position for attack
                attackYRef = targetFish.position.y;
                attackThetaRef = targetFishRP.theta0;
                //kp = 0;
            }
        }

        else if (attackToggle == 1) //if the fish is in attack sequence
        {
            fishOffset = fishOffset - speedOfAtk * GameTime.deltaTime;
            if (fishOffset < 1.5f) //fish has finished attacking
            {
                attackToggle = 2; //for the next few frames we inititate back sequence
            } 
        }
        else if (attackToggle == 2)
        {
            fishOffset = fishOffset + speedOfBck * GameTime.deltaTime;
            //attackYRef = attackYRef + (targetFish.position.y - attackYRef) * 1.2f * GameTime.deltaTime;
            if (kp < 3)
            {
                kp = kp + 0.01f;
            } 
            else
            {
                kp = 3;
            }
            attackThetaRef = attackThetaRef + (targetFishRP.theta0 - attackThetaRef) * 0.5f * GameTime.deltaTime;
            if (fishOffset >= 8f - (speedOfBck * GameTime.deltaTime)) //fish has finished backing
            {
                attackYRef = targetFish.position.y;
                fishOffset = 8f;
                dealtDmg = 0; //predator can deal dmg again
                attackToggle = 0; //whole attack cycle is finished, now we start random again
            }
        }
        /*
        if (attackToggle == 0)
        {
            xk_1 = xo + 1f / tau * (targetFishRP.vk * Mathf.Cos(theta0) * GameTime.deltaTime + getU(yo, a, b, flowVariable) * GameTime.deltaTime + kp * (targetFish.position.x - fishOffset - xo) * GameTime.deltaTime);
            yk_1 = yo + 1f / tau * (targetFishRP.vk * Mathf.Sin(theta0) * GameTime.deltaTime + kp * (targetFish.position.y - yo) * GameTime.deltaTime);
            thetak_1 = theta0 + 1f / tau *(-1f * getdU(yo, a, flowVariable) * Mathf.Pow(Mathf.Cos(theta0), 2)) * GameTime.deltaTime + kp * (targetFishRP.theta0 - theta0) * GameTime.deltaTime;
        }
        else
        {
            xk_1 = xo + 1f / tau * (vk * Mathf.Cos(theta0) * GameTime.deltaTime + getU(yo, a, b, flowVariable) * GameTime.deltaTime + kp * (targetFish.position.x - fishOffset - xo) * GameTime.deltaTime);
            yk_1 = yo + 1f / tau * (vk * Mathf.Sin(theta0) * GameTime.deltaTime + kp * (targetFish.position.y - yo) * GameTime.deltaTime);
            thetak_1 = theta0 + (-1f * getdU(yo, a, flowVariable) * Mathf.Pow(Mathf.Cos(theta0), 2)) * GameTime.deltaTime + kp * (targetFishRP.theta0 - theta0) * GameTime.deltaTime;

        }
        */

        /*
        if (turnUp){
            if (thetak_1 >= Mathf.PI/4){
                thetak_1 = theta0;
            } else {
                thetak_1 = theta0 +  (2f/(2*Mathf.PI*Mathf.Pow(l,2))) * GameTime.deltaTime;
            }

            xk_1 = xo + 3f* Mathf.Cos(thetak_1) * GameTime.deltaTime;
            yk_1 = yo + 3f* Mathf.Sin(thetak_1) * GameTime.deltaTime;
        } else if (turnDown){
            if (thetak_1 <= -Mathf.PI/4) {
                thetak_1 = theta0;
            } else {
                thetak_1 = theta0 +  (-2f/(2*Mathf.PI*Mathf.Pow(l,2))) * GameTime.deltaTime;
            }

            xk_1 = xo + 3f* Mathf.Cos(thetak_1) * GameTime.deltaTime;
            yk_1 = yo + 3f* Mathf.Sin(thetak_1) * GameTime.deltaTime;
        } else {
            //angle adjustment prep
            Vector2 pred_2_fish_vect = new Vector2 (targetFish.position.x - xo, targetFish.position.y - yo);
            Vector2 horizontal_vect = new Vector2 (1,0); //unit vector on the horizontal
            float desired_angle = Mathf.Deg2Rad * Vector2.Angle(pred_2_fish_vect,horizontal_vect); //Vector2.Angle gives the smallest angle in degrees between two vectors.
            if (pred_2_fish_vect.y < 0 ){
                desired_angle = -desired_angle;
            }

            UR_x =  -1*(Mathf.Pow(rockRadius,2))*(Mathf.Pow((xo-xa),2)-Mathf.Pow((yo-ya),2)) / Mathf.Pow((Mathf.Pow((xo-xa),2)+Mathf.Pow((yo-ya),2)),2);
            UR_y =  2*(Mathf.Pow(rockRadius,2))*((xo-xa)*(yo-ya)) / Mathf.Pow((Mathf.Pow((xo-xa),2)+Mathf.Pow((yo-ya),2)),2);

            t2 = getU(yo,Manager.instance.a, Manager.instance.b, Manager.instance.flow_variable);
            t3 = -t2* Mathf.Sign(Mathf.Cos(theta0))*UR_x;
            tt3 = t2*UR_y;    

            if (kp_vanish){ //this condition is triggered when predator enters a rock's ObstacleProximity hitbox, THIS MODELS THE BEHAVIOUR AROUND THE 2.6f RADIUS, BUT WE HAVE TO IMPLEMENT SOMETHING TO HARD BOUNDARIES THE 2.6F RADIUS BECAUSE THE MODEL DOESNT WORK WHEN IT TOUCHES.      

                xk_1 = xo + (t2+t3)*GameTime.deltaTime + 5.0f*Mathf.Cos(theta0)*GameTime.deltaTime;
                yk_1 = yo + Mathf.Sign(Mathf.Cos(theta0))*(tt3)*GameTime.deltaTime + 5.0f*Mathf.Sin(theta0)*GameTime.deltaTime;
                //thetak_1 = theta0 + (-1f) * getdU(yo, a, flowVariable) * Mathf.Pow(Mathf.Cos(theta0), 2) * GameTime.deltaTime;
                thetak_1 = theta0;

            } else { //this is normal condition

                xk_1 = xo + (t2+t3)*GameTime.deltaTime + 3.0f*Mathf.Cos(theta0)*GameTime.deltaTime + kp * (targetFish.position.x - fishOffset - xo) * GameTime.deltaTime;
                yk_1 = yo + Mathf.Sign(Mathf.Cos(theta0))*(tt3)*GameTime.deltaTime + 3.0f*Mathf.Sin(theta0)*GameTime.deltaTime + ((targetFish.position.y + randomNoise - yo) * GameTime.deltaTime);
                thetak_1 = theta0 + (-1f * getdU(yo, a, flowVariable) * Mathf.Pow(Mathf.Cos(theta0), 2) * GameTime.deltaTime + kp * (desired_angle - theta0) * GameTime.deltaTime);
                
            }

            //xk_1 = xo + (getU(yo, a, b, flowVariable) * GameTime.deltaTime + kp * (targetFish.position.x - fishOffset - xo) * GameTime.deltaTime);
            //yk_1 = yo + ((targetFish.position.y + randomNoise - yo) * GameTime.deltaTime);

            
            //thetak_1 = theta0 + (-1f * 0.0f * getdU(yo, a, flowVariable) * Mathf.Pow(Mathf.Cos(theta0), 2) * GameTime.deltaTime + kp * (targetFishRP.theta0 - theta0) * GameTime.deltaTime);
        }
        */

        /*
        if (avoidFlagStrong){
            Vector2 backoffVector = new Vector2 (transform.position.x - currObstacleVector.x, transform.position.y - currObstacleVector.y);
            backoffVector.Normalize();
            Vector2 Nudge = backoffVector * 2f;
            Debug.Log("Strong Nudging");
            //Debug.Log(Nudge);
            yk_1 = yo + Nudge.y * GameTime.deltaTime;
            xk_1 = xo + Nudge.x * GameTime.deltaTime;
            thetak_1 = theta0;
        }
        else if (avoidFlag){
            Vector2 backoffVector = new Vector2 (transform.position.x - currObstacleVector.x, transform.position.y - currObstacleVector.y);
            backoffVector.Normalize();
            Vector2 lightNudge = backoffVector * 0.2f;
            Debug.Log("Nudging");
            yk_1 = yo + 1.5f*lightNudge.y * GameTime.deltaTime;
            xk_1 = xo + lightNudge.x * GameTime.deltaTime;
            if (theta0 >= 0 ){
                //thetak_1 = theta0 + -1f * Mathf.Abs(kp * (targetFishRP.theta0 - theta0) * GameTime.deltaTime);
                thetak_1 = theta0 + -1f*(Mathf.PI/4) * GameTime.deltaTime;               
            } else {
                //thetak_1 = theta0 + Mathf.Abs(kp * (targetFishRP.theta0 - theta0) * GameTime.deltaTime); 
                thetak_1 = theta0 + (Mathf.PI/4) * GameTime.deltaTime; 
            }

            //yk_1 = yo + (-vk)*0.1f * Mathf.Sin(theta0) * GameTime.deltaTime;
            //xk_1 = xo + (-vk)*0.1f * Mathf.Cos(theta0)  * GameTime.deltaTime;
        } else {
            xk_1 = xo + (getU(yo, a, b, flowVariable) * GameTime.deltaTime + kp * (targetFish.position.x - fishOffset - xo) * GameTime.deltaTime);
            yk_1 = yo + ((targetFish.position.y + randomNoise - yo) * GameTime.deltaTime);
            thetak_1 = theta0 + (-1f * 0.0f * getdU(yo, a, flowVariable) * Mathf.Pow(Mathf.Cos(theta0), 2) * GameTime.deltaTime + kp * (targetFishRP.theta0 - theta0) * GameTime.deltaTime);
        }
        */
        /*
        if (avoidFlag){
            xk_1 = xo +  -0.2f*kp * (targetFish.position.x - fishOffset - xo) * GameTime.deltaTime;
            yk_1 = yo + -0.2f*(targetFish.position.y + randomNoise - yo) * GameTime.deltaTime;
        } else {
            xk_1 = xo + (getU(yo, a, b, flowVariable) * GameTime.deltaTime + kp * (targetFish.position.x - fishOffset - xo) * GameTime.deltaTime);
            yk_1 = yo + ((targetFish.position.y + randomNoise - yo) * GameTime.deltaTime);
            //xk_1 = xo + (targetFishRP.vk * Mathf.Cos(theta0) * GameTime.deltaTime + getU(yo, a, b, flowVariable) * GameTime.deltaTime + kp * (targetFish.position.x - fishOffset - xo) * GameTime.deltaTime);
            //yk_1 = yo + (targetFishRP.vk * Mathf.Sin(theta0) * GameTime.deltaTime +  (targetFish.position.y + randomNoise - yo) * GameTime.deltaTime);
        }*/

        //thetak_1 = theta0 + (-1f * 0.0f * getdU(yo, a, flowVariable) * Mathf.Pow(Mathf.Cos(theta0), 2) * GameTime.deltaTime + kp * (targetFishRP.theta0 - theta0) * GameTime.deltaTime);
        

        /*
        if (u_atk == 0){
            desire_angle = Mathf. Atan((targetFish.position.y + randomNoise-transform.position.y)/(targetFish.position.x-transform.position.x));
            thetak_1 = theta0 + 1f / tau *(-1f * 0.0f * getdU(yo, a, flowVariable) * Mathf.Pow(Mathf.Cos(theta0), 2) * GameTime.deltaTime + kp * (desire_angle -theta0) *GameTime.deltaTime);   
        }*/



        //fish translation for next frame
        predatorPos = new Vector2(xk_1, yk_1);



        //vertical boundary, fish 'bounces off' when they go into the riverbanks
        if (predatorPos.y >= y_boundary)
        {
            predatorPos.y = y_boundary;
            /*if (Mathf.Cos(theta0) > 0)
            {
                theta0 = -0.785398163f;
                thetak_1 = -0.785398163f;
            }
            else
            {
                theta0 = -2.35619449f;
                thetak_1 = -2.35619449f;
            }*/
        }
        else if (predatorPos.y <= -y_boundary)
        {
            predatorPos.y = -y_boundary;
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

        //translating and rotating the fish according to curr frame
        transform.position = predatorPos;
        transform.eulerAngles = new Vector3(0f, 0f, thetak_1 * Mathf.Rad2Deg);

        //updating for the next frame;
        theta0 = thetak_1;
        fishOffset = fishOffset_k1;

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        /*
        if (other.tag == "ObstacleOutside")
        {            
            //Debug.Log("predaotr hits rock sadge");
            avoidFlag = true;
            //this part will start the avoid motion either up or down.
            if (other.transform.position.y >= transform.position.y) { //predator is from bottom going up into a rock
                StartCoroutine(avoidRockDown(1.5f));
            } else { //predator is from top going down into a rock
                StartCoroutine(avoidRockUp(1.5f));
            }

  
            //currObstacleVector = other.transform.position;

            /*
            Vector2 normal = Vector2.Perpendicular(other.ClosestPoint(transform.position));
            Vector2 directionVector = new Vector2(Mathf.Cos(theta0), Mathf.Sin(theta0));
            Vector2 resultVector = new Vector2();

            resultVector = Vector2.Reflect(directionVector, normal);
            if (resultVector.x < 0)
            {
                theta0 = 360f - (Mathf.Atan2(resultVector.x, resultVector.y) * -1f);
            }
            else
            {
                theta0 = Mathf.Atan2(resultVector.x, resultVector.y);
            }
            //////
        }*/
        if (other.tag == "ObstacleProximity"){
            xa = other.transform.position.x;
            ya = other.transform.position.y;
            kp_vanish = true;
        }

    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "ObstacleOutside")
        {
            avoidFlagStrong = false;
        }

        if(other.tag == "ObstacleProximity"){
            kp_vanish = false;
            fishOffset = targetFish.position.x - transform.position.x;
        }
    }

    public void toggleDmg()
    {
        dealtDmg = -dealtDmg + 1;
    }
}
