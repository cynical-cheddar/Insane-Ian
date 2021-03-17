using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UiBar : MonoBehaviour
{
    public GameObject bar;

    public GameObject divider;
    public Text numberHolder;

   // List<GameObject> dividers = new List<GameObject>();
    // Start is called before the first frame update
    
    public void setProgressBar(float fraction){
        if(fraction<0) fraction = 0;
        bar.GetComponent<RectTransform>().localScale = new Vector3(1, (fraction), 1);
    }
    public void setnumber(string text){
        numberHolder.text = text;
    }
    /*
    public void setDivider(float fraction, float widthScale){
        GameObject dividerInstance = Instantiate(divider, transform.position, Quaternion.identity);
        dividerInstance.transform.SetParent(bar.transform.parent);
        float width = bar.transform.parent.GetComponent<RectTransform>().rect.height;
        float pos = -width/2 + fraction * width;
        Debug.Log(pos + "aa" + fraction);
        dividerInstance.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0,0,0);
        dividerInstance.GetComponent<RectTransform>().localPosition = new Vector3(0,pos,0);
        dividerInstance.GetComponent<RectTransform>().localScale = new Vector3(1.4f, widthScale, 1f);
        dividers.Add(dividerInstance);
    }*/
}
