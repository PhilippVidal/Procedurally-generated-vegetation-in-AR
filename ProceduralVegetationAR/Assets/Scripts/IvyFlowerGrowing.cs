using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IvyFlowerGrowing : MonoBehaviour
{
    float mBlendA = 0f;
    float mBlendB = 0f;
    SkinnedMeshRenderer mSkinnedMeshRenderer; 
    void Start()
    {
        mSkinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
    }
    public void StartGrowing()
    {
        StartCoroutine(Grow());
    }

    IEnumerator Grow()
    {
        bool fullyGrown = false; 
        while(!fullyGrown)
        {
            if (mBlendA >= 100f)
            {
                if (mBlendB >= 100f)
                {
                    fullyGrown = true;
                }

                mSkinnedMeshRenderer.SetBlendShapeWeight(1, mBlendB++);
            }
            else
            {
                if (mBlendA >= 50f)
                {
                    mSkinnedMeshRenderer.SetBlendShapeWeight(1, mBlendB++);
                }

                mSkinnedMeshRenderer.SetBlendShapeWeight(0, mBlendA++);
            }

            if (!fullyGrown)
            {
                yield return new WaitForSeconds(0.015f);
            }        
        }
    }
}
