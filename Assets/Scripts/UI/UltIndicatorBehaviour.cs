using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UltIndicatorBehaviour : MonoBehaviour
{
    public Image image;
    public TextMeshProUGUI text;

    // Start is called before the first frame update
    void Start() {
        text.color = new Color(image.color.r, image.color.g, image.color.b);
        Invoke(nameof(Die), 0.8f);
    }

    void Die() {
        Destroy(gameObject);
    }
}
