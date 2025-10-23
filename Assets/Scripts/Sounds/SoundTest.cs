using UnityEngine;

public class SoundTest : MonoBehaviour
{
    public string soundName = "MainMenuMusic";

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (SoundManager.Instance != null)
            {
                Debug.Log("Attempting to play sound: " + soundName);
                SoundManager.Instance.PlaySound(soundName);
            }
            else
            {
                Debug.LogWarning("SoundManager instance is null!");
            }
        }
    }
}

