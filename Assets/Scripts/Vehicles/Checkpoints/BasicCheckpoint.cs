using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;
using Gamestate;

public class BasicCheckpoint : MonoBehaviour {
    public List<Transform> locations;
    public int checkpointCount = 50;
    private List<Transform> checkpointList = new List<Transform>();
    private int prev1, prev2, next;
    private int current;
    public PhotonView photonView;
    private GamestateTracker gamestateTracker;

    public struct LocationStruct {
        public List<int> indexList;
        public LocationStruct(List<int> listInt) {
            indexList = listInt;
        } 
    }

    public LocationStruct indexListStruct = new LocationStruct();

    void Start() {
        photonView = GetComponent<PhotonView>();
        indexListStruct.indexList = new List<int>();

        if (PhotonNetwork.IsMasterClient) { //if you are the host, generate checkpoints
            if (locations.Count < 3) {
                Debug.LogError("Less than three checkpoints");
            } else {
                prev1 = Random.Range(0, locations.Count - 1);
                indexListStruct.indexList.Add(prev1); //add one checkpoint
                next = prev1;
                prev2 = prev1;
                for (int i = 0; i < checkpointCount - 1; ++i) { // add sequential checkpoints
                    while (next == prev1 || next == prev2) { //next checkpoint cannot be either of the previous two
                        next = Random.Range(0, locations.Count);
                    }
                    indexListStruct.indexList.Add(next);
                    prev2 = prev1;
                    prev1 = next;
                }
            }
            string details = JsonUtility.ToJson(indexListStruct);
            photonView.RPC(nameof(ShareList_RPC), RpcTarget.All, details);
        } 
        

        current = 0;
    }

    [PunRPC]
    void ShareList_RPC(string intList) {
        indexListStruct.indexList = JsonUtility.FromJson<LocationStruct>(intList).indexList;
        foreach (int i in indexListStruct.indexList) { // turn index list into location list
            checkpointList.Add(locations[i]);
        }
        
    }

    private List<int> GetListFromHost() {
        throw new System.NotImplementedException();
    }

    public Vector3 NextCheckpoint(Vector3 t) {
        current++;
        if (current >= checkpointList.Count) current = 0;
        return checkpointList[current].position;


    }

    [PunRPC]
    public void UpdatePosition_RPC(float x, float y, float z, short gunnerId) {
        if (PhotonNetwork.LocalPlayer.ActorNumber == gunnerId) {
            transform.position = new Vector3(x, y, z);
        }
    }
}
