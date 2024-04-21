using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class WallRunning : MonoBehaviour
{
    [Header("Input Handler")] 
    [SerializeField] private InputHandler _inputHandler;
    
    [Header("Walls To Run")] 
    [SerializeField] private LayerMask _wallMask;

    [Header("Wall Run")] 
    [SerializeField] private float _wallRunGravity;
    [SerializeField] private float _wallRunTime;
    [SerializeField] private float _wallCheckDistance;
    [SerializeField] private float _wallRunSpeed;
    [SerializeField] private float _wallClimbSpeed;

    [Header("Wall Jump")] 
    [SerializeField] private float _wallJumpUpForce;
    [SerializeField] private float _wallJumpSideForce;
    [SerializeField] private float _WallExitTime;

    [Header("Wall Exit")] 
    [SerializeField] private float _wallExitUpForce;
    [SerializeField] private float _wallExitSideForce;
    
    private float _wallExitTimer;
    private float _wallRunTimer;

    private RaycastHit _wallLeftHit;
    private RaycastHit _wallRightHit;

    private bool _right;
    private bool _left;
    private bool _exitingWall;
    
    private Vector2 _movementInput;
    
    //Private Component Refs
    private Rigidbody _rgBody;
    private PlayerController _playerController;
    private CameraController _cameraController;
    
    private void Awake()
    {
        _rgBody = GetComponent<Rigidbody>();
        _playerController = GetComponent<PlayerController>();
        _cameraController = transform.root.GetComponentInChildren<CameraController>();
    }

    private void Start()
    {
        _inputHandler.MoveEvent += MovementInput;
        _inputHandler.JumpEvent += JumpInput;
    }

    private void FixedUpdate()
    {
        CheckForWall();
        CheckState();
        //check if I can wall run
    }

    private void WallJump(float upForce, float sideForce)
    {
        if (!_playerController.IsWallRunning) return;
        if(!_playerController.CanJump) return;
        if(_playerController.IsJumping) return;

        _exitingWall = true;
        
        Vector3 wallNormal = _right ? _wallRightHit.normal : _wallLeftHit.normal;

        Vector3 wallJumpForce = transform.up * upForce + wallNormal * sideForce;

        Vector3 currentVelocity = _rgBody.velocity;
        _rgBody.velocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);
        _rgBody.AddForce(wallJumpForce * _rgBody.mass, ForceMode.Impulse);
    }

    private void MovementInput(Vector2 input)
    {
        _movementInput = input;
    }
    
    private void JumpInput()
    {
        WallJump(_wallJumpUpForce, _wallJumpSideForce);   
    }
    
    private void CheckForWall()
    {
        _right = Physics.Raycast(transform.position, transform.right, out _wallRightHit, _wallCheckDistance, _wallMask);
        _left = Physics.Raycast(transform.position, -transform.right, out _wallLeftHit, _wallCheckDistance, _wallMask);
    }

    private void CheckState()
    {
        //Has hit wall and is in air
        if((_left || _right) && _movementInput.magnitude > 0 && !_playerController.IsGrounded && !_exitingWall)
        {
            if (!_playerController.IsWallRunning)
            {
                _playerController.IsWallRunning = true;
                _wallRunTimer = 0;
                _cameraController.ChangeFOV(CameraFOV.WALLRUNNING);
            }

            WallRun();
            
            _rgBody.AddForce(Vector3.down * _wallRunGravity * _rgBody.mass);
            
            if (_wallRunTimer < _wallRunTime)
            {
                _wallRunTimer += Time.fixedDeltaTime;
            }
            
            if (_wallRunTimer >= _wallRunTime && _playerController.IsWallRunning)
            {
                WallJump(_wallExitUpForce, _wallExitSideForce);
                _exitingWall = true;
                _wallRunTimer = 0;
            }
        }
        else
        {
            _playerController.IsWallRunning = false;
            _cameraController.ChangeTilt(CameraTilt.NEUTRAL);
        }

        if (_exitingWall)
        {
            _playerController.IsWallRunning = false;

            if (!_playerController.IsSprinting)
            {
                _cameraController.ChangeFOV(CameraFOV.NEUTRAL);
            }
            
            _cameraController.ChangeTilt(CameraTilt.NEUTRAL);
            
            _wallExitTimer += Time.fixedDeltaTime;
            
            if (_wallExitTimer > _WallExitTime)
            {
                _wallExitTimer = 0;
                _exitingWall = false;
            }
        }
    }
    
    private void WallRun()
    {
        Vector3 currentVelocity = _rgBody.velocity;
        _rgBody.velocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);

        Vector3 wallNormal;

        if (_right)
        {
            wallNormal = _wallRightHit.normal;
            _cameraController.ChangeTilt(CameraTilt.RIGHT);
        }
        else
        {
            wallNormal = _wallLeftHit.normal;
            _cameraController.ChangeTilt(CameraTilt.LEFT);
        }
        
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if ((transform.forward - wallForward).magnitude > (transform.forward - -wallForward).magnitude)
        {
            wallForward = -wallForward;
        }

        if ((_right && _movementInput.x > .7f) || (_left && _movementInput.x < -.7f))
        {
            //Upwards
            _rgBody.velocity = new Vector3(currentVelocity.x, _wallClimbSpeed, currentVelocity.z);
        }
        else if((_right && _movementInput.x < -.7f) || (_left && _movementInput.x > .7f))
        {
            //Downwards
            _rgBody.velocity = new Vector3(currentVelocity.x, -_wallClimbSpeed, currentVelocity.z);
        }
        
        //If Player isn't trying to get away from wall. (Jumps)
       
        _rgBody.AddForce(-wallNormal * (100 * _rgBody.mass) , ForceMode.Force);
        
        _playerController.ApplyForce(wallForward, _wallRunSpeed);
    }
}
