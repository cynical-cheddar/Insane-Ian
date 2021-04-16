using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PotatoUi : MonoBehaviour
{
    public GameObject uiText;

    public List<Image> imageRenderers;
    public void SetText(bool set)
    {
        uiText.SetActive(set);

        foreach (Image i in imageRenderers)
        {
            i.enabled = set;
        }
    }
}
