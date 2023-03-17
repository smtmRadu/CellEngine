using NeuroForge;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class PhysarumEngine : MonoBehaviour
{
    public Image ENV_Img;
    public int agentsCount = 0;

    [Header("Environment Hyperparameters")]
    [Range(0.001f, 1f)] public float timeScale = 1;
    [Range(0.001f, 1f)] public float resolutionScale = 1;
    [Range(0, 0.15f)] public float populationPercentage = 0.05f;
    [Range(0, 1f)] public float decayT = 0.1f;
    public Color agentsColor = Color.green;
    public Color chemicalsColor = Color.red;
    public Color backgroundColor = Color.black;

    [Header("Agents Hyperparameters")]
    public float rotationAngle = 45f;
    public float sensorAngle = 45f; //22.5f
    public int sensorOffset = 9;
    public int sensorWidth = 1;
    public int stepSize = 1;
    public int despositTrail = 5;
    public int pCD = 0;
    public int sMIn = 0;

    PhysarumEnvironment environment;
    private Texture2D ENV_Tex;
    private List<PhysarumAgent> agents = new List<PhysarumAgent>();
    private float[,] kernel = new float[3, 3]
    {
        {1/9f, 1/9f, 1/9f},
        {1/9f, 1/9f, 1/9f},
        {1/9f, 1/9f, 1/9f}
    };
    public void Awake()
    {
        int env_width = (int)(Screen.width * resolutionScale);
        int env_height = (int)(Screen.height * resolutionScale);

        // Initialize environment
        environment = new PhysarumEnvironment(env_width, env_height);
        ENV_Tex = new Texture2D(env_width, env_height);
        ENV_Tex.filterMode = FilterMode.Point;
        ENV_Img.sprite = Sprite.Create(ENV_Tex, new Rect(0, 0, env_width, env_height), new Vector2(.5f, .5f));

        // Initialize population
        for (int i = 0; i < populationPercentage * env_height * env_width; i++)
        {
            int randX = Functions.RandomRange(1, env_width);
            int randY = Functions.RandomRange(1, env_height);
            Vector2Int randP = new Vector2Int(randX, randY);
            CreateAgent(randP);
        }

    }
    public void Update()
    {
        if (Time.frameCount % (int)(1f / timeScale) != 0)
            return;

        Draw();
        AgentsStep();
        DiffuseChemicals();
        RenderAgents();
    }

    public void Draw()
    {
        if (!Input.GetMouseButton(0))
            return;

        Vector3 mousePos = Input.mousePosition * resolutionScale;
        Vector2Int agPos = new Vector2Int((int)mousePos.x, (int)mousePos.y);

        if (environment.agents[agPos.x, agPos.y] == true)
            return;

        CreateAgent(agPos);
    }
    void CreateAgent(Vector2Int agentPosition)
    {
        PhysarumAgent newAgent =
            new PhysarumAgent(agentPosition, Random.Range(0f, 360f), rotationAngle, sensorAngle,
                              sensorOffset, sensorWidth, stepSize, despositTrail,
                              pCD, sMIn, environment);
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
        Functions.Shuffle(agents);
        foreach (var ag in agents)
        {
            ag.SensoryStage_2();
        }
    }
    void DiffuseChemicals()
    {
        // remember here -> padd the image you mf
        float[,] diffused_chemicals_map = new float[environment.chemicals.GetLength(0), environment.chemicals.GetLength(1)];

        for (int i = 1; i < environment.chemicals.GetLength(0) - 1; i++)
        {
            for (int j = 1; j < environment.chemicals.GetLength(1) - 1; j++)
            {
                float sum = 0f;
                for (int k_i = 0; k_i < kernel.GetLength(0); k_i++)
                {
                    for (int k_j = 0; k_j < kernel.GetLength(1); k_j++)
                    {
                        sum += kernel[k_i, k_j] * environment.chemicals[i + k_i - 1, j + k_j - 1];
                    }
                
                }
                diffused_chemicals_map[i, j] = (1 - decayT) * sum;
            }
        }
        environment.chemicals = diffused_chemicals_map;
    }
    void RenderAgents()
    {
        Color[,] pixels = new Color[environment.agents.GetLength(0), environment.agents.GetLength(1)];

        for (int i = 0; i < pixels.GetLength(0); i++)
        {
            for (int j = 0; j < pixels.GetLength(1); j++)
            {
                if (environment.agents[i, j] == true)
                    pixels[i, j] = agentsColor;
                else
                    pixels[i, j] = Color.Lerp(backgroundColor, chemicalsColor, environment.chemicals[i, j]);
            }
        }
       
        ENV_Tex.SetPixels(Functions.FlatOf(pixels));
        ENV_Tex.Apply();
    }


}
