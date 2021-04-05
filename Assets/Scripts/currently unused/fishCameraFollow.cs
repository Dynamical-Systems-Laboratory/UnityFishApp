using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//the camera rn is 
public class fishCameraFollow : MonoBehaviour
{
    public Transform target; //drag the desired object to this bar in unity, the camera will follow this object
    public float smoothTime = 0.3f;
    public float posY;
    public float minX = 0;
    public float maxX = 40;

    public float horizontalSize; //how big is the camera horizontally
    public float verticleSize; // how big is the camera vertically
    private Vector3 velocity = Vector3.zero;

    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        
       //Get the updated size of the camera (in case user changes screen size during play) 
        //verticleSize = Camera.main.orthographicSize;
        //horizontalSize = verticleSize * Screen.width / Screen.height;
        /*
        Vector3 targetPosition = target.TransformPoint(new Vector3(0, posY, -10));
        Vector3 desiredPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        transform.position = new Vector3(Mathf.Clamp(desiredPosition.x, minX, maxX), desiredPosition.y, desiredPosition.z);
        */
        /*
        //camera is more than 8 behind fish, move camera forward to exact 8 behind fish.
        if (target.position.x - transform.position.x > -8f)
        {
            transform.position = new Vector3(target.position.x + 8f, 0, -10);
        }*/

        /*
        //fish is on the left edge of the camera, keep fish on the left edge if fish keeps going left.

        if (transform.position.x - target.position.x > (horizontalSize-1f))
        {
            transform.position = new Vector3(target.position.x + (horizontalSize-1f), 0, -10);
        }
        //transform.position = new Vector3 (target.position.x, 0, -10);
        */
        
    }
}
