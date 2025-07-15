using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PickableObject))]
[RequireComponent(typeof(Outline))]
public class PerfumeBottle : MonoBehaviour, IInteractable
{
    public float totalCapacity = 100f; // mL
    public float currentVolume = 0f;

    public Renderer liquidRenderer;

    // Hangi esanslardan ne kadar eklendi?
    private Dictionary<EssenceDataSO, float> contents = new();

    private bool isPouring = false;
    private Coroutine pourRoutine;

    private EssenceBottle essenceBottle;

    public string GetInteractText() => "";

    public void Interact()
    {
        var controller = FindObjectOfType<InteractionController>();
        var held = controller.GetHeldObject();
        if (held == null) return;

        var essence = held.GetComponent<EssenceBottle>();
        if (essence == null || essence.remainingAmount <= 0f) return;

        if (!isPouring)
        {
            pourRoutine = StartCoroutine(PourEssence(essence));
            isPouring = true;
        }
    }

    public void StopPouring()
    {
        isPouring = false;
        if (pourRoutine != null) StopCoroutine(pourRoutine);
    }

    private IEnumerator<WaitForEndOfFrame> PourEssence(EssenceBottle essence)
    {
        while (isPouring && currentVolume < totalCapacity && essence.remainingAmount > 0f)
        {
            float pourAmount = essence.pourSpeed * Time.deltaTime;

            // Limit kontrolleri
            pourAmount = Mathf.Min(pourAmount, totalCapacity - currentVolume);
            pourAmount = Mathf.Min(pourAmount, essence.remainingAmount);

            // Ekle
            currentVolume += pourAmount;
            essence.remainingAmount -= pourAmount;

            // Renk karışımı mantığı (basit): ilk renk varsa karışır
            if (contents.ContainsKey(essence.data))
                contents[essence.data] += pourAmount;
            else
                contents[essence.data] = pourAmount;

            UpdateLiquidVisual();
            yield return new WaitForEndOfFrame();
        }

        isPouring = false;
    }

    void UpdateLiquidVisual()
    {
        if (liquidRenderer == null) return;

        float fillRatio = currentVolume / totalCapacity;

        foreach (var kvp in contents)
        {
            liquidRenderer.material.SetColor("_SideColor", kvp.Key.essenceSideColor);
            liquidRenderer.material.SetColor("_TopColor", kvp.Key.essenceSideColor);
            break;
        }

        liquidRenderer.material.SetFloat("_FillAmount", fillRatio);
    }
}
