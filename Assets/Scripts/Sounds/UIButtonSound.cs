using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("SoundLibrary")]
    [SerializeField] private string clickSound = "1";
    [SerializeField] private string hoverSound = "2";

    private Button _button;
    private bool _hoverPlayed;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(PlayClickSound);
    }

    private void PlayClickSound()
    {
        SoundManager.Instance?.PlaySound(clickSound);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_hoverPlayed)
        {
            SoundManager.Instance?.PlaySound(hoverSound);
            _hoverPlayed = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _hoverPlayed = false;
    }
}
