using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NodeType
{
    NONE,
    START,
    EDGE,
    WALL,
    GROUND,
    SLOPE
}
public struct Node 
{
    public Vector3 position;
    public Quaternion rotation;

    public Vector3 up;
    public NodeType type;

    public Node(Vector3 position, Quaternion rotation, Vector3 up, NodeType type)
    {
        this.position = position;
        this.rotation = rotation;
        this.up = up;
        this.type = type;
    }   
}

