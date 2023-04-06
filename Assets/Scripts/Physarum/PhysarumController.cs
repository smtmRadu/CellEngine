using NeuroForge;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.GraphicsBuffer;

public class PhysarumController : MonoBehaviour
{
    public PhysarumEngine engineRef;
    bool isChangingColor = false;

    [Header("Controls")]
    public int penSize = 15;
    public int eraserSize = 40;

    [Tooltip("How often a new change appears")]
    public Vector2 mixColorRange = new Vector2(2.5f, 3.5f);
    private float changeColorTime = 3f;

    [Space]
    public Vector2 decayTRange = new Vector2(0.1f, 0.2f);
    Vector2 colorShiftRange = new Vector2(0.4f, 0.6f);

    [Space]
    List<SensoryType> sensoryTypesRange = new List<SensoryType>() { SensoryType.ChemoAttraction, SensoryType.ChemoRepulsion };
    public Vector2 RA_Range = new Vector2(45f, 135f);
    public Vector2 SA_Range = new Vector2(45f, 135f);
    public Vector2Int SO_Range = new Vector2Int(3, 27);
    public Vector2Int SS_Range = new Vector2Int(1, 3);

    private void Awake()
    {
        if (this.enabled == false)
            return;

        penSize = (int)(penSize * engineRef.resolutionScale);
        eraserSize = (int)(eraserSize * engineRef.resolutionScale);

    }
    private void Start()
    {
        DeployChange(Random.Range(mixColorRange.x * 4, mixColorRange.y * 4));
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
            engineRef.AllowIntersection();
        if(Input.GetKeyUp(KeyCode.S))
            engineRef.DisallowIntersection();

        if (Input.GetKeyDown(KeyCode.R))
            DeployChange(Random.Range(mixColorRange.x, mixColorRange.y));

        if (Input.GetKeyDown(KeyCode.N))
            engineRef.AddSpecies();

        if (Input.GetKeyDown(KeyCode.L))
            engineRef.CutSpecies();

        if (Input.GetKeyDown(KeyCode.A))
            engineRef.ApplyParameters();

        if (Input.GetMouseButton(0))
            DrawAgents();

        if (Input.GetMouseButton(1))
            EraseAgents();

        if (Input.GetKeyDown(KeyCode.Escape))
            SceneManager.LoadScene("MainMenu");

    }
    void DrawAgents()
    {
        Vector3 mousePos = Input.mousePosition * engineRef.resolutionScale;
        Vector2Int mousePosInt = new Vector2Int((int)mousePos.x, (int)mousePos.y);


        for (int i = -penSize; i <= penSize; i++)
        {
            for (int j = -penSize; j <= penSize; j++)
            {

                float deltaDist = Vector2.Distance(mousePosInt, new Vector2(mousePosInt.x + i, mousePosInt.y + j));
                try
                {
                    if (deltaDist < penSize &&
                    engineRef.environment.agents[mousePosInt.x + i + (mousePosInt.y - j) * engineRef.environment.width] == 0 &&
                                        Random.value < 0.25f)
                    {
                        engineRef._createAgent(engineRef.speciesCount, new Vector2Int(mousePosInt.x + i, mousePosInt.y - j), Random.Range(0f, 360f));
                    }
                }
                catch { }
            }

        }
    }
    void EraseAgents()
    {
        Vector3 mousePos = Input.mousePosition * engineRef.resolutionScale;
        Vector2Int mousePosInt = new Vector2Int((int)mousePos.x, (int)mousePos.y);

        for (int i = -eraserSize; i <= eraserSize; i++)
        {
            for (int j = -eraserSize; j <= eraserSize; j++)
            {

                float deltaDist = Vector2.Distance(mousePosInt, new Vector2(mousePosInt.x + i, mousePosInt.y + j));


                if (deltaDist < eraserSize)
                {
                    try
                    {
                        int arrPos = mousePosInt.x + i + (mousePosInt.y - j) * engineRef.environment.width;


                        engineRef.environment.chemicals[arrPos] = 0;
                        engineRef.environment.spec_mask[arrPos] = 0;

                        if (engineRef.environment.agents[arrPos] > 0)
                        {
                            engineRef.environment.agents[arrPos] = 0;
                            // Search for the agent on this position
                            PhysarumAgent x = null;
                            foreach (var ag in engineRef.agents)
                            {
                                if ((int)ag.position.x + (int)ag.position.y * engineRef.environment.width == arrPos)
                                {
                                    x = ag;
                                    break;
                                }
                            }
                            engineRef.agents.Remove(x);
                            engineRef.agentsCount--;
                        }
                    }
                    catch { }

                }


            }

        }
    }




