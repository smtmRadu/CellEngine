using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using System;

public class ImageConstructor : MonoBehaviour
{
    [SerializeField] PhysarumEngine engineRef;
    [SerializeField] ComputeShader sobelShader;
    [SerializeField] TMPro.TMP_Text imageRenderedNowTMPro;
    [SerializeField] int AT_INDEX_IMAGE = 0;
    [SerializeField] List<(string,Sprite)> images = new List<(string, Sprite)>();


    [SerializeField] int depT_WhenRenderingImage = 0;
    [SerializeField] float decayT_WhenRenderingImage = 0f;
    public void RenderImage()
    {
        var name_img = OpenFileDialog_Imgs();
        var image = name_img.Item2;
        var name = name_img.Item1;

        // Filter and Resize
        image = ScaleTexture(image, engineRef.environment.width, engineRef.environment.height);
        image = SobelFilter(image);
            
        // Add the image to the list
        Sprite newSpr = Sprite.Create(image, new Rect(0f, 0f, image.width, image.height), new Vector2(0.5f, 0.5f));
        images.Add((Path.GetFileName(name), newSpr));

        // Also render it immediatelly
        AT_INDEX_IMAGE = images.Count - 1;
        RenderNextImage();
    }
    
    public void LoadPattern()
    {
        var dialog = new System.Windows.Forms.OpenFileDialog();
        dialog.Filter = "JSON Files (*.pttn)|*.pttn";

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            var filePath = dialog.FileName;
            try
            {
                string data = File.ReadAllText(filePath);
                SpeciesParametersSerializableArray speciesParam = JsonUtility.FromJson<SpeciesParametersSerializableArray>(data);

                for (int i = 1; i < engineRef.species_param.Length; i++)
                {
                    engineRef.species_param[i].SetFrom(speciesParam.values[i]);
                }

                Debug.Log("Pattern loaded successfully!");
            }
            catch
            {
                Debug.Log("The pattern couldn't be loaded!");
            }
        }
    }
    public void SavePattern()
    {
        SpeciesParametersSerializableArray speciesParameters = new SpeciesParametersSerializableArray(engineRef.species_param);

        var dialog = new System.Windows.Forms.SaveFileDialog();
        dialog.Filter = "JSON Files (*.pttn)|*.pttn";

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            var filePath = dialog.FileName;

            try
            {
                string jsonData = JsonUtility.ToJson(speciesParameters);
                File.WriteAllText(filePath, jsonData);

                Debug.Log("Pattern saved successfully!");
            }
            catch
            {
                Debug.Log("The pattern couldn't be saved!");
            }
        }
    }

    

    public void RenderNextImage()
    {
        if (images.Count == 0)
            RenderImage();

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
        engineRef.decayT = decayT_WhenRenderingImage;
        for (int i = 0; i < engineRef.environment.spec_mask.Length; i++)
        {
            engineRef.environment.spec_mask[i] = 1;
        }
        for (int i = 1; i < engineRef.species_param.Length; i++)
        {
            engineRef.species_param[i].depT = depT_WhenRenderingImage;
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
    private (string, Texture2D) OpenFileDialog_Imgs()
    {
        // Open file dialog to choose an image file
        var dialog = new System.Windows.Forms.OpenFileDialog();
        dialog.Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            // Load image file as texture
            byte[] imageData = File.ReadAllBytes(dialog.FileName);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageData);
            return (dialog.FileName, texture);
        }
        return (null, null);
    }

    private class SpeciesParametersSerializableArray
    {
        [SerializeField] public SpeciesParametersSerializable[] values;

        public SpeciesParametersSerializableArray(SpeciesParameters[] sp)
        {
            values = sp.Select(x => new SpeciesParametersSerializable(x)).ToArray();
        }
    }

}
