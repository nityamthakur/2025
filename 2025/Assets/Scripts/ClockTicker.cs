using System.Collections;
using UnityEngine;

public class ClockTicker : MonoBehaviour
{
    [SerializeField] private Transform hourHand, minuteHand;
    private readonly float updateTime = 0.5f;
    private float workTime, hourDegreesPerTick, minuteDegreesPerTick;
    private Coroutine tickingCoroutine;
    private GameManager gameManager;

    private void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>();   
    }

    public void StartClockMovement(float time)
    {
        workTime = time;

        // Calculate how many degrees each tick should move the hands
        int totalTicks = Mathf.CeilToInt(workTime / updateTime);
        hourDegreesPerTick = 180f / totalTicks; // Move 180 degrees from left to right
        minuteDegreesPerTick = 360f * 6 / totalTicks; // Move 360 x 6 degrees

        if (tickingCoroutine != null)
            StopCoroutine(tickingCoroutine);

        tickingCoroutine = StartCoroutine(TickClock());
    }

    public void StopClockMovement()
    {
        if (tickingCoroutine != null)
            StopCoroutine(tickingCoroutine);

        tickingCoroutine = null;
    }

    private IEnumerator TickClock()
    {
        float currentHourRotation = 0f;
        float currentMinuteRotation = 0f;

        int totalTicks = Mathf.CeilToInt(workTime / updateTime);
        for (int i = 0; i < totalTicks; i++)
        {
            currentHourRotation += hourDegreesPerTick;
            currentMinuteRotation += minuteDegreesPerTick;

            if (hourHand)
                hourHand.eulerAngles = new Vector3(0, 0, -currentHourRotation);

            if (minuteHand)
                minuteHand.eulerAngles = new Vector3(0, 0, -currentMinuteRotation);

            yield return new WaitForSeconds(updateTime);
            if (gameManager.gameZoomPaused)
            {
                Debug.Log("Time Stopped for TickClock");
                yield return new WaitUntil(() => gameManager.gameZoomPaused == false);
                Debug.Log("Time Resumed for TickClock");
            }

        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        tickingCoroutine = null;
    }

    private void OnEnable()
    {
        EventManager.StartClockMovement += StartClockMovement;
        EventManager.StopClockMovement += StopClockMovement;
    }

    private void OnDisable()
    {
        EventManager.StartClockMovement -= StartClockMovement;
        EventManager.StopClockMovement -= StopClockMovement;
    }
}
