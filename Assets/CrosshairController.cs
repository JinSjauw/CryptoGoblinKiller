using Obscure.SDC;
using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    [SerializeField] private Crosshair _innerCrosshair;
    [SerializeField] private Crosshair _outerCrosshair;
    [SerializeField] private float _changeTime;
    // Start is called before the first frame update
    void Start()
    {
        _innerCrosshair.SetSize(Vector2.one * 30, 1f);
        _outerCrosshair.SetSize(Vector2.one * 60, 1f);
    }

    public void ChangeCrosshair(Vector2 innerSize, Vector2 outerSize)
    {
        _innerCrosshair.SetSize(Vector2.one * innerSize, _changeTime);
        _outerCrosshair.SetSize(Vector2.one * outerSize, _changeTime);
    }
}
