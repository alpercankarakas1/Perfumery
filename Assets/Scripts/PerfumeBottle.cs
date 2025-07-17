using System.Collections;
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

    public string GetInteractText() => "[E] Parfum sise al";

    public void Interact()
    {
        var interaction = FindObjectOfType<InteractionController>();
        interaction.PickUpObject(gameObject);
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


            #region renk degisim test 

            if (currentAmount >= 10f)
            {
                liquidRenderer.material.SetColor("_SideColor", essence.data.essenceSideColor);
                liquidRenderer.material.SetColor("_TopColor", essence.data.essenceTopColor);
            }

            #endregion

            Debug.Log($"→ Dökülüyor: {pourAmount:F2} mL | Parfüm Şişesi: {currentAmount:F1} / {capacity} mL | Esans Kaldı: {essence.currentAmount:F1} mL");

            yield return null;
        }

        StopPouring();
    }

    void UpdateLiquidVisuals()
    {

        //shader

        //liquidRenderer.material.SetColor("_SideColor", color);
        //liquidRenderer.material.SetColor("_TopColor", color);
    }
}
