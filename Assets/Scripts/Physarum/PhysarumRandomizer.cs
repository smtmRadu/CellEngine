using NeuroForge;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysarumRandomizer : MonoBehaviour
{
    public PhysarumEngine engineRef;
    bool isMixing = true;

    [Tooltip("How often a new change appears")]
    public Vector2 mixTimeRange = new Vector2(2.5f, 3.5f);
    private float mixTime = 3f;

    [Space]
    public Vector2 decayTRange = new Vector2(0.1f, 0.2f);
    Vector2 colorShiftRange = new Vector2(0.4f, 0.6f);

    [Space]
    List<SensoryType> sensoryTypesRange = new List<SensoryType>() { SensoryType.ChemoAttraction, SensoryType.ChemoRepulsion };
    Vector2 RA_Range = new Vector2(45f, 135f);
    Vector2 SA_Range = new Vector2(45f, 135f);
    Vector2Int SO_Range = new Vector2Int(9, 19);
    List<int> stepSizeRange = new List<int>() { 1, 2};


    private void Awake()
    {
        if (this.enabled == false)
            return;

        StartCoroutine(DeployChange(Random.Range(mixTimeRange.x/2, mixTimeRange.y/2)));
    }
    private void Update()
    {
        if (Input.GetKey(KeyCode.R) && !isMixing)
            StartCoroutine(DeployChange(Random.Range(mixTimeRange.x, mixTimeRange.y)));
    }

    IEnumerator DeployChange(float mix_time)
    {
        isMixing = true;

        mixTime = mix_time;
        engineRef.useSensors = false;
        engineRef.allowIntersection = true;

        StartCoroutine(LerpColors());
        yield return new WaitForSeconds(mixTime);

        engineRef.useSensors = true;
        engineRef.allowIntersection = false;
        Sample_And_Apply();
        isMixing = false;
    }
    IEnumerator LerpColors()
    {
        //Color Old_ag = new Color(engineRef.agentsColor.r, engineRef.agentsColor.g, engineRef.agentsColor.b);
        Color Old_c1 = new Color(engineRef.chemColor1.r, engineRef.chemColor1.g, engineRef.chemColor1.b);
        Color Old_c2 = new Color(engineRef.chemColor2.r, engineRef.chemColor2.g, engineRef.chemColor2.b);
        Color Old_bg = new Color(engineRef.backgroundColor.r, engineRef.backgroundColor.g, engineRef.backgroundColor.b);

        //Color Targ_ag = new Color(Random.value, Random.value, Random.value);
        Color Targ_c1 = new Color(Random.Range(0.4f, 0.8f), Random.Range(0.4f, 0.8f), Random.Range(0.3f, 0.8f));
        Color Targ_c2 = new Color(Random.Range(0.1f, 1f), Random.Range(0.1f, 1f), Random.Range(0.1f, 0.9f));
        Color Targ_bg = new Color(Random.Range(0f, 0.1f), Random.Range(0f, 0.1f), Random.Range(0f, 0.1f));

        float timeElapsed = 0;
        while(timeElapsed < mixTime)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            timeElapsed += Time.deltaTime;
            float tm_lerp = timeElapsed / mixTime;

            //engineRef.agentsColor = Color.Lerp(Old_ag, Targ_ag, tm_lerp);
            engineRef.chemColor1 = Color.Lerp(Old_c1, Targ_c1, tm_lerp);
            engineRef.chemColor2 = Color.Lerp(Old_c2, Targ_c2, tm_lerp);
            engineRef.backgroundColor = Color.Lerp(Old_bg, Targ_bg, tm_lerp);
        }
    }

    private void Sample_And_Apply()
    {
        engineRef.sensoryType = Functions.RandomIn(sensoryTypesRange);
        engineRef.decayT = Random.Range(decayTRange.x, decayTRange.y);
        engineRef.chemColorShift = Random.Range(colorShiftRange.x, colorShiftRange.y);

        engineRef.rotationAngle = Random.Range(RA_Range.x, RA_Range.y);
        engineRef.sensorAngle = Random.Range(SA_Range.x, SA_Range.y);
        engineRef.sensorOffset = Random.Range(SO_Range.x, SO_Range.y);
        engineRef.stepSize = Functions.RandomIn(stepSizeRange);

        engineRef.ApplyHPChange();
    }

    
}
