using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class circleBehaviour : MonoBehaviour
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

    public static float NextGaussian2(float mean, float standard_deviation) {
        return mean + NextGaussian() * standard_deviation;
    }

    public static float NextGaussian3 (float meanb, float standard_deviationb, float min, float max) {
        float x;
        do {
            x = NextGaussian2(meanb, standard_deviationb);
        } while (x < min || x > max);
        return x;
    }

    //variables for calculating next position
    public float xo;
    public float yo;
    //public float V=20.0f;
    public float theta0 = 0.0f;

    //private float xk_1;
    //private float yk_1;
    private float thetak_1;

    private float randomAngle; 
    //private float dt = 0.033f;

    //variables for movement
    private float latestDirectionChangeTime;
    private readonly float directionChangeTime = 0.5f;
    private float objectVelocity = 2f;
    private Vector2 movementDirection;
    private Vector2 movementPerSecond;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector2(0,0);
        latestDirectionChangeTime = 0f;
        calculateNewMovementVector();
    }

    // Update is called once per frame

    void calculateNewMovementVector(){
        randomAngle = NextGaussian3(0.0f,1.0f,-20.0f,20.0f);
        movementDirection = new Vector2(Mathf.Cos(theta0), Mathf.Sin(theta0)).normalized;
        movementPerSecond = movementDirection * objectVelocity;

        thetak_1 = theta0  -3*theta0*directionChangeTime + 4*Mathf.Sqrt(directionChangeTime)*directionChangeTime;
         
        theta0 = thetak_1;
        print(movementPerSecond);
    }
    void Update()
    {
     //if the changeTime was reached, calculate a new movement vector
     if (Time.time - latestDirectionChangeTime > directionChangeTime){
         latestDirectionChangeTime = Time.time;
         calculateNewMovementVector();
     }
     
     //move enemy: 
     transform.position = new Vector2(transform.position.x + (movementPerSecond.x * Time.deltaTime), 
     transform.position.y + (movementPerSecond.y * Time.deltaTime));
    }
}
