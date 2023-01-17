using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshFilter))]
public class GrowPlant : MonoBehaviour
{
    enum GrowthAnimationType
    {
        Flower, 
        Grass,
        BigPlant
    }

    Vector3 mMaxScale;

    [SerializeField] GrowthAnimationType animType;
    [SerializeField] float timeInterval = 0.05f;
    [SerializeField] float timeToScale = 2f;

    MeshSquare mResponsibleMeshSquare;
    bool mIsFullyGrown;
    MeshFilter mMeshFilter; 

    private void Awake()
    {
        mMeshFilter = GetComponent<MeshFilter>();
    }
    private void Start()
    {
        mIsFullyGrown = false;
        mResponsibleMeshSquare = GameManager.MESHSQUAREMANAGER.FindResponsibleMeshSquare(transform.position);
        GameObject testobj = new GameObject();
        testobj.transform.parent = transform;
        testobj.transform.position = mResponsibleMeshSquare.mNullPoint + new Vector3(0.5f * mResponsibleMeshSquare.mExtentX, 0f, 0.5f * mResponsibleMeshSquare.mExtentZ);
        mMaxScale = transform.localScale;
        StartGrowing(animType.ToString());
    }
    public void StartGrowing(string name)
    {
        if (name != string.Empty)
        {
            StartCoroutine("Grow" + name);
        }     
    }

    IEnumerator GrowGrass()
    {
        mMaxScale.y *= Random.Range(0.5f, 1f);

        Vector3 scale = mMaxScale;
        scale.y = 0f;

        bool fullyGrown = false;
        gameObject.transform.localScale = scale;
        float increase = (timeInterval / timeToScale);
        while (!fullyGrown) //Hier startet die Coroutine wieder
        {
            if (scale.y < mMaxScale.y)
            {
                scale.y += increase * mMaxScale.y;
            }

            if (scale.x >= mMaxScale.x && scale.y >= mMaxScale.y && scale.z >= mMaxScale.z)
            {
                scale = mMaxScale;
                fullyGrown = true;  //Beende die While-Schleife und 
                                    //somit das Pausieren und Weiterausführen der Coroutine
            }

            gameObject.transform.localScale = scale;
            //Warte für eine bestimme Anzahl an Sekunden (z.B. 0.05)
            yield return new WaitForSeconds(timeInterval);  //Coroutine "pausiert" die Ausführung an dieser Stelle
        }

        HasFullyGrown(1);
    }

    
    IEnumerator GrowFlower()
    {
        Vector3 scale = Vector3.zero;
        bool fullyGrown = false;
        gameObject.transform.localScale = scale;
        float increase = (timeInterval / timeToScale);
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

            if (scale.x >= mMaxScale.x && scale.y >= mMaxScale.y && scale.z >= mMaxScale.z)
            {
                scale = mMaxScale;
                fullyGrown = true;
            }

            gameObject.transform.localScale = scale;

            yield return new WaitForSeconds(timeInterval);
        }     
    }

    IEnumerator GrowBigPlant()
    {
        Vector3 scale = new Vector3(mMaxScale.x * 0.1f, 0f, mMaxScale.z * 0.1f);
        bool fullyGrown = false;

        mMaxScale.x *= Random.Range(0.7f, 1f);
        mMaxScale.y *= Random.Range(0.7f, 1f);
        mMaxScale.z *= Random.Range(0.7f, 1f);

        gameObject.transform.localScale = scale;
        float increase = (timeInterval / timeToScale);
        while (!fullyGrown)
        {
            if (scale.y < mMaxScale.y)
            {
                scale.y += increase * mMaxScale.y;
            }

            if (scale.y > mMaxScale.y * 0.5f)
            {
                if (scale.x < mMaxScale.x)
                {
                    scale.x += increase * mMaxScale.x * 2;
                }

                if (scale.z < mMaxScale.z)
                {       
                    scale.z += increase  * mMaxScale.z * 2;
                }
            }

            if (scale.x >= mMaxScale.x && scale.y >= mMaxScale.y && scale.z >= mMaxScale.z)
            {
                scale = mMaxScale;
                fullyGrown = true; 
            }

            gameObject.transform.localScale = scale;

            yield return new WaitForSeconds(timeInterval);
        }

        HasFullyGrown(0);
    }

    void HasFullyGrown(int index)
    {
        if (!mIsFullyGrown)
        {
            mIsFullyGrown = true;
            mResponsibleMeshSquare.AddToCombineList(mMeshFilter, index);
        }
    }  
}
