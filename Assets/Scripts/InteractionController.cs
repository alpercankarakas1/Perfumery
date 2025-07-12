using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractionController : MonoBehaviour
{
    public float interactRange = 3f;
    public KeyCode interactKey = KeyCode.E;
    public Camera playerCamera;
    public TextMeshProUGUI interactTextUI;

    public Transform holdParent;
    private GameObject heldObject;

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
        if (Input.GetKeyDown(interactKey))
        {
            // Eğer elimizde nesne varsa, bırak
            if (heldObject != null)
            {
                DropHeldObject();
                return;
            }

            // Elimiz boşsa, raycast ile bir şey arayalım
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
            {
                // Öncelik: IInteractable var mı?
                var interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    interactable.Interact();
                }
                else
                {
                    // IInteractable yoksa, PickableObject var mı?
                    var pickable = hit.collider.GetComponent<PickableObject>();
                    if (pickable != null)
                    {
                        PickUpObject(pickable.gameObject);
                    }
                }
            }
        }
    }
    
    public void PickUpObject(GameObject obj)
    {
        heldObject = obj;
        var rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        heldObject.transform.SetParent(holdParent);
        heldObject.transform.localPosition = Vector3.zero;
        heldObject.transform.localRotation = Quaternion.identity;
    }

    public void DropHeldObject()
    {
        var rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;

        heldObject.transform.SetParent(null);
        heldObject = null;
    }

    
}
