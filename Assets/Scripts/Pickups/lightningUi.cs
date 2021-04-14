using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class lightningUi : MonoBehaviour
{
    // Start is called before the first frame update
    public List<Sprite> sprites;
    public Image image;
    public float delay = 0.1f;


    private void Start()
    {
        StartCoroutine(SpriteSwap());
    }

    // Update is called once per frame
    IEnumerator SpriteSwap()
    {
        int i = 0;
        while (true)
        {
            image.sprite = sprites[i];
            i++;
            if (i > sprites.Count-1) i = 0;
            yield return new WaitForSecondsRealtime(delay);
        }
    }
}
