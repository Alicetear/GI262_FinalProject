using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    private Camera mainCam;

    [Header("ตั้งค่าองศาการเล็ง")]
    public float rotationOffset = 180f; // 💡 ปรับเป็น 180 เพื่อแก้ปัญหาปืนหันกลับด้านซ้าย/ขวา

    private void Start()
    {
        mainCam = Camera.main;
    }

    private void Update()
    {
        Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 lookDir = mousePos - transform.position;

        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;

        // 💡 บังคับให้มุมอยู่เฉพาะซีกซ้ายของจอ (ระหว่าง 90 ถึง 180 และ -180 ถึง -90 องศา)
        // ถ้าเมาส์ลากไปฝั่งขวา ให้ล็อกมุมไว้ที่ขอบซ้ายสุดแทน
        if (angle > -90 && angle < 90)
        {
            // ถ้าเมาส์อยู่ขวา ให้หันไปทางซ้ายสุด (เลือกมุม 90 หรือ 180 ตามต้องการ)
            angle = 90f;
        }

        transform.rotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);
    }

}