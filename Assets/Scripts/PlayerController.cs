using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Component Refs")]
    [SerializeField] private InputHandler _inputHandler;
    [SerializeField] private Transform _cameraAnchor;
    [SerializeField] private CameraController _cameraController;
    
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
    //[SerializeField] private float _wallRunSpeed;
    [SerializeField] private float _maxMoveSpeed;
    [SerializeField] private float _maxSprintSpeed;
    [SerializeField] private float _acceleration;
    [SerializeField] private AnimationCurve _accelerationFactor;
    [SerializeField] private float _maxAcceleration;
    [SerializeField] private AnimationCurve _maxAccelerationFactor;
    [SerializeField] private float _maxInAirAcceleration;
    
    [Header("Mouse Sensitivity")]
    [SerializeField] private float _horizontalSensitivity;
    [SerializeField] private float _verticalSensitivity;
    
    //Private Component Refs
    private Rigidbody _rgBody;
    
    //Player state
    private PlayerState _state;
    
    //Private Flags
    private bool _isGrounded;
    private bool _canJump = true;
    private bool _isJumping;
    private bool _isSprinting;
    private bool _isWallRunning;
    private bool _isSwinging;
    
    //Public Properties
    public bool IsGrounded => _isGrounded;
    public bool CanJump => _canJump;
    public bool IsJumping => _isJumping;
    public bool IsSprinting => _isSprinting;
    public bool IsWallRunning
    {
        get => _isWallRunning;
        set => _isWallRunning = value; 
    }

    public bool IsSwinging
    {
        get => _isSwinging;
        set => _isSwinging = value;
    }
    
    //Private Timers
    private float _jumpTimer;
    
    //Input Variables
    private Vector2 _movementInput;
    private Vector2 _mouseInput;
    
    //Player transform
    private float _horizontalRotation;
    private float _verticalRotation;
    
    //Physics Variables
    private float _bodyMass;
    private Vector3 _targetVelocity;
    
    #region Unity Functions
    
    private void Awake()
    {
        //Lock mouse
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        
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
        LookAtMouse();
        
        if(_isWallRunning) return;
        
        GroundCheck();
        ApplyGravity();
        HandleMove(_movementInput);
        
        if (_isJumping || _isSwinging) return;
        
        Hover();
        
    }

    private void Update()
    {
        _mouseInput = Mouse.current.delta.value;
        
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

    private void LateUpdate()
    {
        _cameraController.transform.position = _cameraAnchor.position;
        _cameraController.RotateCamera(_horizontalRotation, _verticalRotation);
    }

    #endregion
    
    #region Private Functions

    private void ApplyGravity()
    {
        _rgBody.AddForce(Vector3.down * _gravity * _bodyMass);
    }

    private bool RaycastUnderPlayer(float distance,LayerMask layerToCheck, out RaycastHit hit)
    {
        if (Physics.Raycast(transform.position, -transform.up, out hit, distance, layerToCheck))
        {
            return true;
        }
        
        return false;
    }
    
    private bool RaycastUnderPlayer(float distance,LayerMask layerToCheck)
    {
        if (Physics.Raycast(transform.position, -transform.up, distance, layerToCheck))
        {
            return true;
        }
        
        return false;
    }

    private void GroundCheck()
    {
        if (RaycastUnderPlayer(_rideHeight, LayerMask.GetMask("Ground")))
        {
            _isGrounded = true;
        }
        else if(RaycastUnderPlayer(_rideHeight + _rideHeight / 2, LayerMask.GetMask("SlopedGround")))
        {
            _isGrounded = true;
        }
        else
        {
            _isGrounded = false;
        }
    }
    
    private void Hover()
    {
        //Get the offset
        if (RaycastUnderPlayer(_rideHeight + _rideHeight / 2, LayerMask.GetMask("Ground", "SlopedGround"), out RaycastHit hit))
        {
            Vector3 velocity = _rgBody.velocity;
            Vector3 rayDirection = -transform.up;

            float rayDirectionVelocity = Vector3.Dot(rayDirection, velocity);
            float x = hit.distance - _rideHeight;

            float springForce = ((x * _springPower) - (rayDirectionVelocity * _dampPower)) * _bodyMass;
            
            _rgBody.AddForce(rayDirection * springForce);
        }
        
        //Debug sphere
        _rideLineEnd.localPosition = new Vector3(0, -_rideHeight, 0);
    }

    private void HandleMove(Vector2 movementInput)
    {
        //Need to check if it is x or y that is 0 then use 
        if (!_isGrounded && movementInput.magnitude <= 0)
        {
            return;
        }
        
        Vector3 targetDirection = new Vector3(movementInput.x, 0, movementInput.y);
        targetDirection = transform.TransformDirection(targetDirection);
        
        ApplyForce(targetDirection);
    }
    
    private void HandleJump()
    {
        if (_isWallRunning) return;
        
        Vector3 velocity = _rgBody.velocity;
        Vector3 jumpForce = transform.up * _jumpPower;
        jumpForce *= _bodyMass;
        
        if ((_isGrounded) && _canJump)
        {
            _isJumping = true;
            //Reset rgBody velocity;
            _rgBody.velocity = new Vector3(velocity.x, 0, velocity.z);
            _rgBody.AddForce(jumpForce, ForceMode.Impulse);
            //Log("Jumped!", "Jump()");
            _canJump = false;
        }
        else if (!_isGrounded && _canJump && _canDoubleJump)
        {
            _isJumping = true;
            _rgBody.velocity = new Vector3(velocity.x, 0, velocity.z);
            _rgBody.AddForce(jumpForce, ForceMode.Impulse);
            //Log("Jumped!", "Jump()");
            _canJump = false;
        }
    }
    
    private void LookAtMouse()
    {
        _verticalRotation -= _mouseInput.y * _verticalSensitivity * Time.deltaTime;
        _verticalRotation = Mathf.Clamp(_verticalRotation, -90f, 90f);
        _horizontalRotation += _mouseInput.x * _horizontalSensitivity * Time.deltaTime;
        
        //if(_isWallRunning) return;
        
        _rgBody.MoveRotation(Quaternion.AngleAxis(_horizontalRotation, Vector3.up));
    }
    
    private void Log(string msg, string funcName = "")
    {
        Debug.Log("[PlayerController]" + funcName + ": " + msg);
    }
    
    #endregion

    #region Public Functions

    public void ApplyForce(Vector3 targetDirection, float wallRunSpeed = 0)
    {
        Vector3 unitVelocity = _targetVelocity.normalized;

        float velocityDot = Vector3.Dot(targetDirection, unitVelocity);
        float currentAcceleration = _acceleration * _accelerationFactor.Evaluate(velocityDot);

        float speed = _isSprinting ? _maxSprintSpeed : _maxMoveSpeed;

        if (_isWallRunning)
        {
            speed = wallRunSpeed;
        }
        
        Vector3 maxTargetVelocity = targetDirection * speed;

        _targetVelocity = Vector3.MoveTowards(_targetVelocity, maxTargetVelocity, currentAcceleration * Time.fixedDeltaTime);
        
        Vector3 neededAcceleration = (_targetVelocity - _rgBody.velocity) / Time.fixedDeltaTime;
        float maxAcceleration = _maxAcceleration * _maxAccelerationFactor.Evaluate(velocityDot);
        
        neededAcceleration = Vector3.ClampMagnitude(neededAcceleration, maxAcceleration);
        
        Vector3 force = Vector3.Scale(neededAcceleration * _rgBody.mass, new Vector3(1, 0, 1));

        if (!_isGrounded && !_isWallRunning)
        {
            force *= _maxInAirAcceleration;
        }
        
        _rgBody.AddForce(force);
    }
    
    #endregion
    
    #region Input Callbacks

    private void Move(Vector2 movementInput)
    {
        //Log("MovementInput:" + movementInput, "Move()");
        _movementInput = movementInput;

        if (_movementInput.magnitude <= 0)
        {
            _cameraController.ChangeFOV(CameraFOV.NEUTRAL);
        }
    }
    
    private void Jump()
    {
        //Log("Jumped!", "Jump()");
        HandleJump();
    }
    
    private void Sprint(bool isSprinting)
    {
        //Log("Sprint? " + isSprinting);
        _isSprinting = isSprinting;
        
        if (_movementInput.magnitude > 0 && _isSprinting)
        {
            _cameraController.ChangeFOV(CameraFOV.SPRINTING);
        }
        else if (!_isSprinting && !_isWallRunning)
        {
            _cameraController.ChangeFOV(CameraFOV.NEUTRAL);
        }
    }

    #endregion
}
