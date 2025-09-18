using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

[RequireComponent(typeof(PickableObject))]
[RequireComponent(typeof(Outline))]
public class PerfumeBottle : MonoBehaviour, IInteractable
{
    public float capacity = 100f; // mL
    public float currentAmount = 0f;
    public Renderer liquidRenderer;
    private Coroutine pourRoutine;
    private Dictionary<EssenceDataSO, float> essenceContents = new();
    private Color mixedColor = Color.clear;

    //drip mode

    public Transform dripCameraPoint;
    public Transform essencePlacePoint;
    public Transform dropperHoldPoint;

    public string GetInteractText()
    {
        string text = $"[E] Parfum sisesini al\n";
        text += $"Doluluk: {currentAmount:F1} / {capacity} mL\n";

        if (essenceContents.Count == 0)
        {
            text += "(Henüz içerik eklenmedi)";
        }
        else
        {
            text += "İçerik:\n";
            foreach (var kvp in essenceContents)
            {
                float percent = (kvp.Value / currentAmount) * 100f;
                text += $"- {kvp.Key.essenceName}: {kvp.Value:F1} mL ({percent:F0}%)\n";
            }
        }

        return text.TrimEnd();
    }


    public void Interact()
    {
        var interaction = FindObjectOfType<InteractionController>();
        interaction.PickUpObject(gameObject);
    }

    //public void StartDripInteraction(EssenceBottle essence)
    //{
    //    var dripController = FindObjectOfType<DripModeController>();
    //    dripController.EnterDripMode(essence, this);
    //}

    public void AddEssence(EssenceDataSO data, float amount)
    {
        if (currentAmount + amount > capacity) return;

        currentAmount += amount;

        if (essenceContents.ContainsKey(data))
            essenceContents[data] += amount;
        else
            essenceContents[data] = amount;

        UpdateLiquidVisuals();
    }

    public void StartPouring(EssenceBottle essence)
    {
        if (pourRoutine != null) return;

        pourRoutine = StartCoroutine(PourFromEssence(essence));
    }

    public void StopPouring()
    {
        if (pourRoutine != null)
        {
            StopCoroutine(pourRoutine);
            pourRoutine = null;
        }
    }
    
    private IEnumerator PourFromEssence(EssenceBottle essence)
    {
        while (essence != null && !essence.IsEmpty && currentAmount < capacity)
        {
            float pourAmount = essence.pourSpeed * Time.deltaTime;

            // limit kontrolleri
            pourAmount = Mathf.Min(pourAmount, essence.currentAmount);
            pourAmount = Mathf.Min(pourAmount, capacity - currentAmount);

            essence.currentAmount -= pourAmount;
            currentAmount += pourAmount;

            if (essenceContents.ContainsKey(essence.data))
                essenceContents[essence.data] += pourAmount;
            else
                essenceContents[essence.data] = pourAmount;

            UpdateLiquidVisuals();

            #region DEBUG
            foreach (var kvp in essenceContents)
            {
                Debug.Log($"→ İçerik: {kvp.Key.essenceName} - {kvp.Value:F1} mL");
            }

            Debug.Log($"→ Dökülüyor: {pourAmount:F2} mL | Parfüm Şişesi: {currentAmount:F1} / {capacity} mL | Esans Kaldı: {essence.currentAmount:F1} mL");
            #endregion

            yield return null;
        }

        StopPouring();
    }

    void UpdateLiquidVisuals()
    {
        if (essenceContents == null || essenceContents.Count == 0) return;

        float totalAmount = 0f;
        foreach (var kvp in essenceContents)
            totalAmount += kvp.Value;

        if (totalAmount <= 0f) return;

        Color mixedSide = Color.black;
        Color mixedTop = Color.black;

        foreach (var kvp in essenceContents)
        {
            float ratio = kvp.Value / totalAmount;
            mixedSide += kvp.Key.essenceSideColor * ratio;
            mixedTop += kvp.Key.essenceTopColor * ratio;
        }


        // Ekstra beyazlik
        mixedSide = Color.Lerp(mixedSide, Color.white, 0.1f);
        mixedTop = Color.Lerp(mixedTop, Color.white, 0.1f);

        // Shadera gonder
        liquidRenderer.material.SetColor("_SideColor", mixedSide);
        liquidRenderer.material.SetColor("_TopColor", mixedTop);
    }
}
