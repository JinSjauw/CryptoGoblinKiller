using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Event Channels/Wave Event Channel")]
public class WaveEventChannel : ScriptableObject
{
    public UnityAction<int, int, float> NewWaveEvent;
    public UnityAction WavesClearedEvent;
    
    public void OnNewWave(int waveNumber, int maxWaves, float waveTime)
    {
        NewWaveEvent?.Invoke(waveNumber, maxWaves, waveTime);
    }

    public void OnWavesCleared()
    {
        WavesClearedEvent?.Invoke();
    }
}
