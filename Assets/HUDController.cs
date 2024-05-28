using System;
using UnityEngine;

public class HUDController : MonoBehaviour
{
    [Header("Event Channels")] 
    [SerializeField] private PlayerEventChannel _playerEventChannel;

    [Header("Field Refs")] 
    [SerializeField] private Transform healthBar;

    private void Start()
    {
        //Subscribe all events here
    }
}
