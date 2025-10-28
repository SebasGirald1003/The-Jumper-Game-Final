using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    public void Play(string Scene)
    {
        SceneManager.LoadScene(Scene);
    }
}
