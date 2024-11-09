using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedDestroy : MonoBehaviour
{
    [SerializeField] private float destroyTime = 10f;

    void Start()
    {
        Destroy(gameObject, destroyTime);
    }
}
