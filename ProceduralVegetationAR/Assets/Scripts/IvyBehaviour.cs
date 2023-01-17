using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class IvyBehaviour 
{

    public static float BehaviourAngleBias(Vector3 position, Vector3 forward, Vector3 up, Pathfinding pathfindingScript)
    {
        float angleDown = Vector3.Angle(Vector3.down, up);
        //float angleBias = 0f;
        float angleBiasStrength = 0.8f;
        //Is the plant on the ground, 
        //on a wall and facing down 
        //or up-side-down?
        if (angleDown >= 150f)
        {
            return ClimbingPlantBehaviourOnGround(angleDown, position, forward, up) * angleBiasStrength;
        }
        else if (angleDown < 150f && angleDown > 30f)
        {
            return ClimbingPlantBehaviourWallFacingDown(angleDown, position, forward, up) * angleBiasStrength;
        }
        else
        {
            return ClimbingPlantBehaviourOnCeiling(angleDown, pathfindingScript.GetLastNodeOnWall(), position, forward, up) * angleBiasStrength;
        }
    }

    static float ClimbingPlantBehaviourOnGround(float angleToGround, Vector3 position, Vector3 forward, Vector3 up)
    {      
        float maxRaycastDistance = GameManager.SETTINGS.IVYSETTINGS.MaxSensingDistance; 
        float maxSensingAngle = GameManager.SETTINGS.IVYSETTINGS.MaxAngleSensing;
        
        Vector3 direction = Vector3.zero;

        float angleIncrements = (maxSensingAngle * 2) / 5;
        float currentAngle = -maxSensingAngle;

        RaycastData data = new RaycastData();
        data.Distance = maxRaycastDistance;
        data.StartPosition = position;


        for (int i = 0; i < 5; i++)
        {
            data.Direction = (Quaternion.AngleAxis(currentAngle, up) * forward).normalized;
            currentAngle += angleIncrements;

            data.HasHit = Physics.Raycast(data.StartPosition, data.Direction, out data.Hit, data.Distance, GameManager.SETTINGS.SpatialMappingMeshLayerMask);

            if (data.HasHit)
            {
                Vector3 hitVector = data.Hit.point - data.StartPosition;

                if (direction == Vector3.zero)
                {
                    direction = hitVector;
                }
                else
                {
                    if (direction.sqrMagnitude > hitVector.sqrMagnitude)
                    {
                        direction = hitVector;
                    }
                }
            }
        }
       
        if (direction == Vector3.zero)
        {
            return 0f;
        }
        else
        {
            return Vector3.SignedAngle(forward, direction, up);
        }
    }

    static float ClimbingPlantBehaviourWallFacingDown(float angleToGround, Vector3 position, Vector3 forward, Vector3 up)
    {
        //Vector3 facingDirection = forward;
        float facingAngleDown = Vector3.Angle(Vector3.down, forward);
        float maxAngleChange = GameManager.SETTINGS.IVYSETTINGS.MaxAngleChange;

        if (facingAngleDown < 90f)
        {
            //check -maxAngleChange and maxAngleChange and choose which one changes
            //the direction in a better way to move further up again
            float angleNegativ = Vector3.Angle(Vector3.down, (Quaternion.AngleAxis(-maxAngleChange, up) * forward).normalized);            
            float anglePositiv = Vector3.Angle(Vector3.down, (Quaternion.AngleAxis(maxAngleChange, up) * forward).normalized);

            if (angleNegativ >= anglePositiv)
            {
                return -maxAngleChange;
            }
            else if (angleNegativ == anglePositiv)
            {             
                if (Random.value < 0.5f)
                {
                    return -maxAngleChange;
                }
                else
                {
                    return maxAngleChange;
                }
            }
            else
            {
                return maxAngleChange;
            }
        }

        return 0f;
    }

    static float ClimbingPlantBehaviourOnCeiling(float angleToGround, Node node, Vector3 position, Vector3 forward, Vector3 up)
    {
        float bias = 0f;
        if (node.type != NodeType.NONE)
        {
            float maxAngleChange = GameManager.SETTINGS.IVYSETTINGS.MaxAngleChange;

            Vector3 targetDirection = node.position - position;

            float angleNegativ = Vector3.Angle(targetDirection, (Quaternion.AngleAxis(-maxAngleChange, up) * forward).normalized);
            float anglePositiv = Vector3.Angle(targetDirection, (Quaternion.AngleAxis(maxAngleChange, up) * forward).normalized);
            
            if (angleNegativ < anglePositiv)
            {
                bias = -maxAngleChange;
            }
            else
            {
                bias = maxAngleChange;
            }

            bias *= Random.Range(0.7f, 1f);
        }

        return bias;
    }
}
