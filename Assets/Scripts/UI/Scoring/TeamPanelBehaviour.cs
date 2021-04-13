using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TeamPanelBehaviour : MonoBehaviour
{
    public TextMeshProUGUI TeamName;
    public TextMeshProUGUI TeamScore;
    public TextMeshProUGUI TeamKDA;
    public TextMeshProUGUI TeamCheckpoints;
    public int id;
    public GameObject parentPanel;
    public TeamPanelBehaviour parentTeamPanelBehaviour;
    public bool isFirstPanel = false;
    public bool isCurrentTeam = false;

    Vector3 initialPosition;
    public RectTransform rectTransform;

    private void Start() {
        Debug.Log(" scoreboard team panel start called");
        if (isFirstPanel) initialPosition = new Vector3(90f, 20f, 0f);
        else initialPosition = transform.localPosition;
        UpdateTransform(isCurrentTeam);
        Debug.Log(" scoreboard team panel start done");
    }

    public void UpdateTransform(bool isCurrentTeam) {
        Debug.Log(" scoreboard team panel update transform called");
        Debug.Log(" parentPanel: " + parentPanel);
        Debug.Log(" parentPanel.transform: " + parentPanel.transform);
        Debug.Log(" parentTeamPanelBehaviour: " + parentTeamPanelBehaviour);
        this.isCurrentTeam = isCurrentTeam;
        if (isFirstPanel) {
            if (isCurrentTeam) {
                transform.localScale = new Vector3(1f, 1f, 1f);
                transform.localPosition = initialPosition + new Vector3(0f, 0f, 0f);
            } else {
                transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
                transform.localPosition = initialPosition + new Vector3(90f, 20f, 0f);
            }
        } else {
            if (isCurrentTeam) {
                transform.localScale = new Vector3(1f, 1f, 1f);
                transform.localPosition = parentPanel.transform.localPosition + new Vector3(-90f, -110f, 0f);
            } else if (parentTeamPanelBehaviour.isCurrentTeam) {
                transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
                transform.localPosition = parentPanel.transform.localPosition + new Vector3(90f, -110f, 0f);
            } else {
                transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
                transform.localPosition = parentPanel.transform.localPosition + new Vector3(0f, -90f, 0f);
            }
        }
        Debug.Log(" scoreboard team panel update transform dome");
    }

}
