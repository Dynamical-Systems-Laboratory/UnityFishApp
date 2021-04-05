using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


//wrapper for time
public static class GameTime {
    public static bool isPaused = false;
    public static float deltaTime {get {return isPaused ? 0 : Time.deltaTime;}}
}

public class Manager : MonoBehaviour
{
    public static Manager instance;
    //pause menu
    public GameObject pMenu;
 

    public static int poll = 0;    
    public static int temp = 0;
    public static int flow = 0;

    
    //the listed values are their "normal value"
    public float rotate_sensitivity = 0.35f; //0.2 for low, 0.35 for normal, 0.5 for high
    public float speed_sensitivity = 10f; //5 for low, 10 for normal, 15 for high
    public float flow_variable = 1f; //1.5 for low speed fish, 1 for normal speed fish, 0.5 for high speed fish
    public float food_decreaseSpeed = 0.1f; //0.05 for low, 0.1 for normal, 0.2 for high
    public float food_availability = 2; //1 for scarce, 2 is normal, 3 is plenty
    public float egg_successChance = 70f; //out of 100, the percentage for a laid egg to hatch.
    public float predator_avoidance = 0.45f; //0.7 for low avoidance, 0.45 for normal avoidance, 0.1 for high avoidance. This is predator chance to attack per frame

    //variables used in flow calculation, uniform for every game objects
    public float a = -0.01389f; //constant of the flow parabola
    public float b = 2f; //maximum flow (at the center of the parabola)
    public float y_boundary = 12f;

    public float[][] table = new float[7][];

    public bool isTutorial;

    private void setup(int a, int b, int c, int d, int e, int f, int g){
        rotate_sensitivity = table[0][a];
        speed_sensitivity = table[1][b];
        flow_variable = table[2][c];
        food_decreaseSpeed = table[3][d];
        food_availability = table[4][e];
        egg_successChance = table[5][f];
        predator_avoidance = table[6][g];

        Debug.Log("Variable Setup Complete, check Manager class for results");
    }


    void Awake(){
        instance = this;
        GameTime.isPaused = false;
        //set up the values depending on varying levels 
        table[0] = new float[]{0.5f,0.3f,0.1f,0.05f}; //rotate sens, level = 1 + 2*poll - 1*temp
        table[1] = new float[]{9f,8f,7f,7f}; //speed sens, level = 1 + 2*poll - 1*temp
        table[2] = new float[]{1f,2f};  //flow variable, level = 0 + 1*flow
        table[3] = new float[]{0.2f,0.25f,0.3f,0.35f,0.4f}; //food decrease speed, level = 0 + 1*poll + 2*temp + 1*flow
        table[4] = new float[]{1f,2f,3f}; //food availability CURRENTLY NOT USED AND ALWAYS DEFAULTS TO 2
        table[5] = new float[]{80f,60f}; //egg succ chance, level = 0 + 1*poll
        table[6] = new float[]{0.008f,0.01f,0.02f}; //predator avoidance AKA how often the predator attacks, level = 0 + 1*poll + 1*flow

        setup(1+2*poll-temp,
        1+2*poll-temp,
        0+flow,
        0+poll+2*temp+flow,
        1,
        0+poll,
        0+poll+flow);

        if (isTutorial){
            setup(1,1,0,0,1,0,0); //ideal environment kinda
            flow_variable = 0;
            //predator_avoidance = -0.5f;
            food_decreaseSpeed = 0;
        }
        

        /*
        else if (STATIC.pollLevel == "l")
        {
            if (STATIC.tempLevel == "m")
            {
                if (STATIC.flowLevel == "l") {setup(0,0,0,0,0,1,1);} //rotate sens, speed sens, average speed of feesh (inverse of flow speed), food decrease speed, food availability, egg succ chance, predator avoidance
                if (STATIC.flowLevel == "m") {setup(1,1,1,1,1,1,1);}
                if (STATIC.flowLevel == "h") {setup(1,1,2,2,2,1,0);}
            }
            if (STATIC.tempLevel == "h")
            {
                if (STATIC.flowLevel == "l") {setup(2,2,2,2,0,1,1);}
                if (STATIC.flowLevel == "m") {setup(2,2,2,2,1,1,1);}
                if (STATIC.flowLevel == "h") {setup(2,2,2,2,1,1,1);}
            }
        } 
        else if (STATIC.pollLevel == "h")
        {
            if (STATIC.tempLevel == "m")
            {
                if (STATIC.flowLevel == "l") {setup(0,0,0,0,0,0,0);}
                if (STATIC.flowLevel == "m") {setup(0,0,0,0,2,0,0);}
                if (STATIC.flowLevel == "h") {setup(0,0,0,2,2,0,0);}
            }
            if (STATIC.tempLevel == "h")
            {
                if (STATIC.flowLevel == "l") {setup(2,2,2,2,0,0,0);}
                if (STATIC.flowLevel == "m") {setup(2,2,0,2,1,0,0);} //not yet known
                if (STATIC.flowLevel == "h") {setup(0,2,0,0,2,0,0);} //not really "decaying" 
            }
        }
        else //this situation occurs when pollLevel is not low or high, which is either error or debug mode, we go by default values here.
        {
            setup(1,1,1,0,2,2,2); //ideal environment kinda
            //setup(0,0,0,0,0,0,0);
            //setup(1,1,1,1,1,1,1);
            //setup(2,2,2,2,2,2,2);
        }*/
    }
    void Start()
    {
        Debug.Log(poll+","+temp+","+flow);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape)) {
            togglePause();
        }
    }

    public void restart()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);

    }

    public void loadMenu()
    {

        SceneManager.LoadScene("stage selection_v2");
    }

    public void togglePause()
    {
        GameTime.isPaused = !GameTime.isPaused;
        pMenu.SetActive(!pMenu.activeSelf);
    }

}
