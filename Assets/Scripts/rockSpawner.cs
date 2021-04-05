using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rockSpawner : MonoBehaviour
{
    //this is the rock prefab
    public GameObject rockPrefab;
    public GameObject foodPrefab;
    public GameObject eggspotPrefab; //after instiating this, set its "fish" reference to fishRef.

    public RandomPatrol fishRef;
    public Manager manager;
    

    //this is the fish position

    public Transform fishTrans;
    public float fishXL; //fish X location
    public float distanceTillRock = 20.0f; //How many units the fish goes forward before a new rock is spawned
    


    //how much food spawned per cycle, set by manager at start()
    public float foodAvail; 
    public float flowVariable;

    public float flowA;
    public float flowB;
    public float y_boundary;


    // Start is called before the first frame update
    void Start()
    {
        fishXL = fishTrans.position.x - (distanceTillRock);

        //set up from manager
        foodAvail = manager.food_availability;
        flowVariable = manager.flow_variable;

        flowA = manager.a;
        flowB = manager.b;
        y_boundary = manager.y_boundary;

    }
    private void spawnRockAndFood()
    {
        //spawning a new rock from the prefab
        GameObject a = Instantiate(rockPrefab) as GameObject;

        GameObject b = Instantiate(foodPrefab) as GameObject;
        b.GetComponent<foodBehaviour>().a = flowA;
        b.GetComponent<foodBehaviour>().b = flowB;
        b.GetComponent<foodBehaviour>().y_boundary = y_boundary;
        b.GetComponent<foodBehaviour>().flowVariable = flowVariable;

        

        //put this rock somewhere on the scene
        a.transform.position = new Vector2(2f + Camera.main.transform.position.x + Camera.main.orthographicSize * Screen.width / Screen.height, Random.Range(-6f, 6f));
        b.transform.position = new Vector2(5f + Camera.main.transform.position.x + Camera.main.orthographicSize * Screen.width / Screen.height, Random.Range(-6f, 6f));
        if (foodAvail == 2){
            GameObject c = Instantiate(foodPrefab) as GameObject;
            c.GetComponent<foodBehaviour>().a = flowA;
            c.GetComponent<foodBehaviour>().b = flowB;
            c.GetComponent<foodBehaviour>().y_boundary = y_boundary;
            c.GetComponent<foodBehaviour>().flowVariable = flowVariable;
            c.transform.position = new Vector2(8f + Camera.main.transform.position.x + Camera.main.orthographicSize * Screen.width / Screen.height, Random.Range(-6f, 6f));
        }
        if (foodAvail == 3){
            GameObject d = Instantiate(foodPrefab) as GameObject;
            d.GetComponent<foodBehaviour>().a = flowA;
            d.GetComponent<foodBehaviour>().b = flowB;
            d.GetComponent<foodBehaviour>().y_boundary = y_boundary;
            d.GetComponent<foodBehaviour>().flowVariable = flowVariable;
            d.transform.position = new Vector2(16f + Camera.main.transform.position.x + Camera.main.orthographicSize * Screen.width / Screen.height, Random.Range(-6f, 6f));
        }
        if ( STATIC.flowLevel != "h" || (Random.Range(0,100) <= 70)) { //if flow level is high, then the egg spot have a 70% chance of spawning per cycle instead of 100% per cycle
            GameObject e = Instantiate(eggspotPrefab) as GameObject;
            e.GetComponent<eggLaying>().fish = fishRef; //setting the reference to the fish after instantiation.
            e.transform.position = new Vector2(10f + Camera.main.transform.position.x + Camera.main.orthographicSize * Screen.width / Screen.height, Random.Range(-6f, 6f));
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (GameTime.isPaused){return;}
        if (fishTrans.position.x - fishXL > distanceTillRock)
        {
            spawnRockAndFood();
            fishXL = fishTrans.position.x;

        }

    }
}
