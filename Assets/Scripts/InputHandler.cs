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
    public UnityAction<Vector2> GrappleActuateEvent;
    public UnityAction ShootEvent;
    public UnityAction<float> WeaponChangeEvent;
    public UnityAction WeaponReloadEvent;
    
    private Vector2 _grappleRopeInput;
    
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

    public void DisableGameplayInput()
    {
        _input.Gameplay.Disable();
    }
    
    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            MoveEvent?.Invoke(context.ReadValue<Vector2>());
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            JumpEvent?.Invoke();
        }
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            SprintEvent?.Invoke(true);
        }

        if (context.canceled)
        {
            SprintEvent?.Invoke(false);
        }
    }

    public void OnGrapple(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            GrappleEvent?.Invoke(true);
        }

        if (context.canceled)
        {
            GrappleEvent?.Invoke(false);
        }
    }

    public void OnExtendRope(InputAction.CallbackContext context)
    {
       
    }

    public void OnShortenRope(InputAction.CallbackContext context)
    {
       
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ShootEvent?.Invoke();
        }
    }

    public void OnChangeWeapon(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            WeaponChangeEvent?.Invoke(context.ReadValue<float>());
        }
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            WeaponReloadEvent?.Invoke();
        }
    }
}
