using TMPro;
using UnityEngine;

public class PlacementPopupUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _buildingType;
    [SerializeField] private TextMeshProUGUI _buildingCost;
    [SerializeField] private TextMeshProUGUI _buildingTime;
    [SerializeField] private TextMeshProUGUI _buildingDescription;
    [SerializeField] private Vector3 offset = new Vector3(0, 0.5f, 0); // Slightly above mouse in world space

    private Camera _mainCam;

    private void Awake()
    {
        _mainCam = Camera.main;
    }

    public void Initialize(BuildingData_SO data)
    {
        _buildingType.text = $"Type: {data.BuildingName}";
        _buildingCost.text = $"Material Cost: {data.MaterialCost}";
        _buildingTime.text = $"Build Time: {data.BuildTime}";
        _buildingDescription.text = $"{data.Description}";
    }
    public void FollowMouse()
    {
        if (_mainCam == null)
            _mainCam = Camera.main;

        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        Ray ray = _mainCam.ScreenPointToRay(Input.mousePosition);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 worldPos = ray.GetPoint(distance);

            transform.position = worldPos + new Vector3(0, 2f, 0) + offset;

            transform.rotation = Quaternion.LookRotation(_mainCam.transform.forward, Vector3.up);
        }
    }
}
