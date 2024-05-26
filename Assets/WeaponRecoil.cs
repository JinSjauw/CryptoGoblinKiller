using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class WeaponRecoil : MonoBehaviour
{
    [SerializeField] private Transform _weaponHolder;
    
    [SerializeField] private float _recoilX;
    [SerializeField] private float _recoilY;
    [SerializeField] private float _recoilZ;

    [SerializeField] private float _snappiness;
    [SerializeField] private float _returnSpeed;

    private Quaternion _startRotation;
    private Vector3 _currentRotation;
    private Vector3 _targetRotation;

    //Will try out with 2 springs;

    private void Awake()
    {
        _startRotation = _weaponHolder.localRotation;
    }

    private void Update()
    {
        _targetRotation = Vector3.Lerp(_targetRotation, _startRotation.eulerAngles, _returnSpeed * Time.deltaTime);
        _currentRotation = Vector3.Slerp(_currentRotation, _targetRotation, _snappiness * Time.deltaTime);
        _weaponHolder.transform.localRotation = Quaternion.Euler(_currentRotation);
    }

    public void PlayRecoil()
    {
        _targetRotation += new Vector3(_recoilX, Random.Range(-_recoilY, _recoilY), Random.Range(-_recoilZ, _recoilZ));
    }
}
