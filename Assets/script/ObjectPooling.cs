using System.Collections.Generic;
using UnityEngine;

public class ObjectPooling : MonoBehaviour
{
    public static ObjectPooling Instance;

    [System.Serializable]
    public class Pool
    {
        public string tag;          
        public GameObject prefab;   
        public int size;            
    }

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    private void Awake()
    {
        // บังคับให้ Singleton ตั้งค่าตัวเองตั้งแต่เสี้ยววินาทีแรกที่เปิดเกม
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // ย้ายการสร้างคลังขยะขึ้นมาทำใน Awake() ทันที เพื่อการันตีว่าคลังสร้างเสร็จก่อนที่ Spawner จะเริ่มรัน
        InitializePools();
    }

    private void InitializePools()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            if (pool.prefab == null)
            {
                Debug.LogError($"<color=red>ข้อผิดพลาด:</color> ลืมลาก Prefab ใส่ในช่องของแท็ก {pool.tag} หรือเปล่าครับ?");
                continue;
            }

            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab, transform);
                obj.SetActive(false); 
                objectPool.Enqueue(obj);
            }

            // ตรวจสอบเพื่อป้องกันการใส่แท็กซ้ำซ้อนในตาราง Inspector
            if (!poolDictionary.ContainsKey(pool.tag))
            {
                poolDictionary.Add(pool.tag, objectPool);
            }
            else
            {
                Debug.LogWarning($"มีชื่อแท็กซ้ำกันในระบบ: {pool.tag}");
            }
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        // ตรวจสอบความปลอดภัย: หากระบบยัง Gen คลังไม่เสร็จ หรือไม่มีคีย์ชื่อนี้อยู่จริง
        if (poolDictionary == null || !poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("ยังไม่มีคลังชื่อแท็ก: " + tag + " ลงทะเบียนไว้ในตารางระบบ");
            return null;
        }

        if (poolDictionary[tag].Count == 0)
        {
            Debug.LogWarning("ขยะในคลังแท็ก " + tag + " หมดเกลี้ยงชั่วคราว! ลองเพิ่มค่า Size ใน Inspector ครับ");
            return null;
        }

        GameObject objectToSpawn = poolDictionary[tag].Dequeue();

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        if (objectToSpawn.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        poolDictionary[tag].Enqueue(objectToSpawn);
        return objectToSpawn;
    }

    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(transform); 
    }
}