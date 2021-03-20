using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UiBar : MonoBehaviour
{
    public GameObject bar;
    public TextMeshProUGUI numberHolder;
    public GameObject specialValueEffect;
    public float specialValueLimit = 100f;
    public bool specialValueShouldBeGreaterThanOrEqualToLimit = true;

    // List<GameObject> dividers = new List<GameObject>();
    // Start is called before the first frame update

    private void Update() {
        if (specialValueEffect != null) {
            if (specialValueShouldBeGreaterThanOrEqualToLimit) {
                if (float.Parse(numberHolder.text) >= specialValueLimit) specialValueEffect.SetActive(true);
                else specialValueEffect.SetActive(false);
            } else {
                if (float.Parse(numberHolder.text) <= specialValueLimit) specialValueEffect.SetActive(true);
                else specialValueEffect.SetActive(false);
            }

        }
    }

    public void SetProgressBar(float fraction){
        if (fraction < 0) fraction = 0;
        bar.GetComponent<RectTransform>().localScale = new Vector3(1, fraction, 1);
    }
    public void SetNumber(string text){
        numberHolder.text = text;
    }
}
