using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IvyLeaf : MonoBehaviour
{
    public Ivy mIvy;

    Vector3 mMaxScale;

    float mTimeInterval;
    float mTimeToScale;
    float mScale; 

    private void Awake()
    {
        mScale = transform.localScale.x;      
    }
    private void Start()
    {    
        transform.localScale = Vector3.zero;
    }
    public void StartGrowing(float timeInterval, float timeToScale, Ivy ivy)
    {
        mMaxScale = new Vector3();
        mMaxScale.x = mScale;
        mMaxScale.y = mScale;
        mMaxScale.z = mScale;

        mIvy = ivy;
        mTimeInterval = timeInterval;
        mTimeToScale = timeToScale;

        StartCoroutine(GrowIvyLeaf());
    }

    IEnumerator GrowIvyLeaf()
    {
        Vector3 scale = Vector3.zero;
        bool fullyGrown = false;
        gameObject.transform.localScale = scale;
        float increase = (mTimeInterval / mTimeToScale);

        while (!fullyGrown)
        {       
            if (scale.x < mMaxScale.x)
            {
                scale.x += increase * mMaxScale.x;
            }
            if (scale.y < mMaxScale.y)
            {
                scale.y += increase * mMaxScale.y;
            }
            if (scale.z < mMaxScale.z)
            {
                scale.z += increase * mMaxScale.z;
            }

            gameObject.transform.localScale = scale;


            if (scale.x >= mMaxScale.x && scale.y >= mMaxScale.y && scale.z >= mMaxScale.z)
            {
                gameObject.transform.localScale = mMaxScale;
                fullyGrown = true;
            }

            yield return new WaitForSeconds(mTimeInterval);
        }

        mIvy.AddFullyGrownLeaf(gameObject);       
    }  
}
