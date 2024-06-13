using System;
using System.Collections.Generic;
using Ami.BroAudio;
using TMPro;
using UnityEngine;
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
    [SerializeField] private CrosshairController _crosshairController;
    [SerializeField] private Vector2 _shotgunCrossInnerSize;
    [SerializeField] private Vector2 _shotgunCrossOuterSize;
    
    [SerializeField] private Vector2 _revolverCrossInnerSize;
    [SerializeField] private Vector2 _revolverCrossOuterSize;

    [Header("Objectives")]
    //[SerializeField] private Transform _objectiveUIContainer;
    [SerializeField] private Transform _loseScreen;
    [SerializeField] private Transform _winScreen;
    [SerializeField] private TextMeshProUGUI _waveCounter;
    [SerializeField] private TextMeshProUGUI _waveTimer;

    [Header("Objective Icons")] 
    [SerializeField] private Image _torusFill;
    [SerializeField] private Image _pyramidFill;
    [SerializeField] private Image _cubeFill;
    [SerializeField] private Image _xFill;

    //[SerializeField] private Transform _objectiveUIPrefab;
    
    private Dictionary<Shape, Image> _objectiveIcons;
    private Dictionary<Shape, float> _objectiveIconFillAlphas;

    
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
        _objectiveIcons = new Dictionary<Shape, Image>();
        _objectiveIconFillAlphas = new Dictionary<Shape, float>();
        
        _objectiveIcons.Add(Shape.TORUS, _torusFill);
        _objectiveIcons.Add(Shape.PYRAMID, _pyramidFill);
        _objectiveIcons.Add(Shape.CUBE, _cubeFill);
        _objectiveIcons.Add(Shape.CROSS, _xFill);
        
        _objectiveIconFillAlphas.Add(Shape.TORUS, 0);
        _objectiveIconFillAlphas.Add(Shape.PYRAMID, 0);
        _objectiveIconFillAlphas.Add(Shape.CUBE, 0);
        _objectiveIconFillAlphas.Add(Shape.CROSS, 0);
        
        //Subscribe all events here
        _playerEventChannel.PlayerSpawnEvent += PlayerSpawned;
        _playerEventChannel.ChangedHealthEvent += HealthChanged;
        _playerEventChannel.HealthRechargeStartEvent += HealthRecharging;
        _playerEventChannel.HealthRechargeStopEvent += HealthStopRecharging;
        
        _weaponEventChannel.WeaponChangeEvent += WeaponChanged;
        _weaponEventChannel.FireEvent += WeaponFire;
        _weaponEventChannel.ReloadStartEvent += WeaponReload;
        
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

        foreach (Shape shape in _objectiveIcons.Keys)
        {
            float fillAlpha = _objectiveIconFillAlphas[shape];
            
            if(fillAlpha <= 0) continue;
            
            Image iconFill = _objectiveIcons[shape];
            
            fillAlpha = Mathf.MoveTowards(fillAlpha, 0, Time.deltaTime / 1);
            iconFill.color = Color.Lerp(Color.white, Color.red, fillAlpha);

            _objectiveIconFillAlphas[shape] = fillAlpha;
            //iconFill.color = Color.Lerp()
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
        _playerEventChannel.Unsubscribe();
        _weaponEventChannel.Unsubscribe();
        _waveEventChannel.Unsubscribe();
        _objectiveEventChannel.Unsubscribe();
        
        //Time.timeScale = 0;
        BroAudio.Stop(BroAudioType.All);
        _inputHandler.DisableGameplayInput();
        _winScreen.gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }
    
    private void NewWaveSpawned(int waveNumber, int maxWaves, float waveTime)
    {
        _waveCounter.text = waveNumber + "/" + maxWaves;
        _waveTime = waveTime;
    }

    //Temp
    private void OnLose()
    {
        //Time.timeScale = 0;
        BroAudio.Stop(BroAudioType.All);
        _loseScreen.gameObject.SetActive(true);
        _inputHandler.DisableGameplayInput();

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }
    
    private void ObjectiveHealthChanged(float health, float maxHealth, Shape id)
    {
        _objectiveIconFillAlphas[id] = 1;
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
            _crosshairController.ChangeCrosshair(_shotgunCrossInnerSize, _shotgunCrossOuterSize);
        }
        else
        {
            _shotgunIconController.gameObject.SetActive(false);
            _revolverIconController.gameObject.SetActive(true);
            _currentGunIconController = _revolverIconController;
            _crosshairController.ChangeCrosshair(_revolverCrossInnerSize, _revolverCrossOuterSize);
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
