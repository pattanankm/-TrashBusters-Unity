using UnityEngine;

public class TrashSpawner : MonoBehaviour
{
    public string[] trashTags = { "E-Waste", "Plastics", "Organics" };
    public float spawnInterval = 1.5f;
    public float spawnWidth = 8f; // ขอบเขตความกว้างหน้าจอที่จะสุ่มปล่อยขยะ

    private float timer;

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnTrash();
            timer = 0f;
        }
    }

    void SpawnTrash()
    {
        // สุ่มตำแหน่ง X และสุ่มประเภทขยะ
        float randomX = Random.Range(-spawnWidth, spawnWidth);
        Vector3 spawnPos = new Vector3(randomX, transform.position.y, 0);
        string randomTag = trashTags[Random.Range(0, trashTags.Length)];

        // เรียกใช้ผ่าน Object Pooling แทนการ Instantiate
        ObjectPooling.Instance.SpawnFromPool(randomTag, spawnPos, Quaternion.identity);
    }
}