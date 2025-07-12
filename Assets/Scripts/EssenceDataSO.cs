using UnityEngine;

[CreateAssetMenu(menuName = "Perfume/Essence Data")]
public class EssenceDataSO : ScriptableObject
{
    public string essenceName;
    public Color essenceColor;
    public Color essenceSideColor;
    public float intensity;
    public EssenceType essenceType;
    public Sprite icon;

    public enum EssenceType
    {
        Alcohol, // alkolu de karistiracagimiz icin esans olarak saydim
        Floral,
        Fruity,
        Woody,
        Spicy,
        Sweet,
        Aquatic,
        Citrus,
        Smoky
    }
}
