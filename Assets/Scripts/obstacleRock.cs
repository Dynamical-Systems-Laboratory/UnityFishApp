using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class obstacleRock : MonoBehaviour
{
    public float x_limit; //the coordinates of left most point in the main camera
    // Start is called before the first frame update
    void Start()
    {
        x_limit = Camera.main.transform.position.x - (Camera.main.orthographicSize * Screen.width / Screen.height);

    }

    // Update is called once per frame
    void Update()
    {
        if (GameTime.isPaused){return;}
        x_limit = Camera.main.transform.position.x - (Camera.main.orthographicSize * Screen.width / Screen.height);
        if (transform.position.x < x_limit - 5f)
        {
            Destroy(this.gameObject); 
        }
    }
}
