using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Weapon Recoil Data")]
public class RecoilData : ScriptableObject
{
    [Header("Recoil Rotation")]
    [SerializeField] private float _recoilX;
    [SerializeField] private float _recoilY;
    [SerializeField] private float _recoilZ;

    [SerializeField] private float _snappiness;
    [SerializeField] private float _returnSpeed;

    [Header("Recoil Position")] 
    [SerializeField] private float _backwardsRecoil;
    [SerializeField] private SpringData _recoilPositionSpring;

    public float RecoilX => _recoilX;
    public float RecoilY => _recoilY;
    public float RecoilZ => _recoilZ;
    public float Snappiness => _snappiness;
    public float ReturnSpeed => _returnSpeed;
    public float BackwardRecoil => _backwardsRecoil;
    public SpringData RecoilSpringData => _recoilPositionSpring;
}
