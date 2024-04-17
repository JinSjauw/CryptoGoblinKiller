using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Component Refs")]
    [SerializeField] private InputHandler _inputHandler;

    [Header("General Physics")] 
    [SerializeField] private float _gravity;
    
    [Header("Leg Spring")] 
    [SerializeField] private Transform _rideLineEnd;
    [SerializeField] private float _rideHeight;
    [SerializeField] private float _springPower;
    [SerializeField] private float _dampPower;

    [Header("Jump")] 
    [SerializeField] private bool _canDoubleJump;
    [SerializeField] private float _jumpDelay;
    [SerializeField] private float _jumpPower;

    [Header("Move")] 
    [SerializeField] private float _maxMoveSpeed;
    [SerializeField] private float _maxSprintSpeed;
    [SerializeField] private float _acceleration;
    [SerializeField] private AnimationCurve _accelerationFactor;
    [SerializeField] private float _maxAcceleration;
    [SerializeField] private AnimationCurve _maxAccelerationFactor;
    [SerializeField] private float _maxInAirAcceleration;
    
    //Private Component Refs
    private Rigidbody _rgBody;
    
    //Private Flags
    private bool _isGrounded;
    private bool _canJump = true;
    private bool _isJumping;
    private bool _isSprinting;
    
    //Private Timers
    private float _jumpTimer;
    
    //Input Variables
    private Vector2 _movementInput;
    
    //Physics Variables
    private float _bodyMass;
    private Vector3 _targetVelocity;
    
    #region Unity Functions
    
    private void Awake()
    {
        //Get Component Refs
        _rgBody = GetComponent<Rigidbody>();
        _bodyMass = _rgBody.mass;
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
        ApplyGravity();
        
        if (_isJumping) return;
        
        Hover();
        HandleMove(_movementInput);
    }

    private void Update()
    {
        if (!_canJump)
        {
            _jumpTimer += Time.deltaTime;

            if (_jumpTimer >= _jumpDelay)
            {
                _canJump = true;
                _isJumping = false;
                _jumpTimer = 0;
            }
        }
    }

    #endregion
    
    #region Private Functions

    private void ApplyGravity()
    {
        _rgBody.AddForce(Vector3.down * _gravity * _bodyMass);
    }
    
    private void Hover()
    {
        //Get the offset
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit,_rideHeight, LayerMask.GetMask("Ground")))
        {
            Vector3 velocity = _rgBody.velocity;
            Vector3 rayDirection = -transform.up;

            float rayDirectionVelocity = Vector3.Dot(rayDirection, velocity);
            float x = hit.distance - _rideHeight;

            float springForce = ((x * _springPower) - (rayDirectionVelocity * _dampPower)) * _bodyMass;
            
            _rgBody.AddForce(rayDirection * springForce);

            _isGrounded = true;
        }
        else
        {
            _isGrounded = false;
        }

        _rideLineEnd.localPosition = new Vector3(0, -_rideHeight, 0);
    }

    private void HandleMove(Vector2 movementInput)
    {
        Vector3 targetDirection = new Vector3(movementInput.x, 0, movementInput.y);
        targetDirection = transform.TransformDirection(targetDirection);
        
        Vector3 unitVelocity = _targetVelocity.normalized;

        float velocityDot = Vector3.Dot(targetDirection, unitVelocity);
        float currentAcceleration = _acceleration * _accelerationFactor.Evaluate(velocityDot);

        Vector3 maxTargetVelocity = targetDirection * (_isSprinting ? _maxSprintSpeed : _maxMoveSpeed);

        _targetVelocity = Vector3.MoveTowards(_targetVelocity, maxTargetVelocity, currentAcceleration * Time.fixedDeltaTime);

        Vector3 neededAcceleration = (_targetVelocity - _rgBody.velocity) / Time.fixedDeltaTime;
        float maxAcceleration = _maxAcceleration * _maxAccelerationFactor.Evaluate(velocityDot);

        if (!_isGrounded)
        {
            neededAcceleration *= _maxInAirAcceleration;
            maxAcceleration *= _maxInAirAcceleration;
        }
        
        neededAcceleration = Vector3.ClampMagnitude(neededAcceleration, maxAcceleration);

        _rgBody.AddForce(Vector3.Scale(neededAcceleration * _rgBody.mass, new Vector3(1, 0, 1)));
    }
    
    private void HandleJump()
    {
        Vector3 velocity = _rgBody.velocity;
        Vector3 jumpForce = transform.up * _jumpPower;
        jumpForce *= _bodyMass;
        
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
        _movementInput = movementInput;
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
