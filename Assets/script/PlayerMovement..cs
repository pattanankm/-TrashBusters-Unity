using UnityEngine;
using UnityEngine.InputSystem; 

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("ความเร็วเดินตอนตัวเปล่า (แนวราบ ซ้าย-ขวา)")]
    public float emptyHandSpeed = 10f; 
    
    [Tooltip("แรงกระโดด ยิ่งเยอะยิ่งโดดสูง")]
    public float jumpForce = 9f;

    [Tooltip("ตัวหารน้ำหนักขยะ ยิ่งค่านี้น้อย ขยะจะยิ่งไม่ค่อยถ่วงมือ")]
    public float weightEffectFactor = 0.2f; 

    private float currentSpeed;
    private Rigidbody2D rb;
    private float horizontalInput;
    private bool jumpRequested = false;

    [Header("Ground Check (ระบบเช็คพื้น)")]
    [Tooltip("ใส่ Layer ที่ใช้นับว่าเป็นพื้น (เช่น Default หรือสร้าง Layer Ground)")]
    public LayerMask groundLayer;
    [Tooltip("จุดตรวจจับพื้นล่างเท้าตัวละคร (สร้าง Empty GameObject ไปแปะไว้ที่เท้าแล้วลากมาใส่)")]
    public Transform groundCheckPoint;
    [Tooltip("รัศมีวงกลมที่ใช้ตรวจจับพื้น")]
    public float groundCheckRadius = 0.2f;
    private bool isGrounded;

    [Header("Carrying System")]
    public Transform holdPoint; 
    private GameObject carriedObject;
    public bool IsCarrying => carriedObject != null;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentSpeed = emptyHandSpeed;
    }

    private void Update()
    {
        // 1. เช็คว่าตัวละครเหยียบพื้นอยู่หรือไม่ (ใช้ OverlapCircle ตรวจสอบตรงจุดเท้า)
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);

        // 2. ระบบอ่านค่าปุ่มกด (New Input System)
        horizontalInput = 0f;

        if (Keyboard.current != null)
        {
            // เดินซ้าย-ขวา (แกน X)
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontalInput = -1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontalInput = 1f;

            // เปลี่ยนจากเดินขึ้นฟ้า เป็นการ "กดกระโดด" (W หรือ ปุ่มลูกศรขึ้น)
            // เช็คเงื่อนไขว่าต้องอยู่บนพื้นเท่านั้นถึงจะกระโดดได้ (ป้องกันการโดดดับเบิ้ลจัมพ์กลางอากาศ)
            if ((Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame) && isGrounded)
            {
                jumpRequested = true;
            }
        }

        // 3. ปรับความเร็วตามสถานะการอุ้มขยะ (ส่งผลเฉพาะความเร็วในการเดินแกน X)
        if (IsCarrying && carriedObject.TryGetComponent<Rigidbody2D>(out Rigidbody2D trashRb))
        {
            currentSpeed = emptyHandSpeed / (1f + (trashRb.mass * weightEffectFactor));
        }
        else
        {
            currentSpeed = emptyHandSpeed;
        }
    }

    private void FixedUpdate()
    {
        // แก้ไข Logic การเคลื่อนที่ให้เป็นสไตล์ Platformer 2D
        // แกน X = ขยับตามปุ่มซ้ายขวาคูณความเร็ว
        // แกน Y = ปล่อยให้เป็นไปตามแรงโน้มถ่วงและแรงกระโดด (rb.linearVelocity.y) ไม่ไปล็อคค่ามันเป็น 0 แบบเดิม
        rb.linearVelocity = new Vector2(horizontalInput * currentSpeed, rb.linearVelocity.y);

        // ถ้าผู้เล่นกดกระโดดมาในเฟรมนั้น ๆ ให้ส่งแรงดันขึ้นแกน Y ในจังหวะฟิสิกส์
        if (jumpRequested)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpRequested = false; // เคลียร์คำสั่งกระโดด
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        bool isAnyTrash = collision.gameObject.CompareTag("E-Waste") || 
                          collision.gameObject.CompareTag("Plastics") || 
                          collision.gameObject.CompareTag("Organics");

        if (isAnyTrash && !IsCarrying)
        {
            PickUpTrash(collision.gameObject);
        }
    }

    private void PickUpTrash(GameObject trash)
    {
        carriedObject = trash;

        if (trash.TryGetComponent<Rigidbody2D>(out Rigidbody2D trashRb))
        {
            trashRb.bodyType = RigidbodyType2D.Kinematic;
            trashRb.linearVelocity = Vector2.zero;
        }

        if (trash.TryGetComponent<Collider2D>(out Collider2D trashCollider))
        {
            trashCollider.enabled = false;
        }

        trash.transform.position = holdPoint.position;
        trash.transform.SetParent(transform);
    }

    public GameObject ReleaseTrash()
    {
        if (!IsCarrying) return null;

        GameObject released = carriedObject;
        released.transform.SetParent(null);

        if (released.TryGetComponent<Rigidbody2D>(out Rigidbody2D trashRb)) 
        {
            trashRb.bodyType = RigidbodyType2D.Dynamic;
        }
        
        if (released.TryGetComponent<Collider2D>(out Collider2D trashCollider)) 
        {
            trashCollider.enabled = true;
        }

        carriedObject = null;
        return released;
    }

    // วาดวงกลมสีแดงโชว์ในหน้า Scene เพื่อให้เห็นตำแหน่งจุดเช็คพื้นใต้เท้าชัดเจน
    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }
}