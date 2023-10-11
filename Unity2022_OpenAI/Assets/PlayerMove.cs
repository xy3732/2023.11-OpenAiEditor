using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float speed = 10f;

    private void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        transform.Translate(direction * speed * Time.deltaTime);
    }
}