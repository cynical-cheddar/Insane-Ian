using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Random = System.Random;

public class AnnouncerManager : MonoBehaviour
{
    private AudioSource announcerAudioSource;

    [Serializable]
    public struct AnnouncerShouts
    {
        public AnnouncerClipsToPlay potatoPickup;
    }

    [Serializable]
    public struct AnnouncerClipsToPlay
    {
        
        public List<AudioClip> localClips;
        public List<AudioClip> otherClips;
        public List<AudioClip> globalClips;
    }
    
    

    // works out which announcer clip to play, then sends rpcs to everyone else telling em to play the correct synced clip
    public void PlayAnnouncerLine(AnnouncerClipsToPlay clipSelection)
    {
        int myClipIndex = -1;
        int theirClipIndex = -1;
        int globalClipIndex = -1;

        if (clipSelection.localClips.Count > 0) myClipIndex = UnityEngine.Random.Range(0, clipSelection.localClips.Count - 1);
        if (clipSelection.otherClips.Count > 0) theirClipIndex = UnityEngine.Random.Range(0, clipSelection.otherClips.Count - 1);
        if (clipSelection.globalClips.Count > 0) globalClipIndex = UnityEngine.Random.Range(0, clipSelection.globalClips.Count - 1);
    }

    [PunRPC]
    void PlayClipFromSelection()
    {
        
    }
    
     
    
    void Start()
    {
        announcerAudioSource = GetComponent<AudioSource>();
    }


}
