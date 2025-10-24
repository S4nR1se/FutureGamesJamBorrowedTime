using TMPro;
using UnityEngine;

public class ConstructionPopupUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _buildingType;
    [SerializeField] private TextMeshProUGUI _buildingTime;
    [SerializeField] private TextMeshProUGUI _buildingDescription;

    private Vector3 offset = new Vector3(0, 10f, 20f);

    private Camera _mainCam;

    private void Awake()
    {
        _mainCam = Camera.main;
    }
    public void Initialize(BuildingData_SO data, int remainingBuildTime)
    {
        _buildingType.text = $"Type: {data.BuildingName}";
        _buildingTime.text = $"Build Time: {remainingBuildTime}";
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
