using System;
using UnityEngine;
using UnityEngine.AI;

public class TestEnemy : MonoBehaviour
{
    [SerializeField] private Transform _playerTarget;

    private NavMeshAgent _agent;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _agent.SetDestination(_playerTarget.transform.position);
    }
}
