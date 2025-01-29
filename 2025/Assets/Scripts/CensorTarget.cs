using UnityEngine;

public class CensorTarget : MonoBehaviour
{
    private Entity parentEntity; // Reference to the parent media object

    void Start()
    {
        // Get the parent object (Entity)
        parentEntity = GetComponentInParent<Entity>();
        if (parentEntity == null)
        {
            Debug.LogError("No parent Entity found for CensorTarget!");
        }
    }

    private void OnMouseDown()
    {
        // Call the censoring logic on the parent object
        parentEntity.CensorObject();
    }
}