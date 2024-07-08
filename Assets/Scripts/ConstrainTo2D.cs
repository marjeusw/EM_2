using UnityEngine;

public class ConstrainTo2D : MonoBehaviour
{
    void LateUpdate()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
    }
}