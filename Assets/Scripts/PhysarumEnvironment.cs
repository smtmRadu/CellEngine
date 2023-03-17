using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysarumEnvironment
{
    public bool[,] agents;
    public float[,] chemicals;

    public PhysarumEnvironment(int w, int h)
    {
        agents = new bool[w, h];
        chemicals = new float[w, h];
    }

}
