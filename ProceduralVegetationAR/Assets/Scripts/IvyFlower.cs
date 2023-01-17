using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IvyFlower : MonoBehaviour
{
    GameObject mIvyPrefab;
    int mNumberOfIvys;
    float mIvyRadius; 

    Ivy[] mGrownIvys;
    bool mHasBeenHit;

    public IvyFlowerGrowing mIvyFlowerGrowingScript; 

    private void Start()
    {
        mIvyPrefab = GameManager.SETTINGS.IVYFLOWERSETTINGS.IvyPrefab;
        int numberOfIvys = GameManager.SETTINGS.IVYFLOWERSETTINGS.NumberOfIvys;
        if (numberOfIvys <= 3)
        {
            mNumberOfIvys = numberOfIvys;
        }
        else
        {
            mNumberOfIvys = Random.Range(3, numberOfIvys);
        }     
        mIvyRadius = GameManager.SETTINGS.IVYSETTINGS.Radius; 
        mGrownIvys = new Ivy[mNumberOfIvys];
        mHasBeenHit = false;

        SpawnIvys();
    }

    public bool HasBeenHit()
    {
        if (!mHasBeenHit)
        {
            mHasBeenHit = true;

            mIvyFlowerGrowingScript.StartGrowing();

            for (int i = 0; i < mNumberOfIvys; i++)
            {
                mGrownIvys[i].StartGrowing();
            }

            return true; 
        }

        return false; 
    }

    void SpawnIvys()
    {
        float degrees = 0f;
        float degreeIncrements = 360f / (float)mNumberOfIvys;
         
        for (int i = 0; i < mNumberOfIvys; i++)
        {
            GameObject ivy = Instantiate(mIvyPrefab, transform.position + transform.up * mIvyRadius, Quaternion.identity, transform);
            mGrownIvys[i] = ivy.GetComponent<Ivy>();

            Quaternion rotation = Quaternion.Euler(0f, degrees, 0f);
            degrees += degreeIncrements;
            ivy.transform.localRotation = rotation;
        }
    }
}
