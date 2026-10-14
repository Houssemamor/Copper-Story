using UnityEngine;
using UnityEngine.SceneManagement;

// Trigger to load next level when player collides with goal object
public class LevelExit : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "Lvl2";

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"LevelExit triggered by: {other.name}, tag: {other.tag}");

        var playerMovement = other.GetComponentInParent<PlayerMovement>();
        Debug.Log($"PlayerMovement found: {playerMovement != null}");

        if (playerMovement == null)
        {
            return;
        }

        Debug.Log("Loading next level: " + nextSceneName);
        LoadNextLevel();
    }

    private void LoadNextLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(nextSceneName);
    }
}