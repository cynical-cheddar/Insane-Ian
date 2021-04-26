using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class StartGameCountdownTimer : MonoBehaviour
{

    public Animation prepare;
    public Animation three;
    public Animation two;
    public Animation one;
    public Animation fight;

    AnnouncerManager announcerManager;
    void Start()
    {
        announcerManager = FindObjectOfType<AnnouncerManager>();
        if(PhotonNetwork.IsMasterClient){
            announcerManager.PlayAnnouncerLine(announcerManager.announcerShouts.matchStart);
        }
        prepare.Play();
        Invoke(nameof(playThree), 1f);
        Invoke(nameof(playTwo), 2f);
        Invoke(nameof(playOne), 3f);
        Invoke(nameof(playFight), 4f);
        Destroy( gameObject,5f);
    }

    void playThree()
    {
        three.Play();
    }
    void playTwo()
    {
        two.Play();
    }
    void playOne()
    {
        one.Play();
    }
    void playFight()
    {
        fight.Play();
    }
}
