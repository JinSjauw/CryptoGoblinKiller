using System;
using System.Collections.Generic;
using System.Xml;
using Ami.BroAudio;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [SerializeField] private InputHandler _inputHandler;
    
    [Header("Event Channels")]
    [SerializeField] private PlayerEventChannel _playerEventChannel;
    [SerializeField] private WeaponEventChannel _weaponEventChannel;
    [SerializeField] private ObjectiveEventChannel _objectiveEventChannel;
    [SerializeField] private WaveEventChannel _waveEventChannel;
    
    [Header("UI Object Refs")]
    [SerializeField] private Image _healthBar;
    [SerializeField] private Image _shotgunIcon;
    [SerializeField] private Image _revolverIcon;
    [SerializeField] private TextMeshProUGUI _ammoCounter;
    [SerializeField] private Transform _objectiveUIContainer;
    [SerializeField] private Transform _loseScreen;
    [SerializeField] private Transform _winScreen;
    [SerializeField] private TextMeshProUGUI _waveCounter;
    [SerializeField] private TextMeshProUGUI _waveTimer;
        
    [Header("UI Prefabs")] 
    [SerializeField] private Transform _objectiveUIPrefab;
    
    private Dictionary<Guid, Image> _objectiveIcons;
    
    private int _maxHealth = 250;
    private int _maxAmmo = 6;
    
    private float _waveTime;
    
    private void Start()
    {
        _objectiveIcons = new Dictionary<Guid, Image>();
        
        //Subscribe all events here
        _playerEventChannel.PlayerSpawnEvent += PlayerSpawned;
        _playerEventChannel.ChangedHealthEvent += HealthChanged;

        _weaponEventChannel.WeaponChangeEvent += WeaponChanged;
        _weaponEventChannel.FireEvent += WeaponFire;
        _weaponEventChannel.ReloadStartEvent += WeaponReload;

        _objectiveEventChannel.PointInitEvent += InitPoint;
        _objectiveEventChannel.HealthChangedEvent += ObjectiveHealthChanged;

        _objectiveEventChannel.LoseEvent += OnLose;

        _waveEventChannel.NewWaveEvent += NewWaveSpawned;
        _waveEventChannel.WavesClearedEvent += OnWin;
    }

    private void Update()
    {
        if (_waveTime <= 0)
        {
            _waveTimer.text = "00:00";
            return;
        }

        _waveTime -= Time.deltaTime;

        float minutes = Mathf.FloorToInt(_waveTime / 60);
        float seconds = Mathf.FloorToInt(_waveTime % 60);

        _waveTimer.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void OnWin()
    {
        _winScreen.gameObject.SetActive(true);
    }
    
    private void NewWaveSpawned(int waveNumber, int maxWaves, float waveTime)
    {
        _waveCounter.text = "Wave: " + waveNumber + "/" + maxWaves;
        _waveTime = waveTime;
    }

    //Temp
    private void OnLose()
    {
        Time.timeScale = 0;
        BroAudio.Stop(BroAudioType.All);
        _loseScreen.gameObject.SetActive(true);
        _inputHandler.DisableGameplayInput();
    }

    private void InitPoint(Guid id)
    {
        Image objectiveIcon = Instantiate(_objectiveUIPrefab, _objectiveUIContainer).GetChild(0).GetComponent<Image>();
        objectiveIcon.fillAmount = 1;
        
        _objectiveIcons.Add(id, objectiveIcon);
    }
    
    private void ObjectiveHealthChanged(float health, float maxHealth, Guid id)
    {
        _objectiveIcons[id].fillAmount = health / maxHealth;
    }

    private void WeaponReload(int maxAmmo)
    {
        _ammoCounter.text = maxAmmo + "/" + maxAmmo;
        _maxAmmo = maxAmmo;
    }

    private void WeaponFire(int ammo)
    {
        _ammoCounter.text = ammo + "/" + _maxAmmo;
    }

    private void WeaponChanged(WeaponType type)
    {
        if (type == WeaponType.SHOTGUN)
        {
            _shotgunIcon.gameObject.SetActive(true);
            _revolverIcon.gameObject.SetActive(false);
        }
        else
        {
            _shotgunIcon.gameObject.SetActive(false);
            _revolverIcon.gameObject.SetActive(true);
        }
    }

    private void PlayerSpawned(int maxHealth)
    {
        _maxHealth = maxHealth;
    }

    private void HealthChanged(float health)
    {
        _healthBar.fillAmount = health / _maxHealth;

        if (health <= 0)
        {
            OnLose();
        }
    }
}
