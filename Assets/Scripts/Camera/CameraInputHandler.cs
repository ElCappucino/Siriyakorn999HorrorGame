using UnityEngine;
using UnityEngine.InputSystem;

public class CameraInputHandler : MonoBehaviour
{
    private CameraControl cameraControl;

    private void Awake()
    {
        cameraControl = GetComponent<CameraControl>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector2 inputDirection = context.ReadValue<Vector2>();

            cameraControl.PanCamera(inputDirection);
        }
        
    }

    public void OnHoldTalisman(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            cameraControl.HoldTalisman(true);
        }
        else if (context.canceled)
        {
            cameraControl.HoldTalisman(false);
        }

    }
}
