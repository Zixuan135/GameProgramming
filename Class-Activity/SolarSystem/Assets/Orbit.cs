using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Orbit : MonoBehaviour
{
    public Vector3 orbitSpeed = new Vector3(0, 20, 0);

    void Update()
    {
        transform.Rotate(orbitSpeed * Time.deltaTime);
    }
}