using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GunIconController : MonoBehaviour
{
    [SerializeField] private Transform _bulletIconPrefab;
    [SerializeField] private Image _gunFill;
    [SerializeField] private GridLayoutGroup _bulletIconGrid;
    
    [Header("Image Width(pixels)")] 
    [SerializeField] private float _widthWeaponIcon;
    
    private List<Transform> _bullets;
    private int _bulletAmount;

    private float _reloadTime;
    private float _reloadAlpha;
    private bool _isReloading;
    
    private void Update()
    {
        if (_isReloading)
        {
            _reloadAlpha = Mathf.MoveTowards(_reloadAlpha, 1, Time.deltaTime / _reloadTime);
            _gunFill.fillAmount = Mathf.Lerp(0, 1, _reloadAlpha);
        }

        if (_isReloading && _reloadAlpha >= 1)
        {
            Reload();
        }
    }

    private void Reload()
    {
        _isReloading = false;
        _reloadAlpha = 0;
        _gunFill.fillAmount = 0;
        
        float cellWidth =  _widthWeaponIcon / _bulletAmount - _bulletIconGrid.spacing.x; 
        
        _bulletIconGrid.cellSize = new Vector2(cellWidth, _bulletIconGrid.cellSize.y);
        
        for (int i = 0; i < _bulletAmount; i++)
        {
            _bullets.Add(Instantiate(_bulletIconPrefab, _bulletIconGrid.transform));
        }
    }

    public void Initialize(int bulletsToReload)
    {
        _bullets = new List<Transform>();
        if (_bullets.Count > 0)
        {
            foreach (Transform bullet in _bullets)
            {
                Destroy(bullet.gameObject);
            }
            
            _bullets.Clear();
        }
        
        _bulletAmount = bulletsToReload;
        Reload();
    }
    
    public void StartReload(int bulletsToReload, float reloadTime)
    {
        _isReloading = true;
        _reloadAlpha = 0;
        _reloadTime = reloadTime;
        _bulletAmount = bulletsToReload;
       
        if (_bullets.Count > 0)
        {
            foreach (Transform bullet in _bullets)
            {
                Destroy(bullet.gameObject);
            }
            
            _bullets.Clear();
        }
    }

    public void Shoot()
    {
        //Delete one bullet;
        if (_bullets.Count > 0)
        {
            Transform bullet = _bullets.First();
            _bullets.Remove(bullet);
            Destroy(bullet.gameObject);
        }
    }
}
