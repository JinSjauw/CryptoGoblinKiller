using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "Input/Input Handler")]
public class InputHandler : ScriptableObject, StandardInput.IGameplayActions
{
    //Movement Events
    public UnityAction<Vector2> MoveEvent;
    public UnityAction JumpEvent;
    public UnityAction<bool> SprintEvent;
    public UnityAction<bool> GrappleEvent;
    
    private StandardInput _input;
    
    private void OnEnable()
    {
        if (_input == null)
        {
            _input = new StandardInput();
        }
        
        _input.Gameplay.Enable();
        _input.Gameplay.SetCallbacks(this);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            MoveEvent.Invoke(context.ReadValue<Vector2>());
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            JumpEvent.Invoke();
        }
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            SprintEvent.Invoke(true);
        }

        if (context.canceled)
        {
            SprintEvent.Invoke(false);
        }
    }

    public void OnGrapple(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            GrappleEvent.Invoke(true);
        }

        if (context.canceled)
        {
            GrappleEvent.Invoke(false);
        }
    }
}
