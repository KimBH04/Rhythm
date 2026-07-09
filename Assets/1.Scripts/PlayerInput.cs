using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(ChartManager))]
public class PlayerInput : MonoBehaviour
{
    private ChartManager chartManager;

    void Start()
    {
        chartManager = GetComponent<ChartManager>();
    }

    public void OnButtonAction(InputAction.CallbackContext context)
    {
        int idx = context.action.name switch
        {
            "1" => 0,
            "2" => 1,
            "3" => 2,
            "4" => 3,
            _   => throw new ArgumentOutOfRangeException($"{context.action.name} is invalid name.")
        };

        if (context.started)
        {
            chartManager.OnClick(idx);
        }
        else if (context.canceled)
        {
            chartManager.OnCancel(idx);
        }
    }
}
