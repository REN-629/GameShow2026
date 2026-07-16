using System.Collections.Generic;
using UnityEngine;

public class HeldItemVisualLayerApplier : MonoBehaviour
{
    [Header("Layer変更する見た目Root")]
    public Transform[] visualRoots;

    [Header("Rendererを自動検索する")]
    public bool autoFindRenderersIfEmpty = true;

    [Header("Collider付きObjectは変更しない")]
    public bool skipObjectsWithCollider = true;

    private readonly Dictionary<Transform, int> originalLayers =
        new Dictionary<Transform, int>();

    public void ApplyHeldLayer(int heldLayer)
    {
        originalLayers.Clear();

        if (visualRoots != null && visualRoots.Length > 0)
        {
            foreach (Transform root in visualRoots)
                ApplyLayerRecursive(root, heldLayer);

            return;
        }

        if (autoFindRenderersIfEmpty)
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);

            foreach (Renderer renderer in renderers)
            {
                if (renderer == null)
                    continue;

                GameObject obj = renderer.gameObject;

                if (skipObjectsWithCollider && obj.GetComponent<Collider>() != null)
                    continue;

                SaveAndSetLayer(obj.transform, heldLayer);
            }
        }
    }

    public void RestoreOriginalLayers()
    {
        foreach (KeyValuePair<Transform, int> pair in originalLayers)
        {
            if (pair.Key != null)
                pair.Key.gameObject.layer = pair.Value;
        }

        originalLayers.Clear();
    }

    void ApplyLayerRecursive(Transform root, int layer)
    {
        if (root == null)
            return;

        SaveAndSetLayer(root, layer);

        foreach (Transform child in root)
            ApplyLayerRecursive(child, layer);
    }

    void SaveAndSetLayer(Transform target, int layer)
    {
        if (target == null)
            return;

        if (skipObjectsWithCollider && target.GetComponent<Collider>() != null)
            return;

        if (!originalLayers.ContainsKey(target))
            originalLayers.Add(target, target.gameObject.layer);

        target.gameObject.layer = layer;
    }
}
