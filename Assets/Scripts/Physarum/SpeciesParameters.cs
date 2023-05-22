using System;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Remember. On SpeciesParameters[], the first element is doing nothing!
/// </summary>
[System.Serializable]
public class SpeciesParameters : MonoBehaviour
{
    public Color chemCol1 = Color.red;
    public Color chemCol2 = Color.blue;
    public SensoryType sensorType = SensoryType.ChemoAttraction;
    public float RA = 45f;
    public float SA = 22.5f;
    public int SO = 9;
    public int SS = 1;
    public const int SW = 1;
    public int depT = 5;
    public float pCD = 0f;
    public float sMin = 0f;
    public bool canIntersect = false;

    public void SetFrom(SpeciesParametersSerializable serializableOther)
    {
        this.chemCol1 = serializableOther.chemCol1;
        this.chemCol2 = serializableOther.chemCol2;
        this.sensorType = serializableOther.sensorType;
        this.RA = serializableOther.RA;
        this.SA = serializableOther.SA;
        this.SO = serializableOther.SO;
        this.SS = serializableOther.SS;
        this.depT = serializableOther.depT;
        this.pCD = serializableOther.pCD;
        this.sMin = serializableOther.sMin;
        this.canIntersect = serializableOther.canIntersect;
    }
}
[Serializable]
public class SpeciesParametersSerializable
{
    [SerializeField] public Color chemCol1 = Color.red;
    [SerializeField] public Color chemCol2 = Color.blue;
    [SerializeField] public SensoryType sensorType = SensoryType.ChemoAttraction;
    [SerializeField] public float RA = 45f;
    [SerializeField] public float SA = 22.5f;
    [SerializeField] public int SO = 9;
    [SerializeField] public int SS = 1;
    [SerializeField] public const int SW = 1;
    [SerializeField] public int depT = 5;
    [SerializeField] public float pCD = 0f;
    [SerializeField] public float sMin = 0f;
    [SerializeField] public bool canIntersect = false;

    public SpeciesParametersSerializable(SpeciesParameters sp)
    {
        if (sp == null)
            return;

        this.chemCol1 = sp.chemCol1;
        this.chemCol2 = sp.chemCol2;
        this.pCD = sp.pCD;
        this.sMin = sp.sMin;
        this.canIntersect = sp.canIntersect;
        this.RA = sp.RA;
        this.SA = sp.SA;
        this.SO = sp.SO;
        this.SS = sp.SS;
        this.depT = sp.depT;
        this.sensorType = sp.sensorType;    
    }


}