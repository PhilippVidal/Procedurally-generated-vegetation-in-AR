using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameSettings : MonoBehaviour
{
    [Header("Plant Settings")]
    public List<Plant> Plants;
    public IvySettings IVYSETTINGS;
    public IvyFlowerSettings IVYFLOWERSETTINGS;

    [Header("Object Pooling Settings")]
    public float percentageToRefillAt = 0.1f;
    public float refillPercentage = 0.5f;

    [Header("Object Placer Settings")]
    public float Radius = 1f;
    public float Distance = 0.15f;
    public float MaxGroundAngle = 40f;
    public int RaycastAttempts = 40;

    public int MaxAttemptsToFindNewPosition = 5;
    public float RaycastAttemptDelay = 1f;
    public float NewPosFindingDelay = 1f;

    public int ObjectCountToSpawn = 10;

    public LayerMask SpatialMappingMeshLayerMask;
    public LayerMask ValidAreaSmallPlantsMeshLayerMask;
    public LayerMask ValidAreaBigPlantsMeshLayerMask;
    public LayerMask NeedsRoomLayerMask;
    public LayerMask NeedNoRoomLayerMask;
}

[System.Serializable]
public class IvySettings
{  
    [Header("Tip Settings")]
    [Range(3, 32)]
    public int MeshResolution = 3;
    public float Radius = 0.002f;
    [Range(0.1f, 3f)]
    public float TipDistanceMultiplier = 3f;
    public float GrowthSpeed = 0.4f; 


    [Header("Body & Leaf Settings")]
    public string LeafPoolName = "IvyLeaf";
    public float LeafGrowthTime = 2f;
    public float MinDistanceToNextLeaf = 0.05f;
    public float MaxDistanceToNextLeaf = 0.1f;
    public float LeafEdgeDistance = 0.1f;
    public float RandomRotationX = -10f;
    public float RandomRotationY = 20f;
    public float RandomRotationZ = 10f; 
    public float BranchPercentage = 0.05f; 

    [Header("Path Settings")]
    public int Iterations = 50;
    public float NodeRadius = 0.01f;
    public float MaxAngleChange = 25f;
    public float MaxRaycastDistance = 0.2f;
    public float MaxSensingDistance = 1f;
    public float MaxAngleSensing = 45f;
    public int MaxTriesToFindNewPath = 10;
}

[System.Serializable]
public class IvyFlowerSettings
{
    public GameObject IvyPrefab;
    public GameObject IvyFlowerPrefab;
    public int NumberOfIvys = 6;
    public float RequiredRadius = 0.1f;
    public LayerMask IvyFlowerLayerMask;
}