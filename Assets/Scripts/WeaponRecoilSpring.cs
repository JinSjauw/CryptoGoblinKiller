using System;
using UnityEngine;

public class WeaponRecoilSpring : MonoBehaviour
{
    [SerializeField] private Transform _weaponHolder;

    [Header("Recoil Position")] 
    [SerializeField] private float _backwardsRecoil;
    [SerializeField] private SpringData _recoilTargetSpring;
    [SerializeField] private SpringData _recoilPositionSpring;
    
    private Vector3 _startPosition;
    private Vector3 _backDirection;
    
    private SpringUtils.SpringMotionParams _springMotionParams;
    private float _recoilCurrent;
    private float _recoilCurrentDelta;
    
    private void Awake()
    {
        _springMotionParams = new SpringUtils.SpringMotionParams();
        _startPosition = _weaponHolder.localPosition;
        _backDirection = _weaponHolder.TransformDirection(Vector3.right);
    }

    // Update is called once per frame
    void Update()
    {
        CalculateSpring(ref _recoilCurrent, ref _recoilCurrentDelta, 0, _recoilPositionSpring.frequency, _recoilPositionSpring.dampingRatio);
        HandleSpringValue(_recoilCurrent);
    }

    private void HandleSpringValue(float value)
    {
        _weaponHolder.localPosition = _startPosition + _backDirection * value;
    }
    
    private void CalculateSpring(ref float position, ref float velocity, float target, float frequency, float dampingRatio)
    {
        SpringUtils.CalcDampedSpringMotionParams(_springMotionParams, Time.deltaTime, frequency, dampingRatio);
        SpringUtils.UpdateDampedSpringMotion(ref position, ref velocity, target, _springMotionParams);
    }

    public void PlayRecoil()
    {
        _recoilCurrent = _backwardsRecoil;
        HandleSpringValue(_recoilCurrent);
    }
}
