using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlantType
{
    None,
    SmallPlant,
    BigPlant,
    Ivy
}

[System.Serializable]
public class Plant 
{
    public string Name;
    public GameObject[] PlantModels;
    public bool GrowsOnGround = false;
    //public bool NeedsRoom = true; 
    public float RequiredRadius = 0.1f;
    public int RequiredAmount;
    public PlantType PlantType;
    public float Commonness; 
}

