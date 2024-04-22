using System;
using Unity.VisualScripting;
using UnityEngine;

public class GrapplingHook : MonoBehaviour
{
    [SerializeField] private InputHandler _inputHandler;
    
    [Header("Grapple - Swinging")] 
    [SerializeField] private float _maxSwingDistance;
    //Physics
    [SerializeField] private float _ropeSpring;
    [SerializeField] private float _ropeDamper;
    [SerializeField] private float _ropeMass;

    [SerializeField] private AnimationCurve _ropeVelocityCurve;
    [SerializeField] private float _ropeTravelTime;

    private PlayerController _playerController;
    private CameraController _cameraController;
    private LineRenderer _lineRenderer;
    
    private LayerMask _grappleLayer;

    private Vector3 _swingAnchorPoint;
    private SpringJoint _swingJoint;

    private Vector3 _currentGrapplePosition;
    private float _ropeAlpha;
    
    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _cameraController = transform.root.GetComponentInChildren<CameraController>();
        _lineRenderer = GetComponent<LineRenderer>();
        _grappleLayer = LayerMask.GetMask("Wall");
    }

    // Start is called before the first frame update
    void Start()
    {
        _inputHandler.GrappleEvent += ListenGrappleEvent;
    }

    
    // Update is called once per frame
    void Update()
    {
        
    }

    private void LateUpdate()
    {
        DrawRope();
    }

    private void ListenGrappleEvent(bool state)
    {
        if (state) StartSwing();
        if (!state) StopSwing();
    }
    
    private void StartSwing()
    {
        RaycastHit hit;
        if (Physics.Raycast(_cameraController.transform.position, _cameraController.CameraForward(), out hit,
                _maxSwingDistance, _grappleLayer))
        {
            _swingAnchorPoint = hit.point;
            _swingJoint = transform.AddComponent<SpringJoint>();
            _swingJoint.autoConfigureConnectedAnchor = false;
            _swingJoint.connectedAnchor = _swingAnchorPoint;

            float distanceFromAnchor = Vector3.Distance(transform.position, _swingAnchorPoint);
            
            //Configure Rope
            //_swingJoint.connectedBody = GetComponent<Rigidbody>();
            _swingJoint.maxDistance = distanceFromAnchor * .85f;
            _swingJoint.minDistance = distanceFromAnchor * .2f;

            _swingJoint.spring = _ropeSpring;
            _swingJoint.damper = _ropeDamper;
            _swingJoint.massScale = _ropeMass;
            
            _lineRenderer.positionCount = 2;
            _playerController.IsSwinging = true;
        }
    }
    
    private void StopSwing()
    {
        _lineRenderer.positionCount = 0;
        _ropeAlpha = 0;
       Destroy(_swingJoint);
       _playerController.IsSwinging = false;
    }

    private void DrawRope()
    {
        if(!_swingJoint) return;
        
        _ropeAlpha = Mathf.MoveTowards(_ropeAlpha, 1, 1 / _ropeTravelTime * Time.deltaTime);
        _currentGrapplePosition = Vector3.Lerp(transform.position, _swingAnchorPoint, _ropeAlpha);
        
        _lineRenderer.SetPosition(0, transform.position);
        _lineRenderer.SetPosition(1, _currentGrapplePosition);
    }
    
}
