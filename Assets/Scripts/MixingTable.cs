using UnityEngine;

public class MixingTable : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        Debug.Log("mixing table ile etkilesime gecildi.");
    }

    public string GetInteractText()
    {
        return "[E] Karışım Masası";
    }
}
