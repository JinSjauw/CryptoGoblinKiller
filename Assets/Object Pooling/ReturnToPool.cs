using System;
using System.Collections;
using UnityEngine;

public class ReturnToPool : MonoBehaviour
{
    [SerializeField] private float lifeTime;
    
    private ObjectPool _objectPool;
    private StickToObject _stickToObject;
    
    private Coroutine _coroutine;

    private void Awake()
    {
        if (_objectPool == null)
        {
            _objectPool = FindObjectOfType<ObjectPool>();
        }

        _stickToObject = GetComponent<StickToObject>();
    }

    private void OnDisable()
    {
        if(_coroutine == null) return;
        StopCoroutine(_coroutine);
    }

    private IEnumerator TimeLife()
    {
        yield return new WaitForSeconds(lifeTime);
        
        if (_objectPool != null)
        {
            //gameObject.SetActive(false);
            _stickToObject.UnStick();
            _objectPool.ReturnGameObject(gameObject);
        }
    }
    
    public void SetPool(ObjectPool objectPool)
    {
        _objectPool = objectPool;
        _coroutine = StartCoroutine(TimeLife());
    }
}
