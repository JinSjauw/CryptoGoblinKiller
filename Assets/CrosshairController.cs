using Obscure.SDC;
using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    [SerializeField] private Crosshair _innerCrosshair;
    [SerializeField] private Crosshair _outerCrosshair;
    // Start is called before the first frame update
    void Start()
    {
        _innerCrosshair.SetSize(Vector2.one * 30, 2f);
        _outerCrosshair.SetSize(Vector2.one * 60, 6f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
