using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class NotePlacementInput : MonoBehaviour, IPointerClickHandler
{
    public static event Action<int, double> OnTrackClickEvent;

    [SerializeField] private int line;

    public void OnPointerClick(PointerEventData eventData)
    {
        var point = Camera.main.ScreenToWorldPoint(eventData.position);
        var worldY = point.y;
        OnTrackClickEvent?.Invoke(line, worldY);
    }
}
