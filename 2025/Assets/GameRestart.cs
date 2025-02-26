using UnityEngine;
using UnityEngine.SceneManagement;

public class GameRestart : MonoBehaviour
{
    void Start()
    {
        // Immediately load the main scene
        SceneManager.LoadScene(0);
    }
}
