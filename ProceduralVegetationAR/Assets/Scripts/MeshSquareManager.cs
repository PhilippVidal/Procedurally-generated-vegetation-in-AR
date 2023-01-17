using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshSquareManager : MonoBehaviour
{
    float mSize = 1f;
    List<MeshSquare> mMeshSquares;

    private void Start()
    {
        mMeshSquares = new List<MeshSquare>();
        StartCoroutine(MeshCombineCoroutine());
    }

    public MeshSquare FindResponsibleMeshSquare(Vector3 position)
    {
        for (int i = 0; i < mMeshSquares.Count; i++)
        {
            if (mMeshSquares[i].Contains(position))
            {
                return mMeshSquares[i];
            }           
        }
        MeshSquare meshSquare = AddNewMeshSquare(position);
        return meshSquare;
    }

    MeshSquare AddNewMeshSquare(Vector3 position)
    {
        

        float extentX;
        float extentZ;
        float x; 
        if (position.x > 0)
        {
            x = Mathf.Floor(position.x);
            extentX = mSize;
        }
        else
        {
            x = Mathf.Ceil(position.x);
            extentX = -mSize;
        }

        float z;
        if (position.z > 0)
        {
            z = Mathf.Floor(position.z);
            extentZ = mSize;
        }
        else
        {
            z = Mathf.Floor(position.z);
            extentZ = -mSize;
        }

        Vector3 nullPoint = new Vector3(x, 0f, z);

        int number = mMeshSquares.Count;

        GameObject obj = new GameObject("MeshSquareGameObject " + number);
        //obj.transform.parent = transform;
        obj.transform.parent = GameManager.INSTANCE.gameObject.transform;

        obj.transform.position = nullPoint;

        MeshSquare meshSquare = obj.AddComponent<MeshSquare>();
        meshSquare.Init(nullPoint, extentX, extentZ);

        mMeshSquares.Add(meshSquare);
        return meshSquare;
    }

    IEnumerator MeshCombineCoroutine()
    {
        while(true)
        {
            for (int i = 0; i < mMeshSquares.Count; i++)
            {
                mMeshSquares[i].CombineMeshs();
            }

            yield return new WaitForSeconds(5f);
        }
    }
}
