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
    
    [Header("Health")]
    [SerializeField] private Image _healthFill;
    [SerializeField] private Image _healthBackgroundFill;
    [SerializeField] private Image _healthBackground;
    [SerializeField] private Color _healingColour;
    [SerializeField] private Color _neutralColour;
    
    [Header("Guns")]
    [SerializeField] private GunIconController _shotgunIconController;
    [SerializeField] private GunIconController _revolverIconController;
    
    [Header("Objectives")]
    [SerializeField] private Transform _objectiveUIContainer;
    [SerializeField] private Transform _loseScreen;
    [SerializeField] private Transform _winScreen;
    [SerializeField] private TextMeshProUGUI _waveCounter;
    [SerializeField] private TextMeshProUGUI _waveTimer;
    
    [Header("UI Prefabs")] 
    [SerializeField] private Transform _objectiveUIPrefab;
    
    private Dictionary<Guid, Image> _objectiveIcons;
    private GunIconController _currentGunIconController;
    
    private int _maxHealth = 250;
    private float _healthAlpha = 1;
    private float _healthBackgroundAlpha = 1;
    private float _healthChangeRate = .1f;
    private bool _isRecharging;
    private bool _takenDamage;
    
    private int _maxAmmo = 6;
    
    private float _waveTime;
    
    private void Start()
    {
        _objectiveIcons = new Dictionary<Guid, Image>();
        
        //Subscribe all events here
        _playerEventChannel.PlayerSpawnEvent += PlayerSpawned;
        _playerEventChannel.ChangedHealthEvent += HealthChanged;
        _playerEventChannel.HealthRechargeStartEvent += HealthRecharging;
        _playerEventChannel.HealthRechargeStopEvent += HealthStopRecharging;
        
        _weaponEventChannel.WeaponChangeEvent += WeaponChanged;
        _weaponEventChannel.FireEvent += WeaponFire;
        _weaponEventChannel.ReloadStartEvent += WeaponReload;

        _objectiveEventChannel.PointInitEvent += InitPoint;
        _objectiveEventChannel.HealthChangedEvent += ObjectiveHealthChanged;
        
        _objectiveEventChannel.LoseEvent += OnLose;

        _waveEventChannel.NewWaveEvent += NewWaveSpawned;
        _waveEventChannel.WavesClearedEvent += OnWin;
        
        _shotgunIconController.Initialize(_maxAmmo);
        _revolverIconController.Initialize(_maxAmmo);
    }

    private void Update()
    {
        if (_takenDamage)
        {
            _healthBackgroundAlpha = Mathf.MoveTowards(_healthBackgroundAlpha, 0, _healthChangeRate * Time.deltaTime);
            _healthBackgroundFill.fillAmount = Mathf.Lerp(0, 1, _healthBackgroundAlpha);
        }
        else if (_healthAlpha >= _healthBackgroundAlpha && _takenDamage)
        {
            _healthBackgroundAlpha = _healthAlpha;
            _healthBackgroundFill.fillAmount = Mathf.Lerp(0, 1, _healthBackgroundAlpha);
            _takenDamage = false;
        }
        
        if (_isRecharging)
        {
            _healthAlpha = Mathf.MoveTowards(_healthAlpha, 1, _healthChangeRate * Time.deltaTime);
            //_healthBackgroundAlpha = _healthAlpha;
            _healthFill.fillAmount = Mathf.Lerp(0, 1, _healthAlpha);
           _healthBackground.color = _healingColour;
        }
        
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

    #region Event Callbacks

    private void HealthStopRecharging()
    {
        _isRecharging = false;
        _healthBackground.color = _neutralColour;
    }

    private void HealthRecharging(float health, float healingRate)
    {
        _healthChangeRate = healingRate;
        _isRecharging = true;
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

    private void WeaponReload(int maxAmmo, float reloadTime)
    {
        _maxAmmo = maxAmmo;
        _currentGunIconController.StartReload(maxAmmo, reloadTime);
    }

    private void WeaponFire(int ammo)
    {
        _currentGunIconController.Shoot();
    }

    private void WeaponChanged(WeaponType type)
    {
        if (type == WeaponType.SHOTGUN)
        {
            _shotgunIconController.gameObject.SetActive(true);
            _revolverIconController.gameObject.SetActive(false);
            _currentGunIconController = _shotgunIconController;
        }
        else
        {
            _shotgunIconController.gameObject.SetActive(false);
            _revolverIconController.gameObject.SetActive(true);
            _currentGunIconController = _revolverIconController;
        }
    }

    private void PlayerSpawned(int maxHealth)
    {
        _maxHealth = maxHealth;
    }

    private void HealthChanged(float health)
    {
        _takenDamage = true;
        _healthBackgroundAlpha = _healthAlpha;
        _healthAlpha = health / _maxHealth;
        
        _healthFill.fillAmount = _healthAlpha;
        _healthBackgroundFill.fillAmount = _healthBackgroundAlpha;
        
        if (health <= 0)
        {
            OnLose();
        }
    }

    #endregion
}
