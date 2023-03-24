using UnityEngine;


public class PhysarumAgent
{
    // Real time TRANSFORM
    PhysarumEnvironment env;
    public int speciesID = 1;
    public Vector2 position;
    public float orientationDegrees;

    // Parameters
    public SensoryType sensorType;
    public float rotationAngle;
    public float sensorAngle; //22.5F
    public int sensorOffset;
    public const int sensorWidth = 1;
    public int stepSize;
    public int depositTrail;
    public float pCD; // prob of random change in direction
    public float sMin;
    public bool canIntersect;
   

    public PhysarumAgent(int speciesID, Vector2 position, SensoryType sensorType, float orientationDegrees, float rotationAngle, float sensorAngle, int sensorOffset, int stepSize, int depositTrail, float pCD, float sMin, PhysarumEnvironment env, bool canIntersect)
    {
        this.speciesID = speciesID;
        this.position = position;
        this.sensorType = sensorType;
        this.orientationDegrees = orientationDegrees;
        this.rotationAngle = rotationAngle;
        this.sensorAngle = sensorAngle;
        this.sensorOffset = sensorOffset;
        this.stepSize = stepSize;
        this.depositTrail = depositTrail;
        this.pCD = pCD;
        this.sMin = sMin;
        this.env = env;
        this.canIntersect = canIntersect;
    }

    
    public void MotorStage_1()
    {
        if(Random.value < pCD)
            orientationDegrees = Random.Range(0f, 360f);

        
        // Calculate next position
        float oldX = position.x;
        float oldY = position.y;

        float orientationRadians = Mathf.Deg2Rad * orientationDegrees;
        float newX = position.x + Mathf.Cos(orientationRadians) * stepSize;
        float newY = position.y + Mathf.Sin(orientationRadians) * stepSize;

        // If can move -> deposit trail in new location
        if(
            newX >= 0 && newX < env.width &&
            newY >= 0 && newY < env.height &&
            (env.agents[(int)newX + (int)newY * env.width] == 0 || canIntersect)
          )
        {
            position.x = newX;
            position.y = newY;

            env.agents[(int)oldX + (int)oldY * env.width] = 0;
            env.agents[(int)position.x + (int)position.y * env.width] = speciesID;
            env.chemicals[(int)position.x + (int)position.y * env.width] += depositTrail;
        }
        else // -> give complete new orientation
        {
            orientationDegrees = Random.Range(0f, 360f);
        }
    }
    public void SensoryStage_2()
    {
        // Sample trailmap values
        float F = 0;
        float FL = 0;
        float FR = 0;
        float dirRad = Mathf.Deg2Rad * orientationDegrees;

        // Sample F
        float F_xPos = position.x + Mathf.Cos(dirRad) * sensorOffset;
        float F_yPos = position.y + Mathf.Sin(dirRad) * sensorOffset;
        if (F_xPos < env.width && F_yPos < env.height && F_xPos >= 0 && F_yPos >= 0)
            F = env.chemicals[(int)F_xPos + (int)F_yPos * env.width];
            
        // Sample FL
        float FL_xPos = position.x + Mathf.Cos(dirRad + Mathf.Deg2Rad * sensorAngle) * sensorOffset;
        float FL_yPos = position.y + Mathf.Sin(dirRad + Mathf.Deg2Rad * sensorAngle) * sensorOffset;
        if (FL_xPos < env.width && FL_yPos < env.height && FL_xPos >= 0 && FL_yPos >= 0)
            FL = env.chemicals[(int)FL_xPos + (int)FL_yPos * env.width];

        // Sample FR
        float FR_xPos = position.x + Mathf.Cos(dirRad - Mathf.Deg2Rad * sensorAngle) * sensorOffset;
        float FR_yPos = position.y + Mathf.Sin(dirRad - Mathf.Deg2Rad * sensorAngle) * sensorOffset;
        if (FR_xPos < env.width && FR_yPos < env.height && FR_xPos >= 0 && FR_yPos >= 0)
            FR = env.chemicals[(int)FR_xPos + (int)FR_yPos * env.width];


        if (sensorType == SensoryType.ChemoAttraction)
            ChemoAttraction(F, FL, FR);
        else
            ChemoRepulsion(F, FL, FR);
    }

    public void ChemoAttraction(float F, float FL, float FR)
    {
        if (F > FL && F > FR)
        {
            // stay facing same direction
            return;
        }
        else if (F < FL && F < FR)
        {
            //Rotate randomly to left or right by RA
            if (Random.value < 0.5f)
                orientationDegrees += rotationAngle; // left
            else
                orientationDegrees -= rotationAngle; // right
        }
        else if (FL < FR)
        {
            //Rotate right by RA
            orientationDegrees -= rotationAngle;
        }
        else if (FR < FL)
        {
            //Rotate left by RA
            orientationDegrees += rotationAngle;
        }
        else
        {
            // continue facing same direction
        }
    }
    public void ChemoRepulsion(float F, float FL, float FR)
    {
        if(F < FL && F < FR)
        {
            // stay facing same direction
            return;
        }
        else if(F > FL && F > FR)
        {

            //Rotate randomly to left or right by RA
            if (Random.value < 0.5f)
                orientationDegrees += rotationAngle; // left
            else
                orientationDegrees -= rotationAngle; // right
        }
        else if(FL > FR)
        {
            //Rotate right by RA
            orientationDegrees -= rotationAngle;
        }
        else if (FR > FL)
        {
            //Rotate left by RA
            orientationDegrees += rotationAngle;
        }
        else
        {
            // continue facing same direction
        }
    }
}
