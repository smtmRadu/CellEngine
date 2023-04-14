using NeuroForge;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PhysarumSceneManager : MonoBehaviour
{
    public PhysarumEngine PEngine;
    public PhysarumMenu menuRef;
    public Texture2D mouseCursor;

    [Header("Audio")]
    public AudioSource audioSource;
    public List<AudioClip> bgMusic = new List<AudioClip>();


    [Header("Fadeing")]
    [SerializeField] TMPro.TMP_Text fpsCounter;
    public float fadeRate = 0.95f;
    public float appearRate = 0.9f;
    private Vector2 mouseLastPosition;
    public float timeStep__AfterTheyStartFading = 2f;
    [SerializeField] private float timeLeft__UntilTheyStartFading;

    public List<Image> toFadeImages = new List<Image>();
    public List<TMPro.TMP_Text> toFadeText = new List<TMPro.TMP_Text>();

    private void Awake()
    {
        Cursor.SetCursor(mouseCursor, Vector3.zero, CursorMode.ForceSoftware);


        var pipe = FindObjectOfType<PhysarumPipe>();
        if (pipe != null)
        {
            //use pipe
            PEngine.resolutionScale = pipe.resolution;
            PEngine.initializationType = pipe.initType;
            PEngine.populationPercentageInit = pipe.population;

            menuRef.Resolution.value = pipe.resolution;
            menuRef.SpawnType.value = (int)pipe.initType;
            menuRef.Population.value = pipe.population;

        }
        else
        {
            //create pipe
            pipe = new PhysarumPipe();
            var newPipeGO = new GameObject("Pipe");         
            newPipeGO.AddComponent(pipe.GetType());
            DontDestroyOnLoad(newPipeGO);
        }
    }
    private void Start()
    {
        if(bgMusic.Count > 0)
        {
            audioSource.clip = Functions.RandomIn(bgMusic);
            audioSource.Play();
        }

        toFadeImages.Add(menuRef.menuImage);
        toFadeImages.Add(menuRef.leftFade);
        toFadeImages.Add(menuRef.rightFade);

        toFadeText.Add(menuRef.fpsCounter);
        toFadeText.Add(menuRef.speciesNo);
        toFadeText.Add(menuRef.populationPerc);
        toFadeText.Add(menuRef.imageRendered);
        toFadeText.Add(menuRef.menuLabel);
    }

    private void Update()
    {
        FadeReferenceControls();
        fpsCounter.text = (1f / Time.deltaTime).ToString("0.0 FPS");

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ResetScene();
        }
    }
    void FadeReferenceControls()
    {

        timeLeft__UntilTheyStartFading -= Time.deltaTime;


        if (timeLeft__UntilTheyStartFading <= 0f)
        {
            // you can start fade
            foreach (var item in toFadeImages)
            {
                if (item.enabled == false)
                    continue;

                item.color = new Color(item.color.r, item.color.g, item.color.b, item.color.a * fadeRate < 1e-3f ? 1e-3f : item.color.a * fadeRate);
            }
            foreach (var item in toFadeText)
            {
                if (item.enabled == false)
                    continue;

                item.color = new Color(item.color.r, item.color.g, item.color.b, item.color.a * fadeRate < 1e-3f ? 1e-3f : item.color.a * fadeRate);
            }
        }
        else
        {
            foreach (var item in toFadeImages)
            {
                if (item.enabled == false)
                    continue;

                item.color = new Color(item.color.r, item.color.g, item.color.b, item.color.a / appearRate > 1 ? 1 : item.color.a / appearRate);
            }
            foreach (var item in toFadeText)
            {
                if (item.enabled == false)
                    continue;

                item.color = new Color(item.color.r, item.color.g, item.color.b, item.color.a / appearRate > 1 ? 1 : item.color.a / appearRate);
            }
        }

        if (mouseLastPosition != (Vector2)Input.mousePosition)
        {
            timeLeft__UntilTheyStartFading = timeStep__AfterTheyStartFading;
        }

        mouseLastPosition = Input.mousePosition;
    }

    void ResetScene()
    {
        var pipe = FindObjectOfType<PhysarumPipe>();
        // prepare pipe
        pipe.initType = (InitAgentsType)menuRef.SpawnType.value;
        pipe.population = menuRef.Population.value;
        pipe.resolution = menuRef.Resolution.value;




        SceneManager.LoadScene("Physarum");
    }
}
