using UnityEngine;

public class DropPreview : MonoBehaviour
{
    public GameObject previewMesh; // Şeffaf mesh (child objede olabilir)
    public Material validMat;
    public Material invalidMat;

    private MeshRenderer[] renderers;

    void Awake()
    {
        if (previewMesh != null)
            renderers = previewMesh.GetComponentsInChildren<MeshRenderer>();
    }

    public void SetPosition(Vector3 pos, Vector3 normal)
    {
        transform.position = pos;
        transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);
    }

    public void SetValid(bool valid)
    {
        if (renderers == null) return;

        Material mat = valid ? validMat : invalidMat;
        foreach (var r in renderers)
        {
            r.material = mat;
        }
    }

    public void SetActive(bool active)
    {
        if (previewMesh != null)
            previewMesh.SetActive(active);
    }
}
