using UnityEngine;

public class Entity : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"Collided with: {collision.gameObject.name}, Tag: {collision.gameObject.tag}");
        if (collision.gameObject.CompareTag("DropBox"))
        {
            Debug.Log("DropBox detected, destroying...");
            Destroy(collision.gameObject);
        }
    }

    private void OnDestroy()
    {
        EventManager.OnMediaDestroyed?.Invoke(gameObject);
    }
}