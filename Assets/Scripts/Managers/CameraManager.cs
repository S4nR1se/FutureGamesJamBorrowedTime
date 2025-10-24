using UnityEngine;

public class CameraManager : Manager
{
    private Camera _camera;

    [Header("Zoom Settings")]
    private const float ZOOMSPEED = 10f;
    private const float MINZOOM = 10f;
    private const float MAXZOOM = 40f;
    private const float ZOOMERSPEED = 50f;

    private float targetZoom;
    private Vector3 initialPosition;

    public override void Initialize()
    {
        _camera = Camera.main;
        targetZoom = _camera.orthographicSize;
        initialPosition = _camera.transform.position;
    }

    void Update()
    {
        HandleZoom();
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scroll) > 0.01f)
        {
            float oldSize = _camera.orthographicSize;
            targetZoom -= scroll * ZOOMSPEED;
            targetZoom = Mathf.Clamp(targetZoom, MINZOOM, MAXZOOM);

            if (scroll > 0) // Zooming in
            {
                // Zoom toward mouse
                Vector3 mouseWorldBeforeZoom = _camera.ScreenToWorldPoint(Input.mousePosition);
                _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, targetZoom, Time.deltaTime * ZOOMERSPEED);
                Vector3 mouseWorldAfterZoom = _camera.ScreenToWorldPoint(Input.mousePosition);
                _camera.transform.position += (mouseWorldBeforeZoom - mouseWorldAfterZoom);
            }
            else if (scroll < 0) // Zooming out
            {
                // Zoom out from mouse position, but gradually return to initial position
                Vector3 mouseWorldBeforeZoom = _camera.ScreenToWorldPoint(Input.mousePosition);
                _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, targetZoom, Time.deltaTime * ZOOMERSPEED);
                Vector3 mouseWorldAfterZoom = _camera.ScreenToWorldPoint(Input.mousePosition);
                _camera.transform.position += (mouseWorldBeforeZoom - mouseWorldAfterZoom);

                // Lerp the camera position back toward the initial position smoothly
                _camera.transform.position = Vector3.Lerp(_camera.transform.position, initialPosition, Time.deltaTime * (ZOOMERSPEED * 0.5f));
            }
            else
            {
                _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, targetZoom, Time.deltaTime * ZOOMERSPEED);
            }
        }
    }
}
