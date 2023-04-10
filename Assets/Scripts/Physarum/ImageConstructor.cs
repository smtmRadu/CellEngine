using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ImageConstructor : MonoBehaviour
{
    public PhysarumEngine engineRef;
    public List<Sprite> images;
    [SerializeField] int atImageIndex = 0;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            if (engineRef.whatWeRender != WhatWeRender.Image)
                atImageIndex--;

            if(atImageIndex == images.Count || atImageIndex < 0)
                atImageIndex = 0;

            engineRef.whatWeRender = WhatWeRender.Image;
            FormImage(images[atImageIndex].texture);
            atImageIndex++;
            
        }
        else if(Input.GetKeyDown(KeyCode.A))
        {
            engineRef.whatWeRender = WhatWeRender.Agents;
            BackToNormal();
        }
        else if( Input.GetKeyDown(KeyCode.C)) 
        {
            engineRef.whatWeRender = WhatWeRender.Chemicals;
            BackToNormal();
        }
    }

    public void FormImage(Texture2D sourceTexture)
    {
        // Rezise the texture to fit the env resolution
        var texture = new Texture2D(sourceTexture.width, sourceTexture.height, sourceTexture.format, sourceTexture.mipmapCount > 1);
        texture.filterMode = sourceTexture.filterMode;
        texture.wrapMode = sourceTexture.wrapMode;
        texture.LoadRawTextureData(sourceTexture.GetRawTextureData());

        texture = ScaleTexture(texture, engineRef.environment.width, engineRef.environment.height);
        // Convolute the image and extract edge lines with Sobel filter
        // TODO

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
    public void BackToNormal()
    {
        engineRef.decayT = 0.15f;
        foreach(var item in engineRef.species_param)
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