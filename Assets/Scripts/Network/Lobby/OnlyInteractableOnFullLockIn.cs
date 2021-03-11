using System.Collections;
using System.Collections.Generic;
using Gamestate;
using UnityEngine;
using UnityEngine.UI;
public class OnlyInteractableOnFullLockIn : MonoBehaviour
{
    private GamestateTrackerUtils gamestateTrackerUtils;

    private Button btn;
    // Start is called before the first frame update
    void Start()
    {
        gamestateTrackerUtils = FindObjectOfType<GamestateTrackerUtils>();
        btn = GetComponent<Button>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gamestateTrackerUtils.AllPlayerTeamsHaveVehicles()) btn.interactable = true;
        else btn.interactable = false;
    }
}
