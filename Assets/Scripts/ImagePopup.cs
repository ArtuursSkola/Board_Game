using UnityEngine;

public class ImagePopup : MonoBehaviour
{
    public GameObject imagePanel;   // The image or panel you want to show

    public void ShowImage()
    {
        if (imagePanel != null)
            imagePanel.SetActive(true);
    }

    public void HideImage()
    {
        if (imagePanel != null)
            imagePanel.SetActive(false);
    }
}
