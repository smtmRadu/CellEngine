using NeuroForge;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FractalGenerator : MonoBehaviour
{
    public float chance_forMandelBrot = 0.25f;
    public int KERNEL_INDEX = 0;
    public Image bgImage;
    private RenderTexture rendTexl;

    [Range(0.01f,1f) ]public float resolutionScale;
    public int iterations = 100;
    [Range(0,10), Tooltip("One iteration at this number of frames")]public int iterationsIncreaseRate = 10;
    private bool zoomIn = true;
    public float zoomingRate = 0.999f;
    public float scale = 1f;
    const float minScale = 1e-6f;

    public Vector2 c;
    public float c_modif_rate = 0.99999f;
    public float xOffset = -1f;
    public float decayFX = 1108.4f;

    public Color fractalColor1 = Color.red;  
    public Color fractalColor2 = Color.green;

    [Range(0.0f, 1f)] public float colorShift2 = 0.5f;
    [Range(0.0f, 1f)] public float colorShift1 = 0.5f;

    public float changeColorFreqencyTime = 5f; 
    private float changeColorTimeLeft = 0.01f;
    public float diffuseColorTime = 3f;
    public ComputeShader compute;
    const int THREADS = 12; // it forgets to render in parallel a part of the image so i keep on 12 threads

    private readonly List<Vector2> c_values = new List<Vector2>()
    {
        new Vector2(-0.8f, 0.156f),
        new Vector2(-0.74543f, 0.11301f),
        new Vector2(-1.2f, 0.156f),
        new Vector2(-1.23f, -0.11f),
        new Vector2(-1.23f, 0.1f),
        new Vector2(-1.23456f, 0.107f),
        new Vector2(-0.054004f,0.68f),
        new Vector2(-0.74008f, 0.148788f),
    };


    private void Awake()
    {
        int width = (int) (Screen.width * resolutionScale);
        int height = (int) (Screen.height * resolutionScale);

        KERNEL_INDEX = Functions.RandomIn(new List<int>() { 0,1}, new List<float>() { chance_forMandelBrot, (1f - chance_forMandelBrot)});
        c = Functions.RandomIn(c_values);


        rendTexl = new RenderTexture(width, height, 10);
        rendTexl.enableRandomWrite = true;
        rendTexl.Create();
        bgImage.color = Color.white;
    }
    private void Update()
    {
        UpdateScales();
        ComputeFractal();

        // Input from user
        c_modif_rate += Input.mouseScrollDelta.y * 1e-5f * scale;

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
            iterations += Time.frameCount % iterationsIncreaseRate == 0 ? 1 : 0;

        }
        else
        {
            scale /= zoomingRate;
            xOffset += scale / decayFX;
            iterations -= Time.frameCount % iterationsIncreaseRate == 0 ? 1 : 0;
        }

        c.x *= c_modif_rate;
        c.y *= c_modif_rate;
    }
    void ComputeFractal()
    {
        ComputeBuffer colorBuff = new ComputeBuffer(2, sizeof(float) * 4);
        colorBuff.SetData(new[] { fractalColor1 }, 0, 0, 1);
        colorBuff.SetData(new[] { fractalColor2 }, 0, 1, 1);
        compute.SetBuffer(KERNEL_INDEX, "colorsBuffer", colorBuff);

        compute.SetVector("c", c);
        compute.SetInt("iterations", iterations);
        compute.SetInt("width", rendTexl.width);
        compute.SetInt("height", rendTexl .height);
        compute.SetFloat("xOffset", xOffset + 1);
        compute.SetFloat("scale", scale);

        compute.SetFloat("colorShift1", colorShift1);
        compute.SetFloat("colorShift2", colorShift2);

        compute.SetTexture(KERNEL_INDEX, "Result", rendTexl); // insert values
        compute.Dispatch(KERNEL_INDEX, rendTexl.width / THREADS, rendTexl.height / THREADS, 1);

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

        float oldShift1 = colorShift1;
        float oldShift2 = colorShift2;

        Color col1Targ = new Color(Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), Random.Range(0.5f, 1f));
        Color col2Targ = new Color(Random.Range(0.2f, 0.8f), Random.Range(0.2f, .8f), Random.Range(0.2f, .8f));

        float tgShift1 = Random.Range(0.4f, 0.6f);
        float tgShift2 = Random.Range(0.4f, 0.6f);

        float timeElapsed = 0f;
        while(timeElapsed < diffuseColorTime)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            float tm_lerp = timeElapsed / diffuseColorTime;
            timeElapsed += Time.deltaTime;

            fractalColor1 = Color.Lerp(col1Old, col1Targ, tm_lerp);
            fractalColor2 = Color.Lerp(col2Old, col2Targ, tm_lerp);

            colorShift1 = Mathf.Lerp(oldShift1, tgShift1, tm_lerp);
            colorShift2 = Mathf.Lerp(oldShift2, tgShift2, tm_lerp);
        }
    }
}
