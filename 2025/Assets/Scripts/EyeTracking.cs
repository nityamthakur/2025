// Modified Code Originally from Qookie Games on Youtube
// https://www.youtube.com/watch?v=mWX-k453-P8

using System.Collections;
using UnityEngine;

public class EyeTracking : MonoBehaviour
{
    public GameObject pupil;
    public Camera mainCamera;
    public float speed = 10;
    public float intensity = 0.25f;
    private bool isBlinking = false;
    private GameManager gameManager;

    void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>();
    }

    void Start()
    {
        mainCamera = Camera.main;
    }
    // Update is called once per frame
    void Update()
    {
        if (mainCamera != null && gameManager.GetJobScene() != null)
        {
            EyesAim();
            if (!isBlinking) StartCoroutine(EyeBlinkRed());
        }
    }

    void EyesAim()
    {
        /* Get the mouse position in world space rather than screen space. */
        var mouseWorldCoord = mainCamera.ScreenPointToRay(Input.mousePosition).origin;

        /* Get a vector pointing from initialPosition to the target. Vector shouldn't be longer than maxDistance. */
        var originToMouse = mouseWorldCoord - transform.position;
        originToMouse = Vector3.ClampMagnitude(originToMouse, intensity);

        /* Linearly interpolate from current position to mouse's position. */
        pupil.transform.position = Vector3.Lerp(pupil.transform.position, transform.position + originToMouse, speed * Time.deltaTime);
    }

    IEnumerator EyeBlinkRed()
    {
        while (true)
        {
            isBlinking = true;
            pupil.GetComponent<SpriteRenderer>().color = Color.red;
            yield return new WaitForSeconds(0.5f);
            pupil.GetComponent<SpriteRenderer>().color = Color.black;
            yield return new WaitForSeconds(3f);
        }
    }
}
