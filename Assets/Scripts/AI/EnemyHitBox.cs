using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class EnemyHitBox : MonoBehaviour
{
    [SerializeField] private bool _resetHead;
    [SerializeField] private HitBoxType _type;
    [SerializeField] private VisualEffect _hitFX;

    [SerializeField] private SpringData _headSpringData;
    [SerializeField] private float _targetHeadScale;
    private SpringUtils.SpringMotionParams _springMotionParams;

    private bool _animateHead;
    private float _headScale;
    private float _headScaleDelta;

    private HealthComponent _healthComponent;
    
    private void Awake()
    {
        _headScale = 1;
        _springMotionParams = new SpringUtils.SpringMotionParams();

        _healthComponent = GetComponentInParent<HealthComponent>();
    }

    private void Update()
    {
        if (_animateHead)
        {
            //Do head animation here
            //Head Swells up and then implodes.
            SpringUtils.CalcDampedSpringMotionParams(_springMotionParams, Time.deltaTime, _headSpringData.frequency, _headSpringData.dampingRatio);
            SpringUtils.UpdateDampedSpringMotion(ref _headScale, ref _headScaleDelta, _targetHeadScale, _springMotionParams);

            if (_headScale <= _targetHeadScale)
            {
                _animateHead = false;
                
                if(_resetHead) StartCoroutine(ResetHead(1.5f));
            }
            
            transform.localScale *= _headScale;
        }
    }

    private IEnumerator ResetHead(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        transform.localScale = Vector3.one;
        _headScale = 1;
        Debug.Log("Reset Head");
    }
    
    public void Hit(float damage)
    {
        _healthComponent.TakeDamage(damage, _type);
        
        //_hitFX.Play();
        if (_type == HitBoxType.HEAD)
        {
            if (!_animateHead)
            {
                _headScale += .25f;
                _animateHead = true;
            }
        }
        else
        {
            //_hitFX.Play();
        }
    }
    
}

public enum HitBoxType
{
    DEFAULT = 0,
    CHEST = 1,
    HEAD = 2,
    LEG = 3,
}
