using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TeamNameSetup : MonoBehaviour
{
    public TextMeshPro teamName;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public void SetupTeamName(string name)
    {
        teamName.SetText(name);
    }
}
