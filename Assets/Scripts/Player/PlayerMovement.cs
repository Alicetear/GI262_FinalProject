using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Stats")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Movement Bounds Behind the Barricade")]
    [SerializeField] private float minX = 6f;  // Left boundary.
    [SerializeField] private float maxX = 9f;  // Right boundary.
    [SerializeField] private float minY = -4f; // Lower boundary.
    [SerializeField] private float maxY = 4f;  // Upper boundary.

    private Rigidbody2D rb;
    private Vector2 movement;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Read horizontal and vertical movement input.
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
    }

    private void FixedUpdate()
    {
        // Calculate the next position.
        Vector2 newPos = rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime;

        // Keep the player inside the area behind the barricade.
        newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
        newPos.y = Mathf.Clamp(newPos.y, minY, maxY);

        // Move the rigidbody.
        rb.MovePosition(newPos);
    }
}