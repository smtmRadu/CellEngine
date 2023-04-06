using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PhysarumSceneManager : MonoBehaviour
{
    public PhysarumEngine PEngine;
    public Texture2D mouseCursor;

    [Header("Fadeing")]
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
    }
    void Update()
    {
        FadeReferenceControls();

    }

    void FadeReferenceControls()
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
