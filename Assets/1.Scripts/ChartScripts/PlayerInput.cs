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
        if (context.started)
        {
            chartManager.OnClick((int)context.ReadValue<float>());
        }
        else if (context.canceled)
        {
            chartManager.OnCancel((int)context.ReadValue<float>());
        }
    }
}
