using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DamageIndicatorBehaviour : MonoBehaviour
{
    HealthBehaviour healthBehaviour;
    public Image image;
    public TextMeshProUGUI damageText;
    public Sprite damageSprite;
    public Sprite healSprite;

    // Start is called before the first frame update
    void Start() {
        healthBehaviour = FindObjectOfType<HealthBehaviour>();
        damageText.text = healthBehaviour.damageTaken.ToString();
        if (healthBehaviour.damageTaken > 0) {
            image.sprite = healSprite;
            image.color = new Color(0, 154, 0, 0.3f);
            damageText.color = new Color(0, 204, 0);
        } else {
            image.sprite = damageSprite;
            image.color = new Color(154, 0, 0, 0.3f);
            damageText.color = new Color(204, 0, 0);
        }
        Invoke(nameof(Die), 0.8f);
    }

    void Die() {
        Destroy(gameObject);
    }
}
