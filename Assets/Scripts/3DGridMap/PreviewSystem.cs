using System.Drawing;
using UnityEngine;
using UnityEngine.UIElements;

public class PreviewSystem : MonoBehaviour
{
    [SerializeField] private float _previewYOffset = 0.06f;

    [SerializeField] private GameObject _cellIndicator;
    private GameObject _previewObject;

    [SerializeField] private Material _previewMaterialPrefab;
    private Material _previewMaterialInstance;

    private Renderer _cellIndicatorRender;

    private void Start()
    {
        _previewMaterialInstance = new Material(_previewMaterialPrefab);
        _cellIndicator.SetActive(false);
        _cellIndicatorRender = _cellIndicator.GetComponentInChildren<Renderer>();
    }

    public void StartShowingPlacementPreview(GameObject Prefab, Vector2Int Size)
    {
        _previewObject = Instantiate(Prefab);
        PreparePreview(_previewObject);
        PrepareCursor(Size);
        _cellIndicator.SetActive(true);
    }

    private void PreparePreview(GameObject PreviewObject)
    {
        Renderer[] Renderers = _previewObject.GetComponentsInChildren<Renderer>();
        foreach(Renderer renderer in Renderers)
        {
            Material[] Materials = renderer.materials;
            for(int i = 0; i < Materials.Length;++i)
            {
                Materials[i] = _previewMaterialInstance;
            }
            renderer.materials = Materials;
        }
    }

    private void PrepareCursor(Vector2Int Size)
    {
        if(Size.x > 0 || Size.y > 0)
        {
            _cellIndicator.transform.localScale = new Vector3(Size.x, 1, Size.y);
            _cellIndicatorRender.material.mainTextureScale = Size;
        }
    }

    public void StopShowingPreview()
    {
        _cellIndicator.SetActive(false);
        Destroy(_previewObject);
    }

    public void UpdatePosition(Vector3 Position, bool Validity)
    {
        MovePreview(Position);
        MoveCursor(Position);
        ApplyFeedback(Validity);
    }

    private void MovePreview(Vector3 Position)
    {
        _previewObject.transform.position = new Vector3(Position.x, Position.y + _previewYOffset, Position.z);
    }


    private void MoveCursor(Vector3 Position)
    {
        _cellIndicator.transform.position = Position;
    }

    private void ApplyFeedback(bool Validity)
    {
        UnityEngine.Color c = Validity ? UnityEngine.Color.white : UnityEngine.Color.red;
        c.a = 0.5f;
        _cellIndicatorRender.material.color = c;
        _previewMaterialInstance.color = c;
    }
}
