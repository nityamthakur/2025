using UnityEngine;

public class Entity : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        bool draggable = GetComponent<Draggable>();
        Debug.Log($"Draggable? {draggable}");

        //Debug.Log($"Collided with: {collision.gameObject.name}, Tag: {collision.gameObject.tag}");
        if (collision.gameObject.CompareTag("DropBox"))
        {
            //Debug.Log("DropBox detected, destroying...");
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        EventManager.OnMediaDestroyed?.Invoke(gameObject);
    }
}