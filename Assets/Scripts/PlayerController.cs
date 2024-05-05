using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Object Refs")]
    [SerializeField] private InputHandler _inputHandler;
    [SerializeField] private Transform _cameraAnchor;
    [SerializeField] private CameraController _cameraController;
    [SerializeField] private WeaponController _weaponController;
    [SerializeField] private TextMeshProUGUI _speedText;
    
    [Header("General Physics")] 
    [SerializeField] private float _gravity;
    
    [Header("Leg Spring")] 
    [SerializeField] private float _rideHeight;
    [SerializeField] private float _springPower;
    [SerializeField] private float _dampPower;
    
    [Header("Camera Tilt Spring")] 
    [SerializeField] private float _tiltAmount;
    [SerializeField] private SpringData _tiltSpring;
    
    [Header("Jump")] 
    [SerializeField] private bool _canDoubleJump;
    [SerializeField] private float _jumpDelay;
    [SerializeField] private float _jumpPower;

    [Header("Move")]
    //[SerializeField] private float _wallRunSpeed;
    [SerializeField] private float _maxSwingingSpeed;
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

    public float Acceleration
    {
        set => _acceleration = value;
    }
    
    public bool IsSwinging
    {
        get => _isSwinging;
        set => _isSwinging = value;
    }
    
    //Private Timers
    private float _jumpTimer;
    private float _velocityTime;
    
    //Input Variables
    private Vector2 _movementInput;
    private Vector2 _mouseInput;
    
    //Player transform
    private float _horizontalRotation;
    private float _verticalRotation;
    
    //Physics Variables
    private float _bodyMass;
    private Vector3 _currentVelocity;
    private float _currentSpeed;
    private float _currentAcceleration;
    
    #region Unity Functions
    
    private void Awake()
    {
        //Lock mouse
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        //Get Component Refs
        _rgBody = GetComponent<Rigidbody>();
        _bodyMass = _rgBody.mass;
        
        //_weaponController.SetCamera(_cameraController);
    }

    void Start()
    {
        _inputHandler.MoveEvent += Move;
        _inputHandler.JumpEvent += Jump;
        _inputHandler.SprintEvent += Sprint;
    }
    
    void FixedUpdate()
    {
        _speedText.text = "Speed: " + _rgBody.velocity.magnitude + "\n" 
                          + "TargetVelocity: " + _currentVelocity.magnitude;
        
        if(_isWallRunning) return;
        
        RotatePlayer();
        GroundCheck();
        
        if(_isSwinging) return;
        
        ApplyGravity();
        
        if (_isJumping) return;
        
        HandleMove(_movementInput);
        Hover();
    }

    private void Update()
    {
        _mouseInput = Mouse.current.delta.value;
        LookAtMouse();
        
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
        Vector3 anchorPosition = _cameraAnchor.position;
        
        _cameraController.transform.position = anchorPosition;
        _cameraController.RotateCamera(_horizontalRotation, _verticalRotation);

        _weaponController.FollowAnchor(anchorPosition);
        _weaponController.RotateWeaponHolder(_horizontalRotation, _verticalRotation);
    }

    #endregion
    
    #region Private Functions

    private void ApplyGravity()
    {
        _rgBody.AddForce(Vector3.down * (_gravity * _bodyMass));
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
    }
    
    private void HandleCameraTilt(float x)
    {
        //Checking input is moving to the left or right
        if (x > .71f)
        {
            _cameraController.ChangeTilt(TiltState.LEFT, _tiltSpring, _tiltAmount );
            _weaponController.ChangeTilt(TiltState.LEFT, _tiltSpring, _tiltAmount);
        }
        else if (x < -.71f)
        {
            _cameraController.ChangeTilt(TiltState.RIGHT, _tiltSpring, _tiltAmount);
            _weaponController.ChangeTilt(TiltState.RIGHT, _tiltSpring, _tiltAmount);
        }
        else
        {
            _cameraController.ChangeTilt(TiltState.NEUTRAL, _tiltSpring);    
            _weaponController.ChangeTilt(TiltState.NEUTRAL, _tiltSpring);
        }
    }
    
    private void HandleMove(Vector2 movementInput)
    {
        //Need to check if it is x or y that is 0 then use 
        if (!_isGrounded && movementInput.magnitude <= 0)
        {
            return;
        }
        
        HandleCameraTilt(movementInput.x);
        
        //Log("Movement: " + movementInput);
        
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
        _verticalRotation -= _mouseInput.y * _verticalSensitivity * Time.smoothDeltaTime;
        _verticalRotation = Mathf.Clamp(_verticalRotation, -90f, 90f);
        _horizontalRotation += _mouseInput.x * _horizontalSensitivity * Time.smoothDeltaTime;
    }

    private void RotatePlayer()
    {
        _rgBody.MoveRotation(Quaternion.AngleAxis(_horizontalRotation, Vector3.up));
    }
    
    private void Log(string msg, string funcName = "")
    {
        Debug.Log("[PlayerController]" + funcName + ": " + msg);
    }
    
    #endregion

    #region Public Functions

    public Vector2 GetMouseInput()
    {
        return _mouseInput;
    }

    public Vector3 GetCurrentVelocity()
    {
        return _rgBody.velocity;
    }

    public Vector2 GetMoveInput()
    {
        return _movementInput;
    }
    
    public void ApplyForce(Vector3 targetDirection, float maxMoveSpeed = 0)
    {
        Vector3 unitVelocity = _currentVelocity.normalized;

        float velocityDot = Vector3.Dot(targetDirection, unitVelocity);
        float desiredAcceleration = _acceleration * _accelerationFactor.Evaluate(velocityDot);

        float desiredSpeed = _isSprinting ? _maxSprintSpeed : _maxMoveSpeed;

        if (_isWallRunning && maxMoveSpeed > 0)
        {
            desiredSpeed = maxMoveSpeed;
        }
        
        Vector3 desiredVelocity = targetDirection * desiredSpeed;
        
        _currentVelocity = Vector3.MoveTowards(_currentVelocity, desiredVelocity, desiredAcceleration * Time.fixedDeltaTime);
        
        Vector3 neededAcceleration = (_currentVelocity - _rgBody.velocity) / Time.fixedDeltaTime;
        
        float maxAcceleration = _maxAcceleration * _maxAccelerationFactor.Evaluate(velocityDot);
        neededAcceleration = Vector3.ClampMagnitude(neededAcceleration, maxAcceleration);
        
        Vector3 force = Vector3.Scale(neededAcceleration * _rgBody.mass, new Vector3(1, 0, 1));
        
        if (!_isGrounded && !_isWallRunning && !_isSwinging)
        {
            force *= _maxInAirAcceleration;
        }
        
        //Do a collide and slide when wallrunning
        if (_isWallRunning && Physics.Raycast(transform.position, _currentVelocity.normalized, out RaycastHit hit, 1.5f, LayerMask.GetMask("Wall")))
        {
            Vector3 snapToSurface = unitVelocity * (hit.distance);
            Vector3 leftover = _currentVelocity - snapToSurface;
            
            float magnitude = leftover.magnitude;
            leftover = Vector3.ProjectOnPlane(leftover, hit.normal).normalized;
            leftover *= magnitude;
            
            _rgBody.velocity = leftover;
            _currentVelocity = _rgBody.velocity;
            
            return;
        }
        
        _rgBody.AddForce(force);
        _currentVelocity = _rgBody.velocity;
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
