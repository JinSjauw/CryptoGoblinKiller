using System;
using UnityEngine;

public class WallRunning : MonoBehaviour
{
    [Header("Walls To Run")] 
    [SerializeField] private LayerMask _wallMask;
    
    [Header("Wall Run")]
    [SerializeField] private float _wallRunForce;
    [SerializeField] private float _wallRunTime;
    [SerializeField] private float _wallCheckDistance;
    
    private float _wallRunTimer;

    private RaycastHit _wallLeftHit;
    private RaycastHit _wallRightHit;

    private bool _right;
    private bool _left;

    private Vector2 _movementInput;
    
    //Private Component Refs
    private Rigidbody _rgBody;
    private PlayerController _playerController;

    private void Awake()
    {
        _rgBody = GetComponent<Rigidbody>();
        _playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        _movementInput = _playerController.GetMovementInput();
        CheckForWall();
        CheckWallRun();
        //check if I can wall run
    }

    private void CheckForWall()
    {
        _right = Physics.Raycast(transform.position, transform.right, out _wallRightHit, _wallCheckDistance, _wallMask);
        _left = Physics.Raycast(transform.position, -transform.right, out _wallLeftHit, _wallCheckDistance, _wallMask);
    }

    private void CheckWallRun()
    {
        if((_left || _right) && _movementInput.magnitude > 0 && !_playerController.IsGrounded())
        {
            _playerController.SetWallRunning(true);
            WallRun();
        }
        else
        {
            _playerController.SetWallRunning(false);
        }
    }
    
    private void WallRun()
    {
        Vector3 currentVelocity = _rgBody.velocity;
        _rgBody.velocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);
        
        Vector3 wallNormal = _right ? _wallRightHit.normal : _wallLeftHit.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);
        
        //Apply forces along the wall to wall run;
        _rgBody.AddForce(wallForward * _wallRunForce, ForceMode.Force);
    }
}
