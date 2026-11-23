using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    private Camera mainCam;

    [Header("Aim Angle")]
    public float rotationOffset = 180f; // Use a 180-degree offset for sprites facing the opposite direction.

    private void Start()
    {
        mainCam = Camera.main;
    }

    private void Update()
    {
        Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 lookDir = mousePos - transform.position;

        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;

        // Limit aim to the left half of the screen (90 to 180 or -180 to -90 degrees).
        // Clamp the aim when the cursor moves to the right.
        if (angle > -90 && angle < 90)
        {
            // Use the configured left-facing angle when the cursor is on the right.
            angle = 90f;
        }

        transform.rotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);
    }

}