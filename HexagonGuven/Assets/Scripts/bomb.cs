using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bomb : MonoBehaviour
{
    private uint count = 9;
    TextMesh countText;

    private void Start()
    {
        countText = transform.GetChild(0).GetComponent<TextMesh>();
        countText.text = count.ToString();
    }
    private void Update()
    {
        //bomb always looks up
        //even in rotation animations
        transform.up = Vector3.up;
    }
    public uint getCount()
    {
        return count;
    }
    //Decrease count and update on the screen
    public void decreaseCount()
    {
        count--;
        if(countText != null)
            countText.text = count.ToString();
    }
}
