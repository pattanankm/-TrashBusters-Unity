using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StageSelector : MonoBehaviour
{
    [System.Serializable]
    public class StageButtonUI
    {
        public string stageSceneName; // ชื่อฉาก (Scene) ของด่านนี้ที่จะให้โหลด
        public Button stageButton;    // ตัวปุ่มคอมโพเนนต์
        public GameObject lockIcon;   // รูปกุญแจล็อกที่สร้างไว้บังปุ่ม
        public Image[] stars;         // อาร์เรย์เก็บรูปดาว 3 ดวง (ลาก Star1, Star2, Star3 มาใส่)
    }

    [Header("สไปรท์รูปดาว")]
    public Sprite activeStarSprite;  // ลากรูปดาวสีเหลือง (ที่ได้แล้ว) มาใส่ตรงนี้
    public Sprite inactiveStarSprite;// ลากรูปดาวสีเทา (ที่ยังไม่ได้) มาใส่ตรงนี้

    [Header("ตั้งค่าปุ่มทั้ง 5 ด่าน")]
    public StageButtonUI[] stageElements = new StageButtonUI[5];

    private void Start()
    {
        RefreshStageSelectionUI();
    }

    // ฟังก์ชันตรวจสอบและอัปเดตสถานะปุ่มด่านทั้งหมด
    public void RefreshStageSelectionUI()
    {
        for (int i = 0; i < stageElements.Length; i++)
        {
            // ด่านแรก (Index 0) จะเปิดให้เล่นเสมอเป็นค่าเริ่มต้น
            bool isUnlocked = (i == 0);

            // ถ้าเป็นด่าน 2, 3, 4, 5 ให้ไปเช็คเงื่อนไขจาก PlayerPrefs ของด่านก่อนหน้า
            if (i > 0)
            {
                string previousStageUnlockedKey = "Stage_" + (i - 1) + "_Unlocked";
                // ดึงค่ามาเช็ค ถ้าเท่ากับ 1 แสดงว่าด่านก่อนหน้าได้ 3 ดาวและสั่งปลดล็อกไว้แล้ว
                isUnlocked = PlayerPrefs.GetInt(previousStageUnlockedKey, 0) == 1;
            }

            // 1. ควบคุมการเปิด/ปิดปุ่มและการแสดงกุญแจล็อก
            if (stageElements[i].stageButton != null)
            {
                stageElements[i].stageButton.interactable = isUnlocked;
            }
            if (stageElements[i].lockIcon != null)
            {
                stageElements[i].lockIcon.SetActive(!isUnlocked); // ถ้าปลดล็อกแล้ว ให้ปิดรูปกุญแจ
            }

            // 2. ดึงจำนวนดาวที่เคยทำได้ในด่านนี้มาแสดง
            string starKey = "Stage_" + i + "_Stars";
            int starsEarned = PlayerPrefs.GetInt(starKey, 0); // ถ้าไม่เคยเล่นจะได้ 0 ดาว

            // วนลูปเพื่อเปลี่ยนสี/เปลี่ยนรูปสไปรท์ดาวทั้ง 3 ดวงตามแต้มที่ได้
            for (int starIdx = 0; starIdx < stageElements[i].stars.Length; starIdx++)
            {
                if (stageElements[i].stars[starIdx] != null)
                {
                    if (starIdx < starsEarned)
                    {
                        // ถ้าดาวดวงนั้นน้อยกว่าแต้มที่ได้ -> เปลี่ยนเป็นดาวสีเหลือง
                        stageElements[i].stars[starIdx].sprite = activeStarSprite;
                    }
                    else
                    {
                        // ถ้ายังไม่ได้แต้มดวงนี้ -> เป็นดาวสีเทา
                        stageElements[i].stars[starIdx].sprite = inactiveStarSprite;
                    }
                }
            }
        }
    }

    // ฟังก์ชันสแตนดาร์ดสำหรับกดเปลี่ยนฉาก (จะไปผูกกับ OnClick ของแต่ละปุ่ม)
    public void LoadStageScene(int stageIndex)
    {
        string sceneName = stageElements[stageIndex].stageSceneName;
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("ยังไม่ได้ใส่ชื่อ Scene ของด่านที่ " + (stageIndex + 1));
        }
    }
    
    // ปุ่มโกง (Cheat Button) ไว้สำหรับทดสอบระบบ เผื่อขี้เกียจเล่นให้ได้ 3 ดาวเองตอนเทส
    [ContextMenu("Reset All Progress")]
    public void ResetAllProgress()
    {
        PlayerPrefs.DeleteAll();
        RefreshStageSelectionUI();
        Debug.Log("ล้างข้อมูลการเล่นทั้งหมดเรียบร้อยแล้ว!");
    }
}