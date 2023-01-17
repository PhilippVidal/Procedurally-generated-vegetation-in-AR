using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Pathfinding : MonoBehaviour
{
    public GameObject mHelper;
    public Ivy mIvy; 

    int mIterations;
    float mMaxAngleChange;
    float mMaxRaycastDistance;
    //float mOffsetFromGround;
    float mIvyRadius;

    bool mPathCanBeFound;

    LayerMask mSpatialMappingLayerMask;
    LayerMask mIvyFlowerMask; 

    public void init()
    {
        mMaxAngleChange = GameManager.SETTINGS.IVYSETTINGS.MaxAngleChange;
        mMaxRaycastDistance = GameManager.SETTINGS.IVYSETTINGS.MaxRaycastDistance;
        //mOffsetFromGround = GameManager.SETTINGS.IVYSETTINGS.Radius;
        mIvyRadius = GameManager.SETTINGS.IVYSETTINGS.Radius;
        mSpatialMappingLayerMask = GameManager.SETTINGS.SpatialMappingMeshLayerMask;
        mIvyFlowerMask = GameManager.SETTINGS.IVYFLOWERSETTINGS.IvyFlowerLayerMask;
    }

    public void FindPath(int iterations)
    {
        mIterations = iterations;
        StartCoroutine(GeneratePath());
    }

    IEnumerator GeneratePath()
    {
        int maxFailCount = GameManager.SETTINGS.IVYSETTINGS.MaxTriesToFindNewPath;
        int failedCount = 0;
        for (int i = 0; i < mIterations; i++)
        {
            mPathCanBeFound = true; 

            FindNextPoint();

            //stop if no path can be found after x attempts 
            if (mPathCanBeFound)
            {
                failedCount = 0;
            }
            else
            {
                failedCount++;

                if (failedCount > maxFailCount)
                {
                    break;
                }
            }
            

            yield return new WaitForSeconds(0.1f);
        }
    }
    
    void FindNextPoint()
    {     
        float randAngle = Random.Range(-mMaxAngleChange, mMaxAngleChange);
        float angleBias = IvyBehaviour.BehaviourAngleBias(mHelper.transform.position, mHelper.transform.forward, mHelper.transform.up, this);

        float dirAngle = CapAngle(randAngle + angleBias);

        Vector3 newDirection = (Quaternion.AngleAxis(dirAngle, mHelper.transform.up) * mHelper.transform.forward).normalized;

        Vector3 oldUp = mHelper.transform.up;
        Vector3 oldForward = mHelper.transform.forward;


        UpdateHelper(mHelper.transform.position, mHelper.transform.up, newDirection);

        RaycastData firstRaycast = new RaycastData(mHelper.transform.position, newDirection, mMaxRaycastDistance);

        //raycast to avoid ivy flowers
        firstRaycast.HasHit = Physics.Raycast(firstRaycast.StartPosition, firstRaycast.Direction, out firstRaycast.Hit, firstRaycast.Distance, mIvyFlowerMask);
        if (firstRaycast.HasHit)
        {
  
            float anglePos = Vector3.Angle(firstRaycast.Hit.normal, (Quaternion.AngleAxis(mMaxAngleChange, oldUp) * oldForward).normalized);
            float angleNeg = Vector3.Angle(firstRaycast.Hit.normal, (Quaternion.AngleAxis(-mMaxAngleChange, oldUp) * oldForward).normalized);

            if (anglePos <= angleNeg)
            {
                dirAngle = mMaxAngleChange;
            }
            else
            {
                dirAngle = -mMaxAngleChange;
            }

            firstRaycast.Direction = (Quaternion.AngleAxis(dirAngle, oldUp) * oldForward).normalized;
            UpdateHelper(mHelper.transform.position, mHelper.transform.up, firstRaycast.Direction);           
        }


        firstRaycast.HasHit = Physics.Raycast(firstRaycast.StartPosition, firstRaycast.Direction, out firstRaycast.Hit, firstRaycast.Distance, mSpatialMappingLayerMask);

        //if the first raycast hits something it's either a wall or a slope, otherwise it must be (more or less) even ground or an edge
        if (firstRaycast.HasHit)
        {
            HandleIncline(firstRaycast);
        }
        else
        {
            HandleLevelOrDecline(firstRaycast);
        }     
    }

    float CapAngle(float angle)
    {
        float cappedAngle = angle;
        if (angle > mMaxAngleChange)
        {
            cappedAngle = mMaxAngleChange; 
        }
        if (angle < -mMaxAngleChange)
        {
            cappedAngle = -mMaxAngleChange;
        }

        return cappedAngle;
    }   

    void HandleLevelOrDecline(RaycastData firstRaycast)
    {
        RaycastData secondRaycast = new RaycastData(firstRaycast.StartPosition + (firstRaycast.Direction * firstRaycast.Distance), GetDownVector(), mIvyRadius * 2f);
        
        secondRaycast.HasHit = Physics.Raycast(secondRaycast.StartPosition, secondRaycast.Direction, out secondRaycast.Hit, secondRaycast.Distance, mSpatialMappingLayerMask);

        //if there is ground beneath the end of the first raycast, then assume there is no edge
        if (secondRaycast.HasHit)
        {
            HandleLevel(secondRaycast);
        }
        else
        {
            HandleDecline(secondRaycast, firstRaycast);
        }     
    }

    void HandleLevel(RaycastData raycast)
    {
        AddNewNodeToPath(raycast.HitPosWithOffset(mIvyRadius), raycast.Hit.normal, NodeType.GROUND);
        Vector3 newForward = GetNewForwardVector(mHelper.transform.right, raycast.Hit.normal);
        UpdateHelper(raycast.HitPosWithOffset(mIvyRadius), raycast.Hit.normal, newForward);
    }

    void HandleIncline(RaycastData firstRaycast)
    {

        AddNewNodeToPath(firstRaycast.Hit.point + (Vector3.Distance(firstRaycast.Hit.point, mHelper.transform.position) * 0.5f * firstRaycast.Direction * -1f), GetDownVector() * -1f, NodeType.WALL);
      
        AddNewNodeToPath(firstRaycast.HitPosWithOffset(mIvyRadius), firstRaycast.Hit.normal, NodeType.WALL);

        Vector3 newForward = GetNewForwardVector(mHelper.transform.right, firstRaycast.Hit.normal);
        UpdateHelper(firstRaycast.HitPosWithOffset(mIvyRadius), firstRaycast.Hit.normal, newForward);
    }


    void HandleDecline(RaycastData secondRaycast, RaycastData firstRaycast)
    {
        RaycastData thirdRaycast = new RaycastData(secondRaycast.StartPosition + (secondRaycast.Direction * secondRaycast.Distance), firstRaycast.Direction * -1, firstRaycast.Distance);
        thirdRaycast.HasHit = Physics.Raycast(thirdRaycast.StartPosition, thirdRaycast.Direction, out thirdRaycast.Hit, thirdRaycast.Distance, mSpatialMappingLayerMask);

        if (thirdRaycast.HasHit)
        {
            //assign the point after edge with an offset
            Vector3 pointAfterEdge = thirdRaycast.Hit.point + thirdRaycast.Hit.normal * mIvyRadius;
  
            //Calculate right-vector and find direction along surface
            Vector3 rightVector = Vector3.Cross(( secondRaycast.StartPosition + secondRaycast.Direction * secondRaycast.Distance) - thirdRaycast.Hit.point, secondRaycast.Direction * -1f);     
            Vector3 lineDirection = Quaternion.AngleAxis(-90f, rightVector) * thirdRaycast.Hit.normal;

            Vector3 pointOnEdge = MathFunctions.LineWithPlaneIntersect(firstRaycast.StartPosition, pointAfterEdge, secondRaycast.Direction * -1f, lineDirection);

            Vector3 pointOnEdgeNormal = (mHelper.transform.up + thirdRaycast.Hit.normal).normalized;
            Vector3 pointOnEdgeWithOffset = pointOnEdge + (pointOnEdgeNormal * mIvyRadius) + ((mHelper.transform.up * -1) * mIvyRadius);

            AddNewNodeToPath(pointOnEdgeWithOffset, pointOnEdgeNormal, NodeType.EDGE);
            Vector3 newForward = GetNewForwardVector(mHelper.transform.right, thirdRaycast.Hit.normal);
            UpdateHelper(pointOnEdgeWithOffset, thirdRaycast.Hit.normal, newForward);
        }
        else
        {
            mPathCanBeFound = false;
        }
    }

    void UpdateHelper(Vector3 position, Vector3 up, Vector3 forward)
    {
        mHelper.transform.position = position;       
        mHelper.transform.rotation = Quaternion.LookRotation(forward, up);
    }


    Vector3 GetNewForwardVector(Vector3 normal1, Vector3 normal2)
    {    
        return MathFunctions.PlaneIntersectionLineDirection(normal1, normal2);       
    }

    Vector3 GetDownVector()
    {
        return mHelper.transform.up * -1f;
    }

    void AddNewNodeToPath(Vector3 position, Vector3 up, NodeType nodetype)
    {
        Node newNode = new Node();
        newNode.position = position;
        newNode.up = up;
        newNode.type = nodetype;

        mIvy.AddNodeToPath(newNode);
    }

    public Node GetLastNodeOnWall()
    {
        List<Node> nodes = mIvy.GetPath();
        int count = nodes.Count;

        for (int i = count - 1; i >= 0; i--)
        {
            if (nodes[i].type == NodeType.WALL /*|| nodes[i].type == NodeType.GROUND*/)
            {
                return nodes[i];
            }
        }

        return new Node(Vector3.zero, Quaternion.identity, Vector3.zero, NodeType.NONE);
    }
}
