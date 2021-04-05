using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class eggLaying : MonoBehaviour
{
    public RandomPatrol fish;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (GameTime.isPaused){return;}
        if (transform.position.x < Camera.main.transform.position.x - (Camera.main.orthographicSize * Screen.width / Screen.height) - 5f)
        {
            Destroy(this.gameObject);
        }

    }

    public void laying() //changing the tag from "Egg Spot" to "Egg Spot_Full"
    {
        gameObject.tag = "Egg Spot_Full";
    }

    //This function will be called from animation event after the layingEgg_Good animation is played
    public void laidEggs_Good()
    {
        //give the awards for successfully laying eggs
        fish.food += fish.eggRewardAmount;
        fish.foodBar.SetFood(fish.food);
        fish.layingEgg = 0; //fish is done laying eggs, can now move freely
    }

    public void laidEggs_Bad()
    {
        fish.layingEgg = 0; //fish is done laying eggs, can now move freely
    }
}
