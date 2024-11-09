using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpin : MonoBehaviour
{
    [SerializeField] private Vector3 rotateDirection;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(rotateDirection.x * Time.deltaTime, rotateDirection.y * Time.deltaTime, rotateDirection.z * Time.deltaTime, Space.Self);
    }
}
