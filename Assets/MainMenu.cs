using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void Load_CGOL()
    {
        SceneManager.LoadScene("Conway");
        
    }
    public void Load_Physarum()
    {
        SceneManager.LoadScene("Physarum");
    }
    public void QuitApp()
    {
        Application.Quit();
    }
}
