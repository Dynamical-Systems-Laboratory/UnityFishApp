using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Beta_access : MonoBehaviour
{

    public Text user_input; 
    public Text place_holder;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void check_input() {
        if (user_input.text == "DSL2021"){
            SceneManager.LoadScene("stage selection_v2");
        }
        else {
            user_input.text = "";
            place_holder.text = "Not found, please try again.";
        }
    }
}
