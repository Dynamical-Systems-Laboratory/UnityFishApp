using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class select_stage_v2 : MonoBehaviour
{
    public Button startButton;
    public Slider pollSlider;
    public Slider tempSlider;
    public Slider flowSlider;

    public Text pollText;
    public Text tempText;
    public Text flowText;

    private string[] pollTitles;
    private string[] tempTitles;
    private string[] flowTitles;


    public void getDiffLevel()
    {
        Manager.poll = (int)pollSlider.value;
        Manager.temp = (int)tempSlider.value;
        Manager.flow = (int)flowSlider.value;

        Debug.Log("STATIC POLL: " + Manager.poll);
        Debug.Log("STATIC TEMP: " + Manager.temp);
        Debug.Log("STATIC FLOW: " + Manager.flow);

        SceneManager.LoadScene("fish_in_water");
    }

    public void goTutorial()
    {
        SceneManager.LoadScene("tutorial");
    }

    public void quitApp()
    {
        Application.Quit();
    }


    void Awake()
    {
        pollTitles = new string[] {"Pollution Level:<color=green>Low</color>",
        "Pollution Level:<color=red>High</color>"};
        tempTitles = new string[] {"Temp Level:<color=green>Normal</color>",
        "Temp Level:<color=red>High</color>"};
        flowTitles = new string[] {"Flow Level:<color=green>Low</color>",
        "Flow Level:<color=red>High</color>"};

        
        if (! PlayerPrefs.HasKey("FIRSTTIMEOPENING"))
        {
            Debug.Log("First Time Opening");

            //Set first time opening to false
            PlayerPrefs.SetInt("FIRSTTIMEOPENING", 1);

            //Do your stuff here
            goTutorial();
        }
    }
    void Start()
    {
        Button bttn = startButton.GetComponent<Button>();
        bttn.onClick.AddListener(getDiffLevel);
        pollSlider.value = Manager.poll;
        tempSlider.value = Manager.temp;
        flowSlider.value = Manager.flow;
    }


    void Update()
    {
        pollText.text = pollTitles[(int)pollSlider.value];
        tempText.text = tempTitles[(int)tempSlider.value];
        flowText.text = flowTitles[(int)flowSlider.value];
    }



}
