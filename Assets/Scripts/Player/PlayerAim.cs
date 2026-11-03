using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    [Header("Aiming Settings")]
    [SerializeField] private Transform gunPivot;

    private void Update()
    {
        AimAtCursor();
    }

    private void AimAtCursor()
    {
        if (gunPivot == null) return; // กันโค้ดพังถ้าลืมใส่ช่อง

        // 1. ดึงพิกัดเมาส์ (บังคับระยะ Z ให้ชนพอดีกับกล้อง เพื่อแก้บั๊กมองไม่เห็น)
        Vector3 screenPos = Input.mousePosition;
        screenPos.z = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        worldPos.z = 0f;

        // 2. คำนวณองศา
        Vector3 aimDirection = worldPos - gunPivot.position;
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        // 3. หมุนปืน (บวก 180 องศา เพื่อชดเชยที่กระบอกปืนชี้ไปทางซ้าย)
        gunPivot.rotation = Quaternion.Euler(0f, 0f, angle + 180f);

        // 4. เรดาร์ตรวจจับ (ตัวแฉความจริง)
        Debug.Log($"[ระบบเล็ง] กำลังรัน! องศา: {angle + 180f} | เมาส์อยู่: {worldPos}");
    }
}