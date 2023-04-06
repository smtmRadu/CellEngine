using NeuroForge;
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
    public int stepSize;
    public int depositTrail;
    public float pCD; // prob of random change in direction
    public float sMin; // sensivity threshold
    public bool canIntersect;

    public const int sensorWidth = 1;
    public const float otherSpecRepulsionStrength = 10f;
    public PhysarumAgent(Vector2 position, float orientationDegrees, SpeciesParameters param, PhysarumEnvironment env)
    {
        this.speciesID = param.speciesID;
        this.position = position;     
        this.orientationDegrees = orientationDegrees;
        this.sensorType = param.sensoryType;
        this.rotationAngle = param.RA;
        this.sensorAngle = param.SA;
        this.sensorOffset = param.SO;
        this.stepSize = param.SS;
        this.depositTrail = param.depT;
        this.pCD = param.pCD;
        this.sMin = param.sMin;
        this.env = env;
        this.canIntersect = param.canIntersect;
    }

    
    public void MotorStage_1()
    {
        if(Functions.randomValue < pCD)
            orientationDegrees = Random.Range(0f, 360f);

        
        // Calculate next position
        float oldX = position.x;
        float oldY = position.y;

        float orientationRadians = Mathf.Deg2Rad * orientationDegrees;
        float newX = position.x + Mathf.Cos(orientationRadians) * stepSize;
        float newY = position.y + Mathf.Sin(orientationRadians) * stepSize;

        // If can move -> deposit trail in new location
            if (
            newX >= 0 && newX < env.width &&
            newY >= 0 && newY < env.height &&
            (env.agents[(int)newX + (int)newY * env.width] == 0 || canIntersect)
            )
            {
                position.x = newX;
                position.y = newY;

                env.agents[(int)oldX + (int)oldY * env.width] = 0;

                int location = (int)position.x + (int)position.y * env.width;
                env.agents[location] = speciesID;
                env.chemicals[location] += depositTrail;
                env.spec_mask[location] = speciesID;
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

        // Mechanism: 
        // On chemoattraction, if the attractant is from other species, the chemical is purely negates to do not create attraction for it
        // On chemorepulsion, if the attractant is from other species, the chemical is doubled to invoke repulive strength


        // Sample F
        float F_xPos = position.x + Mathf.Cos(dirRad) * sensorOffset;
        float F_yPos = position.y + Mathf.Sin(dirRad) * sensorOffset;
        if (F_xPos < env.width && F_yPos < env.height && F_xPos >= 0 && F_yPos >= 0)
        {
            int p = (int)F_xPos + (int)F_yPos * env.width;
            if (env.spec_mask[p] == speciesID)
                F = env.chemicals[p];
            else
                F = sensorType == SensoryType.ChemoAttraction ? 
                    -env.chemicals[p] : 
                    otherSpecRepulsionStrength * env.chemicals[p];

        }
        // Sample FL
        float FL_xPos = position.x + Mathf.Cos(dirRad + Mathf.Deg2Rad * sensorAngle) * sensorOffset;
        float FL_yPos = position.y + Mathf.Sin(dirRad + Mathf.Deg2Rad * sensorAngle) * sensorOffset;
        if (FL_xPos < env.width && FL_yPos < env.height && FL_xPos >= 0 && FL_yPos >= 0)
        {
            int p = (int)FL_xPos + (int)FL_yPos * env.width;
            if (env.spec_mask[p] == speciesID)
                FL = env.chemicals[p];
            else
                FL = sensorType == SensoryType.ChemoAttraction ? 
                    -env.chemicals[p] : 
                    otherSpecRepulsionStrength * env.chemicals[p];
        }
        // Sample FR
        float FR_xPos = position.x + Mathf.Cos(dirRad - Mathf.Deg2Rad * sensorAngle) * sensorOffset;
        float FR_yPos = position.y + Mathf.Sin(dirRad - Mathf.Deg2Rad * sensorAngle) * sensorOffset;
        if (FR_xPos < env.width && FR_yPos < env.height && FR_xPos >= 0 && FR_yPos >= 0)
        {
            int p = (int)FR_xPos + (int)FR_yPos * env.width;
            if (env.spec_mask[p] == speciesID)
                FR = env.chemicals[p];
            else
                FR = sensorType == SensoryType.ChemoAttraction ? 
                    -env.chemicals[p] : 
                    otherSpecRepulsionStrength * env.chemicals[p];
        }


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
            if (Functions.randomValue < 0.5)
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
        if (F < FL && F < FR)
        {
            // stay facing same direction
            return;
        }
        else if(F > FL && F > FR)
        {

            //Rotate randomly to left or right by RA
            if (Functions.randomValue < 0.5)
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
