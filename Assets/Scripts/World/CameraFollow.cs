using Cinemachine;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        CinemachineVirtualCamera camera = GetComponent<CinemachineVirtualCamera>();
        CinemachineFramingTransposer body = camera.GetCinemachineComponent<CinemachineFramingTransposer>();

        body.m_XDamping = 1f;
        body.m_YDamping = 1f;
        body.m_DeadZoneWidth =  0.1f;
        body.m_DeadZoneHeight = 0.5f;
        body.m_ScreenY = 0.8f;
        
    }
}
