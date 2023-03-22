using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Windows;

public class PhysarumSceneManager : MonoBehaviour
{
    public PhysarumEngine PEngine;
   
    void Update()
    {

        HandleIO();

    }

    void HandleIO()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
            SceneManager.LoadScene("MainMenu");

        if (UnityEngine.Input.GetKeyDown(KeyCode.S))
        {
            
            
            var tex = PEngine.ENV_Img.sprite.texture;
            if(tex == null)
            {
                Debug.Log("Texture is null!");
                return;
            }    
            
            byte[] bytes = tex.EncodeToPNG();
            string filepath = Application.persistentDataPath + "/ScreenCapture_#" + Random.Range(0,10000) + ".png";
            // UnityEngine.Windows.File.WriteAllBytes(filepath, bytes);
            Debug.Log("Image saved! [" + filepath + "]");
        }
    }
}
