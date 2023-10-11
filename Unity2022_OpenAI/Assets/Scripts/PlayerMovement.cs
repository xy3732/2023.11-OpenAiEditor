using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 10f;
    private Rigidbody rb;
    private bool isGrounded = true;
    private int collectedCoins = 0;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        float jumpInput = Input.GetAxis("Jump");

        Vector3 moveDirection = new Vector3(horizontalInput, 0f, verticalInput).normalized;

        transform.Translate(moveDirection * speed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded || jumpInput > 0f && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
        else if (collision.gameObject.CompareTag("Coin"))
        {
            Coins coin = collision.gameObject.GetComponent<Coins>();
            if (coin != null)
            {
                collectedCoins += coin.totalCoins;
                GetComponent<PlayerManager>().moneyAdd(collectedCoins);
                Destroy(collision.gameObject);
            }
        }
    }
}