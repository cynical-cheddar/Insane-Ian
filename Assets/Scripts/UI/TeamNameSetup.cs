using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TeamNameSetup : MonoBehaviour
{
    public TextMeshPro teamName;
    public Color32 white = new Color32(255, 255, 255, 255);
    public Color32 red   = new Color32(255,   0,   0, 255);
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public void SetupTeamName(string name)
    {
        teamName.SetText(name);
        teamName.color = white;
    }

    public void ChangeColour(bool isPotato)
    {
        if(isPotato)
        {
            teamName.color = red;
        } else
        {
            teamName.color = white;
        }
    }
}
