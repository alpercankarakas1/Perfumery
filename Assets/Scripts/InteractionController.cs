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

    public Material validMaterial;
    public Material invalidMaterial;
    public LayerMask placeableMask;

    private Vector3 previewBoundsSize;
    public LayerMask overlapCheckMask; // carpisma kontrol mask

    private GameObject previewInstance;
    private MeshRenderer[] previewRenderers;
    private bool isDropValid;
    private Vector3 lastValidDropPos;
    private Vector3 lastValidDropNormal;

    void Update()
    {
        HandleRaycast();
        HandleInteraction();

        if (heldObject != null)
            HandleDropPreview();

    }


    void HandleRaycast()
    {
        if (heldObject != null)
        {
            currentInteractable = null;
            interactTextUI.enabled = false;
            return;
        }

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

    void HandleDropPreview()
    {
        if (heldObject == null || previewInstance == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, placeableMask))
        {
            // masanin ustu mu
            float angle = Vector3.Angle(hit.normal, Vector3.up);
            if (angle > 25f) // 0 = düz yukarı, >25 = yan veya dikey yüzey
            {
                previewInstance.SetActive(false);
                isDropValid = false;
                return;
            }

            // pos rot
            previewInstance.SetActive(true);
            previewInstance.transform.position = hit.point;
            previewInstance.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

            // overlap
            bool isOverlapping = Physics.CheckBox(
                hit.point,
                previewBoundsSize / 2f,
                previewInstance.transform.rotation,
                overlapCheckMask
            );

            if (!isOverlapping)
            {
                foreach (var rend in previewRenderers)
                    rend.material = validMaterial;

                lastValidDropPos = hit.point;
                lastValidDropNormal = hit.normal;
                isDropValid = true;
            }
            else
            {
                foreach (var rend in previewRenderers)
                    rend.material = invalidMaterial;
                isDropValid = false;
            }
        }
        else
        {
            previewInstance.SetActive(false);
            isDropValid = false;
        }
    }

    public void PickUpObject(GameObject obj)
    {
        heldObject = obj;

        // rig
        var rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        // col
        foreach (var col in heldObject.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        // pos
        heldObject.transform.SetParent(holdParent);
        heldObject.transform.localPosition = Vector3.zero;
        heldObject.transform.localRotation = Quaternion.identity;

        // === PREVIEW INSTANCE OLUŞTUR ===
        previewInstance = Instantiate(heldObject, Vector3.zero, Quaternion.identity);
        previewInstance.name = heldObject.name + "_Preview";

        // comp sil
        var essence = previewInstance.GetComponent<EssenceBottle>();
        if (essence != null) DestroyImmediate(essence);

        var pickable = previewInstance.GetComponent<PickableObject>();
        if (pickable != null) DestroyImmediate(pickable);

        var interactable = previewInstance.GetComponent<IInteractable>() as MonoBehaviour;
        if (interactable != null) DestroyImmediate(interactable);

        foreach (var col in previewInstance.GetComponentsInChildren<Collider>())
            DestroyImmediate(col);

        foreach (var rigid in previewInstance.GetComponentsInChildren<Rigidbody>())
            DestroyImmediate(rigid);

        // bounds
        Bounds totalBounds = new Bounds(previewInstance.transform.position, Vector3.zero);
        foreach (var rend in previewInstance.GetComponentsInChildren<Renderer>())
        {
            totalBounds.Encapsulate(rend.bounds);
        }
        previewBoundsSize = totalBounds.size;

        // materyal
        previewRenderers = previewInstance.GetComponentsInChildren<MeshRenderer>();
        foreach (var rend in previewRenderers)
            rend.material = invalidMaterial;

        previewInstance.SetActive(false);
    }

    public void DropHeldObject()
    {
        if (!isDropValid || previewInstance == null) return;

        // drop
        heldObject.transform.SetParent(null);
        heldObject.transform.position = lastValidDropPos;
        heldObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, lastValidDropNormal);

        // col
        foreach (var col in heldObject.GetComponentsInChildren<Collider>())
        {
            col.enabled = true;
        }

        // rig
        var rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
        }

        // prew sil
        Destroy(previewInstance);
        previewInstance = null;
        previewRenderers = null;
        heldObject = null;
        isDropValid = false;
    }

}
