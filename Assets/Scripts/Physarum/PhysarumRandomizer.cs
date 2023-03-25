using NeuroForge;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PhysarumRandomizer : MonoBehaviour
{
    public PhysarumEngine engineRef;
    bool isChangingColor = false;

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
    public Vector2Int stepSizeRange = new Vector2Int(1, 3);

    private void Awake()
    {
        if (this.enabled == false)
            return;

        DeployChange(Random.Range(mixColorRange.x / 2f, mixColorRange.y / 2f));
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            engineRef.useSensors = false;
            engineRef.allowIntersection = true;
        }

        if(Input.GetKeyUp(KeyCode.S))
        {
            engineRef.useSensors = true;
            engineRef.allowIntersection = false;
        }

        if (Input.GetKeyDown(KeyCode.R))
            DeployChange(Random.Range(mixColorRange.x, mixColorRange.y));
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

        //Color Old_ag = new Color(engineRef.agentsColor.r, engineRef.agentsColor.g, engineRef.agentsColor.b);
        Color Old_c1 = new Color(engineRef.chemColor1.r, engineRef.chemColor1.g, engineRef.chemColor1.b);
        Color Old_c2 = new Color(engineRef.chemColor2.r, engineRef.chemColor2.g, engineRef.chemColor2.b);
        Color Old_bg = new Color(engineRef.backgroundColor.r, engineRef.backgroundColor.g, engineRef.backgroundColor.b);

        //Color Targ_ag = new Color(Random.value, Random.value, Random.value);
        Color Targ_c1 = new Color(Random.Range(0.4f, 0.8f), Random.Range(0.4f, 0.8f), Random.Range(0.3f, 0.8f));
        Color Targ_c2 = new Color(Random.Range(0.1f, 1f), Random.Range(0.1f, 1f), Random.Range(0.1f, 0.9f));
        Color Targ_bg = new Color(Random.Range(0f, 0.1f), Random.Range(0f, 0.1f), Random.Range(0f, 0.1f));

        float timeElapsed = 0;
        while(timeElapsed < changeColorTime)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            timeElapsed += Time.deltaTime;
            float tm_lerp = timeElapsed / changeColorTime;

            //engineRef.agentsColor = Color.Lerp(Old_ag, Targ_ag, tm_lerp);
            engineRef.chemColor1 = Color.Lerp(Old_c1, Targ_c1, tm_lerp);
            engineRef.chemColor2 = Color.Lerp(Old_c2, Targ_c2, tm_lerp);
            engineRef.backgroundColor = Color.Lerp(Old_bg, Targ_bg, tm_lerp);
        }
        isChangingColor = false;
    }

    private void Sample_And_Apply()
    {
        engineRef.sensoryType = Functions.RandomIn(sensoryTypesRange);
        engineRef.decayT = Random.Range(decayTRange.x, decayTRange.y);
        engineRef.chemColorShift = Random.Range(colorShiftRange.x, colorShiftRange.y);

        engineRef.rotationAngle = Random.Range(RA_Range.x, RA_Range.y);
        engineRef.sensorAngle = Random.Range(SA_Range.x, SA_Range.y);
        engineRef.sensorOffset = Random.Range(SO_Range.x, SO_Range.y + 1);
        engineRef.stepSize = Random.Range(stepSizeRange.x, stepSizeRange.y + 1);

        engineRef.ApplyHPChange();
    }

    
}
