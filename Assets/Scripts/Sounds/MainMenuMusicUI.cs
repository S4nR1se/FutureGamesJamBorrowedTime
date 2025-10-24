using System.Collections;
using UnityEngine;

public class MainMenuMusicUI : MonoBehaviour
{
    // Sorry Illia or Christina, I didn't found better way to make it working ;(
    void Start()
    {
        StartCoroutine(PlayDelayed());
    }

    private IEnumerator PlayDelayed()
    {
        yield return new WaitUntil(() => SoundManager.Instance != null);
        yield return new WaitForSeconds(0.01f);
        SoundManager.Instance.PlaySound("MainMenuMusic");
    }

}
