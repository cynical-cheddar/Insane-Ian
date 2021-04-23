using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Random = System.Random;
using Gamestate;

public class AnnouncerManager : MonoBehaviour
{

    public enum AnnouncerShoutsTags{potatoPickup, potatoDrop, onKilled, suicide, matchStart, threeMinutes, twoMinutes, oneMinute, thirtySeconds, tenSecondCountdown, matchEnd};
    private AudioSource announcerAudioSource;

    [Serializable]
    public struct AnnouncerShouts
    {
        public AnnouncerClipsToPlay potatoPickup;
        public AnnouncerClipsToPlay potatoDrop;

        public AnnouncerClipsToPlay onKilled;
        public AnnouncerClipsToPlay suicide;

        public AnnouncerClipsToPlay matchStart;

        public AnnouncerClipsToPlay threeMinutes;

        public AnnouncerClipsToPlay twoMinutes;

        public AnnouncerClipsToPlay oneMinute;

        public AnnouncerClipsToPlay thirtySeconds;

        public AnnouncerClipsToPlay tenSecondCountdown;

        public AnnouncerClipsToPlay matchEnd;
    }

    [Serializable]
    public struct AnnouncerClipsToPlay
    {
        public AnnouncerShoutsTags announcerShoutsTag;
        public List<AudioClip> localClips;
        public List<AudioClip> otherClips;
        public List<AudioClip> globalClips;
    }
    
    public AnnouncerShouts announcerShouts;


    Queue<AudioClip> clipQueue;
    
    void Update()
    {
        /*
        if (announcerAudioSource.isPlaying == false && clipQueue.Count > 0) {
            announcerAudioSource.clip = clipQueue.Dequeue();
            announcerAudioSource.Play();
        }*/
    }
        

    AnnouncerClipsToPlay GetAnnouncerClipsToPlay(AnnouncerShoutsTags shoutTag){
        if(announcerShouts.potatoPickup.announcerShoutsTag == shoutTag) return announcerShouts.potatoPickup;
        if(announcerShouts.potatoDrop.announcerShoutsTag == shoutTag) return announcerShouts.potatoDrop;
        if(announcerShouts.matchStart.announcerShoutsTag == shoutTag) return announcerShouts.matchStart;
        if(announcerShouts.onKilled.announcerShoutsTag == shoutTag) return announcerShouts.onKilled;
        if(announcerShouts.suicide.announcerShoutsTag == shoutTag) return announcerShouts.suicide;
        if(announcerShouts.threeMinutes.announcerShoutsTag == shoutTag) return announcerShouts.threeMinutes;
        if(announcerShouts.twoMinutes.announcerShoutsTag == shoutTag) return announcerShouts.twoMinutes;
        if(announcerShouts.oneMinute.announcerShoutsTag == shoutTag) return announcerShouts.oneMinute;
        if(announcerShouts.thirtySeconds.announcerShoutsTag == shoutTag) return announcerShouts.thirtySeconds;
        if(announcerShouts.tenSecondCountdown.announcerShoutsTag == shoutTag) return announcerShouts.tenSecondCountdown;
        if(announcerShouts.matchEnd.announcerShoutsTag == shoutTag) return announcerShouts.matchEnd;
        return announcerShouts.potatoPickup;
    }



    AnnouncerShoutsTags GetShoutTag(AnnouncerClipsToPlay clipSelection){
        return clipSelection.announcerShoutsTag;
    }

    // works out which announcer clip to play, then sends rpcs to everyone else telling em to play the correct synced clip
    public void PlayAnnouncerLine(AnnouncerClipsToPlay clipSelection, int driverId, int gunnerId)
    {

        AnnouncerShoutsTags shoutTag = GetShoutTag(clipSelection);

        int myClipIndex = -1;
        int theirClipIndex = -1;
        int globalClipIndex = -1;

        if (clipSelection.localClips.Count > 0) myClipIndex = UnityEngine.Random.Range(0, clipSelection.localClips.Count);
        if (clipSelection.otherClips.Count > 0) theirClipIndex = UnityEngine.Random.Range(0, clipSelection.otherClips.Count);
        if (clipSelection.globalClips.Count > 0) globalClipIndex = UnityEngine.Random.Range(0, clipSelection.globalClips.Count);

        bool playGlobal = false;
        if(myClipIndex < 0 || theirClipIndex < 0) playGlobal = true;

        GetComponent<PhotonView>().RPC(nameof(PlayClipFromSelection), RpcTarget.All, myClipIndex, theirClipIndex, globalClipIndex, (int)shoutTag, driverId, gunnerId, playGlobal);
    }

    
    public void PlayAnnouncerLine(AnnouncerClipsToPlay clipSelection)
    {

        AnnouncerShoutsTags shoutTag = GetShoutTag(clipSelection);

        int myClipIndex = -1;
        int theirClipIndex = -1;
        int globalClipIndex = -1;

        if (clipSelection.localClips.Count > 0) myClipIndex = UnityEngine.Random.Range(0, clipSelection.localClips.Count);
        if (clipSelection.otherClips.Count > 0) theirClipIndex = UnityEngine.Random.Range(0, clipSelection.otherClips.Count);
        if (clipSelection.globalClips.Count > 0) globalClipIndex = UnityEngine.Random.Range(0, clipSelection.globalClips.Count);

        bool playGlobal = false;
        if(myClipIndex < 0 || theirClipIndex < 0) playGlobal = true;

        GetComponent<PhotonView>().RPC(nameof(PlayClipFromSelection), RpcTarget.All, myClipIndex, theirClipIndex, globalClipIndex, (int)shoutTag, 0, 0, playGlobal);
    }




    [PunRPC]
    void PlayClipFromSelection(int myClipIndex, int theirClipIndex, int globalClipIndex, int shoutTagInt, int driverId, int gunnerId, bool playGlobal)
    {
        AnnouncerClipsToPlay clipsToPlay = GetAnnouncerClipsToPlay( (AnnouncerShoutsTags)  shoutTagInt);

        // we got clips to play

        if(!playGlobal){

            if((PhotonNetwork.LocalPlayer.ActorNumber == driverId || PhotonNetwork.LocalPlayer.ActorNumber == gunnerId)){
               // PlaySound(clipsToPlay.localClips[myClipIndex]);
               announcerAudioSource.PlayOneShot(clipsToPlay.localClips[myClipIndex]);
            }
            else{
              //  PlaySound(clipsToPlay.otherClips[theirClipIndex]);
              announcerAudioSource.PlayOneShot(clipsToPlay.otherClips[theirClipIndex]);
            }
        }
        else{
           // PlaySound(clipsToPlay.globalClips[globalClipIndex]);
           announcerAudioSource.PlayOneShot(clipsToPlay.globalClips[globalClipIndex]);
        }
    }
    
     
    
    void Start()
    {
        announcerAudioSource = GetComponent<AudioSource>();
    }
    /*
    public void PlaySound(AudioClip clip)
        {
            clipQueue.Enqueue(clip);
        }*/


}
