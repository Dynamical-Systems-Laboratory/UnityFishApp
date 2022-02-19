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

    public Text pollDescription;
    public Text tempDescription;
    public Text flowDescription;

    private string[] pollTitles;
    private string[] pollDescriptions;
    private string[] tempTitles;
    private string[] tempDescriptions;
    private string[] flowTitles;
    private string[] flowDescriptions;
    


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
        pollTitles = new string[] {"Pollution Level:<color=blue>Low</color>",
        "Pollution Level:<color=red>High</color>"};
        pollDescriptions = new string[] {"The fish swims in relatively clean water. Fish swimming requires less effort; therefore, its fitness will decrease slower and it will avoid predators more easily",
        "The fish swims in heavily polluted water. Fitness will drop faster because the fish needs to detoxify. Its movements are heavily restricted and slowed down, making it difficult to avoid predators. Additionally, eggs will be less likely to hatch due to non-ideal water conditions."
        };

        tempTitles = new string[] {"Temp Level:<color=blue>Normal</color>",
        "Temp Level:<color=red>High</color>"};
        tempDescriptions = new string[] {"The fish swims at a normal water temperature. Its fitness will decrease slower.",
        "The fish swims in a higher than optimal temperature. This can cause frenetic swimming and increase anxiety. Fitness will decrease faster and eggs will be less likely to hatch."
        };

        flowTitles = new string[] {"Flow Level:<color=blue>Normal</color>",
        "Flow Level:<color=red>Altered</color>"};
        flowDescriptions = new string[] {"The river has a normal water flow speed.",
        "The river has a higher than normal water flow speed. The fish will need to expend more energy to resist the flow. Its fitness will decrease faster. The increased flow speed also makes it more difficult to maneuver and avoid predators. Eggs will have a lower chance of survival due to the high flow speed."
        };

        
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
        pollDescription.text = pollDescriptions[(int)pollSlider.value];

        tempText.text = tempTitles[(int)tempSlider.value];
        tempDescription.text = tempDescriptions[(int)tempSlider.value];

        flowText.text = flowTitles[(int)flowSlider.value];
        flowDescription.text = flowDescriptions[(int)flowSlider.value];


    }



}
