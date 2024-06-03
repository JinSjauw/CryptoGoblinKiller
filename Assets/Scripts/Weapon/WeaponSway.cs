using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    [Header("Object Refs")]
    [SerializeField] private PlayerController _playerController;

    [Header("Settings")] 
    [SerializeField] private WeaponSwayData _data;
    [SerializeField] private WeaponSwayData _moveSettings;
    [SerializeField] private WeaponSwayData _idleSettings;
    
    [Header("Sway")] 
    [SerializeField] private bool _sway;
    [SerializeField] private float _step = 0.01f;
    [SerializeField] private float _maxStepDistance = 0.06f;
    private Vector3 _swayPosition;

    [Header("Sway Rotation")] 
    [SerializeField] private bool _swayRotation;
    [SerializeField] private float _rotationStep = 4f;
    [SerializeField] private float _maxRotationStep = 5f;
    private Vector3 _swayEulerRotation;

    [Header("Bobbing")] 
    [SerializeField] private bool _bobOffset;
    [SerializeField] private float _speedModifier;
    [SerializeField] private Vector3 _travelLimit = Vector3.one * 0.025f;
    [SerializeField] private Vector3 _bobLimit = Vector3.one * 0.01f;

    [Header("Bob Rotation")] 
    [SerializeField] private bool _bobRotation;
    [SerializeField] private Vector3 _bobMultiplier;
    private Vector3 _bobEulerRotation;
    
    [Header("Smoothing")]
    [SerializeField] private float smooth = 10f;
    [SerializeField] private float smoothRot = 10f;
    
    private float _speedCurve;
    private float _curveSin { get => Mathf.Sin(_speedCurve); }
    private float _curveCos { get => Mathf.Cos(_speedCurve); }
    private Vector3 _bobPosition;
    
    private Vector3 _initialPosition;
    private Vector3 _initialRotation;

    private Vector2 _moveInput;
    private Vector2 _mouseInput;
    private Vector3 _velocity;
    
    #region Unity Functions

    private void Awake()
    {
        _initialPosition = transform.localPosition;
        _initialRotation = transform.eulerAngles;
        
        if (_idleSettings != null)
        {
            LoadData(_idleSettings);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        _moveInput = _playerController.GetMoveInput();
        _mouseInput = _playerController.GetMouseInput();
        _velocity = _playerController.GetCurrentVelocity() * _speedModifier;

        if (_moveInput == Vector2.zero && _data != _idleSettings)
        {
            _data = _idleSettings;
            LoadData(_data);
        }
        else if(_moveInput != Vector2.zero && _data != _moveSettings)
        {
            _data = _moveSettings;
            LoadData(_data);
        }
        
        Sway();
        BobOffset();
        SwayRotation();
        BobRotation();
        CompositePositionRotation();
    }

    #endregion

    #region Private Functions
    
    private void CompositePositionRotation()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, _swayPosition + _bobPosition, Time.deltaTime * smooth);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(_swayEulerRotation) * Quaternion.Euler(_bobEulerRotation), Time.deltaTime * smoothRot);
    }
    
    private void Sway()
    {
        if (!_sway) { _swayPosition = Vector3.zero; return; }

        Vector3 invertLook = _mouseInput * -_step;
        invertLook.x = Mathf.Clamp(invertLook.x, -_maxStepDistance, _maxStepDistance);
        invertLook.y = Mathf.Clamp(invertLook.y, -_maxStepDistance, _maxStepDistance);

        _swayPosition = invertLook + _initialPosition;
    }

    private void SwayRotation()
    {
        if(!_swayRotation) { _swayEulerRotation = Vector3.zero; return; }

        Vector2 invertLook = _mouseInput * _rotationStep;
        invertLook.x = Mathf.Clamp(invertLook.x, -_maxRotationStep, _maxRotationStep);
        invertLook.y = Mathf.Clamp(invertLook.y, -_maxRotationStep, _maxRotationStep);

        _swayEulerRotation = new Vector3(invertLook.y, invertLook.x, invertLook.x) + _initialRotation;
    }

    private void BobOffset()
    {
        _speedCurve += Time.deltaTime * (_playerController.IsGrounded ? _velocity.magnitude : 1 * _speedModifier) + Time.deltaTime;

        if (_speedCurve > 100) _speedCurve = 0;

        if (_bobOffset == false) { _bobPosition = Vector3.zero; return; }

        _bobPosition.x = (_curveCos * _bobLimit.x * (_playerController.IsGrounded ? 1 : 0)) -
                         (_moveInput.x * _travelLimit.x);
        _bobPosition.y = (_curveSin * _bobLimit.y) - (_velocity.y * _travelLimit.y);

        _bobPosition.z = -(_moveInput.y * _travelLimit.z);
    }

    private void BobRotation()
    {
        if (!_bobRotation) { _bobEulerRotation = Vector3.zero; return; }

        _bobEulerRotation.x = (_moveInput != Vector2.zero
            ? _bobMultiplier.x * (Mathf.Sin(2 * _speedCurve))
            : _bobMultiplier.x * Mathf.Sin(2 * _speedCurve) / 2);
        _bobEulerRotation.y = (_moveInput != Vector2.zero ? _bobMultiplier.y * _curveCos : 0);
        _bobEulerRotation.z = (_moveInput != Vector2.zero ? _bobMultiplier.z * _curveCos * _moveInput.x : 0);
    }

    private void LoadData(WeaponSwayData data)
    {
        _sway = data.sway;
        _step = data.step;
        _maxStepDistance = data.maxStepDistance;

        _swayRotation = data.swayRotation;
        _rotationStep = data.rotationStep;
        _maxRotationStep = data.maxRotationStep;

        _bobOffset = data.bobOffset;
        _speedModifier = data.speedModifier;
        _travelLimit = data.travelLimit;
        _bobLimit = data.bobLimit;

        _bobRotation = data.bobRotation;
        _bobMultiplier = data.bobMultiplier;
    }
    
    #endregion
    
}
