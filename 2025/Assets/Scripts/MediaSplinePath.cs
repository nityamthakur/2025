// Modified Code Originally from Code Monkey on Youtube
// https://www.youtube.com/watch?v=7j_BNf9s0jM

using UnityEngine;
using System.Collections;

public class MediaSplinePath : MonoBehaviour
{
    [SerializeField] private Transform enterStartPoint;
    [SerializeField] private Transform enterEndPoint;
    [SerializeField] private Transform leaveStartPointA;
    [SerializeField] private Transform leaveEndPointA;
    [SerializeField] private Transform leaveStartPointB;
    [SerializeField] private Transform leaveEndPointB;
    [SerializeField] private Transform mediaObject; // The object to move
    [SerializeField] private float duration = 1f;  // Default movement time
    private bool isMoving = false;

    public float GetDuration()
    {
        return duration;
    }
    
    public void EntranceMovement(Transform target, System.Action onComplete = null) 
    {
        StartMovement(target, enterStartPoint, enterEndPoint, duration, true, onComplete);
    }
    public void ExitMovementDestroy(Transform target) 
    {
        StartMovement(target, leaveStartPointA, leaveEndPointA, duration, false);
    }    
    public void ExitMovementAccept(Transform target) 
    {
        StartMovement(target, leaveStartPointB, leaveEndPointB, duration, false);
    }

    // Function to move an object between two points with optional ease-in/out
    public void StartMovement(Transform target, Transform start, Transform end, float moveTime, bool easeOut, System.Action onComplete = null)
    {
        if (!isMoving)
        {
            EventManager.DisplayDeskOverlay?.Invoke(true); 
            StartCoroutine(MoveBetweenPoints(target, start.position, end.position, moveTime, easeOut, onComplete));
        }
    }

    private IEnumerator MoveBetweenPoints(Transform target, Vector3 start, Vector3 end, float moveTime, bool easeOut, System.Action onComplete = null)
    {
        isMoving = true;
        float elapsedTime = 0f;

        while (elapsedTime < moveTime)
        {
            float t = elapsedTime / moveTime; // Normalize time (0 to 1)
            float easedT = easeOut ? 1 - (1 - t) * (1 - t) : t * t; // Quadratic ease-in or ease-out
            target.position = Vector3.Lerp(start, end, easedT);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        target.position = end; // Ensure exact position at the end
        isMoving = false;
        EventManager.DisplayDeskOverlay?.Invoke(false); 

        onComplete?.Invoke(); // Call the callback if provided
    }
}
