using NeuroForge;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor;

public class MainMenu : MonoBehaviour
{
    public Texture2D mouseCursor;
    [Header("Fadeing")]
    public List<Image> fadingImages = new List<Image>();
    public List<TMPro.TMP_Text> fadingText = new List<TMPro.TMP_Text>();
    public float timeStep__AfterTheyStartFading = 5f;
    [SerializeField] private float timeLeft__UntilTheyStartFading;
    public float fadeRate = 0.95f;
    public float appearRate = 0.99f;
    private Vector2 mouseLastPosition;


    [Header("Audio")]
    public AudioSource buttonAUdioSource;
    public AudioSource backgroundMusicAudioSource;
    public List<AudioClip> backgroundMusic = new List<AudioClip>();

    public void Awake()
    {
        Cursor.SetCursor(mouseCursor, Vector3.zero, CursorMode.ForceSoftware);
        timeLeft__UntilTheyStartFading = timeStep__AfterTheyStartFading;
        backgroundMusicAudioSource.clip = Functions.RandomIn(backgroundMusic);
        backgroundMusicAudioSource.Play();
    }

    public void Update()
    {
        if(backgroundMusicAudioSource.isPlaying == false)
        {
            backgroundMusicAudioSource.clip = Functions.RandomIn(backgroundMusic);
            backgroundMusicAudioSource.Play();
        }
        FadeButtons();
    }

    public void FadeButtons()
    {
        timeLeft__UntilTheyStartFading -= Time.deltaTime;


        if(timeLeft__UntilTheyStartFading <= 0f)
        {
            // can start fade
            foreach (var item in fadingImages)
            {
                item.color = new Color(item.color.r, item.color.g, item.color.b, item.color.a * fadeRate < 1e-3f ? 1e-3f : item.color.a * fadeRate);
            }
            foreach (var item in fadingText)
            {
                item.color = new Color(item.color.r, item.color.g, item.color.b, item.color.a * fadeRate < 1e-3f ? 1e-3f : item.color.a * fadeRate);
            }
        }
        else
        {
            // start unfade
            foreach (var item in fadingImages)
            {
                item.color = new Color(item.color.r, item.color.g, item.color.b, item.color.a / appearRate > 1 ? 1 : item.color.a / appearRate);
            }
            foreach (var item in fadingText)
            {
                item.color = new Color(item.color.r, item.color.g, item.color.b, item.color.a / appearRate > 1 ? 1 : item.color.a / appearRate);
            }
        }
        
        if(mouseLastPosition != (Vector2)Input.mousePosition)
        {
            timeLeft__UntilTheyStartFading = timeStep__AfterTheyStartFading;
        }
       
        mouseLastPosition = Input.mousePosition;
    }

    public void PlayHoverSound()
    {
        buttonAUdioSource.Play();
    }
    public void Load_CGOL()
    {
        SceneManager.LoadScene("Conway");
        
    }
    public void Load_Physarum()
    {
        SceneManager.LoadScene("Physarum");
    }
    public void QuitApp()
    {
        Application.Quit();
    }
}
