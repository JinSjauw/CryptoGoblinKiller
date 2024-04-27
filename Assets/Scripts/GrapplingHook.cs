using Unity.VisualScripting;
using UnityEngine;

public class GrapplingHook : MonoBehaviour
{

    [Header("Object Refs")] 
    [SerializeField] private InputHandler _inputHandler;
    [SerializeField] private Transform _grappleTransform;
    
    [Header("Swinging Time Variables")]
    [SerializeField] private float _maxSwingTime;
    [SerializeField] private float _maxSwingDistance;
    [SerializeField] private float _ropeCutoffTime;
    
    [Header("Swinging Physics Variables")]
    [SerializeField] private float _swingingGravity;
    [SerializeField] private float _sideThrustForce;
    [SerializeField] private float _ropePullForwardForce;
    [SerializeField] private float _ropePullUpForce;
    
    [Header("Rope Physics Properties")]
    [SerializeField] private float _ropeSpring;
    [SerializeField] private float _ropeDamper;
    [SerializeField] private float _ropeMass;
    
    [Header("Rope Animation")]
    [SerializeField] private AnimationCurve _ropeVelocityCurve;
    [SerializeField] private float _ropeTravelTime;

    //Private Component Refs
    private PlayerController _playerController;
    private CameraController _cameraController;
    private LineRenderer _lineRenderer;
    private Rigidbody _rgBody;
    
    //Stored layer(s) for grappling
    private LayerMask _grappleLayer;
    
    private Vector3 _swingAnchorPoint;
    private SpringJoint _swingJoint;

    private Vector3 _currentGrapplePosition;
    private float _currentSwingDistance;
    
    private Vector2 _moveInput;
    
    private float _ropeAlpha;
    private float _ropeCutoffTimer;
    private float _swingingTimer;

    #region Unity Functions
    
    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _cameraController = transform.root.GetComponentInChildren<CameraController>();
        _lineRenderer = _grappleTransform.GetComponent<LineRenderer>();
        _rgBody = GetComponent<Rigidbody>();
        
        _grappleLayer = LayerMask.GetMask("Wall");
    }

    // Start is called before the first frame update
    private void Start()
    {
        _inputHandler.MoveEvent += ListenMoveEvent;
        _inputHandler.GrappleEvent += ListenGrappleEvent;
    }

    private void FixedUpdate()
    {
        //Debug.DrawRay(_grappleTransform.position, _swingAnchorPoint - _grappleTransform.position, Color.green, 2);
        
        if (!_playerController.IsSwinging) return;

        _swingingTimer += Time.fixedDeltaTime;
        
        if (_swingingTimer >= _maxSwingTime)
        {
            StopSwing();
        }
        
        _rgBody.AddForce(Vector3.down * (_swingingGravity * _rgBody.mass));
        
        AirMovement();
        ActuateHook();
        
        if (Physics.Raycast(_grappleTransform.position, _swingAnchorPoint - _grappleTransform.position, _currentSwingDistance * 0.95f,
                _grappleLayer))
        {
            _ropeCutoffTimer += Time.fixedDeltaTime;
            //Set timer to stop the swing
            if (_ropeCutoffTimer >= _ropeCutoffTime)
            {
                StopSwing();
            }
            //Debug.Log("Line broken: " + hit.collider.name);
            //StopSwing(); 
        }
    }

    private void LateUpdate()
    {
        DrawRope();
    }
    #endregion

    #region Input Callbacks
    
    private void ListenMoveEvent(Vector2 moveInput)
    {
        _moveInput = moveInput;
    }
    
    private void ListenGrappleEvent(bool state)
    {
        if (state) StartSwing();
        if (!state) StopSwing();
    }
    
    #endregion

    #region Private Functions
    private void StartSwing()
    {
        RaycastHit hit;
        //Have a separate object that would contain the actual grappling hook firing point & spring component;
        
        if (Physics.Raycast(_cameraController.transform.position, _cameraController.CameraForward(), out hit,
                _maxSwingDistance, _grappleLayer))
        {
            _swingAnchorPoint = hit.point;
            _swingJoint = _grappleTransform.AddComponent<SpringJoint>();
            _swingJoint.autoConfigureConnectedAnchor = false;
            _swingJoint.connectedAnchor = _swingAnchorPoint;

            float distanceFromAnchor = Vector3.Distance(_grappleTransform.position, _swingAnchorPoint);
            
            //Configure Rope
            _swingJoint.maxDistance = distanceFromAnchor * .85f;
            _swingJoint.minDistance = distanceFromAnchor * .2f;

            _swingJoint.spring = _ropeSpring;
            _swingJoint.damper = _ropeDamper;
            _swingJoint.massScale = _ropeMass;
            
            _lineRenderer.positionCount = 2;
            
            _playerController.IsSwinging = true;
        }
    }
    
    private void AirMovement()
    {
        float mass = _rgBody.mass;
        float sideForce = _sideThrustForce * mass;
        
        if (_moveInput.x >= 1) _rgBody.AddForce(transform.right * sideForce);
        if (_moveInput.x <= -1) _rgBody.AddForce(-transform.right * sideForce);
    }

    private void ActuateHook()
    {
        Vector3 directionToPoint = _swingAnchorPoint - _grappleTransform.position;
        _rgBody.AddForce(directionToPoint.normalized * (_ropePullForwardForce * _rgBody.mass));
        _rgBody.AddForce(transform.up * (_ropePullUpForce * _rgBody.mass));
        
        _currentSwingDistance = Vector3.Distance(_grappleTransform.position, _swingAnchorPoint);
        
        _swingJoint.maxDistance = _currentSwingDistance * 0.8f;
        _swingJoint.minDistance = _currentSwingDistance * 0.15f;
    }
    
    private void StopSwing()
    {
        _lineRenderer.positionCount = 0; 
        _ropeAlpha = 0;
        _playerController.IsSwinging = false;
        _swingingTimer = 0; 
        _ropeCutoffTimer = 0;
        Destroy(_swingJoint);
    }
    
    private void DrawRope()
    {
        if(!_swingJoint) return;
        
        _ropeAlpha = Mathf.MoveTowards(_ropeAlpha, 1, 1 / _ropeTravelTime * Time.deltaTime);
        _currentGrapplePosition = Vector3.Lerp(_grappleTransform.position, _swingAnchorPoint, _ropeVelocityCurve.Evaluate(_ropeAlpha));
        
        _lineRenderer.SetPosition(0, _grappleTransform.position);
        _lineRenderer.SetPosition(1, _currentGrapplePosition);
    }
    
    #endregion
}
