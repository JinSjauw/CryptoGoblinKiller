using UnityEngine;

[CreateAssetMenu (menuName = "Sway Data")]
public class WeaponSwayData : ScriptableObject
{
    [Header("Position Offset")] 
    public Vector3 _initialPosition;
    public Vector3 _initialRotation;
    
    [Header("Sway")] 
    public bool sway;
    public float step = 0.01f;
    public float maxStepDistance = 0.06f;

    [Header("Sway Rotation")] 
    public bool swayRotation;
    public float rotationStep = 4f;
    public float maxRotationStep = 5f;

    [Header("Bobbing")] 
    public bool bobOffset;
    public float speedModifier;
    public Vector3 travelLimit = Vector3.one * 0.025f;
    public Vector3 bobLimit = Vector3.one * 0.01f;

    [Header("Bob Rotation")] 
    public bool bobRotation;
    public Vector3 bobMultiplier;
}
