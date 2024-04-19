using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera _playerCamera;

    private void Awake()
    {
        _playerCamera = GetComponentInChildren<Camera>();
    }

    public void RotateCamera(float horizontal, float vertical)
    {
        Vector3 rotationVector = transform.right * vertical + transform.up * horizontal;

        _playerCamera.transform.localEulerAngles = rotationVector;
    }
}
