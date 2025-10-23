using System.Collections;
using UnityEngine;

public class MainMenuMusicUI : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(PlayDelayed());
    }

    private IEnumerator PlayDelayed()
    {
        yield return new WaitUntil(() => SoundManager.Instance != null);
        yield return new WaitForSeconds(0.1f);
        SoundManager.Instance.PlaySound("MainMenuMusic");
    }

}
