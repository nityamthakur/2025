using UnityEngine;

public class SortingOrderChanger : MonoBehaviour
{
    public void ChangeSortingOrders(int num)
    {
        int changes = 0;
        foreach (var sr in GetComponentsInChildren<SpriteRenderer>(true))
        {
            sr.sortingOrder += num;
            changes++;
        }

        foreach (var mr in GetComponentsInChildren<MeshRenderer>(true))
        {
            mr.sortingOrder += num;
            changes++;
        }
    }
}
