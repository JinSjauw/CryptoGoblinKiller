
using UnityEngine;
using Random = UnityEngine.Random;

public class WeaponRecoil : MonoBehaviour
{
    [SerializeField] private Transform _weaponHolder;
    
    [Header("Recoil Rotation")]
    [SerializeField] private float _recoilX;
    [SerializeField] private float _recoilY;
    [SerializeField] private float _recoilZ;

    [SerializeField] private float _snappiness;
    [SerializeField] private float _returnSpeed;

    [Header("Recoil Position")] 
    [SerializeField] private float _backwardsRecoil;
    [SerializeField] private SpringData _recoilPositionSpring;
    
    private Quaternion _startRotation;
    private Vector3 _currentRotation;
    private Vector3 _targetRotation;
    
    //Spring
    private Vector3 _startPosition;
    private Vector3 _backDirection;
    
    private SpringUtils.SpringMotionParams _springMotionParams;
    private float _recoilCurrent;
    private float _recoilCurrentDelta;

    //Will try out with 2 springs;

    private void Awake()
    {
        _springMotionParams = new SpringUtils.SpringMotionParams();
        
        _startPosition = _weaponHolder.localPosition;
        _backDirection = _weaponHolder.TransformDirection(Vector3.forward);
        
        _startRotation = _weaponHolder.localRotation;
        //_targetRotation = _startRotation.eulerAngles;
    }

    private void Update()
    {
        _targetRotation = Vector3.Lerp(_targetRotation, _startRotation.eulerAngles, _returnSpeed * Time.deltaTime);
        _currentRotation = Vector3.Slerp(_currentRotation, _targetRotation, _snappiness * Time.deltaTime);
        _weaponHolder.transform.localRotation = Quaternion.Euler(_currentRotation);
        
        CalculateSpring(ref _recoilCurrent, ref _recoilCurrentDelta, 0, _recoilPositionSpring.frequency, _recoilPositionSpring.dampingRatio);
        HandlePositionValue(_recoilCurrent);
    }
    
    private void HandlePositionValue(float value)
    {
        _weaponHolder.localPosition = _startPosition + _backDirection * -value;
    }

    private void CalculateSpring(ref float position, ref float velocity, float target, float frequency, float dampingRatio)
    {
        SpringUtils.CalcDampedSpringMotionParams(_springMotionParams, Time.deltaTime, frequency, dampingRatio);
        SpringUtils.UpdateDampedSpringMotion(ref position, ref velocity, target, _springMotionParams);
    }
    
    public void PlayRecoil()
    {
        _targetRotation += new Vector3(_recoilX, Random.Range(-_recoilY, _recoilY), Random.Range(-_recoilZ, _recoilZ));
        
        _recoilCurrent = _backwardsRecoil;
        HandlePositionValue(_recoilCurrent);
    }

    public void ChangeWeapon(Transform weaponTransform, RecoilData recoilData)
    {
        _weaponHolder = weaponTransform;
        _startPosition = _weaponHolder.localPosition;
        _startRotation = Quaternion.identity;

        _recoilX = recoilData.RecoilX;
        _recoilY = recoilData.RecoilY;
        _recoilZ = recoilData.RecoilZ;

        _snappiness = recoilData.Snappiness;
        _returnSpeed = recoilData.ReturnSpeed;

        _backwardsRecoil = recoilData.BackwardRecoil;
        _recoilPositionSpring = recoilData.RecoilSpringData;
    }
}
