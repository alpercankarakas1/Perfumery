using UnityEngine;
using System.Collections;

public class ExtractorStation : MonoBehaviour
{
    [Header("Girdi ve Çýktý")]
    public Item inputItem;                // Örn: Apple.asset
    public GameObject outputPrefab;       // Örn: AppleExtractObject.prefab

    [Header("Ayarlar")]
    public float processTime = 3f;
    public Transform outputSpawnPoint;    // Çýktýnýn doðacaðý yer

    private void OnTriggerEnter(Collider other)
    {
        ItemObject itemObject = other.GetComponent<ItemObject>();
        if (itemObject != null && itemObject.itemData == inputItem)
        {
            StartCoroutine(ProcessItem(itemObject));
        }
    }

    private IEnumerator ProcessItem(ItemObject input)
    {
        Debug.Log("Ýþlem baþladý: " + inputItem.itemName);
        Destroy(input.gameObject); // Elmayý sahneden sil
        yield return new WaitForSeconds(processTime);
        Instantiate(outputPrefab, outputSpawnPoint.position, Quaternion.identity);
        Debug.Log("Çýktý üretildi: " + outputPrefab.name);
    }
}
