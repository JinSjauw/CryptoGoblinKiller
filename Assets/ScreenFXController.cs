using System;
using UnityEngine;
using UnityEngine.Serialization;

public class ScreenFXController : MonoBehaviour
{
    [Header("Object Refs")]
    [SerializeField] private PlayerEventChannel _playerEventChannel;

    [Header("Material Refs")] 
    [SerializeField] private Material _hitFXMaterial;
    [SerializeField] private Material _rechargeFXMaterial;
    
    [Header("FX Decay rate")] 
    [SerializeField] private float _hitFXTimeToDecay;
    [SerializeField] private AnimationCurve _hitFXDecayCurve;
    [SerializeField] private float _rechargeFXTimeToDecay;
    [SerializeField] private AnimationCurve _rechargeFXDecayCurve;
    [SerializeField] private float _rechargeFXTimeToStart;
    [SerializeField] private AnimationCurve _rechargeFXStartCurve;
    
    private int _vignetteIntensity = Shader.PropertyToID("_VignetteIntensity");

    private float _hitFXMaxValue;
    private float _rechargeFXMaxValue;

    private float _hitFXAlpha;
    private float _rechargeFXAlpha;

    private bool _stopRechargeFX;
    private bool _startRechargeFX;
    
    // Start is called before the first frame update
    void Start()
    {
        _playerEventChannel.TakeDamageEvent += ShowHitFX;
        _playerEventChannel.HealthRechargeStartEvent += ShowRechargeFX;
        _playerEventChannel.HealthRechargeStopEvent += HideRechargeFX;

        _hitFXMaxValue = _hitFXMaterial.GetFloat(_vignetteIntensity);
        _rechargeFXMaxValue = _rechargeFXMaterial.GetFloat(_vignetteIntensity);
        
        _hitFXMaterial.SetFloat(_vignetteIntensity, 0);
        _rechargeFXMaterial.SetFloat(_vignetteIntensity, 0);
    }

    private void Update()
    {
        if (_hitFXAlpha > 0)
        {
            //Lerp to 0!
            _hitFXAlpha = Mathf.MoveTowards(_hitFXAlpha, 0, Time.deltaTime / _hitFXTimeToDecay);
            _hitFXMaterial.SetFloat(_vignetteIntensity, Mathf.Lerp(0, _hitFXMaxValue, _hitFXDecayCurve.Evaluate(_hitFXAlpha)));
        }

        if (_stopRechargeFX)
        {
            _rechargeFXAlpha = Mathf.MoveTowards(_rechargeFXAlpha, 0, Time.deltaTime / _rechargeFXTimeToDecay);
            _rechargeFXMaterial.SetFloat(_vignetteIntensity, Mathf.Lerp(0, _rechargeFXMaxValue, _rechargeFXDecayCurve.Evaluate(_rechargeFXAlpha)));
            //Lerp to 0!
        }
        else if (_startRechargeFX)
        {
            _rechargeFXAlpha = Mathf.MoveTowards(_rechargeFXAlpha, 1, Time.deltaTime / _rechargeFXTimeToStart);
            _rechargeFXMaterial.SetFloat(_vignetteIntensity, Mathf.Lerp(0, _rechargeFXMaxValue, _rechargeFXStartCurve.Evaluate(_rechargeFXAlpha)));
        }
    }

    private void OnDisable()
    {
        _hitFXMaterial.SetFloat(_vignetteIntensity, _hitFXMaxValue);
        _rechargeFXMaterial.SetFloat(_vignetteIntensity, _rechargeFXMaxValue);
    }

    private void ShowHitFX(float health)
    {
        _hitFXAlpha = 1;
        _hitFXMaterial.SetFloat(_vignetteIntensity, _hitFXMaxValue);
    }

    private void ShowRechargeFX(float health, float healingRate)
    {
        _stopRechargeFX = false;
        _startRechargeFX = true;
    }
    
    private void HideRechargeFX()
    {
        _stopRechargeFX = true;
        _startRechargeFX = false; 
    }
}
