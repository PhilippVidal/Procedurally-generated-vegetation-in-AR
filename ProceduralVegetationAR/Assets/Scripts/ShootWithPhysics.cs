using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootWithPhysics : MonoBehaviour
{
    public GameObject mProjectilePrefab;
    public float mForceForward;
    public float mForceUpward;

    public void Shoot(Vector3 direction)
    {
        Transform cameraTransform = Camera.main.transform;
        Vector3 pos = cameraTransform.position;
        Vector3 dir = direction.normalized;

        GameObject projectile = Instantiate(mProjectilePrefab, pos + dir * 0.2f, Quaternion.identity);
        Rigidbody rigidbody = projectile.GetComponent<Rigidbody>();      
        rigidbody.AddForce(dir * mForceForward, ForceMode.Impulse);
        rigidbody.AddForce(cameraTransform.up * mForceUpward, ForceMode.Impulse);
    }
}
