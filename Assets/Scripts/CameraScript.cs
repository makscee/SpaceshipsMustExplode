using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public static CameraScript Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Ship.Instance.MainModule == null) return;
        transform.position = new Vector3(Ship.Instance.MainModule.transform.position.x,
            Ship.Instance.MainModule.transform.position.y, transform.position.z);
    }
}