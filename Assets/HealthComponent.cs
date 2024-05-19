using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    [SerializeField] private float _health;

    public float Health => _health;
    
    public void TakeDamage(float damage)
    {
        _health -= damage;

        if (_health <= 0)
        {
            Debug.Log("Death for: " + transform.name);
        }
    }
    
}
