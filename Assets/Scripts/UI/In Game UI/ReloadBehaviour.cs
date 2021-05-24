using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReloadBehaviour : MonoBehaviour
{
    Image image;

    private void Start() {
        image = GetComponent<Image>();
    }

    // Only enable the reload icon for a given time
    public IEnumerator Reload(float reloadTime) {
        image.enabled = true;
        yield return new WaitForSecondsRealtime(reloadTime);
        image.enabled = false;
    }
}
