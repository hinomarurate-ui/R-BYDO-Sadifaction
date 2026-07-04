using UnityEngine;

[DefaultExecutionOrder(50000)]
public class ParallaxBG : MonoBehaviour
{
    public float parallax = 0.5f;

    Vector3 startPosition;
    float startCameraX;
    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position;
        startCameraX = Camera.main.transform.position.x;
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        float cameraMove = Camera.main.transform.position.x - startCameraX;
        transform.position = startPosition + Vector3.right * cameraMove * parallax;
    }
}
