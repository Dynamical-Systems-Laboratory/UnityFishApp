using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class timerController : MonoBehaviour
{
    public static timerController instance; //for singleton 

    public Text timeCounter;
    public Text timeCounter_final;

    private TimeSpan timePlaying;
    private bool timerGoing;

    private float elapsedTime;
    // Start is called before the first frame update
    void Awake()
    {
        instance = this; //singleton setup
        timeCounter.text = "Time: 00:00";
        timerGoing = false;
    }

    void Start()
    {

    }

    public void BeginTimer()
    {
        timerGoing = true;
        elapsedTime = 0f;

        StartCoroutine(UpdateTimer());
    }

    public void EndTimer()
    {
        timerGoing = false;
        string timePlayingStr_ = ("Time Survived: " + timePlaying.ToString("mm':'ss"));
        timeCounter_final.text = timePlayingStr_;
    }

    private IEnumerator UpdateTimer()
    {
        while (timerGoing){
            
            //Debug.Log("Am in coroutine");
            
            elapsedTime += GameTime.deltaTime;
            timePlaying = TimeSpan.FromSeconds(elapsedTime);
            string timePlayingStr = ("Time: " + timePlaying.ToString("mm':'ss"));
            timeCounter.text = timePlayingStr;

            yield return null; 
        }
    }


}
