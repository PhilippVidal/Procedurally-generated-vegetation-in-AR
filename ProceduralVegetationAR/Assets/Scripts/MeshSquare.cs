using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(MeshFilter))]
//[RequireComponent(typeof(MeshRenderer))]
public class MeshSquare : MonoBehaviour
{
    public Vector3 mNullPoint;
    public float mExtentX;
    public float mExtentZ;

    //public List<GameObject> mListAlphaOneSided;
    public List<GameObject> mListAlphaDoubleSided;

    //public Queue<MeshFilter> mMeshsReadyToCombineOneSided;
    public Queue<MeshFilter> mMeshsReadyToCombineDoubleSided;

    //GameObject mMeshHolderOneSided;
    GameObject mMeshHolderDoubleSided;

    //MeshFilter mOneSidedMeshFilter;
    MeshFilter mDoubleSidedMeshFilter;

    //MeshRenderer mOneSidedMeshRenderer;
    MeshRenderer mDoubleSidedMeshRenderer;

    //bool mMaterialSetOneSided;
    bool mMaterialSetDoubleSided;

    public void Init(Vector3 nullPoint, float extentX, float extentZ)
    {
        mNullPoint = nullPoint;
        mExtentX = extentX;
        mExtentZ = extentZ;
        //mListAlphaOneSided = new List<GameObject>();
        mListAlphaDoubleSided = new List<GameObject>();
        //mMeshsReadyToCombineOneSided = new Queue<MeshFilter>();
        mMeshsReadyToCombineDoubleSided = new Queue<MeshFilter>();

        //mMeshHolderOneSided = new GameObject("MeshHolderOneSided");
        mMeshHolderDoubleSided = new GameObject("mMeshHolderDoubleSided");
        mMeshHolderDoubleSided.transform.parent = GameManager.INSTANCE.gameObject.transform;

        //mOneSidedMeshFilter = mMeshHolderOneSided.AddComponent<MeshFilter>();
        mDoubleSidedMeshFilter = mMeshHolderDoubleSided.AddComponent<MeshFilter>();

        //mOneSidedMeshRenderer = mMeshHolderOneSided.AddComponent<MeshRenderer>();
        mDoubleSidedMeshRenderer = mMeshHolderDoubleSided.AddComponent<MeshRenderer>();

        //mMaterialSetOneSided = false;
        mMaterialSetDoubleSided = false;

        //mOneSidedMeshFilter.mesh = new Mesh();
        mDoubleSidedMeshFilter.mesh = new Mesh();
    }

    public bool Contains(Vector3 position)
    {
        if (ContainedInX(position.x) && ContainedInZ(position.z))
        {
            return true;
        }
        return false; 
    }

    public void AddToCombineList(MeshFilter meshFilter, int index)
    {
        if (index == 0)
        {
            //mMeshsReadyToCombineOneSided.Enqueue(meshFilter);
        }
        else
        {
            mMeshsReadyToCombineDoubleSided.Enqueue(meshFilter);
        }
    }

    public void AddToList(GameObject obj, int index)
    {
        if (index == 0)
        {
            //mListAlphaOneSided.Add(obj);
        }
        else
        {
            mListAlphaDoubleSided.Add(obj);
        }
    }

    bool ContainedInX(float x)
    {
        if (mExtentX > 0)
        {
            if (mNullPoint.x <= x && (mNullPoint.x + mExtentX) >= x)
            {
                return true;
            }
        }
        else
        {
            if (mNullPoint.x >= x && (mNullPoint.x + mExtentX) <= x)
            {
                return true;
            }
        }

        return false; 
    }

    bool ContainedInZ(float z)
    {
        if (mExtentZ > 0)
        {
            if (mNullPoint.z <= z && (mNullPoint.z + mExtentZ) >= z)
            {
                return true;
            }
        }
        else
        {
            if (mNullPoint.z >= z && (mNullPoint.z + mExtentZ) <= z)
            {
                return true;
            }
        }

        return false;
    }

    public void CombineMeshs()
    {
        /*if (!mMaterialSetOneSided)
        {
            if (mMeshsReadyToCombineOneSided.Count != 0)
            {
                mOneSidedMeshRenderer.sharedMaterial = mMeshsReadyToCombineOneSided.Peek().gameObject.GetComponent<MeshRenderer>().sharedMaterial;
                mMaterialSetOneSided = true;
            }
        }*/

        if (!mMaterialSetDoubleSided)
        {
            if (mMeshsReadyToCombineDoubleSided.Count != 0)
            {
                mDoubleSidedMeshRenderer.sharedMaterial = mMeshsReadyToCombineDoubleSided.Peek().gameObject.GetComponent<MeshRenderer>().sharedMaterial;
                mMaterialSetDoubleSided = true;
            }
        }

        /*if (mMeshsReadyToCombineOneSided.Count != 0)
        {
            List<MeshFilter> meshFilterList = new List<MeshFilter>();
            for (int i = 0; i < mMeshsReadyToCombineOneSided.Count; i++)
            {
                meshFilterList.Add(mMeshsReadyToCombineOneSided.Dequeue());
            }
            MeshCombiner.CombineMeshes(meshFilterList, mOneSidedMeshFilter);
        }*/

        if (mMeshsReadyToCombineDoubleSided.Count != 0)
        {
            List<MeshFilter> meshFilterList = new List<MeshFilter>();
            for (int i = 0; i < mMeshsReadyToCombineDoubleSided.Count; i++)
            {
                meshFilterList.Add(mMeshsReadyToCombineDoubleSided.Dequeue());
            }
            MeshCombiner.CombineMeshes(meshFilterList, mDoubleSidedMeshFilter);
        }
    }
}
