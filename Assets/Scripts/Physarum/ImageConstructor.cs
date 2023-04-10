using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using System.Diagnostics;
using System;
using Unity.VisualScripting;
using UnityEditor;

public class ImageConstructor : MonoBehaviour
{
    [SerializeField] PhysarumEngine engineRef;
    [SerializeField] ComputeShader sobelShader;
    [SerializeField] TMPro.TMP_Text imageRenderedNowTMPro;
    [SerializeField] int AT_INDEX_IMAGE = 0;
    [SerializeField] List<(string,Sprite)> images = new List<(string, Sprite)>();

    public void LoadAndRenderImage()
    {
        string filePath = "";
        string directoryPath = "";

        // Show the open file dialog
        var extensions = new[] { "png", "jpeg", "jpg" };
        var extensionString = string.Join(",", extensions);
        var filter = new[] { $"Image files ({extensionString})", extensionString };
        var title = "Select Image";
        var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var fullFilePath = EditorUtility.OpenFilePanelWithFilters(title, path, filter);

        // Get the directory and file paths
        if (!string.IsNullOrEmpty(fullFilePath))
        {
            directoryPath = Path.GetDirectoryName(fullFilePath);
            filePath = fullFilePath;
        }
        else
            return;

        // Load the Texture from that filepath
        byte[] bytes = File.ReadAllBytes(filePath);
        Texture2D newImage = new Texture2D(2, 2, TextureFormat.RGBA32, true);
        newImage.LoadImage(bytes);

        // Filter and Resize
        newImage = ScaleTexture(newImage, engineRef.environment.width, engineRef.environment.height);
        newImage = SobelFilter(newImage);

        // Add the image to the list
        Sprite newSpr = Sprite.Create(newImage, new Rect(0f, 0f, newImage.width, newImage.height), new Vector2(0.5f, 0.5f));
        images.Add((Path.GetFileName(filePath), newSpr));

        // Also render it immediatelly
        AT_INDEX_IMAGE = images.Count - 1;
        RenderNextImage();
    }

    public void LoadPattern()
    {

    }
    public void SavePatterns()
    {

    }

    public void RenderNextImage()
    {
        if (images.Count == 0)
            LoadAndRenderImage();

        if (engineRef.whatWeRender != WhatWeRender.Image)
            AT_INDEX_IMAGE--;

        if (AT_INDEX_IMAGE == images.Count || AT_INDEX_IMAGE < 0)
            AT_INDEX_IMAGE = 0;

        engineRef.whatWeRender = WhatWeRender.Image;
        PutImageInEnvironment(images[AT_INDEX_IMAGE].Item2.texture);

        imageRenderedNowTMPro.text = "Image Rendered: " + images[AT_INDEX_IMAGE].Item1 + " (" + (AT_INDEX_IMAGE + 1) + "/" + images.Count + ")";
        AT_INDEX_IMAGE++;

        
    }
    public void RenderAgents()
    {
        if (engineRef.whatWeRender == WhatWeRender.Agents)
            return;

        engineRef.whatWeRender = WhatWeRender.Agents;
        BackToNormal();

        imageRenderedNowTMPro.text = "Image Rendered: None (0/" + images.Count + ")";

    }
    public void RenderChemicals()
    {
        if (engineRef.whatWeRender == WhatWeRender.Chemicals)
            return;

        engineRef.whatWeRender = WhatWeRender.Chemicals;
        BackToNormal();

        imageRenderedNowTMPro.text = "Image Rendered: None (0/" + images.Count + ")";
    }


