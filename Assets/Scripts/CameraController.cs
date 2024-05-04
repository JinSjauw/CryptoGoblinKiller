using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Component Refs")] 
    [SerializeField] private Transform _cameraHolder;
    
    [Header("Camera FOV")] 
    [SerializeField] private float _normalFOV;
    [SerializeField] private float _sprintFOV;
    [SerializeField] private float _wallRunFOV;
    
    [Header("FOV Spring Variables")] 
    [SerializeField] private float _fovSpringFrequency;
    [SerializeField] private float _fovDampingRatio;
    
    /*[Header("Tilt Spring Variables")] 
    [SerializeField] private float _tiltSpringFrequency;
    [SerializeField] private float _tiltDampRatio;*/
    
    private Camera _playerCamera;
    
    //Camera FOV
    private SpringUtils.SpringMotionParams _springParamsFOV;
    private CameraFOV _fovState;
    private float _fovDelta;
    private float _lastFovDelta;
    
    private float _currentFOV;
    private float _targetFOV;
    
    //Camera Tilt
    private SpringUtils.SpringMotionParams _springParamsTilt;
    private float _tiltSpringFrequency;
    private float _tiltDampRatio;
    
    private CameraTilt _tiltState;
    private CameraTilt _lastTiltState;
    
    private float _tiltDelta;
    private float _lastTiltDelta;

    private float _currentTilt;
    private float _targetTilt;

    //Flags
    private bool _isChangingFOV;
    private bool _isChangingTilt;
    
    private void Awake()
    {
        _playerCamera = GetComponentInChildren<Camera>();
        _springParamsFOV = new SpringUtils.SpringMotionParams();
        _springParamsTilt = new SpringUtils.SpringMotionParams();

        _currentFOV = _playerCamera.fieldOfView;
    }

    private void Update()
    {
        HandleFOVChange();
        HandleTiltChange();
    }

    private void HandleFOVChange()
    {
        if (_isChangingFOV)
        {
            SpringUtils.CalcDampedSpringMotionParams(_springParamsFOV, Time.deltaTime, _fovSpringFrequency, _fovDampingRatio);
            SpringUtils.UpdateDampedSpringMotion(ref _currentFOV, ref _fovDelta, _targetFOV, _springParamsFOV);
            
            if (Mathf.Abs(_fovDelta - _lastFovDelta) <= 0)
            {
                _isChangingFOV = false;
                _currentFOV = _targetFOV;
            }
            
            UpdateFOV(_currentFOV);
            _lastFovDelta = _fovDelta;
        }
    }

    private void HandleTiltChange()
    {
        if (_isChangingTilt)
        {
            SpringUtils.CalcDampedSpringMotionParams(_springParamsTilt, Time.deltaTime, _tiltSpringFrequency, _tiltDampRatio);
            SpringUtils.UpdateDampedSpringMotion(ref _currentTilt, ref _tiltDelta, _targetTilt, _springParamsTilt);

            if (Mathf.Abs(_tiltDelta - _lastTiltDelta) <= 0)
            {
                _isChangingTilt = false;
                _currentTilt = _targetTilt;
            }

            UpdateTilt(_currentTilt);
            _lastTiltDelta = _tiltDelta;
        }
    }
    
    private void UpdateFOV(float value)
    {
        _playerCamera.fieldOfView = value;
    }

    private void UpdateTilt(float value)
    {
        //Debug.Log(value);
        _playerCamera.transform.localRotation = Quaternion.Euler(0, 0, value);
    }
    
    public void ChangeFOV(CameraFOV fovState)
    {
        //if(_isChangingFOV) return;
        
        if(_fovState == fovState) return;
        _fovState = fovState;
        _isChangingFOV = true;
        
        switch (fovState)
        {
            case CameraFOV.NEUTRAL:
                _targetFOV = _normalFOV;
                break;
            case CameraFOV.SPRINTING:
                _targetFOV = _sprintFOV;
                break;
            case CameraFOV.WALLRUNNING:
                _targetFOV = _wallRunFOV;
                break;
        }
    }

    public void ChangeTilt(CameraTilt tiltState, SpringData spring, float tilt = 0)
    {
        //if (_isChangingTilt) return;
        
        if(_tiltState == tiltState) return;
        _tiltState = tiltState;
        
        _isChangingTilt = true;
        
        switch (tiltState)
        {
            case CameraTilt.NEUTRAL:
                _targetTilt = 0;
                break;
            case CameraTilt.RIGHT:
                _targetTilt = tilt;
                break;
            case CameraTilt.LEFT:
                _targetTilt = -tilt;
                break;
        }

        _tiltSpringFrequency = spring.frequency;
        _tiltDampRatio = spring.dampingRatio;
    }
    
    public void RotateCamera(float horizontal, float vertical)
    {
        Vector3 rotationVector = transform.right * vertical + transform.up * horizontal;

        _cameraHolder.localEulerAngles = rotationVector;
    }

    public Vector3 CameraForward()
    {
        return _playerCamera.transform.forward;
    }
}

public enum CameraFOV
{
    NEUTRAL = 0,
    SPRINTING = 1,
    WALLRUNNING = 2,
}

public enum CameraTilt
{
    NEUTRAL = 0,
    RIGHT = 1,
    LEFT = 2,
}