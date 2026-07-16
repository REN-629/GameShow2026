using UnityEngine;
using UnityEngine.EventSystems;

public class TitleMenuHoverRelay :
    MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    public TitleMonitorMenuController owner;
    public UnityEngine.Events.UnityAction onHoverEnter;
    public UnityEngine.Events.UnityAction onHoverExit;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (owner != null)
            owner.SetCursorHover(true);

        onHoverEnter?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (owner != null)
            owner.SetCursorHover(false);

        onHoverExit?.Invoke();
    }
}
