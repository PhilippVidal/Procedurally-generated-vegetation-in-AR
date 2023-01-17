using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyEffect : MonoBehaviour
{
    public float mLifetime; 
    void Start()
    {
        Destroy(gameObject, mLifetime);
    }
}
