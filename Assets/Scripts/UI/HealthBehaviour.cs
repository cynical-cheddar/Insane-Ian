using UnityEngine;
using UnityEngine.UI;

public class HealthBehaviour : MonoBehaviour {

    public Text healthLabel;

    // Start is called before the first frame update
    void Start() {

    }

    public void SetHealth(float amount) {
        healthLabel.text = Mathf.RoundToInt(amount).ToString();
    }
}
