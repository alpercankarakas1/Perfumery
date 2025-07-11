using UnityEngine;
using UnityEngine.UI;

public class InteractionController : MonoBehaviour
{
    public float interactRange = 3f;
    public KeyCode interactKey = KeyCode.E;
    public Camera playerCamera;
    public Text interactTextUI; // interact edilecek nesneye bakildigi zaman yazan yazi

    private IInteractable currentInteractable;

    void Update()
    {
        HandleRaycast();
        HandleInteraction();
    }

    void HandleRaycast()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
        {
            currentInteractable = hit.collider.GetComponent<IInteractable>();

            if (currentInteractable != null)
            {
                interactTextUI.text = currentInteractable.GetInteractText();
                interactTextUI.enabled = true;
            }
            else
            {
                interactTextUI.enabled = false;
            }
        }
        else
        {
            currentInteractable = null;
            interactTextUI.enabled = false;
        }
    }

    void HandleInteraction()
    {
        if (currentInteractable != null && Input.GetKeyDown(interactKey))
        {
            currentInteractable.Interact();
        }
    }
}
