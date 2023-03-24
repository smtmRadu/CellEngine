using NeuroForge;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class PhysarumEngine : MonoBehaviour
{
    public Image ENV_Img;
    public ComputeShader shader;
    public InitAgentsType initializationType;
    public int agentsCount = 0;
    public int steps = 0;
    const int THREADS = 32; // keep like this only
    [Header("Controls")]
    public int penSize = 15;
    public int eraserSize = 40;

    [Header("Environment Hyperparameters")]
    [Range(0.001f, 1f)] public float timeScale = 1;
    [Range(0.001f, 1f)] public float resolutionScale = 1;
    [Range(0, 0.9f)] public float populationPercentage = 0.05f;
    [Range(0, 0.5f)] public float decayT = 0.1f;
    public Color agentsColor = Color.green;
    public Color chemColor1 = Color.red;
    [Range(1e-4f, 999e-3f)] public float chemColorShift = 0.5f;
    public Color chemColor2 = Color.yellow;
    public Color backgroundColor = Color.black;

    [Header("Agents Hyperparameters")]
    public SensoryType sensoryType = SensoryType.ChemoAttraction;
    [Range(0f, 180f)] public float rotationAngle = 45f;
    [Range(0f, 180f)] public float sensorAngle = 22.5f;
    [Range(1f, 45f)] public int sensorOffset = 9;              
    [Range(1,5)] public int stepSize = 1;
    [Range(1,10)] public int depositTrail = 5;
    [Range(0f, 0.1f)] public float pCD = 0f;
    [Range(0f, 0.1f)] public float sMIn = 0;
    public const int sensorWidth = 1;

    [Header("Debug")]
    public bool renderAgents = true;
    public bool useSensors = true;
    public bool allowIntersection = false;

    PhysarumEnvironment environment;
    private Texture2D ENV_Tex;
    private List<PhysarumAgent> agents = new List<PhysarumAgent>();

    void Awake()
    {
      
        Application.runInBackground = true;
        int env_width = (int)(Screen.width * resolutionScale);
        int env_height = (int)(Screen.height * resolutionScale);

        // Initialize environment
        environment = new PhysarumEnvironment(env_width, env_height);
        ENV_Tex = new Texture2D(env_width, env_height);
        ENV_Tex.filterMode = FilterMode.Point;
        ENV_Img.sprite = Sprite.Create(ENV_Tex, new Rect(0, 0, env_width, env_height), new Vector2(.5f, .5f));
        ENV_Img.color = Color.white;


        penSize = (int)(penSize * resolutionScale);
        eraserSize = (int)(eraserSize * resolutionScale);

        InitAgents(env_width, env_height);

    }
    void Update()
    {
        if (Time.frameCount % (int)(1f / timeScale) != 0)
            return;

        DrawAgents();
        EraseAgents();
        AgentsStep();
        FilterStep();
        RenderStep();
        steps++;
    }

    void InitAgents(int env_width, int env_height)
    {
        if(initializationType == InitAgentsType.Random)
        {
            for (int i = 0; i < populationPercentage * env_height * env_width; i++)
            {
                int randX = Functions.RandomRange(1, env_width);
                int randY = Functions.RandomRange(1, env_height);
                Vector2Int randP = new Vector2Int(randX, randY);
                CreateAgent(1, randP, Random.Range(0f,360f));
            }
        }
        else
        {
            // circle diameter.. like 80% of screen height
            float c_spawn_centerX = env_width / 2;
            float c_spawn_centerY = env_height / 2;
            float c_spawn_radius = (env_height * 0.8f) / 2f;

            for (int i = 0; i < env_width; i++)
            {
                for (int j = 0; j < env_height; j++)
                {
                    float distance = Mathf.Sqrt(
                            (i - c_spawn_centerX) * (i - c_spawn_centerX) +
                            (j - c_spawn_centerY) * (j - c_spawn_centerY));

                    if (!(distance < c_spawn_radius && Random.value < populationPercentage))
                        continue;

                    float orientation = Mathf.Atan2(j - c_spawn_centerY, i - c_spawn_centerX) * Mathf.Rad2Deg;

                    if (initializationType == InitAgentsType.CircleLookingInwards)
                        orientation += 180f;

                    CreateAgent(1, new Vector2Int(i, j), orientation);

                }
            }
            // we use population percentage as a spawn rate

        }
    }
    public void ApplyHPChange()
    {
        foreach (var agent in agents)
        {
            agent.sensorType = sensoryType;
            agent.rotationAngle = rotationAngle;
            agent.sensorAngle = sensorAngle;
            agent.sensorOffset = sensorOffset;
            agent.stepSize = stepSize;
            agent.depositTrail = depositTrail;
            agent.pCD = pCD;
            agent.sMin = sMIn;
        }
    }
    void DrawAgents()
    {
        if (!Input.GetMouseButton(0))
            return;

        Vector3 mousePos = Input.mousePosition * resolutionScale;
        Vector2Int mousePosInt = new Vector2Int((int)mousePos.x, (int)mousePos.y);

        
        for (int i = -penSize; i <= penSize; i++)
        {
            for (int j = -penSize; j <= penSize; j++)
            {

                float deltaDist = Vector2.Distance(mousePosInt, new Vector2(mousePosInt.x + i, mousePosInt.y + j));
                try
                {
                    if (deltaDist < penSize &&
                                        environment.agents[mousePosInt.x + i + (mousePosInt.y - j) * environment.width] == 0 &&
                                        Random.value < 0.2f)
                    {
                        CreateAgent(1, new Vector2Int(mousePosInt.x + i, mousePosInt.y - j), Random.Range(0f, 360f));
                    }
                }
                catch { } 
            }

        }
    }
    void EraseAgents()
    {
        if (!Input.GetMouseButton(1))
            return;

        Vector3 mousePos = Input.mousePosition * resolutionScale;
        Vector2Int mousePosInt = new Vector2Int((int)mousePos.x, (int)mousePos.y);


        for (int i = -eraserSize; i <= eraserSize; i++)
        {
            for (int j = -eraserSize; j <= eraserSize; j++)
            {

                float deltaDist = Vector2.Distance(mousePosInt, new Vector2(mousePosInt.x + i, mousePosInt.y + j));
                

                if(deltaDist < eraserSize)
                {
                    try
                    {
                        int arrPos = mousePosInt.x + i + (mousePosInt.y - j) * environment.width;


                        environment.chemicals[arrPos] = 0;

                        if (environment.agents[arrPos] > 0)
                        {
                            environment.agents[arrPos] = 0;
                            // Search for the agent on this position
                            PhysarumAgent x = null;
                            foreach (var ag in agents)
                            {
                                if ((int)ag.position.x + (int)ag.position.y * environment.width == arrPos)
                                {
                                    x = ag;
                                    break;
                                }
                            }
                            agents.Remove(x);
                            agentsCount--;
                        }
                    }
                    catch { }
                    
                }

               
            }

        }
    }
    void CreateAgent(int speciesID, Vector2Int agentPosition, float orientationDeg)
    {
        PhysarumAgent newAgent =
            new PhysarumAgent(speciesID, agentPosition, sensoryType, orientationDeg, rotationAngle, sensorAngle,
                              sensorOffset, stepSize, depositTrail,
                              pCD, sMIn, environment, allowIntersection);
        agents.Add(newAgent);
        agentsCount++;
    }
   

    void AgentsStep()
    {
        // At each execution step of the scheduler, every agent attempts to move forward one step in the current
        // direction.After every agent has attempted to move, the entire population performs its sensory
        //behavior.If the movement is successful(i.e., if the next site is not occupied) the agent moves to the
        //new site and deposits a constant chemoattractant value.


        //Agents are selected from the population randomly in the motor and sensory stages to avoid the
        //possibility of long term bias by sequential ordering

        Functions.Shuffle(agents);
        foreach (var ag in agents)
        {
            ag.MotorStage_1();
        }

        if (!useSensors)
            return;

        Functions.Shuffle(agents);
        foreach (var ag in agents)
        {
            ag.SensoryStage_2();
        }
    }
    void FilterStep()
    {
        int W = environment.width;
        int H = environment.height;
        
        int kernelIndex = shader.FindKernel("FilterKern");
        
        shader.SetFloat("decayT", decayT);
        shader.SetInt("W", W);
        shader.SetInt("H", H);
       
        ComputeBuffer inputBuffer = new ComputeBuffer(W * H, sizeof(float));
        inputBuffer.SetData(environment.chemicals);
        shader.SetBuffer(kernelIndex, "inputBuffer", inputBuffer);
        
        ComputeBuffer outputBuffer = new ComputeBuffer(W * H, sizeof(float));
        outputBuffer.SetData(environment.chemicals);
        shader.SetBuffer(kernelIndex, "outputBuffer", outputBuffer);
        
        int numThreadGroupsX = (W + THREADS - 1) / THREADS;
        int numThreadGroupsY = (H + THREADS - 1) / THREADS;
              
        shader.Dispatch(kernelIndex, numThreadGroupsX, numThreadGroupsY, 1);
        
        inputBuffer.Dispose();
        outputBuffer.GetData(environment.chemicals);
        outputBuffer.Dispose();
    }
    void RenderStep()
    {
        Color[] pixels = new Color[environment.agents.Length];
        for (int i = 0; i < environment.agents.Length; i++)
        {
            if(renderAgents && environment.agents[i] > 0)
            {
                // Render agents
                pixels[i] = agentsColor;
                continue;
            }
            
            if (environment.chemicals[i] > chemColorShift) 
                pixels[i] = Color.Lerp(chemColor2, chemColor1, (environment.chemicals[i] - chemColorShift) / chemColorShift);
            else
                pixels[i] = Color.Lerp(backgroundColor, chemColor2, environment.chemicals[i] / chemColorShift);
        }

        ENV_Tex.SetPixels(pixels);
        ENV_Tex.Apply();
    }
}

///  Todo: Use the matrix map only as a float[] not as a matrix for performance
/// 
///  Base settings for Chemoattractant
///  
///  RA 45, SA 45, SO 9, 300x300
///  %p5 - %p60 X 0.0001 - 0.5 decayT
///  
///  SO 9, 500x500, %p15, 0.1dT
///  RA 0 - 180, SA 0 - 180
///  
///  Reticular -> RA 45 SA 45
///  Labyrinthine -> RA 90 SA 90
///  Island -> RA RA 120 SA 120
///  Hybrid -> other
///  
///
///  Base settings for Chemorepulsion
///  %p 15, SO 19
///  RA 0 - 180, SA 0 - 180
///  
///  Honeycomb -> %p20, RA 45, SA 45, SO 45
///  Stripes -> %p20, RA 67.5, SA 112.5, SO 13



