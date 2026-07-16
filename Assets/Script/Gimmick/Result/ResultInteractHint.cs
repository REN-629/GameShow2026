using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResultInteractHint : MonoBehaviour
{
    private static readonly List<ResultInteractHint> Instances =
        new List<ResultInteractHint>();

    public string message = "クリックして調べる";
    public Transform positionTarget;
    public Vector3 worldOffset = new Vector3(0f, 0.35f, 0f);
    public TMP_FontAsset fontAsset;
    public float fontSize = 2.5f;
    public Color textColor = Color.white;
    public bool faceCamera = true;
    public Camera targetCamera;

    private TextMeshPro hintText;

    void Awake()
    {
        Instances.Add(this);

        if (positionTarget == null)
            positionTarget = transform;

        if (targetCamera == null)
            targetCamera = Camera.main;

        GameObject textObject =
            new GameObject("ResultInteractHintText");

        textObject.transform.SetParent(transform, true);

        hintText = textObject.AddComponent<TextMeshPro>();
        hintText.text = message;
        hintText.fontSize = fontSize;
        hintText.color = textColor;
        hintText.alignment = TextAlignmentOptions.Center;
        hintText.enableWordWrapping = false;
        hintText.raycastTarget = false;
        hintText.outlineColor = Color.black;
        hintText.outlineWidth = 0.25f;

        if (fontAsset != null)
            hintText.font = fontAsset;

        RefreshVisibility();
    }

    void OnDestroy()
    {
        Instances.Remove(this);
    }

    void LateUpdate()
    {
        if (hintText == null)
            return;

        hintText.transform.position =
            positionTarget.position + worldOffset;

        if (!faceCamera)
            return;

        Camera cam =
            targetCamera != null
            ? targetCamera
            : Camera.main;

        if (cam != null)
        {
            hintText.transform.rotation =
                Quaternion.LookRotation(
                    hintText.transform.position -
                    cam.transform.position
                );
        }
    }

    public void RefreshVisibility()
    {
        if (hintText == null)
            return;

        bool visible =
            ResultCursorController.Instance == null ||
            !ResultCursorController.Instance.IsInteractionLocked;

        hintText.gameObject.SetActive(visible);
    }

    public static void RefreshAll()
    {
        for (int i = Instances.Count - 1; i >= 0; i--)
        {
            if (Instances[i] == null)
            {
                Instances.RemoveAt(i);
                continue;
            }

            Instances[i].RefreshVisibility();
        }
    }
}
