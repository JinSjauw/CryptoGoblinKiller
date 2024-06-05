using System;
using Ami.BroAudio;
using UnityEngine;

public class PlayerSoundController : MonoBehaviour
{
    [Header("Object Refs")]
    [SerializeField] private PlayerEventChannel _playerEventChannel;
    
    [Header("Footsteps")]
    [SerializeField] private SoundID _walkingStep = default;
    [SerializeField] private SoundID _runningStep = default;
    [SerializeField] private float _walkTime;
    [SerializeField] private float _runningTime;

    [Header("Jump & Land")] 
    [SerializeField] private SoundID _jump;
    [SerializeField] private SoundID _land;
    
    private PlayerController _playerController;

    private float _stepTimer;
    private float _stepDelay;
    private SoundID _stepAudio = default;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
    }

    private void Start()
    {
        _playerEventChannel.PlayerJumpEvent += PlayJumpSound;
        _playerEventChannel.PlayerLandEvent += PlayLandSound;
    }

    private void PlayLandSound()
    {
        BroAudio.Play(_land, transform.position);
    }

    private void PlayJumpSound()
    {
        BroAudio.Play(_jump, transform.position);
    }

    private void Update()
    {
        if (_playerController.GetMoveInput().magnitude > 0 && (_playerController.IsGrounded || _playerController.IsWallRunning))
        {
            if (_playerController.IsSprinting || _playerController.IsWallRunning)
            {
                _stepAudio = _runningStep;
                _stepDelay = _runningTime;
            }
            else
            {
                _stepAudio = _walkingStep;
                _stepDelay = _walkTime;
            }
            
            _stepTimer += Time.deltaTime;
            if (_stepTimer > _stepDelay)
            {
                _stepTimer = 0;
                BroAudio.Play(_stepAudio, transform.position);
            }
        }
        else
        {
            _stepTimer = _walkTime;
        }
    }
}
