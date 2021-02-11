using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderUpdateText : MonoBehaviour
{
    public Slider observedSlider;

    public Text variableText;

    public float valueMultiplier = 1f;
    // Start is called before the first frame update
    
    void Start()
    {
        
    }

    public void UpdateSliderLabelText()
    {
        if (variableText != null && observedSlider != null)
        {
            variableText.text = (observedSlider.value * valueMultiplier).ToString();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