    private void PutImageInEnvironment(Texture2D texture)
    {
        // // Rezise the texture to fit the env resolution
        // var texture = new Texture2D(sourceTexture.width, sourceTexture.height, sourceTexture.format, sourceTexture.mipmapCount > 1);
        // texture.filterMode = sourceTexture.filterMode;
        // texture.wrapMode = sourceTexture.wrapMode;
        // texture.LoadRawTextureData(sourceTexture.GetRawTextureData());
        // 
        // texture = ScaleTexture(texture, engineRef.environment.width, engineRef.environment.height);
        // // Convolute the image and extract edge lines with Sobel filter
        // // TODO

        // Assign the texture as chemicals map
        var portraitPixArr = texture.GetPixels().Select(x => x.grayscale).ToArray();
        engineRef.environment.chemicals = portraitPixArr;

        // Other settings to do
        engineRef.decayT = 0;
        for (int i = 0; i < engineRef.environment.spec_mask.Length; i++)
        {
            engineRef.environment.spec_mask[i] = 1;
        }
        for (int i = 1; i < engineRef.species_param.Length; i++)
        {
            engineRef.species_param[i].depT = 0;
        }
    }
    private void BackToNormal()
    {
        engineRef.decayT = 0.15f;
        foreach (var item in engineRef.species_param)
        {
            if (item)
                item.depT = 5;
        }
    }
    private Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, true);
        Color[] rpixels = result.GetPixels(0);
        float incX = (1.0f / targetWidth);
        float incY = (1.0f / targetHeight);
        for (int px = 0; px < rpixels.Length; px++)
        {
            rpixels[px] = source.GetPixelBilinear(incX * ((float)px % targetWidth), incY * Mathf.Floor((float)px / targetWidth));
        }
        result.SetPixels(rpixels, 0);
        result.Apply();
        return result;
    }
    private Texture2D SobelFilter(Texture2D sourceTexture)
    {
        // Create output texture
        RenderTexture outputTexture = new RenderTexture(sourceTexture.width, sourceTexture.height, 0);
        outputTexture.enableRandomWrite = true;
        outputTexture.Create();

        int kernelHandle = sobelShader.FindKernel("SobelFilter");

        // Set input texture
        int sourceTexHandle = Shader.PropertyToID("source");
        Graphics.SetRenderTarget(outputTexture);
        Graphics.Blit(sourceTexture, outputTexture);
        sobelShader.SetTexture(kernelHandle, sourceTexHandle, sourceTexture);

        // Set output texture
        int resultTexHandle = Shader.PropertyToID("result");
        sobelShader.SetTexture(kernelHandle, resultTexHandle, outputTexture);

        // Set dimensions
        sobelShader.SetVector("sourceSize", new Vector2(sourceTexture.width, sourceTexture.height));

        // Set thread group size
        uint threadGroupSizeX, threadGroupSizeY, threadGroupSizeZ;
        sobelShader.GetKernelThreadGroupSizes(kernelHandle, out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);

        // Dispatch compute shader
        uint numThreadGroupsX = (uint)Mathf.CeilToInt((float)sourceTexture.width / (float)threadGroupSizeX);
        uint numThreadGroupsY = (uint)Mathf.CeilToInt((float)sourceTexture.height / (float)threadGroupSizeY);
        sobelShader.Dispatch(kernelHandle, (int)numThreadGroupsX, (int)numThreadGroupsY, 1);

        // Read result back into a new Texture2D
        Texture2D resultTexture = new Texture2D(sourceTexture.width, sourceTexture.height);
        RenderTexture.active = outputTexture;
        resultTexture.ReadPixels(new Rect(0, 0, outputTexture.width, outputTexture.height), 0, 0);
        resultTexture.Apply();

        // Cleanup
        outputTexture.Release();
        return resultTexture;
    }

}
// #region Editor
// [CustomEditor(typeof(ImageConstructor)), CanEditMultipleObjects]
// class ScriptlessCameraSensor : Editor
// {
//     public override void OnInspectorGUI()
//     {
//         List<string> dontInclude = new List<string>() { "m_Script" };
//         var script = (ImageConstructor)target;
// 
//         DrawPropertiesExcluding(serializedObject, dontInclude.ToArray());
//         serializedObject.ApplyModifiedProperties();
// 
//         EditorGUILayout.Separator();
//         if (GUILayout.Button("SendImage"))
//         {
//             script.PassImage();
//         }
//     }
// }
// #endregion