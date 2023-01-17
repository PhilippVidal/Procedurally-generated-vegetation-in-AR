using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterBalloon : MonoBehaviour
{
    Vector3 mRandomRotation; 
    bool mHasSendData = false;
    float mLifeTime = 2f;
    public GameObject mWaterSplashEffect;

    private void Start()
    {
        float value = 360f;
        mRandomRotation = new Vector3(Random.Range(0, value), 0f, Random.Range(0, value * 0.3f));

        Destroy(gameObject, mLifeTime);
    }

    private void Update()
    {      
        Vector3 rot = mRandomRotation * Time.deltaTime * 5f;
        transform.rotation *= Quaternion.Euler(rot.x, rot.y, rot.z);
    }
    private void OnCollisionEnter(Collision collision)
    {
        Vector3 position = collision.GetContact(0).point;
        Vector3 normal = collision.GetContact(0).normal;

        if (!mHasSendData)
        {
            IvyFlower ivyFlower; 
            if (collision.gameObject.TryGetComponent<IvyFlower>(out ivyFlower))
            {
                ivyFlower.HasBeenHit();
            }
            else
            {
                SendHitPoint(position, normal);
            }
           
        }

        //Spawn Water Splash Effect 
        GameObject obj = Instantiate(mWaterSplashEffect, position, Quaternion.identity);
        obj.transform.up = normal;

        Destroy(transform.gameObject);
    }

    void SendHitPoint(Vector3 position, Vector3 normal)
    {
        GameManager.OBJECTPLACER.SpawnObjectsAtHit(position, normal);
        mHasSendData = true; 
    }
}
