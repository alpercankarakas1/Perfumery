using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InteractionController : MonoBehaviour
{
    public float interactRange = 3f;
    
    public Camera playerCamera;
    public TextMeshProUGUI interactTextUI;

    public Transform holdParent;

    public LayerMask overlapCheckMask;

    private GameObject heldObject;

    private IInteractable currentInteractable;

    public Material validMaterial;
    public Material invalidMaterial;
    public LayerMask placeableMask;

    private Vector3 previewBoundsSize;

    private GameObject previewInstance;
    private MeshRenderer[] previewRenderers;
    private bool isDropValid;
    private Vector3 lastValidDropPos;
    private Vector3 lastValidDropNormal;

    private float previewYOffset;

    private Outline lastOutline;

    private PlayerInputActions playerInputActions;
    private InputAction interactAction;
    private InputAction primaryAction;

    public GameObject GetHeldObject()
    {
        return heldObject;
    }

    private void Awake()
    {
        playerInputActions = new PlayerInputActions();

        interactAction = playerInputActions.Player.Interact;
        primaryAction = playerInputActions.Player.Attack;

        interactAction.performed += OnInteractPerformed;
        primaryAction.performed += OnPrimaryActionPerformed;
    }

    private void OnEnable()
    {
        playerInputActions.Enable();
    }

    private void OnDisable()
    {
        playerInputActions.Disable();
    }

    void Update()
    {
        HandleRaycast();
        if (heldObject != null)
            HandleDropPreview();
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (heldObject != null)
        {
            if (isDropValid)
            {
                DropHeldObject();
                return;
            }
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
        {
            var interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null && heldObject == null)
            {
                interactable.Interact();
            }
            else
            {
                var pickable = hit.collider.GetComponent<PickableObject>();
                if (pickable != null && heldObject == null)
                {
                    PickUpObject(pickable.gameObject);
                }
            }
        }
    }

    private void OnPrimaryActionPerformed(InputAction.CallbackContext context)
    {
        if (currentInteractable != null)
        {
            if (currentInteractable is PerfumeBottle perfumeBottle)
            {
                var essenceBottle = heldObject?.GetComponent<EssenceBottle>();
                if (essenceBottle != null)
                {
                    perfumeBottle.Interact();
                }
            }
        }
    }

    void HandleRaycast()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
        {
            var interactable = hit.collider.GetComponent<IInteractable>();

            currentInteractable = interactable;

            if (interactable != null && heldObject == null)
            {
                interactTextUI.text = interactable.GetInteractText();
                interactTextUI.enabled = true;

                Outline outline = hit.collider.GetComponent<Outline>();
                if (outline != null)
                {
                    if (outline != lastOutline)
                    {
                        DisableLastOutline();
                        outline.enabled = true;
                        lastOutline = outline;
                    }
                }
                else
                {
                    DisableLastOutline();
                }
            }
            else
            {
                interactTextUI.enabled = false;
                DisableLastOutline();
            }
        }
        else
        {
            currentInteractable = null;
            interactTextUI.enabled = false;
            DisableLastOutline();
        }
    }

    public void PickUpObject(GameObject obj)
    {
        heldObject = obj;

        var rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        foreach (var col in heldObject.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        heldObject.transform.SetParent(holdParent);
        heldObject.transform.localPosition = Vector3.zero;
        heldObject.transform.localRotation = Quaternion.identity;

        previewInstance = Instantiate(heldObject, Vector3.zero, Quaternion.identity);
        previewInstance.name = heldObject.name + "_Preview";

        var essence = previewInstance.GetComponent<EssenceBottle>();
        if (essence != null) DestroyImmediate(essence);

        var pickable = previewInstance.GetComponent<PickableObject>();
        if (pickable != null) DestroyImmediate(pickable);

        var interactable = previewInstance.GetComponent<IInteractable>() as MonoBehaviour;
        if (interactable != null) DestroyImmediate(interactable);

        var outline = previewInstance.GetComponent<Outline>();
        if (outline != null) DestroyImmediate(outline);

        foreach (var col in previewInstance.GetComponentsInChildren<Collider>())
            DestroyImmediate(col);

        foreach (var rigid in previewInstance.GetComponentsInChildren<Rigidbody>())
            DestroyImmediate(rigid);

        previewRenderers = previewInstance.GetComponentsInChildren<MeshRenderer>();
        foreach (var rend in previewRenderers)
            rend.material = invalidMaterial;

        previewInstance.SetActive(false);

        Bounds totalBounds = new Bounds(previewInstance.transform.position, Vector3.zero);
        float minY = float.MaxValue;

        foreach (var rend in previewInstance.GetComponentsInChildren<Renderer>())
        {
            totalBounds.Encapsulate(rend.bounds);
            if (rend.bounds.min.y < minY)
                minY = rend.bounds.min.y;
        }

        previewBoundsSize = totalBounds.size;

        previewYOffset = previewInstance.transform.position.y - minY;
    }

    public void DropHeldObject()
    {
        if (!isDropValid || previewInstance == null) return;

        heldObject.transform.SetParent(null);
        heldObject.transform.position = lastValidDropPos + new Vector3(0, previewYOffset, 0);
        heldObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, lastValidDropNormal);

        foreach (var col in heldObject.GetComponentsInChildren<Collider>())
        {
            col.enabled = true;
        }

        var rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
        }

        Destroy(previewInstance);
        previewInstance = null;
        previewRenderers = null;
        heldObject = null;
        isDropValid = false;
    }

    void HandleDropPreview()
    {
        if (heldObject == null || previewInstance == null) return;

        previewInstance.SetActive(true);

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, placeableMask))
        {
            float angle = Vector3.Angle(hit.normal, Vector3.up);
            if (angle > 25f)
            {
                previewInstance.SetActive(false);
                isDropValid = false;
                return;
            }

            Vector3 dropPosition = hit.point + new Vector3(0, previewYOffset, 0);
            previewInstance.transform.position = dropPosition;
            previewInstance.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

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

    void DisableLastOutline()
    {
        if (lastOutline != null)
        {
            lastOutline.enabled = false;
            lastOutline = null;
        }
    }
}