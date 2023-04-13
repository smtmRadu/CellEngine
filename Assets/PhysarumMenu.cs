using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhysarumMenu : MonoBehaviour
{
    [SerializeField] PhysarumEngine engineRef;
    [SerializeField] PhysarumSceneManager sceneManager;

    [Space]
    [SerializeField] TMPro.TMP_Text fpsCounter;
    [SerializeField] TMPro.TMP_Text speciesNo;
    [SerializeField] TMPro.TMP_Text populationPerc;
    [SerializeField] TMPro.TMP_Text imageRendered;
    [SerializeField] TMPro.TMP_Text menuLabel;
    [SerializeField] Image menuImage;

    [Space]
    [SerializeField] TMPro.TMP_Dropdown SpeciesNum;
    [SerializeField] TMPro.TMP_Dropdown SensorType;
    [SerializeField] CustomSliderScript RA;
    [SerializeField] CustomSliderScript SA;
    [SerializeField] CustomSliderScript SO;
    [SerializeField] CustomSliderScript SS;
    [SerializeField] CustomSliderScript DepT;
    [SerializeField] CustomSliderScript pCD;
    [SerializeField] CustomSliderScript sMin;

    [SerializeField] TMPro.TMP_Dropdown SpawnType;
    [SerializeField] CustomSliderScript Resolution;
    [SerializeField] CustomSliderScript Population;
    [SerializeField] CustomSliderScript DecayRate;
    [SerializeField] CustomSliderScript ColorShift;

    [Space]
    [SerializeField] Image leftFade;
    [SerializeField] Image rightFade;

    // Menu disallow placeing or erasing 
    [SerializeField] List<GameObject> tutorialInfos;

    private void Awake()
    {
        sceneManager.toFadeImages.Add(menuImage);
        sceneManager.toFadeImages.Add(leftFade);
        sceneManager.toFadeImages.Add(rightFade);

        sceneManager.toFadeText.Add(fpsCounter);
        sceneManager.toFadeText.Add(speciesNo);
        sceneManager.toFadeText.Add(populationPerc);
        sceneManager.toFadeText.Add(imageRendered);
        sceneManager.toFadeText.Add(menuLabel);


    }
    public void OnEnable()
    {
        sceneManager.toFadeImages.Add(menuImage);
        sceneManager.toFadeText.Add(fpsCounter);
        sceneManager.toFadeText.Add(speciesNo);
        sceneManager.toFadeText.Add(populationPerc);
        sceneManager.toFadeText.Add(imageRendered);
        sceneManager.toFadeText.Add(menuLabel);

        SpawnType.gameObject.SetActive(true);
        SpeciesNum.gameObject.SetActive(true);
        SensorType.gameObject.SetActive(true);
        RA.gameObject.SetActive(true);
        SA.gameObject.SetActive(true);
        SO.gameObject.SetActive(true);
        SS.gameObject.SetActive(true);
        DepT.gameObject.SetActive(true);
        pCD.gameObject.SetActive(true);
        sMin.gameObject.SetActive(true);
        Resolution.gameObject.SetActive(true);
        Population.gameObject.SetActive(true);
        DecayRate.gameObject.SetActive(true);
        ColorShift.gameObject.SetActive(true);

        tutorialInfos.ForEach(i => i.SetActive(true));

        sceneManager.toFadeImages.Remove(leftFade);
        sceneManager.toFadeImages.Remove(rightFade);

        leftFade.color = Color.white;
        rightFade.color = Color.white;
    }
    public void OnDisable()
    {
        sceneManager.toFadeImages.Remove(menuImage);
        sceneManager.toFadeText.Remove(fpsCounter);
        sceneManager.toFadeText.Remove(speciesNo);
        sceneManager.toFadeText.Remove(populationPerc);
        sceneManager.toFadeText.Remove(imageRendered);
        sceneManager.toFadeText.Remove(menuLabel);

        SpawnType.gameObject.SetActive(false);
        SpeciesNum.gameObject.SetActive(false);
        SensorType.gameObject.SetActive(false);
        RA.gameObject.SetActive(false);
        SA.gameObject.SetActive(false);
        SO.gameObject.SetActive(false);
        SS.gameObject.SetActive(false);
        DepT.gameObject.SetActive(false);
        pCD.gameObject.SetActive(false);
        sMin.gameObject.SetActive(false);
        Resolution.gameObject.SetActive(false);
        Population.gameObject.SetActive(false);
        DecayRate.gameObject.SetActive(false);
        ColorShift.gameObject.SetActive(false);

        tutorialInfos.ForEach(i => i.SetActive(false));
       
        sceneManager.toFadeImages.Add(leftFade);
        sceneManager.toFadeImages.Add(rightFade);
    }


    public void ChangeParameters()
    {
        int sNumber = SpeciesNum.value + 1; // because in species_param 0 is null
        SpeciesParameters speciesToModify = engineRef.species_param[sNumber];

        // Modify speciesToModify
        speciesToModify.sensorType = (SensoryType)SensorType.value;
        speciesToModify.RA = RA.value;
        speciesToModify.SA = SA.value;
        speciesToModify.SS = (int)SS.value;
        speciesToModify.depT = (int)DepT.value;
        speciesToModify.pCD = pCD.value;
        speciesToModify.sMin = sMin.value;
    }
    public void ChangeSlidersByParameters()
    {
        int sNumber = SpeciesNum.value + 1; // because in species_param 0 is null
        SpeciesParameters speciesToGetParametersFrom = engineRef.species_param[sNumber];

        SensorType.value = (int)speciesToGetParametersFrom.sensorType;
        RA.value = speciesToGetParametersFrom.RA;
        SA.value = speciesToGetParametersFrom.SA;
        SO.value = speciesToGetParametersFrom.SO;
        SS.value = speciesToGetParametersFrom.SS;
        DepT.value = speciesToGetParametersFrom.depT;
        pCD.value = speciesToGetParametersFrom.pCD;
        sMin.value = speciesToGetParametersFrom.sMin;
    }
}
