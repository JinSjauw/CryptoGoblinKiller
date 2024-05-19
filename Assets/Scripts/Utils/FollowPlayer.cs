using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [SerializeField] private Transform _player;
    
    // Update is called once per frame
    void Update()
    {
        transform.position = _player.position;
    }
}
