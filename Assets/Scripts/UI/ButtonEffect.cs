using UnityEngine;
using UnityEngine.UI;

public class ButtonEffect : MonoBehaviour {

    public Color activeColor, notActiveColor;
    private Image thisImage;
    
    
    void Awake () {
        thisImage = GetComponent<Image>();
        
    }

    public void OnPointerClick()
    {
        Activate();
    }

    public void OnPointerEnterAndDown()
    {
        Activate();

    }

    public void OnPointerExit()
    {
        thisImage.color = notActiveColor;
    }

    private void Activate()
    {
        //Debug.Log(transform.name);
        thisImage.color = activeColor;
        
    }
}