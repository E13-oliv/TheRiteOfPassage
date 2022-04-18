using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIButtonsHoverManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {
    [Header("Animated Button")]
    [SerializeField]
    private bool animatedButton;
    private Animator buttonAnimator;

    [Header("Hover Colors")]
    [SerializeField]
    private Color baseColor;
    [SerializeField]
    private Color hoverColor;

    private Text buttonText;

    private void Start()
    {
        // get the button text
        buttonText = GetComponentInChildren<Text>();

        if (animatedButton == true)
        {
            buttonAnimator = GetComponentInChildren<Animator>();
        }
    }

    // mouse hover methods
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (buttonAnimator != null)
        {
            buttonAnimator.SetBool("isHover", true);
        }
        else
        {
            buttonText.color = hoverColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (buttonAnimator != null)
        {
            buttonAnimator.SetBool("isHover", false);
        }
        else
        {
            buttonText.color = baseColor;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (buttonAnimator != null)
        {
            buttonAnimator.SetBool("isHover", false);
        }
        else
        {
            buttonText.color = baseColor;
        }
    }
}