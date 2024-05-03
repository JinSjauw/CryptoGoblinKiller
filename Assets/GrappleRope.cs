using UnityEngine;

public class GrappleRope : MonoBehaviour
{
    [SerializeField] private int _ropeQuality;
    [SerializeField] private float _damper;
    [SerializeField] private float _strength;
    [SerializeField] private float _speed;
    [SerializeField] private float _waveCount;
    [SerializeField] private float _waveHeight;
    [SerializeField] private float _affectCurve;
    
    private LineRenderer _lineRenderer;
    private Vector3 _anchor;
    private SpringUtils.SpringMotionParams _spring;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region Public Functions

    public void DrawRopePositions()
    {
        
    }

    #endregion
}
