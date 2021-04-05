using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class foodBar : MonoBehaviour
{
    // Start is called before the first frame update
    public Slider slider;
    public Gradient gradient;
    public Image fill;

    public void SetMaxFood(float food)
    {
        slider.maxValue = food;
        slider.value = food;

        fill.color = gradient.Evaluate(1f);
    }

    public void SetFood(float food)
    {
        if (food < slider.maxValue)
        {
            slider.value = food;
        } else
        {
            slider.value = slider.maxValue;
        }

        fill.color = gradient.Evaluate(slider.normalizedValue);
    }
}
