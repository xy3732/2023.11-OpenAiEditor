using UnityEngine;

public class player : MonoBehaviour
{
    private Rigidbody _rb;
    public float jumpForce = 10f;
    public float moveSpeed = 5f;
    [SerializeField]private Coins coinsComponent;
    private PlayerManager playerManagerComponent;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();

        GameObject playerManagerObject = GameObject.Find("PlayerManager");

        if (playerManagerObject != null)
        {
            playerManagerComponent = playerManagerObject.GetComponent<PlayerManager>();
        }
        else
        {
            Debug.Log("Cannot find PlayerManager object");
        }
    }

    private void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        transform.Translate(new Vector3(horizontalInput, 0, verticalInput) * moveSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Jump"))
        {
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        Coins coin = other.gameObject.GetComponent<Coins>();
        if (coin != null)
        {
            coinsComponent.totalCoins += 10;
            playerManagerComponent.moneyAdd(coinsComponent.totalCoins);
            Destroy(other.gameObject);
        }
    }
}