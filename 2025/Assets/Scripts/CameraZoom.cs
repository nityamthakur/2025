using System.Collections;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    private float startingZoom;
    private Vector3 startingPosition;
    [SerializeField] GameManager gameManager;

    private float zoom;
    private Coroutine zoomCoroutine;
    [SerializeField] private Camera cam;

    private void Start()
    {
        zoom = cam.orthographicSize;
        startingZoom = cam.orthographicSize;
        startingPosition = cam.transform.position;
    }

    private void Update()
    {
        //float scroll = Input.GetAxis("Mouse ScrollWheel");
        //zoom -= scroll * zoomMultiplier;
        //zoom = Mathf.Clamp(zoom, minZoom, maxZoom);
        //cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, zoom, ref velocity, smoothTime);
    }

    private void ZoomCamera(Transform target, float zoomSize, float duration)
    {
        if (zoomCoroutine != null)
            StopCoroutine(zoomCoroutine);
        zoomCoroutine = StartCoroutine(ZoomToTarget(target, zoomSize, duration));
    }

    IEnumerator ZoomToTarget(Transform target, float zoomSize, float duration = 0f)
    {
        gameManager.gameZoomPaused = true;
        EventManager.CameraZoomed?.Invoke(true);
        Camera cam = Camera.main;
        Vector3 startPos = cam.transform.position;
        float startSize = cam.orthographicSize;

        Vector3 targetPos = new Vector3(target.position.x, target.position.y, cam.transform.position.z);

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float lerp = t / duration;
            cam.transform.position = Vector3.Lerp(startPos, targetPos, lerp);
            cam.orthographicSize = Mathf.Lerp(startSize, zoomSize, lerp);
            yield return null;
        }

        cam.transform.position = targetPos;
        cam.orthographicSize = zoomSize;
    }

    private void ResetCamera(float duration)
    {
        if (zoomCoroutine != null)
            StopCoroutine(zoomCoroutine);
        zoomCoroutine = StartCoroutine(ResetZoom(duration));
    }
    public IEnumerator ResetZoom(float duration = 0.0f)
    {
        gameManager.gameZoomPaused = false;
        Vector3 startPos = cam.transform.position;
        float startSize = cam.orthographicSize;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float lerp = t / duration;
            cam.transform.position = Vector3.Lerp(startPos, startingPosition, lerp);
            cam.orthographicSize = Mathf.Lerp(startSize, startingZoom, lerp);
            yield return null;
        }

        cam.transform.position = startingPosition;
        cam.orthographicSize = startingZoom;
        EventManager.CameraZoomed?.Invoke(false);
    }

    void OnEnable()
    {
        EventManager.ZoomCamera += ZoomCamera;
        EventManager.ResetCamera += ResetCamera;
    }

    void OnDisable()
    {
        EventManager.ZoomCamera -= ZoomCamera;
        EventManager.ResetCamera -= ResetCamera;
    }

    private void OnDestroy()
    {
        StopAllCoroutines();        
    }

}