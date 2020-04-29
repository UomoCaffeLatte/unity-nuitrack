
using UnityEngine.UI;
using UnityEngine;

public class ColorImage : MonoBehaviour
{
    [SerializeField] RawImage background;

    // Start is called before the first frame update
    void Start()
    {
        NuitrackManager.onColorUpdate += drawColor;
    }

    void drawColor(nuitrack.ColorFrame _frame)
    {
        background.texture = _frame.ToTexture2D();
    }
}
