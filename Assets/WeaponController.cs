using System;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Object Refs")]
    [SerializeField] private Transform _weaponHolder;
    [SerializeField] private SpringData _springData;
    
    //private Component Refs
    //private CameraController _cameraController;
    
    //Spring
    private SpringUtils.SpringMotionParams _springParams;

    private TiltState _tiltState;
    private bool _isChangingTilt;
    private float _targetTilt;
    private float _currentTilt;
    private float _weaponTiltDelta;
    private float _lastWeaponTiltDelta;
    
    #region Unity Functions

    private void Awake()
    {
        _springParams = new SpringUtils.SpringMotionParams();
    }

    private void Update()
    {
        TiltWeapon();
    }

    #endregion
    
    #region Private Functions

    private void TiltWeapon()
    {
        if (_isChangingTilt)
        {
            SpringUtils.CalcDampedSpringMotionParams(_springParams, Time.deltaTime, _springData.frequency, _springData.dampingRatio);
            SpringUtils.UpdateDampedSpringMotion(ref _currentTilt, ref _weaponTiltDelta, _targetTilt, _springParams);

            if (Mathf.Abs(_weaponTiltDelta - _lastWeaponTiltDelta) <= 0)
            {
                //_isChangingTilt = false;
                _currentTilt = _targetTilt;
            }

            //Debug.Log("tilt: " + _cameraTilt + " weapon tilt " + _currentTilt);
            
            _weaponHolder.localRotation = Quaternion.Euler(0, 0, _currentTilt);
            _lastWeaponTiltDelta = _weaponTiltDelta;
        }
    }

    #endregion
    
    #region Public Functions

    public void ChangeTilt(TiltState tiltState, SpringData spring, float tilt = 0)
    {
        if(_tiltState == tiltState) return;
        _tiltState = tiltState;
        
        _isChangingTilt = true;
        
        switch (tiltState)
        {
            case TiltState.NEUTRAL:
                _targetTilt = 0;
                break;
            case TiltState.RIGHT:
                _targetTilt = tilt;
                break;
            case TiltState.LEFT:
                _targetTilt = -tilt;
                break;
        }

        _springData = spring;
    }
    
    public void RotateWeaponHolder(float horizontal, float vertical)
    {
        Vector3 rotationVector = transform.right * vertical + transform.up * horizontal;

        _weaponHolder.localEulerAngles = new Vector3(rotationVector.x, rotationVector.y, _weaponHolder.localEulerAngles.z);
    }

    public void FollowAnchor(Vector3 position)
    {
        transform.position = position;
    }

    #endregion
}
