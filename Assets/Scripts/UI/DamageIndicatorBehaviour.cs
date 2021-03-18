using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DamageIndicatorBehaviour : MonoBehaviour
{
    HealthBehaviour healthBehaviour;
    public TextMeshProUGUI damageText;

    // Start is called before the first frame update
    void Start() {
        healthBehaviour = FindObjectOfType<HealthBehaviour>();
        damageText.text = healthBehaviour.damageTaken.ToString();
        if (healthBehaviour.damageTaken > 0) {
            damageText.color = new Color(0, 204, 0);
        } else {
            damageText.color = new Color(204, 0, 0);
        }
        Invoke(nameof(Die), 0.5f);
    }

    void Die() {
        Destroy(gameObject);
    }
}
