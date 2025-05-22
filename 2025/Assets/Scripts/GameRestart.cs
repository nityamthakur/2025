using UnityEngine;
using UnityEngine.SceneManagement;

public class GameRestart : MonoBehaviour
{
    void Start()
    {
        // Immediately load the main scene
        Debug.Log("Game is being Restarted");
        SceneManager.LoadScene(0);
    }
}
