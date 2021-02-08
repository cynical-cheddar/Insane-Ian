using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class OnlyInteractableIfMaster : MonoBehaviour
{
    
    // Start is called before the first frame update
    void Start()
    {
        if(!PhotonNetwork.IsMasterClient)GetComponent<Button>().interactable = false;
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
