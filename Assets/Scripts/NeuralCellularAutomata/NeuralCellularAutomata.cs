using NeuroForge;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
public class NeuralCellularAutomata : MonoBehaviour
{
    public Image imgFrame;
    public ComputeShader compute;
    private float[] frameworkEnv;
    public int penSize = 5;
    const int THREADS = 8;

    public void Awake()
    {
        frameworkEnv = new float[Screen.width * Screen.height];
        var tex = new Texture2D(Screen.width, Screen.height);
        tex.filterMode = FilterMode.Point;
        imgFrame.sprite = Sprite.Create(tex, new Rect(0, 0, Screen.width, Screen.height), new Vector2(.5f, .5f));
        Randomize();
    }
    public void Update()
    {
        //Filter();
        // Draw();
        Render();
    }

   void Randomize()
    {

        int W = Screen.width;
        int H = Screen.height;
        for (int i = 0; i < H; i++)
        {
            for (int j = 0; j < W; j++)
            {
                frameworkEnv[i * H + j] = Random.value;
            }
        }
    }
   void Draw()
    {
        // Do not ever use this
        int W = Screen.width;
        int H = Screen.height;

        if (!Input.GetMouseButton(0))
            return;

        Vector2 mousePos = Input.mousePosition;

        for (int i = 0; i < H; i++)
        {
            for (int j = 0; j < W; j++)
            {
                Debug.Log("here");
                float deltaDistance = Vector2.Distance(mousePos, new Vector2(i,j));
                if (deltaDistance <= penSize)
                    frameworkEnv[i * H + j] = 1f;
            }
        }
    }

    void Filter()
    {


        compute.SetInt("W", Screen.width);
        compute.SetInt("H", Screen.height);

        ComputeBuffer cmpBuff = new ComputeBuffer(frameworkEnv.Length, sizeof(float));
        cmpBuff.SetData(frameworkEnv);
        compute.SetBuffer(0, "inputBuffer", cmpBuff);

        compute.Dispatch(0, (Screen.width + THREADS - 1)/ THREADS, (Screen.width + THREADS - 1) / THREADS, 1);

        cmpBuff.GetData(frameworkEnv);
        cmpBuff.Dispose();

    }

    void Render()
    {
        Color[] pixels = frameworkEnv.Select(x => new Color(x, x, x)).ToArray();
        imgFrame.sprite.texture.SetPixels(pixels);
        imgFrame.sprite.texture.Apply();
    }


}
