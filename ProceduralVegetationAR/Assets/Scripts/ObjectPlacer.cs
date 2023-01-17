using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPlacer : MonoBehaviour
{
    List<Plant> mPlantsToSpawn;
      
    float mRadius;   
    float mDistance; 
    float mMaxGroundAngle;  
    int mRaycastAttempts;
    int mMaxAttemptsToFindNewPosition;
    float mRaycastAttemptDelay;
    float mNewPosFindingDelay;
    int mObjectCountToSpawn;

    LayerMask mSpatialMappingMeshLayerMask;
    LayerMask mValidAreaSmallPlantsMeshLayerMask;
    LayerMask mValidAreaBigPlantsMeshLayerMask;
    LayerMask mNeedsRoomLayerMask;
    LayerMask mNeedsNoRoomLayerMask;
    LayerMask mIvyFlowerLayerMask; 
    
    public int mAmountOfSpawnedObjects = 0;
    float mRaycastDistanceMultiplier = 1.2f;

    float combinedSpawnChanceValues; 

    private void Start()
    {
        mRadius = GameManager.SETTINGS.Radius;
        mDistance = GameManager.SETTINGS.Distance;
        mMaxGroundAngle = GameManager.SETTINGS.MaxGroundAngle;
        mRaycastAttempts = GameManager.SETTINGS.RaycastAttempts;
        mMaxAttemptsToFindNewPosition = GameManager.SETTINGS.MaxAttemptsToFindNewPosition;
        mRaycastAttemptDelay = GameManager.SETTINGS.RaycastAttemptDelay;
        mNewPosFindingDelay = GameManager.SETTINGS.NewPosFindingDelay;
        mObjectCountToSpawn = GameManager.SETTINGS.ObjectCountToSpawn;

        mSpatialMappingMeshLayerMask = GameManager.SETTINGS.SpatialMappingMeshLayerMask;
        mValidAreaSmallPlantsMeshLayerMask = GameManager.SETTINGS.ValidAreaSmallPlantsMeshLayerMask;
        mValidAreaBigPlantsMeshLayerMask = GameManager.SETTINGS.ValidAreaBigPlantsMeshLayerMask;
        mNeedsRoomLayerMask = GameManager.SETTINGS.NeedsRoomLayerMask;
        mNeedsNoRoomLayerMask = GameManager.SETTINGS.NeedNoRoomLayerMask;
        mIvyFlowerLayerMask = GameManager.SETTINGS.IVYFLOWERSETTINGS.IvyFlowerLayerMask;

        mPlantsToSpawn = new List<Plant>();
        SetSpawnablePlants();
    }
    
    void SetSpawnablePlants()
    {
        combinedSpawnChanceValues = 0f; 

        for (int i = 0; i < GameManager.SETTINGS.Plants.Count; i++)
        {
            if (GameManager.SETTINGS.Plants[i].GrowsOnGround)
            {
                mPlantsToSpawn.Add(GameManager.SETTINGS.Plants[i]);
                combinedSpawnChanceValues += GameManager.SETTINGS.Plants[i].Commonness;
            }
        }
    }

    public void PlacePlantsOnWalls(int maxAmount)
    {
        StartCoroutine(TryPlacingPlantsOnWalls(maxAmount));
    }

    IEnumerator TryPlacingPlantsOnWalls(int maxPlantsPlaced)
    {
        RaycastData data = new RaycastData();
        data.StartPosition = new Vector3(0f, 0.2f, 0f);
        data.Distance = 10f;
        int maxPlants = maxPlantsPlaced;
        int plantsPlaced = 0; 

        for (int i = 0; i < mRaycastAttempts; i++)
        {
            if (plantsPlaced >= maxPlants)
            {
                break; 
            }
            Vector3 randDir1 = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)); 
            Vector3 randDir2 = new Vector3(0f, Random.Range(-0.2f, 0.2f), 0f);
            data.Direction = (randDir1 + randDir2).normalized;
            data.HasHit = Physics.Raycast(data.StartPosition, data.Direction, out data.Hit, data.Distance, mSpatialMappingMeshLayerMask);           
        
            if (data.HasHit)
            {

                if (CheckIfWall(data.Hit.normal))
                {

                    if (CheckIfEnoughSpace(data.Hit.point, data.Hit.normal))
                    {
                        
                        SpawnClimbingPlantFlower(data.Hit.point, data.Hit.normal);
                        plantsPlaced++; 
                    }                 
                }         
            }

            yield return new WaitForSeconds(0.1f);
        }      
    }

   
    bool CheckIfWall(Vector3 normal)
    {
        float angle = Vector3.Angle(normal, Vector3.down);

        if (angle > 80f && angle < 100f)
        {
            return true;
        }

        return false; 
    }

    IEnumerator SpawnOnGroundCoroutine(Vector3 hitPosition, Vector3 hitNormal, float radius, int amountToSpawn, int attempts)
    {
        Vector3 position = hitPosition;
        Vector3 normal = hitNormal;
        Vector3 offset = normal * mDistance;

        int amountOfSpawnedObjects = 0;

        for (int i = 0; i < attempts; i++)
        {
            if (amountOfSpawnedObjects >= amountToSpawn)
            {
                break;
            }

            Vector3 localPos = RandomLocalPosUnitCircle() * radius;
            int index = 0;

            float spawnChanceValue = Random.Range(0f, combinedSpawnChanceValues);
            float value = 0f;

            for (int j = 0; j < mPlantsToSpawn.Count; j++)
            {
                value += mPlantsToSpawn[j].Commonness;
                if (spawnChanceValue <= value)
                {
                    index = j;
                    break; 
                }
            }

            if (CheckIfValid(localPos + position + offset, mPlantsToSpawn[index]))
            {
                RaycastData raycastData = RaycastDown(localPos + position + offset);

                if (raycastData.HasHit)
                {
                    Vector3 placementPosition = raycastData.Hit.point;
                    float requiredRadius = mPlantsToSpawn[index].RequiredRadius;
                    Collider[] colliders;

                    if (CheckIfColliding(placementPosition, requiredRadius, mPlantsToSpawn[index], out colliders))
                    {
                        int maxAttempts = mMaxAttemptsToFindNewPosition; 

                        for (int j = 0; j < maxAttempts; j++)
                        {
                            Vector3 totalEvadeVector = Vector3.zero;

                            for (int k = 0; k < colliders.Length; k++)
                            {
                                Vector3 evadeVector = placementPosition - colliders[k].gameObject.transform.position;
                                totalEvadeVector += evadeVector; 
                            }

                            Vector3 oldPlacementPos = placementPosition; 
                            placementPosition += (new Vector3(totalEvadeVector.x, 0f, totalEvadeVector.z)).normalized * requiredRadius;

                            if (Vector3.SqrMagnitude(hitPosition - placementPosition) > (mRadius * mRadius))
                            {
                                break; 
                            }

                            if (!CheckIfColliding(placementPosition, requiredRadius, mPlantsToSpawn[index], out colliders))
                            {
                                Vector3 newPlacementPosition; 
                                Vector3 newUpVector;

                                if (GetNewPlacementPos(placementPosition, offset, out newPlacementPosition, out newUpVector, mPlantsToSpawn[index]))
                                {

                                    SpawnObject(newPlacementPosition, Vector3.up, MathFunctions.RandRotAroundAxis(Vector3.up),index);
                                    amountOfSpawnedObjects++;
                                    break; 
                                }
                            }

                            yield return new WaitForSeconds(mNewPosFindingDelay);
                        }
                    }
                    else
                    {
                        SpawnObject(placementPosition, Vector3.up, MathFunctions.RandRotAroundAxis(Vector3.up), index);
                        amountOfSpawnedObjects++;
                    }
                }
            }

            yield return new WaitForSeconds(mRaycastAttemptDelay);
        }

        yield return null;
    }

    IEnumerator SpawnOnWallCoroutine(Vector3 hitPosition, Vector3 hitNormal)
    {
        if (CheckIfEnoughSpace(hitPosition, hitNormal))
        {
            SpawnClimbingPlantFlower(hitPosition, hitNormal);
        }

        yield return null;
    }

    bool CheckIfEnoughSpace(Vector3 position, Vector3 normal)
    {
        float requiredRadius = GameManager.SETTINGS.IVYFLOWERSETTINGS.RequiredRadius; 
        //Check if there is another IvyFlower in the radius
        //This prevents the player accidentally placing a new flower instead of hitting an existing one 
        Collider[] colliders = Physics.OverlapSphere(position, 0.25f, mIvyFlowerLayerMask);
        if (colliders.Length != 0)
        {
            return false;
        }

        int numberOfIncrements = 12;

        Vector3[] pointsOnCircle = MathFunctions.CircleAroundAxis(position, normal, numberOfIncrements);

        RaycastData data = new RaycastData();
        float offset = 0.01f;

        for (int i = 0; i < pointsOnCircle.Length; i++)
        {
            
            pointsOnCircle[i] = position + (pointsOnCircle[i] - position) * 0.05f;
           
            data.StartPosition = position + normal * offset;
            data.Direction = (pointsOnCircle[i] - position).normalized;
            data.Distance = requiredRadius;

            data.HasHit = Physics.Raycast(data.StartPosition, data.Direction, out data.Hit, data.Distance, mSpatialMappingMeshLayerMask);

            if (data.HasHit)
            {
                return false;
            }
            else
            {
                data.StartPosition = data.StartPosition + data.Direction * data.Distance;
                data.Direction = normal * -1f;
                data.Distance = offset;

                data.HasHit = Physics.Raycast(data.StartPosition, data.Direction, out data.Hit, data.Distance, mSpatialMappingMeshLayerMask);

                if (data.HasHit)
                {
                    float angleToNormal = Vector3.Angle(data.Hit.normal, normal);
                    if (angleToNormal > 10f)
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    void SpawnClimbingPlantFlower(Vector3 hitPosition, Vector3 hitNormal)
    {
        GameObject obj = Instantiate(GameManager.SETTINGS.IVYFLOWERSETTINGS.IvyFlowerPrefab, hitPosition, Quaternion.identity);
        obj.transform.rotation = MathFunctions.RandRotAroundAxis(hitNormal) * Quaternion.FromToRotation(Vector3.up, hitNormal);
        obj.transform.parent = GameManager.INSTANCE.gameObject.transform;

        GameManager.INSTANCE.numberOfSpawnedPlants++;
    }

    public void SpawnObjectsAtHit(Vector3 hitPos, Vector3 hitNormal)
    {    
        float angle = Vector3.Angle(hitNormal, Vector3.up); 

        if(angle <= mMaxGroundAngle)
        {
            StartCoroutine(SpawnOnGroundCoroutine(hitPos, hitNormal, mRadius, mObjectCountToSpawn, mRaycastAttempts));          
        }
        else
        {
            StartCoroutine(SpawnOnWallCoroutine(hitPos, hitNormal));
        }
    }
 
    bool GetNewPlacementPos(Vector3 position, Vector3 offset, out Vector3 newPlacementPosition, out Vector3 newUpVector, Plant plant)
    {
        newPlacementPosition = Vector3.zero;
        newUpVector = Vector3.up;
        Vector3 pos = offset + position;

        if (CheckIfValid(pos, plant))
        {
            
            RaycastData raycastData = RaycastDown(pos);

            if (raycastData.HasHit)
            {
                newPlacementPosition = raycastData.Hit.point;
                newUpVector = raycastData.Hit.normal; 
                return true;
            }
        }
        return false;
    }
   
    bool CheckIfValid(Vector3 position, Plant plant)
    {
        float distance = mDistance * mRaycastDistanceMultiplier;

        if (plant.PlantType == PlantType.SmallPlant)
        {       
            return Physics.Raycast(position, Vector3.down, distance, mValidAreaSmallPlantsMeshLayerMask);
        }
        else if (plant.PlantType == PlantType.BigPlant)
        {
            return Physics.Raycast(position, Vector3.down, distance, mValidAreaBigPlantsMeshLayerMask);
        }
        else
        {
            return false;
        }
        
    }

    RaycastData RaycastDown(Vector3 position)
    {
        RaycastData data = new RaycastData(position, Vector3.down, mDistance * mRaycastDistanceMultiplier);
        data.HasHit = Physics.Raycast(data.StartPosition, data.Direction, out data.Hit, data.Distance, mSpatialMappingMeshLayerMask);
        return data;                
    }

    Vector3 RandomLocalPosUnitCircle()
    {
        Vector2 unitCirclePos = Random.insideUnitCircle;
        Vector3 posLocal = new Vector3(unitCirclePos.x * mRadius, 0f, unitCirclePos.y * mRadius);
        return posLocal; 
    }


    void SpawnObject(Vector3 pos, Vector3 up, Quaternion rotation, int index)
    {
        GameObject obj = GameManager.OBJECTPOOLER.GetObjectFromPool(mPlantsToSpawn[index].Name, pos, Quaternion.identity);
        obj.transform.up = up;
        obj.transform.rotation = rotation;
        obj.transform.parent = GameManager.INSTANCE.gameObject.transform; 

        GameManager.INSTANCE.numberOfSpawnedPlants++; 
    }

    bool CheckIfColliding(Vector3 position, float requiredRadius, Plant plant, out Collider[] colliders)
    {
        colliders = Physics.OverlapSphere(position, requiredRadius, mNeedsRoomLayerMask);

        if (colliders.Length != 0)
        {
            return true;
        }

        return false;    
    }

    public void PlaceObjectsInRadius(Vector3 position, float radius, int increments)
    {
        float angleIncrements = 360f / (float)increments;
        float angle = 0f;

        for (int i = 0; i < increments; i++)
        {
            Vector2 vector2D = MathFunctions.AngleToVector(angle * Mathf.Deg2Rad);
            Vector3 direction = (new Vector3(vector2D.x, 0f, vector2D.y)).normalized;

            StartCoroutine(PlaceObjInRadiusCoroutine(position, direction, radius));
            angle += angleIncrements;
        }     
    }

    IEnumerator PlaceObjInRadiusCoroutine(Vector3 position, Vector3 direction, float radius)
    {
        GameObject helper = new GameObject("PlaceInRadiusHelper");
        helper.transform.parent = gameObject.transform; 
        RaycastData data = new RaycastData(position + direction * radius, Vector3.down, 3f);

        float stepDistance = radius * 0.05f;

        //Find Startposition
        Vector3 startPosition;
        while (true)
        {
 
            data.HasHit = Physics.Raycast(data.StartPosition, data.Direction, out data.Hit, data.Distance, mValidAreaSmallPlantsMeshLayerMask);

            if (data.HasHit)
            {
                startPosition = data.StartPosition;
                break;
            }
            else
            {
                data.StartPosition += -1f * direction * stepDistance;
            }

            yield return new WaitForSeconds(0.05f);
        }

        float remainingDistance = Vector3.Distance(position, data.StartPosition);
        float placementSteps = remainingDistance * 0.1f;
        float maxJitter = 3f;
     
        Vector3 startPositionWithoutOffset = data.StartPosition;

        while (true)
        {
            float jitterOffset = Random.Range(0f, maxJitter * (Vector3.Distance(position, startPositionWithoutOffset) / 10f));
            helper.transform.rotation = Quaternion.LookRotation(startPositionWithoutOffset - position);
            float randValue = Random.value;
            if (randValue >= 0.5f)
            {
                data.StartPosition = startPositionWithoutOffset + jitterOffset * helper.transform.right;
            }
            else
            {
                data.StartPosition = startPositionWithoutOffset + jitterOffset * helper.transform.right * -1f;
            }

            data.HasHit = Physics.Raycast(data.StartPosition, data.Direction, out data.Hit, data.Distance, mValidAreaSmallPlantsMeshLayerMask);

            if (data.HasHit)
            {
                //SpawnPlants
                StartCoroutine(SpawnOnGroundCoroutine(data.Hit.point, data.Hit.normal, 1f, 5, 5));
            }

            startPositionWithoutOffset += -1f * direction * placementSteps;

            if (Vector3.Distance(position, startPositionWithoutOffset) <= 1f)
            {
                break;
            }

            yield return new WaitForSeconds(1f);
        }

        Destroy(helper);
    }
}
