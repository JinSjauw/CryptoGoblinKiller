using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Event Channels/Player Event Channel")]
public class PlayerEventChannel : ScriptableObject
{
    public UnityAction<float> ChangedHealthEvent;
    public UnityAction<int> ChangedAmmoEvent;
    public UnityAction DeathEvent;
    public UnityAction<float> ReloadStartEvent;
    public UnityAction ReloadEndEvent;
    public UnityAction<int> PlayerSpawnEvent;

    public UnityAction PlayerJumpEvent;
    public UnityAction PlayerLandEvent;

    public UnityAction PlayerWallRunStartEvent;
    public UnityAction PlayerWallRunStopEvent;

    public UnityAction<float, float> HealthRechargeStartEvent;
    public UnityAction HealthRechargeStopEvent;
    public UnityAction<float> TakeDamageEvent;

    public void OnHealthChanged(float health)
    {
        ChangedHealthEvent?.Invoke(health);
    }

    public void OnAmmoChanged(int ammo)
    {
        ChangedAmmoEvent?.Invoke(ammo);
    }

    public void OnDeath()
    {
        DeathEvent?.Invoke();
    }

    public void OnReloadStart(float reloadTime)
    {
        ReloadStartEvent?.Invoke(reloadTime);
    }

    public void OnReloadEnd()
    {
        ReloadEndEvent?.Invoke();
    }

    public void OnPlayerSpawn(int maxHealth)
    {
        PlayerSpawnEvent?.Invoke(maxHealth);
    }

    public void OnPlayerJump()
    {
        PlayerJumpEvent?.Invoke();
    }

    public void OnPlayerLand()
    {
        PlayerLandEvent?.Invoke();
    }

    public void OnPlayerWallRunStart()
    {
        PlayerWallRunStartEvent?.Invoke();
    }

    public void OnPlayerWallRunStop()
    {
        PlayerWallRunStopEvent?.Invoke();
    }
    
    public void OnHealthRechargeStart(float health, float healingRate)
    {
        HealthRechargeStartEvent?.Invoke(health, healingRate);
    }

    public void OnHealthRechargeStop()
    {
        HealthRechargeStopEvent?.Invoke();
    }

    public void OnTakeDamage(float damage)
    {
        TakeDamageEvent?.Invoke(damage);
    }
}
