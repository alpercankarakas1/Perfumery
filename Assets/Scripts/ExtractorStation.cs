using UnityEngine;
using System.Collections;

public class ExtractorStation : MonoBehaviour
{
    [Header("Girdi ve ��kt�")]
    public Item inputItem;                // �rn: Apple.asset
    public GameObject outputPrefab;       // �rn: AppleExtractObject.prefab

    [Header("Ayarlar")]
    public float processTime = 3f;
    public Transform outputSpawnPoint;    // ��kt�n�n do�aca�� yer

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
        Debug.Log("��lem ba�lad�: " + inputItem.itemName);
        Destroy(input.gameObject); // Elmay� sahneden sil
        yield return new WaitForSeconds(processTime);
        Instantiate(outputPrefab, outputSpawnPoint.position, Quaternion.identity);
        Debug.Log("��kt� �retildi: " + outputPrefab.name);
    }
}
