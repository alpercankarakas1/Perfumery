using UnityEngine;

[RequireComponent(typeof(PickableObject))]
public class EssenceBottle : MonoBehaviour, IInteractable
{
    public EssenceDataSO data; 

    void Start()
    {
        GetComponentInChildren<Renderer>().material.color = data.essenceColor;
    }

    public void Interact()
    {
        var interaction = FindObjectOfType<InteractionController>();
        interaction.PickUpObject(gameObject);
    }

    public string GetInteractText()
    {
        return $"[E] {data.essenceName} Al";
    }

    public EssenceDataSO GetEssence()
    {
        return data;
    }
}
