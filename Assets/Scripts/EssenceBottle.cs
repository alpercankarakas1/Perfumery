using UnityEngine;

[RequireComponent(typeof(PickableObject))]
[RequireComponent(typeof(Outline))]
public class EssenceBottle : MonoBehaviour, IInteractable
{
    public EssenceDataSO data;

    public float currentAmount = 100f; // mL
    public float capacity = 100f;
    public float pourSpeed = 1f; // mL/sn

    public Transform dropperCap;
    public Transform dropperHoldPose;
    public float dropperCapacity = 10f;
    public float currentDropperAmount = 0f;


    public Renderer liquidRenderer;

    public bool IsEmpty => currentAmount <= 0f;

    void Start()
    {
        UpdateLiquidVisual();
    }

    void Update()
    {
        // UpdateVisuals
    }

    public string GetInteractText()
    {
        return $"[E] {data.essenceName} Esansı Al | Doluluk : {currentAmount}/{capacity} mL";
    }

    public EssenceDataSO GetEssence() => data;

    public void Interact()
    {
        var interaction = FindObjectOfType<InteractionController>();
        interaction.PickUpObject(gameObject);
    }

    void UpdateLiquidVisual()
    {
        // sonradan fill amount eklenecek


        // ....


        liquidRenderer.material.SetColor("_SideColor", data.essenceSideColor);
        liquidRenderer.material.SetColor("_TopColor", data.essenceSideColor);
    }
    
    public void RefillDropper()
    {
        float refillAmount = Mathf.Min(dropperCapacity, currentAmount);
        currentDropperAmount = refillAmount;
        currentAmount -= refillAmount;
    }
}