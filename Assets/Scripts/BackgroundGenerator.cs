//generates the tile background and destroys the previous tile background whenever needed.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundGenerator : MonoBehaviour
{   
    public Transform fish;

    public Transform bg_1;
    public Transform bg_2;


    private float x_NewestBG;
    private float bg_size = 176f;
    private int onBG;
    //0 for default, 1 when on bg 1, 2 when on bg 2 

    void Start()
    {
        x_NewestBG = -24;
        onBG = 1;
    }

    void Update()
    {
        if (fish.position.x >= x_NewestBG + (bg_size/2)) //once fish passes half point of current bg
        {
            if (onBG == 1) {
                x_NewestBG = x_NewestBG + bg_size;
                bg_2.position = new Vector2 (x_NewestBG,0);
                onBG = 2;
            } else if (onBG == 2){ 
                x_NewestBG = x_NewestBG + bg_size;
                bg_1.position = new Vector2 (x_NewestBG,0);
                onBG = 1;
            }
        }
    }



    /*
    public Transform fish; 
    public GameObject backgroundPrefab;
    
    public GameObject newestBackground;
    private GameObject lastBackground; 

    private float x_NewestBG;

    private float bg_size = 176f;
    //background prefab is 176 units long, first background is at -24
    // Start is called before the first frame update
    void Start()
    {
        x_NewestBG = -24;
    }

    // Update is called once per frame
    void Update()
    {
        if (fish.position.x >= x_NewestBG + (bg_size/2)) //once fish passes half point of current bg
        {
            //First, get rid of the background behind the current one since we are comfortably away from it.
            if (lastBackground != null) //if there was a background behind, delete it
            {
                Destroy(lastBackground);
            }

            //Then, declare current background as "last background" and generate the nextBackground;
            lastBackground = newestBackground; 
            
            x_NewestBG = x_NewestBG + bg_size;
            newestBackground = Instantiate(backgroundPrefab) as GameObject; 
            newestBackground.transform.position = new Vector2(x_NewestBG,0); 


        }
    }
    */
}
