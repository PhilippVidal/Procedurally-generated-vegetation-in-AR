using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class NavMeshConverter : MonoBehaviour
{
    NavMeshSurface mNavMeshSurface;
    MeshCollider mMeshCollider; 

    bool mNavMeshCreated = false;   
    Mesh mMesh;
    MeshRenderer mMeshRenderer;

    private void Awake()
    {
        mMeshRenderer = GetComponent<MeshRenderer>();
    }

    private void Start()
    {
        mNavMeshSurface = GameManager.NAVMESHSURFACE;
    }
   
    public void GenerateValidArea(GameObject obj, int agentID)
    {
        BuildNavMesh(agentID);
        ConvertToMesh(obj);
    }

    bool BuildNavMesh(int agentID)
    {
        if(!mNavMeshCreated)
        {
            NavMeshBuildSettings navSettings = NavMesh.GetSettingsByIndex(agentID);
            int agentTypeId = navSettings.agentTypeID;
            mNavMeshSurface.agentTypeID = agentTypeId;

            mNavMeshSurface.BuildNavMesh();
            mNavMeshCreated = true;
            return true; 
        }

        return false; 
    }

    bool ConvertToMesh(GameObject obj)
    {
        if (mNavMeshCreated)
        {
            mMesh = new Mesh();
            Vector3[] vertices;
            int[] triangles;

            NavMeshTriangulation triangulatedNavMesh = NavMesh.CalculateTriangulation();

            vertices = triangulatedNavMesh.vertices;
            triangles = triangulatedNavMesh.indices;

            mMesh.vertices = vertices;
            mMesh.triangles = triangles;

            MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mMesh;
            mMeshCollider = obj.AddComponent<MeshCollider>();
            mMeshCollider.sharedMesh = mMesh;
            mNavMeshCreated = false;

            MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = mMeshRenderer.sharedMaterial;            

            return true; 
        }

        return false;          
    }
}
