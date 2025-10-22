using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private float _zoomInOutAmount = 5.0f;

    private float _minFov = 15.0f;
    private float _maxFov = 90.0f;
    private float _sensitivity = 10.0f;
    private float _fov = 0.0f;
    private float _initialFOV = 0.0f;

    private void Start()
    {
        _fov = Camera.main.fieldOfView;
        _initialFOV = _fov;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.I))
        {
            _fov -= _zoomInOutAmount;
            Camera.main.fieldOfView = _fov;
        }
        else if (Input.GetKeyDown(KeyCode.O))
        {
            _fov += _zoomInOutAmount;
            Camera.main.fieldOfView = _fov;
        }
        else
        {
            _fov = Camera.main.fieldOfView;
            _fov += Input.GetAxis("Mouse ScrollWheel") * _sensitivity;
            _fov = Mathf.Clamp(_fov, _minFov, _maxFov);
            Camera.main.fieldOfView = _fov;
        }
    }
}
