using UnityEngine;
using UnityEngine.SceneManagement;
public class Menu : MonoBehaviour
{

    public GameObject mainMenu;

    public void Play()
    {
        SceneManager.LoadScene("Prototipo");
    }

}
