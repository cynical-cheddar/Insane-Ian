using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectedVehiclesMonitor : MonoBehaviour
{
    // Start is called before the first frame update
    
    private GamestateTrackerUtils gamestateTrackerUtils;


    private List<short> humanTeamsIds;
    private List<short> selectedHumanTeamsIds;

    public bool outputText = true;
    public TextMeshProUGUI text;
    
    void Start()
    {
        gamestateTrackerUtils = FindObjectOfType<GamestateTrackerUtils>();
    }

    // Update is called once per frame
    void Update()
    {
        humanTeamsIds = gamestateTrackerUtils.GetHumanTeamsIds();
        selectedHumanTeamsIds = gamestateTrackerUtils.GetSelectedHumanTeamsIds();
        
        text.text = selectedHumanTeamsIds.Count.ToString() + "/" + humanTeamsIds.Count.ToString();
    }
}
