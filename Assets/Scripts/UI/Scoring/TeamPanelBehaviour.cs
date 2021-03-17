using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamPanelBehaviour : MonoBehaviour
{
    public Text TeamName;
    public Text TeamScore;
    public Text TeamKDA;
    public Image Position;
    public Image PositionShadow;
    public Image Glow;
    public int id;
    public GameObject parentPanel;
    public bool isFirstPanel = false;
    public bool isCurrentTeam = false;

    Vector3 initialPosition;
    public RectTransform rectTransform;

    private void Start() {
        initialPosition = transform.localPosition;
        UpdateTransform(false);
    }

    public void UpdateTransform(bool isCurrentTeam) {
        this.isCurrentTeam = isCurrentTeam;
        if (isFirstPanel) {
            if (isCurrentTeam) {
                transform.localScale = new Vector3(1f, 1f, 1f);
                transform.localPosition = initialPosition;
            } else {
                transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
                transform.localPosition = initialPosition + new Vector3(70f, 20f, 0f);
            }
        } else {
            if (isCurrentTeam) {
                transform.localScale = new Vector3(1f, 1f, 1f);
                transform.localPosition = parentPanel.transform.localPosition + new Vector3(-70f, -120f, 0f);
            } else if () {
                transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
                transform.localPosition = parentPanel.transform.localPosition + new Vector3(70f, -120f, 0f);
            } else {
                transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
                transform.localPosition = parentPanel.transform.localPosition + new Vector3(0f, -110f, 0f);
            }
        }
    }

}
