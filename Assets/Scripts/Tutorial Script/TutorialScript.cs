using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialScript : MonoBehaviour
{

    public static TutorialScript instance;
    public Text dialogText;
    public GameObject endOverlay;
    public GameObject invisButton;

    //predator
    public GameObject predator;
    //the four items in in game canvas
    public GameObject topLeft;
    public GameObject topRight;
    public GameObject bottomLeft;
    public GameObject bottomRight;

    //wave animation
    public GameObject wave;

    //areas
    public GameObject area1;

    //Prefabs for spawning materials
    public GameObject foodPrefab;

    //rocks to appear on stage 5
    public GameObject rock1;
    public GameObject rock2;
    public GameObject rock3;

    //egg spots to appear on stage 6
    public GameObject eggSpot;
    public GameObject eggSpot2;

    private int tutorialStage;
    private string[] messages;

    //toggles
    private bool eggReset = false;
    //make user not able to spam through tutorial (accidentally)
    private bool canClick;
    //turn keys and space bar turns on on stage 2
    public bool canSwim;
    public bool canTurn;
    public bool gotHit;
    public bool attackReset = false;

    public void nextStage()
    {
        tutorialStage += 1;

        if (tutorialStage >= messages.Length){ //if no more dialogs, then we end
            endOverlay.SetActive(true);
        } else { //if more dialogs, play the next dialog
            this.GetComponent<Animator>().Play("next");
        }
    
        if (tutorialStage == 1) {
            RandomPatrol.instance.GetComponent<Animator>().Play("fish_with_bones_swim_v2");
            wave.GetComponent<Animation>().Play();
            RandomPatrol.instance.flowVariable = 1f;
            Manager.instance.flow_variable = 1f;

        }
        if (tutorialStage == 2) {
            wave.GetComponent<Animation>().Stop();
            wave.SetActive(false);
            RandomPatrol.instance.GetComponent<Animator>().Play("fish_with_bones_swim_v2");//fish no longer highlighted
            topLeft.SetActive(true);
            //tutorialStage = 3; // skip this for now, talk with Daniel on this
        }
        if (tutorialStage == 3) {
            //bottomLeft.SetActive(true);
            invisButton.SetActive(false);
            canSwim = true;
            canTurn = true;
            area1.SetActive(true);
        }
        if (tutorialStage == 4) {
            //canTurn = false;
            //canSwim = false;
            invisButton.SetActive(true);
            topLeft.SetActive(true);
            area1.SetActive(false);
            RandomPatrol.instance.swimUp(); // fish no longer swims
            RandomPatrol.instance.foodDecreaseSpeed = 1.0f;
        }
        if (tutorialStage == 5) {
            rock1.SetActive(true);
            rock2.SetActive(true);
            //rock3.SetActive(true);
            invisButton.SetActive(false);
            canTurn = true;
            canSwim = true;
            RandomPatrol.instance.food = 80;
            RandomPatrol.instance.foodDecreaseSpeed = 0.0f;
            StartCoroutine(spawningFood());

        }
        if (tutorialStage ==6) {
            StopCoroutine(spawningFood());
            eggSpot.SetActive(true);
            eggSpot2.SetActive(true);
            bottomRight.SetActive(true);
            RandomPatrol.instance.food = 75;
            RandomPatrol.instance.eggSuccessRate = 0;
        }
        if (tutorialStage ==7) {
            RandomPatrol.instance.eggSuccessRate = 100;
        }
        if (tutorialStage ==8) {
            eggSpot.SetActive(false);
            eggSpot2.SetActive(false);
            

            predator.SetActive(true);


        }
        if (tutorialStage ==9) {
            invisButton.SetActive(true);
        }


            


    }

    //this function is called whever the invisButton is clicked
    public void invisButtonClick(){
        if (canClick){
            canClick = false;
            nextStage();
        }
    }

    //this function will be called when user click up the swim button ONLY MOBILE
    /*
    public void swimComplete(){
        if (tutorialStage == 2){
            nextStage();
        }
    }*/
    private void changeText()
    {
        dialogText.text = messages[tutorialStage];
        
    }

    //this function is called after each dialog change animation is over
    private void animationEndSetup()
    {
        canClick = true; 

    }

    private IEnumerator spawningFood()
    {
        while (tutorialStage == 5){
            if (GameObject.FindGameObjectsWithTag("Food").Length < 2)
            {
                GameObject a = Instantiate(foodPrefab) as GameObject;
                a.GetComponent<foodBehaviour>().a = Manager.instance.a;
                a.GetComponent<foodBehaviour>().b = Manager.instance.b;
                a.GetComponent<foodBehaviour>().y_boundary = Manager.instance.y_boundary;
                a.GetComponent<foodBehaviour>().flowVariable = 2f;
                a.transform.position = new Vector2(1f + Camera.main.transform.position.x + Camera.main.orthographicSize * Screen.width / Screen.height, Random.Range(-6f, 6f));
            
                GameObject b = Instantiate(foodPrefab) as GameObject;
                b.GetComponent<foodBehaviour>().a = Manager.instance.a;
                b.GetComponent<foodBehaviour>().b = Manager.instance.b;
                b.GetComponent<foodBehaviour>().y_boundary = Manager.instance.y_boundary;
                b.GetComponent<foodBehaviour>().flowVariable = 2f;
                b.transform.position = new Vector2(2f + Camera.main.transform.position.x + Camera.main.orthographicSize * Screen.width / Screen.height, Random.Range(-6f, 6f));
            
            }

            yield return null;
        }

    }




    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        tutorialStage = 0;
        canClick = true;
        canSwim = false;
        canTurn = false;
        attackReset = false;
        eggReset = false;
        //this is the dialogs, indexed starting at 0 
        messages = new string[] {"Welcome to the tutorial.\n You will be controling this fish that swims in a river.\nClick to continue.", 
        "The water current pushes the fish to the left. The current is strongest at the center of the river. You will swim against the current to the right.\n <color=yellow>Click to continue.</color> ",
        "The top left bar represents your fish's fitness. To maintain fitness, you will need to eat food and lay eggs.\n<color=yellow>Click to continue.</color>", 
        "Use [space] or [Up], [a] or [Left] and [d] or [Right] on your keyboard to control speed and turns, respectively. \n<color=yellow>Swim to the green area to continue.</color>",
        "The fish loses fitness naturally with time, if it reaches zero, the game ends.\n<color=yellow>Click to continue.</color>",
        "Collecting food pellets that flow down stream recovers fitness.\n<color=yellow>To continue, collect food pellets until your food bar is full.</color>",

        "Laying eggs have the potential to recover a large amount of fitness. Press [enter] to lay eggs.\n<color=yellow>Go to one of the darker spots in the sand and Lay eggs to continue.</color>",
        "Oops, it seems that the eggs failed to hatch this time.\n<color=yellow>Try again at a different spot.</color>",
        "Watch out for the predator fish who attacks randomly.\nAvoid attacks to not lose fitness.\n<color=yellow>Avoid one attack to continue.</color>",
        "That's it, you are all done.\nDo your best to keep that fitness bar full and stay alive!\n<color=yellow>Click to finish tutorial.</color>"};
    }
    void Start()
    {
        tutorialStage = 0;
        RandomPatrol.instance.GetComponent<Animator>().Play("blueFishHighlighted");
    }

    // Update is called once per frame
    void Update()
    {
/*
        if (tutorialStage == 3 && Input.GetKeyUp("space")){
            nextStage();
        }
*/
        if (RandomPatrol.instance.food <= 30){
            RandomPatrol.instance.food = 100;
        }

        if (tutorialStage == 5 && RandomPatrol.instance.food == 100){
            nextStage();
        }
/*
        if((tutorialStage == 6) && RandomPatrol.instance.layingEgg == 1){
            nextStage();
            eggReset = true;
        }
        if (tutorialStage == 7 && RandomPatrol.instance.layingEgg == 0){
            eggReset = false;
        }
        if (tutorialStage ==7 && eggReset == false && RandomPatrol.instance.layingEgg == 1){
            eggReset = true;
        }
*/
        //A simple toggle to triggle next stage sequence whenever the fish finishes laying down eggs.
        if (eggReset == false && RandomPatrol.instance.layingEgg == 1){
            eggReset = true;
        }
        if(eggReset == true && RandomPatrol.instance.layingEgg == 0){
            eggReset = false;
            nextStage();
        }

        //A simple toggle to triggle next stage if fish avoids attack during an attack sequence
        
        if (attackReset == false && predatorBehaviour.instance.Flag_attack == 1.0f && !(predatorBehaviour.instance.p_atk == 0)){
            attackReset = true;
            Debug.Log("what the fuck");
        }
        if (attackReset == true && predatorBehaviour.instance.Flag_attack == 0.0f && !(predatorBehaviour.instance.p_atk == 0)){
            attackReset = false;
            if (gotHit){
                gotHit = false;
                dialogText.text = "Ouch, the predator hit you this time, try to dodge its next attack by going up and down or hiding around the rocks.";
            } else {
                //predatorBehaviour.instance.p_atk = 0.0f;
                nextStage();
            }
        }
        
    }   
    
    

}
