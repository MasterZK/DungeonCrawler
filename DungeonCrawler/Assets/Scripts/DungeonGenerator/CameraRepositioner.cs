using UnityEngine;

public class CameraRepositioner : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Camera.main.transform.SetPositionAndRotation(this.transform.parent.parent.position + new Vector3(0,0,-10),Quaternion.identity);
    }
}
