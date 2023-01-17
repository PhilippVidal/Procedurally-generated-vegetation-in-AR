using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshCombiner
{
    public static void CombineMeshes(List<MeshFilter> fromMeshFilters, MeshFilter toMeshFilter)
    {
        int count = fromMeshFilters.Count;
        List<MeshFilter> allMeshFilters = fromMeshFilters;

        allMeshFilters.Add(toMeshFilter);

        CombineInstance[] combineInstance = new CombineInstance[allMeshFilters.Count];

        for (int i = 0; i < allMeshFilters.Count; i++)
        {
            combineInstance[i].mesh = allMeshFilters[i].mesh;
            combineInstance[i].transform = toMeshFilter.transform.worldToLocalMatrix * allMeshFilters[i].transform.localToWorldMatrix;
        }

        Mesh tempMesh = new Mesh();
        tempMesh.Clear();
        tempMesh.CombineMeshes(combineInstance);
        toMeshFilter.sharedMesh = tempMesh;

        for (int i = 0; i < count; i++)
        {
            fromMeshFilters[i].gameObject.SetActive(false);
        }
    }
    public static void CombineLeaves(GameObject[] from, Transform transform, MeshFilter toFilter)
    {
        
        List<MeshFilter> allMeshFilters = GetAllMeshFilters(from);
        allMeshFilters.Add(toFilter);

        CombineInstance[] combineInstance = new CombineInstance[allMeshFilters.Count];

        for (int i = 0; i < allMeshFilters.Count; i++)
        {
            combineInstance[i].mesh = allMeshFilters[i].mesh;
            combineInstance[i].transform = transform.worldToLocalMatrix * allMeshFilters[i].transform.localToWorldMatrix;
        }

        Mesh tempMesh = new Mesh();
        tempMesh.Clear();
        tempMesh.CombineMeshes(combineInstance);

        toFilter.sharedMesh = tempMesh;

        for (int i = 0; i < from.Length; i++)
        {
            from[i].SetActive(false);
        }
    }

    static List<MeshFilter> GetAllMeshFilters(GameObject[] objs)
    {
        List<MeshFilter> meshFilters = new List<MeshFilter>();
        //Debug.Log("objs length" + objs.Length);
        for (int i = 0; i < objs.Length; i++)
        {
            MeshFilter tempMeshFilter;

            if(objs[i].TryGetComponent<MeshFilter>(out tempMeshFilter))
            {
                meshFilters.Add(tempMeshFilter);
            }

            for (int j = 0; j < objs[i].transform.childCount; j++)
            {
                
                if (objs[i].transform.GetChild(j).gameObject.TryGetComponent<MeshFilter>(out tempMeshFilter))
                {
                    meshFilters.Add(tempMeshFilter);
                }
            }
        }
        return meshFilters;
    }
}