    void DeployChange(float changeColorTime)
    {
       

        this.changeColorTime = changeColorTime;

        Sample_And_Apply();

        if(!isChangingColor)
            StartCoroutine(LerpColors());
        
        
    }
    IEnumerator LerpColors()
    {
        isChangingColor = true;

        Color Old_bg = new Color(engineRef.backgroundColor.r, engineRef.backgroundColor.g, engineRef.backgroundColor.b);
        ChemColors[] old_cc= new ChemColors[engineRef.species_param.Length - 1];
       
        for (int i = 0; i < old_cc.Length; i++)
        {
            old_cc[i].color1 = new Color(engineRef.species_param[i + 1].chemCol1.r, engineRef.species_param[i + 1].chemCol1.g, engineRef.species_param[i+1].chemCol1.b);
            old_cc[i].color2 = new Color(engineRef.species_param[i + 1].chemCol2.r, engineRef.species_param[i + 1].chemCol2.g, engineRef.species_param[i+1].chemCol2.b);
        }

        Color Targ_bg = new Color(Random.Range(0f, 0.1f), Random.Range(0f, 0.1f), Random.Range(0f, 0.1f));
        ChemColors[] targ_cc = new ChemColors[engineRef.species_param.Length - 1];
        for (int i = 0; i <targ_cc.Length; i++)
        {
            targ_cc[i].color1 = new Color(Random.Range(0.4f, 0.8f), Random.Range(0.4f, 0.8f), Random.Range(0.4f, 0.8f));
            targ_cc[i].color2 = new Color(Random.Range(0.1f, 1f), Random.Range(0.1f, 1f), Random.Range(0.1f, 0.9f));
        }
       

        float timeElapsed = 0;
        while(timeElapsed < changeColorTime)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            timeElapsed += Time.deltaTime;
            float tm_lerp = timeElapsed / changeColorTime;

            engineRef.backgroundColor = Color.Lerp(Old_bg, Targ_bg, tm_lerp);
            for (int i = 0; i < targ_cc.Length; i++)
            {
                engineRef.species_param[i + 1].chemCol1 = Color.Lerp(old_cc[i].color1, targ_cc[i].color1, tm_lerp);
                engineRef.species_param[i + 1].chemCol2 = Color.Lerp(old_cc[i].color2, targ_cc[i].color2, tm_lerp);
            }
            
        }
        isChangingColor = false;
    }

    private void Sample_And_Apply()
    {
        
        engineRef.decayT = Random.Range(decayTRange.x, decayTRange.y);
        engineRef.chemColorShift = Random.Range(colorShiftRange.x, colorShiftRange.y);

        foreach (var s_param in engineRef.species_param)
        {
            if (s_param == null)
                continue;
            s_param.sensoryType = Functions.RandomIn(sensoryTypesRange);
            s_param.RA = Random.Range(RA_Range.x, RA_Range.y);
            s_param.SA = Random.Range(SA_Range.x, SA_Range.y);
            s_param.SO = Random.Range(SO_Range.x, SO_Range.y + 1);
            s_param.SS = Random.Range(SS_Range.x, SS_Range.y + 1);
        }
        
        engineRef.ApplyParameters();
    }

    struct ChemColors
    {
        public Color color1;
        public Color color2;
    }
}
