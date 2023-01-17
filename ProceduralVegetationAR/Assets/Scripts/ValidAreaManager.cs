using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValidAreaManager : MonoBehaviour
{
    List<GameObject> mValidAreaMeshes;
    NavMeshConverter mNavMeshConverter;

    private void Awake()
    {
        mNavMeshConverter = GetComponent<NavMeshConverter>();        
    }
    private void Start()
    {
        mValidAreaMeshes = new List<GameObject>();
    }

    public void GenerateValidAreaMeshes()
    {
        AddNewValidAreaMesh(13, 1);
        AddNewValidAreaMesh(14, 2);
    }

    void AddNewValidAreaMesh(int layer, int agentID)
    {
        GameObject obj = new GameObject("ValidAreaMesh" + agentID);
        obj.transform.parent = transform;
        obj.layer = layer;
        mNavMeshConverter.GenerateValidArea(obj, agentID);
        mValidAreaMeshes.Add(obj);
    }

    public void RenderValidAreaMeshes(bool value)
    {     
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).GetComponent<MeshRenderer>().enabled = value;
        }               
    }
}
