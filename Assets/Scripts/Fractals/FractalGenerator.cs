using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FractalGenerator : MonoBehaviour
{
    public Image bgImage;
    private RenderTexture rendTexl;

    [Range(0.01f,1f) ]public float resolutionScale;
    public int iterations = 100;
    [Range(0f,1f)]public float iterationsIncreaseRate = 0.5f;
    private bool zoomIn = true;
    public float zoomingRate = 0.999f;
    public float scale = 1f;
    public float minScale = 1e-5f;
    public float xOffset = -1f;
    public float decayFX = 1100;
    public Color fractalColor1 = Color.red;
    public Color fractalColor2 = Color.green;
    [Range(0.0f,1f)] public float colorShift = 0.5f;
    public Vector2 colorShiftRange = new Vector2(0.3f, 0.6f);
    public float changeColorFreqencyTime = 5f; 
    private float changeColorTimeLeft = 0.01f;
    public float diffuseColorTime = 3f;
    public ComputeShader compute;
    const int THREADS = 12; // it forgets to render in parallel a part of the image so i keep on 12 threads

    private void Awake()
    {
        int width = (int) (Screen.width * resolutionScale);
        int height = (int) (Screen.height * resolutionScale);

        rendTexl = new RenderTexture(width, height, 10);
        rendTexl.enableRandomWrite = true;
        rendTexl.Create();
        bgImage.color = Color.white;
    }
    private void Update()
    {
        UpdateScales();
        ComputeMandelbrotSetGPU();

        
        changeColorTimeLeft -= Time.deltaTime;
        if(changeColorTimeLeft <= 0)
        {
            changeColorTimeLeft = changeColorFreqencyTime;
            StartCoroutine(LerpColors());
            
        }
    }

    public static Texture2D ToTexture2D(RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false);
        var old_rt = RenderTexture.active;
        RenderTexture.active = rTex;

        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();

        RenderTexture.active = old_rt;
        return tex;
    }

    void UpdateScales()
    {
        if (scale < minScale)
            zoomIn = false;

        if (scale > 1)
            zoomIn = true;

        if (zoomIn)
        {
            scale *= zoomingRate;
            xOffset -= scale / decayFX;
            iterations += Random.value < iterationsIncreaseRate ? 1 : 0;

        }
        else
        {
            scale /= zoomingRate;
            xOffset += scale / decayFX;
            iterations -= Random.value < iterationsIncreaseRate ? 1 : 0;
        }
    }
    void ComputeMandelbrotSetGPU()
    {
        ComputeBuffer colorBuff = new ComputeBuffer(2, sizeof(float) * 4);
        colorBuff.SetData(new[] { fractalColor1 }, 0, 0, 1);
        colorBuff.SetData(new[] { fractalColor2 }, 0, 1, 1);
        
        compute.SetBuffer(0, "colorsBuffer", colorBuff);
        
        compute.SetInt("iterations", iterations);
        compute.SetInt("width", rendTexl.width);
        compute.SetInt("height", rendTexl .height);
        compute.SetFloat("xOffset", xOffset + 1);
        compute.SetFloat("scale", scale);
        compute.SetFloat("colorShift", colorShift);

        compute.SetTexture(0, "Result", rendTexl); // insert values
        compute.Dispatch(0, rendTexl.width / THREADS, rendTexl.height / THREADS, 1);

        colorBuff.Dispose();
        if(bgImage.sprite)
        {
            Destroy(bgImage.sprite.texture);
            Destroy(bgImage.sprite);
        }
        
        
        bgImage.sprite = Sprite.Create(ToTexture2D(rendTexl), new Rect(0, 0, rendTexl.width, rendTexl.height), new Vector2(0.5f, 0.5f));    
    }

    IEnumerator LerpColors()
    {
        Color col1Old = new Color(fractalColor1.r, fractalColor1.g, fractalColor1.b);
        Color col2Old = new Color(fractalColor2.r, fractalColor2.g, fractalColor2.b);
        float oldShift = colorShift;

        Color col1Targ = new Color(Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), Random.Range(0.5f, 1f));
        Color col2Targ = new Color(Random.Range(0.2f, 0.8f), Random.Range(0.2f, .8f), Random.Range(0.2f, .8f));
        float tgShift = Random.Range(colorShiftRange.x, colorShiftRange.y);
        float timeElapsed = 0f;
        while(timeElapsed < diffuseColorTime)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            float tm_lerp = timeElapsed / diffuseColorTime;

            timeElapsed += Time.deltaTime;
            fractalColor1 = Color.Lerp(col1Old, col1Targ, tm_lerp);
            fractalColor2 = Color.Lerp(col2Old, col2Targ, tm_lerp);
            colorShift = Mathf.Lerp(oldShift, tgShift, tm_lerp);
            
            
        }
    }
}