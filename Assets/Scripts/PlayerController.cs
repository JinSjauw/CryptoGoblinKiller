using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Component Refs")]
    [SerializeField] private InputHandler _inputHandler;

    [Header("Leg Spring")] 
    [SerializeField] private Transform _rideLineEnd;
    [SerializeField] private float _rideHeight;
    [SerializeField] private float _springPower;
    [SerializeField] private float _dampPower;

    [Header("Jump")] 
    [SerializeField] private bool _canDoubleJump;
    [SerializeField] private float _jumpDelay;
    [SerializeField] private float _jumpPower;
    
    //Private Component Refs
    private Rigidbody _rgBody;
    
    //Private Flags
    private bool _isGrounded;
    private bool _canJump = true;
    private bool _isJumping;
    private bool _isSprinting;
    
    //Private Timers
    private float jumpTimer;
    
    #region Unity Functions
    
    private void Awake()
    {
        //Get Component Refs
        _rgBody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        //Subscribe to all the relevant events
        
        _inputHandler.MoveEvent += Move;
        _inputHandler.JumpEvent += Jump;
        _inputHandler.SprintEvent += Sprint;
    }
    
    void FixedUpdate()
    {
        if (_isJumping) return;
        Hover();
    }

    private void Update()
    {
        if (!_canJump)
        {
            jumpTimer += Time.deltaTime;

            if (jumpTimer >= _jumpDelay)
            {
                _canJump = true;
                _isJumping = false;
                jumpTimer = 0;
            }
        }
    }

    #endregion
    
    #region Private Functions

    private void Hover()
    {
        //Get the offset
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit,_rideHeight, LayerMask.GetMask("Ground")))
        {
            Vector3 velocity = _rgBody.velocity;
            Vector3 rayDirection = -transform.up;

            float rayDirectionVelocity = Vector3.Dot(rayDirection, velocity);
            float x = hit.distance - _rideHeight;

            float springForce = ((x * _springPower)) - (rayDirectionVelocity * _dampPower);
            
            _rgBody.AddForce(rayDirection * springForce);

            _isGrounded = true;
        }
        else
        {
            _isGrounded = false;
        }

        _rideLineEnd.localPosition = new Vector3(0, -_rideHeight, 0);
    }

    private void HandleJump()
    {
        Vector3 velocity = _rgBody.velocity;
        Vector3 jumpForce = transform.up * _jumpPower;
        
        if (_isGrounded && _canJump)
        {
            _isJumping = true;
            //Reset rgBody velocity;
            _rgBody.velocity = new Vector3(velocity.x, 0, velocity.z);
            _rgBody.AddForce(jumpForce, ForceMode.Impulse);
            Log("Jumped!", "Jump()");
            _canJump = false;
        }
        else if (!_isGrounded && _canJump && _canDoubleJump)
        {
            _isJumping = true;
            _rgBody.velocity = new Vector3(velocity.x, 0, velocity.z);
            _rgBody.AddForce(jumpForce, ForceMode.Impulse);
            Log("Jumped!", "Jump()");
            _canJump = false;
        }
    }
    
    private void Log(string msg, string funcName = "")
    {
        Debug.Log("[PlayerController]" + funcName + ": " + msg);
    }
    
    #endregion
    
    #region Input Callbacks

    private void Move(Vector2 movementInput)
    {
        Log("MovementInput:" + movementInput, "Move()");
    }
    
    private void Jump()
    {
        //Log("Jumped!", "Jump()");
        HandleJump();
    }
    
    private void Sprint(bool isSprinting)
    {
        Log("Sprint? " + isSprinting);
    }

    #endregion
}
