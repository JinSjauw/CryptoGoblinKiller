using System;
using System.Collections.Generic;
using System.Xml;
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

    [Header("UI Object Refs")]
    [SerializeField] private Image _healthBar;
    [SerializeField] private Image _shotgunIcon;
    [SerializeField] private Image _revolverIcon;
    [SerializeField] private TextMeshProUGUI ammoCounter;
    [SerializeField] private Transform _objectiveUIContainer;
    [SerializeField] private Transform _loseScreen;
    
    [Header("UI Prefabs")] 
    [SerializeField] private Transform _objectiveUIPrefab;
    
    private Dictionary<Guid, Image> _objectiveIcons;
    
    private int _maxHealth = 250;
    private int _maxAmmo = 6;
    
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
        ammoCounter.text = maxAmmo + "/" + maxAmmo;
        _maxAmmo = maxAmmo;
    }

    private void WeaponFire(int ammo)
    {
        ammoCounter.text = ammo + "/" + _maxAmmo;
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
