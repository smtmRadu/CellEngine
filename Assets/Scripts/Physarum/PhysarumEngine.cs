using NeuroForge;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhysarumEngine : MonoBehaviour
{
    public TMPro.TMP_Text populationTMPro;
    public TMPro.TMP_Text speciesNoTMPro;
    public Image ENV_Img;
    public InitAgentsType initializationType;
    public PhysarumEnvironment environment;
    private Texture2D ENV_Tex;
    public SpeciesParameters[] species_param;
    public List<PhysarumAgent> agents = new List<PhysarumAgent>();


    [Header("Shaders")]
    public ComputeShader filterShader;
    public ComputeShader imageShader;
    public ComputeShader agentsShader;

   
    [Header("Readonly")]
    public int speciesCount = 0;
    public int steps = 0;
    public int THREADS = 32; // keep like this only

    [Header("Environment Hyperparameters")]
    public WhatWeRender whatWeRender = WhatWeRender.Chemicals;
    [Range(0.001f, 1f)] public float resolutionScale = 1;
    [Range(0, 0.9f)] public float populationPercentageInit = 0.05f;
    [Range(0, 0.5f)] public float decayT = 0.1f;
    [Range(1e-4f, 999e-3f)] public float chemColorShift = 0.5f;
    public Color backgroundColor = Color.black;
    
  

    public bool useSensors = true;
    private int resolution;


    void Awake()
    {
        Application.runInBackground = true;
        int env_width = (int)(Screen.width * resolutionScale);
        int env_height = (int)(Screen.height * resolutionScale);
        resolution = env_width * env_height;

        // Initialize environment
        environment = new PhysarumEnvironment(env_width, env_height);
        ENV_Tex = new Texture2D(env_width, env_height);
        ENV_Tex.filterMode = FilterMode.Point;
        ENV_Img.sprite = Sprite.Create(ENV_Tex, new Rect(0, 0, env_width, env_height), new Vector2(.5f, .5f));
        ENV_Img.color = Color.white;
        threadGroupsX = (environment.width + THREADS - 1) / THREADS;
        threadGroupsY = (environment.height + THREADS - 1) / THREADS;

        // Compute shaders
        agents_buff_in = new ComputeBuffer(resolution, sizeof(int));
        chemicals_buff_in = new ComputeBuffer(resolution, sizeof(float));
        spec_mask_buff_in = new ComputeBuffer(resolution, sizeof(float));
        chemicals_buff_out = new ComputeBuffer(resolution, sizeof(float));
        spec_mask_buff_out = new ComputeBuffer(resolution, sizeof(int));
        DISPLAY_buff_out = new ComputeBuffer(resolution, sizeof(float) * 4);
        pixels = new Color[resolution];

       

        InitializeFirstSpecies(env_width, env_height);

    }
    void Update()
    {
        AgentsStep_CPU();
        switch(whatWeRender)
        {
            case WhatWeRender.Chemicals:
                RenderChemis_GPU();
                break;
            case WhatWeRender.Agents:
                RenderAgents_GPU();
                break;
            case WhatWeRender.Image:
                RenderImages_GPU();
                break;
        }
        steps++;
    }

    private void InitializeFirstSpecies(int env_width, int env_height)
    {
        if (initializationType == InitAgentsType.Random)
        {
            for (int i = 0; i < populationPercentageInit * env_height * env_width; i++)
            {
                int randX = Functions.RandomRange(1, env_width);
                int randY = Functions.RandomRange(1, env_height);
                Vector2Int randP = new Vector2Int(randX, randY);
                _createAgent(1, randP, Random.Range(0f, 360f));
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

                    if (!(distance < c_spawn_radius && Random.value < populationPercentageInit))
                        continue;

                    float orientation = Mathf.Atan2(j - c_spawn_centerY, i - c_spawn_centerX) * Mathf.Rad2Deg;

                    if (initializationType == InitAgentsType.CircleLookingInwards)
                        orientation += 180f;

                    _createAgent(1, new Vector2Int(i, j), orientation);

                }
            }
            // we use population percentage as a spawn rate

        }
        speciesCount++;
    }
    public void _createAgent(int speciesID, Vector2Int agentPosition, float orientationDeg)
    {
        PhysarumAgent newAgent =
            new PhysarumAgent(speciesID, agentPosition, orientationDeg, species_param[speciesID], environment);
        agents.Add(newAgent);
        populationTMPro.text = "Population: " + ((float)agents.Count / resolution).ToString("0.000") + "%";
    }

    public void AddSpecies()
    {
        if(species_param.Length - 1 == speciesCount) 
        {
            Debug.Log("Maximum species allowed is " + speciesCount);
            return;
        }
        speciesCount++;

        float chance = 1f / speciesCount;
        for (int i = 0; i < agents.Count; i++)
        {
            // there is 1/species count change to transform this agent into the new species
            if (Functions.randomValue < chance)
            {
                agents[i] = new PhysarumAgent(speciesCount, agents[i].position, agents[i].orientationDegrees, species_param[speciesCount], environment);
            }
        }

        speciesNoTMPro.text = "Species No.: " + speciesCount;
    }
    public void CutSpecies()
    {
        if(speciesCount == 1)
        {
            Debug.Log("Maximum species allowed is 1");
            return;
        }
       
        for (int i = 0; i < agents.Count; i++)
        {
            if (agents[i].speciesID != speciesCount)
                continue;

            int newSpec = Random.Range(1, speciesCount);
            agents[i] = new PhysarumAgent(newSpec, agents[i].position, agents[i].orientationDegrees, species_param[newSpec], environment);
        }

        speciesCount--;

        speciesNoTMPro.text = "Species No.: " + speciesCount;
    }
    public void AllowIntersection()
    {
        useSensors = false;
        foreach (var item in species_param)
        {
            if(item)
                item.canIntersect = true;
        }
    }
    public void DisallowIntersection()
    {
        useSensors = true;
        foreach (var item in species_param)
        {
            if(item)
                 item.canIntersect = false;
        }
    }


    // ONLY RENDERING------------------------------------------------------------------------------------------------------------------------------------------
    ComputeBuffer chemicals_buff_in;
    ComputeBuffer spec_mask_buff_in;
    ComputeBuffer agents_buff_in;

    ComputeBuffer chemicals_buff_out;
    ComputeBuffer spec_mask_buff_out;
    ComputeBuffer DISPLAY_buff_out;

    ComputeBuffer colors1_buff_in;
    ComputeBuffer colors2_buff_in;
    Color[] pixels;

    private int threadGroupsX;
    private int threadGroupsY;
    void AgentsStep_CPU()
    {
        // At each execution step of the scheduler, every agent attempts to move forward one step in the current
        // direction.After every agent has attempted to move, the entire population performs its sensory
        //behavior.If the movement is successful(i.e., if the next site is not occupied) the agent moves to the
        //new site and deposits a constant chemoattractant value.


        //Agents are selected from the population randomly in the motor and sensory stages to avoid the
        //possibility of long term bias by sequential ordering


        // For efficiency reasons, we comment this line
        //Functions.Shuffle(agents);

        foreach (var x in agents)
        {
            x.MotorStage_1();
        }
         
         if (!useSensors)
            return;

         System.Threading.Tasks.Parallel.ForEach(agents, x =>
         {
             x.SensoryStage_2();
         });
    }

    void RenderChemis_GPU()
    {
        // IO buffers
        chemicals_buff_in.SetData(environment.chemicals);
        spec_mask_buff_in.SetData(environment.spec_mask);

        // On species index 0, there is no species
        colors1_buff_in = new ComputeBuffer(speciesCount + 1, sizeof(float) * 4);
        colors2_buff_in = new ComputeBuffer(speciesCount + 1, sizeof(float) * 4);
        colors1_buff_in.SetData(new[] {backgroundColor}, 0, 0, 1);
        colors2_buff_in.SetData(new[] { backgroundColor }, 0, 0, 1);
        for (int i = 1; i <= speciesCount; i++)
        {
            colors1_buff_in.SetData(new[] { species_param[i].chemCol1 }, 0, i, 1);
            colors2_buff_in.SetData(new[] { species_param[i].chemCol2 }, 0, i, 1);
        }

        filterShader.SetBuffer(0, "chemicals_buff_in", chemicals_buff_in);
        filterShader.SetBuffer(0, "spec_mask_buff_in", spec_mask_buff_in);
        filterShader.SetBuffer(0, "chemicals_buff_out", chemicals_buff_out);
        filterShader.SetBuffer(0, "spec_mask_buff_out", spec_mask_buff_out);
        filterShader.SetBuffer(0, "pix_displ_buff_out", DISPLAY_buff_out);
                         
        filterShader.SetBuffer(0, "colors1_buff_in", colors1_buff_in);
        filterShader.SetBuffer(0, "colors2_buff_in", colors2_buff_in);
        filterShader.SetFloat("chemColorShift", chemColorShift);

        filterShader.SetFloat("decayT", decayT);
        filterShader.SetInt("W", environment.width);
        filterShader.SetInt("H", environment.height);



        // Run CS
        filterShader.Dispatch(
            0, 
            threadGroupsX,
            threadGroupsY,
            1);

        chemicals_buff_out.GetData(environment.chemicals);
        spec_mask_buff_out.GetData(environment.spec_mask);

        DISPLAY_buff_out.GetData(pixels);
        ENV_Tex.SetPixels(pixels);
        ENV_Tex.Apply();

        colors1_buff_in.Release();
        colors2_buff_in.Release();
    }
    void RenderAgents_GPU()
    {
        // IO buffers
        agents_buff_in.SetData(environment.agents);
        chemicals_buff_in.SetData(environment.chemicals);
        spec_mask_buff_in.SetData(environment.spec_mask);

        // On species index 0, there is no species
        colors1_buff_in = new ComputeBuffer(speciesCount + 1, sizeof(float) * 4);
        colors1_buff_in.SetData(new[] { backgroundColor }, 0, 0, 1);
        for (int i = 1; i <= speciesCount; i++)
        {
            colors1_buff_in.SetData(new[] { species_param[i].chemCol1 }, 0, i, 1);
        }

        agentsShader.SetBuffer(0, "agents_buff_in", agents_buff_in);
        agentsShader.SetBuffer(0, "chemicals_buff_in", chemicals_buff_in);
        agentsShader.SetBuffer(0, "spec_mask_buff_in", spec_mask_buff_in);
        agentsShader.SetBuffer(0, "chemicals_buff_out", chemicals_buff_out);
        agentsShader.SetBuffer(0, "spec_mask_buff_out", spec_mask_buff_out);
        agentsShader.SetBuffer(0, "pix_displ_buff_out", DISPLAY_buff_out);

        agentsShader.SetBuffer(0, "colors1_buff_in", colors1_buff_in);
        agentsShader.SetFloat("chemColorShift", chemColorShift);

        agentsShader.SetFloat("decayT", decayT);
        agentsShader.SetInt("W", environment.width);
        agentsShader.SetInt("H", environment.height);



        // Run CS
        agentsShader.Dispatch(
            0,
            threadGroupsX,
            threadGroupsY,
            1);

        chemicals_buff_out.GetData(environment.chemicals);
        spec_mask_buff_out.GetData(environment.spec_mask);

        DISPLAY_buff_out.GetData(pixels);
        ENV_Tex.SetPixels(pixels);
        ENV_Tex.Apply();

        colors1_buff_in.Release();
    }
    void RenderImages_GPU()
    {
        // On species index 0, there is no species
        colors1_buff_in = new ComputeBuffer(speciesCount + 1, sizeof(float) * 4);
        colors1_buff_in.SetData(new[] { backgroundColor }, 0, 0, 1);
        for (int i = 1; i <= speciesCount; i++)
        {
            colors1_buff_in.SetData(new[] { species_param[i].chemCol1 }, 0, i, 1);
        }

        agents_buff_in.SetData(environment.agents);
        imageShader.SetBuffer(0, "agents_buff_in", agents_buff_in);
        imageShader.SetBuffer(0, "pix_displ_buff_out", DISPLAY_buff_out);
        imageShader.SetBuffer(0, "colors1_buff_in", colors1_buff_in);

        imageShader.SetInt("W", environment.width);
        imageShader.SetInt("H", environment.height);

        // Run CS
        imageShader.Dispatch(
            0,
            threadGroupsX,
            threadGroupsY,
            1);

        DISPLAY_buff_out.GetData(pixels);
        ENV_Tex.SetPixels(pixels);
        ENV_Tex.Apply();

        colors1_buff_in.Release();
    }
   
    private void OnApplicationQuit()
    {
        agents_buff_in.Release();
        chemicals_buff_in.Release();
        spec_mask_buff_in.Release();
        chemicals_buff_in.Release();
        spec_mask_buff_out.Release();
        chemicals_buff_out.Release();
        DISPLAY_buff_out.Release();
    }
}

public enum WhatWeRender
{
    Chemicals,
    Agents,
    Image
}


