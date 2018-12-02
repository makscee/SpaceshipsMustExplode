using UnityEngine;

public class ModuleShadow : MonoBehaviour
{
    public Color Color;
    private SpriteRenderer _sr;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }
    public void Activate(Color c)
    {
        Color = c;
        _sr.color = c;
        gameObject.SetActive(true);
    }

    public void Disabe()
    {
        gameObject.SetActive(false);
    }
}