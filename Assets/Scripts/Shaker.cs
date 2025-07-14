using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Shaker : MonoBehaviour, IInteractable
{
    [Range(0, 1)]
    public float fillAmount = 0f;

    public MeshRenderer liquidRenderer;
    public float fillSpeed = 0.2f;

    private Color currentColor = Color.clear;
    private Coroutine fillRoutine;
    private MaterialPropertyBlock propBlock;

    private EssenceBottle pouringFrom;

    private void Awake()
    {
        propBlock = new MaterialPropertyBlock();
    }

    public string GetInteractText()
    {
        return "[E] Esansı Shakera Dök";
    }

    public void Interact()
    {
        var controller = FindObjectOfType<InteractionController>();
        var heldObj = controller.GetHeldObject();

        if (heldObj == null) return;

        var essence = heldObj.GetComponent<EssenceBottle>();
        if (essence == null) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            StartPouring(essence);
        }

        if (Input.GetKeyUp(KeyCode.E))
        {
            StopPouring();
        }
    }

    public void StartPouring(EssenceBottle source)
    {
        if (pouringFrom != null) return; 
        pouringFrom = source;
        if (fillRoutine != null) StopCoroutine(fillRoutine);
        fillRoutine = StartCoroutine(PourLoop());
    }

    public void StopPouring()
    {
        pouringFrom = null;
        if (fillRoutine != null)
        {
            StopCoroutine(fillRoutine);
            fillRoutine = null;
        }
    }

    private IEnumerator PourLoop()
    {
        while (pouringFrom != null)
        {
            // shaker dolu mu? şişe boş mu?
            if (fillAmount >= 1f || pouringFrom.remainingAmount <= 0f)
            {
                StopPouring();
                yield break;
            }

            float pourAmount = pouringFrom.pourSpeed * Time.deltaTime;

            fillAmount = Mathf.Clamp01(fillAmount + pourAmount);
            pouringFrom.remainingAmount = Mathf.Clamp01(pouringFrom.remainingAmount - pourAmount);
            pouringFrom.UpdateFillAmount();

            // Renk karışımı
            if (fillAmount == pourAmount) // İlk dolumda
                currentColor = pouringFrom.data.essenceSideColor;
            else
                currentColor = Color.Lerp(currentColor, pouringFrom.data.essenceSideColor, pourAmount);

            UpdateLiquidVisual();
            yield return null;
        }
    }

    void UpdateLiquidVisual()
    {
        if (liquidRenderer == null) return;

        Debug.Log("doluyor: " + fillAmount);

        propBlock.SetFloat("_FillAmount", fillAmount);
        propBlock.SetColor("_SideColor", currentColor);
        propBlock.SetColor("_TopColor", currentColor);
        liquidRenderer.SetPropertyBlock(propBlock);
    }
}
