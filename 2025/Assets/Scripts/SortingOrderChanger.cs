using System.Collections.Generic;
using UnityEngine;

// For moving all layers of the newspaper object and keeping their orders intact
public class SortingOrderChanger : MonoBehaviour
{
    public List<RendererData> rendererDataList = new();

    public void StoreAllRenderOrders()
    {
        foreach (var renderer in GetComponentsInChildren<Renderer>(true))
        {
            RendererData layer = new()
            {
                renderer = renderer,
                baseOrder = renderer.sortingOrder
            };
            rendererDataList.Add(layer);
        }
    }

    public void ChangeSortingOrders(int order)
    {
        foreach (var data in rendererDataList)
        {
            if (data.renderer != null)
                data.renderer.sortingOrder = data.baseOrder + order;
        }
    }
}

[System.Serializable]
public class RendererData
{
    public Renderer renderer;
    public int baseOrder;
}
