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

    private float previewYOffset;

    public Material highlightMaterial;
    private Material[] originalMaterials;
    private Renderer[] highlightedRenderers;
    private IInteractable lastHighlighted;

    public GameObject GetHeldObject()
    {
        return heldObject;
    }

    void Update()
    {
        HandleRaycast();

        if (Input.GetKeyDown(interactKey))
        {
            if (currentInteractable != null && currentInteractable is Shaker)
            {
                HandleInteractionStart();
            }
            else
            {
                HandleInteraction();
            }
        }

        if (Input.GetKeyUp(interactKey))
        {
            if (currentInteractable != null && currentInteractable is Shaker)
            {
                HandleInteractionEnd();
            }
        }

        if (heldObject != null)
            HandleDropPreview();
    }


    void HandleRaycast()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
        {

            var interactable = hit.collider.GetComponent<IInteractable>();

            // elde bir sey varsa sadece shaker nisan alinabilsin
            if (heldObject != null && interactable is not Shaker)
            {
                currentInteractable = null;
                interactTextUI.enabled = false;
                //ClearHighlight();
                return;
            }

            currentInteractable = interactable;

            if (interactable != null)
            {
                interactTextUI.text = interactable.GetInteractText();
                interactTextUI.enabled = true;
                //ApplyHighlight(interactable);
            }
            else
            {
                interactTextUI.enabled = false;
               // ClearHighlight();
            }
        }
        else
        {
            currentInteractable = null;
            interactTextUI.enabled = false;
            //ClearHighlight();
        }
    }

    void HandleInteraction()
    {
        if (Input.GetKeyDown(interactKey))
        {
            if (heldObject != null && currentInteractable == null)
            {
                DropHeldObject();
                return;
            }

            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
            {
                var interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    interactable.Interact();
                }
                else
                {
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

        previewInstance.SetActive(true);

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
            Vector3 dropPosition = hit.point + new Vector3(0, previewYOffset, 0);
            previewInstance.transform.position = dropPosition;
            previewInstance.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

            // overlap
            bool isOverlapping = Physics.CheckBox(
                dropPosition,
                previewBoundsSize / 2f,
                previewInstance.transform.rotation,
                overlapCheckMask
            );

            if (!isOverlapping)
            {
                foreach (var rend in previewRenderers)
                    rend.material = validMaterial;

                lastValidDropPos = dropPosition;
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

        void HandleInteractionStart()
    {
        if (currentInteractable != null)
        {
            if (currentInteractable is Shaker shaker)
            {
                var essence = heldObject?.GetComponent<EssenceBottle>();
                if (essence != null)
                {
                    shaker.StartPouring(essence);
                }
            }
            else
            {
                currentInteractable.Interact();
            }
        }
    }

    void HandleInteractionEnd()
    {
        if (currentInteractable != null && currentInteractable is Shaker shaker)
        {
            shaker.StopPouring();
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

        previewRenderers = previewInstance.GetComponentsInChildren<MeshRenderer>();
        foreach (var rend in previewRenderers)
            rend.material = invalidMaterial;

        previewInstance.SetActive(false);

        // y offset
        Bounds totalBounds = new Bounds(previewInstance.transform.position, Vector3.zero);
        float minY = float.MaxValue;

        foreach (var rend in previewInstance.GetComponentsInChildren<Renderer>())
        {
            totalBounds.Encapsulate(rend.bounds);
            if (rend.bounds.min.y < minY)
                minY = rend.bounds.min.y;
        }

        previewBoundsSize = totalBounds.size;

        // Y offset = Preview'ın pivot ile en alt arasındaki fark
        previewYOffset = previewInstance.transform.position.y - minY;

    }

    public void DropHeldObject()
    {
        if (!isDropValid || previewInstance == null) return;

        // drop
        heldObject.transform.SetParent(null);
        heldObject.transform.position = lastValidDropPos + new Vector3(0, previewYOffset, 0);
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

    void ApplyHighlight(IInteractable interactable)
    {
        if (interactable == lastHighlighted) return;

        ClearHighlight();

        var renderers = (interactable as MonoBehaviour).GetComponentsInChildren<Renderer>();
        highlightedRenderers = renderers;

        originalMaterials = new Material[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            originalMaterials[i] = renderers[i].material;
            renderers[i].material = highlightMaterial;
        }

        lastHighlighted = interactable;
    }

    void ClearHighlight()
    {
        if (highlightedRenderers == null) return;

        for (int i = 0; i < highlightedRenderers.Length; i++)
        {
            if (highlightedRenderers[i] != null && originalMaterials != null && i < originalMaterials.Length)
            {
                highlightedRenderers[i].material = originalMaterials[i];
            }
        }

        highlightedRenderers = null;
        originalMaterials = null;
        lastHighlighted = null;
    }

    
}
