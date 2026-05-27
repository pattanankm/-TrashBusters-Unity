using UnityEngine;
using UnityEngine.InputSystem;

public class SortingBin : MonoBehaviour
{
    [Header("Bin Filter Settings")]
    public string acceptedTag; 

    private void OnTriggerStay2D(Collider2D other)
    {
        // LOG 1: เช็คว่ามีวัตถุอะไรมาซ้อนในเขตถังขยะไหม
        Debug.Log($"[SortingBin] มีวัตถุชื่อ {other.name} กำลังยืนซ้อนอยู่ในถัง {gameObject.name}");

        if (other.TryGetComponent<PlayerMovement>(out PlayerMovement player))
        {
            // LOG 2: เจอตัวละครแล้ว! เช็คสถานะการอุ้ม
            Debug.Log($"[SortingBin] เจอตัวละคร {other.name} แล้ว! สถานะ IsCarrying = {player.IsCarrying}");

            if (player.IsCarrying)
            {
                // LOG 3: เช็คว่าผู้เล่นกดปุ่ม S หรือยัง
                if (Keyboard.current != null)
                {
                    bool sPressed = Keyboard.current.sKey.isPressed;
                    bool downPressed = Keyboard.current.downArrowKey.isPressed;
                    
                    Debug.Log($"[SortingBin] ผู้เล่นกำลังอุ้มขยะ! สถานะปุ่มกด: S={sPressed}, DownArrow={downPressed}");

                    if (sPressed || downPressed)
                    {
                        Debug.Log("[SortingBin] <color=yellow>ตรวจพบการกดปุ่มทิ้งขยะ! กำลังเรียก ReleaseTrash()...</color>");
                        
                        GameObject trash = player.ReleaseTrash();

                        if (trash != null)
                        {
                            Debug.Log($"[SortingBin] ได้รับวัตถุขยะชื่อ {trash.name} มาประมวลผล");
                            EvaluateTrash(trash);
                        }
                        else
                        {
                            Debug.LogWarning("[SortingBin] ReleaseTrash() ส่งค่ากลับมาเป็น null! (ผู้เล่นปล่อยขยะไม่สำเร็จ)");
                        }
                    }
                }
            }
        }
    }

    private void EvaluateTrash(GameObject trash)
    {
        if (trash.CompareTag(acceptedTag))
        {
            int scoreGain = 10;
            if (trash.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
            {
                scoreGain = Mathf.RoundToInt(rb.mass * 10f);
            }

        //     if (GameManager.Instance != null) GameManager.Instance.AddScore(scoreGain);
        //     Debug.Log("<color=green>[SortingBin] ผลลัพธ์: ทิ้งถูกถัง!</color>");
        // }
        // else
        // {
        //     if (GameManager.Instance != null) GameManager.Instance.AddScore(-5);
        //     Debug.Log($"<color=red>[SortingBin] ผลลัพธ์: ทิ้งผิดถัง! ถังนี้รับ {acceptedTag} แต่ขยะคือ {trash.tag}</color>");
        // }

        if (ObjectPooling.Instance != null) ObjectPooling.Instance.ReturnToPool(trash);
    }
}
}