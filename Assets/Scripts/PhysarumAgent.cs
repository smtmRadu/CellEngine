using NeuroForge;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class PhysarumAgent
{
    public Vector2 position;
    public float orientationDegrees;
    public float rotationAngle = 45;
    public float sensorAngle = 45F; //22.5F
    public int sensorOffset = 9;
    public int sensorWidth = 1;
    public int stepSize = 1;
    public int depositTrail = 5;

    // Not used for now
    public float pCD = 0;
    public float sMin = 0;

    PhysarumEnvironment env;

    public PhysarumAgent(Vector2 position, float orientationDegrees, float rotationAngle, float sensorAngle, int sensorOffset, int sensorWidth, int stepSize, int depositTrail, float pCD, float sMin, PhysarumEnvironment env)
    {
        this.position = position;
        this.orientationDegrees = orientationDegrees;
        this.rotationAngle = rotationAngle;
        this.sensorAngle = sensorAngle;
        this.sensorOffset = sensorOffset;
        this.sensorWidth = sensorWidth;
        this.stepSize = stepSize;
        this.depositTrail = depositTrail;
        this.pCD = pCD;
        this.sMin = sMin;
        this.env = env;
    }


    
    public void MotorStage_1()
    {
        // Calculate next position
        float oldX = position.x;
        float oldY = position.y;

        float orientationRadians = Mathf.Deg2Rad * orientationDegrees;
        float newX = position.x + Mathf.Cos(orientationRadians) * stepSize;
        float newY = position.y + Mathf.Sin(orientationRadians) * stepSize;

        // To modify in the future. Actually if the agent pass the margins, we just not render him, we do not block him here
        newX = Mathf.Clamp(newX, 0, env.agents.GetLength(0) - 1);
        newY = Mathf.Clamp(newY, 0, env.agents.GetLength(1) - 1);

        // If can move -> deposit trail in new location
        if(env.agents[(int)newX, (int)newY] == false)
        {
            position.x = newX;
            position.y = newY;

            env.agents[(int)oldX, (int)oldY] = false;
            env.agents[(int)position.x, (int)position.y] = true;
            env.chemicals[(int)position.x, (int)position.y] += depositTrail;
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
        float F_yPos = position.y + Mathf.Cos(dirRad) * sensorOffset;
        if(F_xPos < env.chemicals.GetLength(0) && F_yPos < env.chemicals.GetLength(1) && F_xPos>0 && F_yPos>0)
            F = env.chemicals[(int) F_xPos, (int)F_yPos];

        // Sample FL
        float FL_xPos = position.x + Mathf.Cos(dirRad + Mathf.Deg2Rad * sensorAngle) * sensorOffset;
        float FL_yPos = position.y + Mathf.Cos(dirRad + Mathf.Deg2Rad * sensorAngle) * sensorOffset;
        if (FL_xPos < env.chemicals.GetLength(0) && FL_yPos < env.chemicals.GetLength(1) && FL_xPos > 0 && FL_yPos > 0)
            FL = env.chemicals[(int)FL_xPos, (int)FL_yPos];
       
        // Sample FR
        float FR_xPos = position.x + Mathf.Cos(dirRad - Mathf.Deg2Rad * sensorAngle) * sensorOffset;
        float FR_yPos = position.y + Mathf.Cos(dirRad - Mathf.Deg2Rad * sensorAngle) * sensorOffset;
        if (FR_xPos < env.chemicals.GetLength(0) && FR_yPos < env.chemicals.GetLength(1) && FR_xPos > 0 && FR_yPos > 0)
            FR = env.chemicals[(int)FR_xPos, (int)FR_yPos];

        // Algorithm
        if (F > FL && F > FR)
        {
            // stay facing same direction
            return;
        }
        else if(F < FL && F < FR)
        {
            //Rotate randomly to left or right by RA
            if (Random.value < 0.5f)
                orientationDegrees += rotationAngle; // left
            else
                orientationDegrees -= rotationAngle; // right
        }
        else if(FL < FR)
        {
            //Rotate right by RA
            orientationDegrees -= rotationAngle;
        }
        else if(FR < FL)
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
