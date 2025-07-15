using UnityEngine;

[RequireComponent(typeof(PickableObject))]
[RequireComponent(typeof(Outline))]
public class EssenceBottle : MonoBehaviour, IInteractable
{
    public EssenceDataSO data;
    public Renderer liquidRenderer;

    public float remainingAmount = 1f; 
    public float pourSpeed = 0.2f;

    public string GetInteractText()
    {
        return "[E] Esans Al";
    }

    public EssenceDataSO GetEssence()
    {
        return data;
    }

    public void Interact()
    {
        var interaction = FindObjectOfType<InteractionController>();
        interaction.PickUpObject(gameObject);
    }

    void Start()
    {
        remainingAmount = Random.Range(0.5f, 0.7f);
        UpdateLiquidColor();
        UpdateFillAmount();
    }

    public void UpdateFillAmount()
    {
        liquidRenderer.material.SetFloat("_FillAmount", remainingAmount);
    }

    void UpdateLiquidColor()
    {
        liquidRenderer.material.SetColor("_TopColor", data.essenceSideColor);
        liquidRenderer.material.SetColor("_SideColor", data.essenceSideColor);
    }
}
