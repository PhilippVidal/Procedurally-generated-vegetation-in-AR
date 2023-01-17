using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastData 
{
    public bool HasHit;
    public float Distance;
    public RaycastHit Hit;   
    public Vector3 Direction;
    public Vector3 StartPosition; 

    public RaycastData() { }
    public RaycastData(Vector3 startPosition, Vector3 direction, float distance)
    {
        StartPosition = startPosition;
        Direction = direction;
        Distance = distance; 
    }
    public Vector3 HitPosWithOffset(float offset)
    {
        return Hit.point + Hit.normal * offset;
    }

    public float ActualDistance()
    {
        if (HasHit)
        {
            return (Hit.point - StartPosition).magnitude;
        }
        else
        {
            return Distance; 
        }
    }
}
