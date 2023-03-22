using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PhysarumSceneManager : MonoBehaviour
{
    public PhysarumEngine PEngine;
    void Update()
    {

        HandleIO();

    }

    void HandleIO()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            SceneManager.LoadScene("MainMenu");

        if (Input.GetKeyDown(KeyCode.S) && Input.GetKey(KeyCode.LeftControl))
        {
            Debug.Log("Image saved!");
            var tex = PEngine.ENV_Img.sprite.texture;
        }
    }
}
