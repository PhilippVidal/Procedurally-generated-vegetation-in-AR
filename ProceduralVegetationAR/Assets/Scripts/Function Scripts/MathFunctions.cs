using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathFunctions 
{
    public const float pi = 3.1415f;

    public static Quaternion RandRotAroundAxis(Vector3 axis)
    {
        float randomAngle = Random.Range(0f, 360f);

        Quaternion randomRotation = Quaternion.AngleAxis(randomAngle, axis);

        return randomRotation; 
    }
    public static Vector3[] CircleAroundAxis(Vector3 position, Vector3 axis, int incrementCount)
    {       
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, axis);

        Vector3[] pointsOnCircle = new Vector3[incrementCount];

        for (int i = 0; i < incrementCount; i++)
        {
            float angleInRad = (i / (float)incrementCount) * 2 * pi;
            Vector2 dirVector = AngleToVector(angleInRad);

            Vector3 pointOnCircle = (new Vector3(dirVector.x, 0f, dirVector.y)).normalized;
            pointsOnCircle[i] = (rotation * pointOnCircle) + position;
        }
        
        return pointsOnCircle;
    }

    public static Vector2 AngleToVector(float angRad)
    {
        Vector2 unitVector = new Vector2(Mathf.Cos(angRad), Mathf.Sin(angRad));

        return unitVector; 
    }

    public static Quaternion DirectionToRotation(Vector3 dirVector, Vector3 up)
    {
            return Quaternion.LookRotation(dirVector, up);                
    }

    public static Vector3 LineWithPlaneIntersect(Vector3 posA, Vector3 posC, Vector3 normal, Vector3 dirCB)
    {
        float a = Vector3.Dot((posA - posC), normal);
        float b = Vector3.Dot(dirCB, normal);
        
        return posC + dirCB.normalized * (a / b);        
    }

    //Get the direction of the line at which 2 planes intersect 
    public static Vector3 PlaneIntersectionLineDirection(Vector3 dir1, Vector3 dir2)
    {
        return Vector3.Cross(dir1.normalized, dir2.normalized);
    }
}
