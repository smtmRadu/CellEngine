using UnityEngine;

public class SpeciesParameters : MonoBehaviour
{
    
    public int speciesID = -1;
    public Color chemCol1 = Color.red;
    public Color chemCol2 = Color.blue;
    public SensoryType sensoryType = SensoryType.ChemoAttraction;
    public float RA = 45f;
    public float SA = 22.5f;
    public int SO = 9;
    public int SS = 1;
    public int depT = 5;
    public float pCD = 0f;
    public float sMin = 0f;
    public const int sensorWidth = 1;
    public bool canIntersect = false;
}
