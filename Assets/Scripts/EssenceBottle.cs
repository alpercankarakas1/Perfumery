using UnityEngine;

[RequireComponent(typeof(PickableObject))]
public class EssenceBottle : MonoBehaviour, IInteractable
{
    public EssenceDataSO data;
    public Renderer liquidRenderer;

    void Start()
    {
        liquidRenderer.material.SetColor("_TopColor", data.essenceTopColor);
        liquidRenderer.material.SetColor("_SideColor", data.essenceSideColor);
        liquidRenderer.material.SetFloat("_FillAmount", Random.Range(0.5f,0.7f));
    }

    public void Interact()
    {
        var interaction = FindObjectOfType<InteractionController>();
        interaction.PickUpObject(gameObject);
    }

    public string GetInteractText()
    {
        return $"[E] {data.essenceName} Esans Al";
    }

    public EssenceDataSO GetEssence()
    {
        return data;
    }
}
