using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ConwayGameManager : MonoBehaviour
{
    public ConwaysGOL engineRef;
    public float fadeRate = 0.95f;
    public float appearRate = 0.99f;
    private Vector2 mouseLastPosition;

    public float timeStep__AfterTheyStartFading = 2f;
    [SerializeField] private float timeLeft__UntilTheyStartFading;

    private bool canFade = true;
    public List<Image> toFadeImages = new List<Image>();
    public List<TMPro.TMP_Text> toFadeText = new List<TMPro.TMP_Text>();


    // Update is called once per frame
    void Update()
    {
        HandleIO();
        FadeTutorial();
    }

    void HandleIO()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
            SceneManager.LoadScene("MainMenu");

        if (UnityEngine.Input.GetKeyDown(KeyCode.R))
            engineRef.ResetMatrix();
    }

    void FadeTutorial()
    {

        timeLeft__UntilTheyStartFading -= Time.deltaTime;


        if (timeLeft__UntilTheyStartFading <= 0f)
        {
            // you can start fade
            foreach (var item in toFadeImages)
            {
                item.color = new Color(item.color.r, item.color.g, item.color.b, item.color.a * fadeRate < 1e-3f ? 1e-3f : item.color.a * fadeRate);
            }
            foreach (var item in toFadeText)
            {
                item.color = new Color(item.color.r, item.color.g, item.color.b, item.color.a * fadeRate < 1e-3f ? 1e-3f : item.color.a * fadeRate);
            }
        }
        else
        {
            foreach (var item in toFadeImages)
            {
                item.color = new Color(item.color.r, item.color.g, item.color.b, item.color.a / appearRate > 1 ? 1 : item.color.a / appearRate);
            }
            foreach (var item in toFadeText)
            {
                item.color = new Color(item.color.r, item.color.g, item.color.b, item.color.a / appearRate > 1 ? 1 : item.color.a / appearRate);
            }
        }

        if (mouseLastPosition != (Vector2)Input.mousePosition)
        {
            timeLeft__UntilTheyStartFading = timeStep__AfterTheyStartFading;
        }

        mouseLastPosition = Input.mousePosition;
    }
}
