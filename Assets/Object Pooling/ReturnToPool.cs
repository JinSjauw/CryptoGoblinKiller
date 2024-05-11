using System.Collections;
using UnityEngine;

public class ReturnToPool : MonoBehaviour
{
    [SerializeField] private float lifeTime;
    
    private ObjectPool _objectPool;
    private void Awake()
    {
        if (_objectPool == null)
        {
            _objectPool = FindObjectOfType<ObjectPool>();
        }
    }
    
    private void Update()
    {
        //StartCoroutine(TimeLife());
    }

    private IEnumerator TimeLife()
    {
        yield return new WaitForSeconds(lifeTime);
        if (_objectPool != null)
        {
            gameObject.SetActive(false);
            _objectPool.ReturnGameObject(gameObject);
        }
    }
    
    public void SetPool(ObjectPool objectPool)
    {
        _objectPool = objectPool;
        StartCoroutine(TimeLife());
    }
}
