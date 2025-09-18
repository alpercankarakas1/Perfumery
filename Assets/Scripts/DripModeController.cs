using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class DripModeController : MonoBehaviour
{
    public static DripModeController Instance;

    [Header("References")]
    public Transform dropperObject;
    public Transform dripStartPos;
    public Transform essenceBottleSocket;
    public Transform perfumeBottleTarget;

    [Header("Settings")]
    public float dripAmountPerClick = 1f;
    public float dropperMoveSpeed = 5f;

    private EssenceBottle currentEssence;
    private PerfumeBottle currentPerfume;
    private float dropperFill = 0f;
    private bool isActive = false;
    private Camera camera;

    void Awake()
    {
        Instance = this;
        camera = Camera.main;
        ExitDripMode(); 
    }

    void Update()
    {
        if (!isActive) return;

        Vector2 mouse = Mouse.current.delta.ReadValue();
        dropperObject.transform.Translate(new Vector3(mouse.x, -mouse.y, 0) * Time.deltaTime * dropperMoveSpeed);

        // Sol tıkla damlat
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (dropperFill >= dripAmountPerClick && currentEssence.currentAmount >= dripAmountPerClick)
            {
                currentPerfume.AddEssence(currentEssence.data, dripAmountPerClick);
                dropperFill -= dripAmountPerClick;
                currentEssence.currentAmount -= dripAmountPerClick;

                Debug.Log($"→ Damladı: {dripAmountPerClick} mL | Damlalık: {dropperFill}/10 | Esans: {currentEssence.currentAmount}");
            }
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            float refill = Mathf.Min(10f - dropperFill, currentEssence.currentAmount);
            dropperFill += refill;
            currentEssence.currentAmount -= refill;
            Debug.Log($"← Dolduruldu: {refill} mL | Damlalık: {dropperFill}/10 | Esans: {currentEssence.currentAmount}");
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ExitDripMode();
        }
    }

    public void EnterDripMode(EssenceBottle essence, PerfumeBottle perfume)
    {
        isActive = true;
        currentEssence = essence;
        currentPerfume = perfume;

        dropperFill = 10f;

        //camera.transform.position = dripCamPos.position;
        //camera.transform.rotation = dripCa.rotation;

        dropperObject.gameObject.SetActive(true);
        dropperObject.position = dripStartPos.position;
        dropperObject.rotation = dripStartPos.rotation;

        // Disable player movement here if needed
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ExitDripMode()
    {
        isActive = false;

        dropperObject.gameObject.SetActive(false);

        currentEssence = null;
        currentPerfume = null;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public bool IsInDripMode() => isActive;

}
