using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gamestate;
using TMPro;
using Photon.Pun;

public class UltBarBehaviour : MonoBehaviour
{
    public TextMeshProUGUI value;
    int previousValue = 0;
    public GameObject ultIndicator;
    public Transform ultIndicatorInstantiateTransform;
    public int change;

    void Update() {
        // Set the UI to the correct value, spawn an indicator if there is a change.
        if (previousValue != Mathf.CeilToInt(float.Parse(value.text))) {
            change = Mathf.CeilToInt(float.Parse(value.text)) - previousValue;
            previousValue = Mathf.CeilToInt(float.Parse(value.text));
            if (change > 0) {
                GameObject indicator = Instantiate(ultIndicator, ultIndicatorInstantiateTransform);
                indicator.GetComponentInChildren<TextMeshProUGUI>().text = change.ToString();
            }
        }
    }
}
