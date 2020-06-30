using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class destroyAfter : MonoBehaviour
{
    //Particle effects will die after 3 seconds
    void Start()
    {
        StartCoroutine(destroyAfterSeconds(3));
    }

    IEnumerator destroyAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Destroy(gameObject);
    }
}
