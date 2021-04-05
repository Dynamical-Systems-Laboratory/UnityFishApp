using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class select_stage : MonoBehaviour
{
    public Button startButton;
    public UnityEngine.UI.ToggleGroup pollGroup;
    public UnityEngine.UI.ToggleGroup flowGroup;
    public UnityEngine.UI.ToggleGroup tempGroup;

    // Start is called before the first frame update
    void Start()
    {
        Button bttn = startButton.GetComponent<Button>();
        bttn.onClick.AddListener(getDiffLevel);
    }

    public void getDiffLevel()
    {
        STATIC.pollLevel = pollGroup.ActiveToggles().FirstOrDefault().name;
        STATIC.flowLevel = flowGroup.ActiveToggles().FirstOrDefault().name;
        STATIC.tempLevel = tempGroup.ActiveToggles().FirstOrDefault().name;
        Debug.Log("Pollution: " + pollGroup.ActiveToggles().FirstOrDefault());
        Debug.Log("Flow: " + flowGroup.ActiveToggles().FirstOrDefault());
        Debug.Log("Temperature: " + tempGroup.ActiveToggles().FirstOrDefault());
        Debug.Log("STATIC POLL: " + STATIC.pollLevel);
        SceneManager.LoadScene("fish_in_water");
    }

    public void goTutorial()
    {
        SceneManager.LoadScene("tutorial");
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
