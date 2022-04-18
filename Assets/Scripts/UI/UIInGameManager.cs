using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIInGameManager : MonoBehaviour
{
    // game manager
    private GameManager GM;

    [Header("Panels")]
    [SerializeField]
    private GameObject HUDPanel;
    [SerializeField]
    private GameObject IntroPanel;
    [SerializeField]
    private GameObject interactionPanel;
    [SerializeField]
    private GameObject interactionPanel02;
    [SerializeField]
    private GameObject savingPanel;

    [Header("Backgrounds")]
    [SerializeField]
    private GameObject HPBackground;
    [SerializeField]
    private GameObject ProfileBackground;

    [Header("Stele Pieces")]
    private int noStelePieces = 6;
    [SerializeField]
    private GameObject[] stelePieces;
    [SerializeField]
    private GameObject[] stelePiecesOnStele;

    [Header("Title texts")]
    [SerializeField]
    private Text mifunoText;
    [SerializeField]
    private Text desertText;

    [Header("Debug Text Zones")]
    [SerializeField]
    private Text windowDebug;
    [SerializeField]
    private Text oculusDebug;

    private bool isActionPanelUp = false;
    private bool isActionPanel02Up = false;

    private void Start()
    {
        // fing the GameManager
        GameObject gameManagerGameObject = GameObject.Find("GameManager");
        GM = gameManagerGameObject.GetComponent<GameManager>();

        bool stelePieceGotten = false;

        // set HUD pieces in player inventory
        for(int i = 0; i < noStelePieces; i++)
        {
            stelePieceGotten = GM.getSteleStatus(i);

            if (stelePieceGotten == true)
            {
                updateStelePieces(i);
            }

            stelePieceGotten = false;
        }

        // set HUD pieces already placed on stele
        for (int i = 0; i < noStelePieces; i++)
        {
            stelePieceGotten = GM.getOnSteleStatus(i);

            if (stelePieceGotten == true)
            {
                updateOnStelePieces(i);
            }

            stelePieceGotten = false;
        }
    }

    private IEnumerator showSavingPanelCoroutine()
    {
        oculusDebugText("Speed panel");
        savingPanel.SetActive(true);
        yield return new WaitForSeconds(2.0f);
        savingPanel.SetActive(false);
        yield return null;
    }

    // public methods
    public void UpdateHP(float hp)
    {
        // energy bar level
        // think that it's negative values
        float BGStart = 3.0f;
        float BGEnd = 95.0f;

        float BGGap = BGEnd - BGStart;

        float newY = (BGEnd - BGGap * hp / 100) * -1;

        float localX = HPBackground.transform.localPosition.x;
        float localZ = HPBackground.transform.localPosition.z;

        HPBackground.transform.localPosition = new Vector3(localX, newY, localZ);

        // enegery bar color
        Color green = new Color(0.0f, 0.7215f, 0.0f, 1.0f);
        Color yellow = new Color(1.0f, 0.7215f, 0.0f, 1.0f);
        Color red = new Color(0.7215f, 0.0f, 0.0f, 1.0f);

        if (hp >= 50)
        {
            HPBackground.GetComponent<UnityEngine.UI.Image>().color = Color.Lerp(yellow, green, (hp - 50) / 50);
        } else
        {
            HPBackground.GetComponent<UnityEngine.UI.Image>().color = Color.Lerp(red, yellow, hp / 50);
        }

        // enegery bar color
        Color profileGreen = new Color(0.0f, 0.7215f, 0.0f, 0.66f);
        Color profileYellow = new Color(1.0f, 0.7215f, 0.0f, 0.66f);
        Color profileRed = new Color(0.7215f, 0.0f, 0.0f, 0.66f);

        if (hp >= 50)
        {
            ProfileBackground.GetComponent<UnityEngine.UI.Image>().color = Color.Lerp(yellow, green, (hp - 50) / 50);
        }
        else
        {
            ProfileBackground.GetComponent<UnityEngine.UI.Image>().color = Color.Lerp(red, yellow, hp / 50);
        }
    }

    public void updateStelePieces(int stelePieceID)
    {
        stelePieces[stelePieceID].SetActive(true);
    }

    public void updateOnStelePieces(int stelePieceID)
    {
        stelePiecesOnStele[stelePieceID].SetActive(true);
    }

    public void showHUDPanel()
    {
        HUDPanel.SetActive(true);
    }

    public void hideHUDPanel()
    {
        HUDPanel.SetActive(false);
    }

    public void showIntroPanel()
    {
        IntroPanel.SetActive(true);
    }

    public void hideIntroPanel()
    {
        IntroPanel.SetActive(false);
    }

    public void showActionPanel(string panelText)
    {
        interactionPanel.GetComponentInChildren<Text>().text = panelText;
        interactionPanel.SetActive(true);
        isActionPanelUp = true;
    }

    public void showActionPanel02(string panelText)
    {
        interactionPanel02.GetComponentInChildren<Text>().text = panelText;
        interactionPanel02.SetActive(true);
        isActionPanel02Up = true;
    }

    public void hideActionPanel()
    {
        interactionPanel.SetActive(false);
        isActionPanelUp = false;
    }

    public void hideActionPanel02()
    {
        interactionPanel02.SetActive(false);
        isActionPanel02Up = false;
    }

    public Text getTitleMifunoText()
    {
        return mifunoText;
    }

    public Text getTitleDesertText()
    {
        return desertText;
    }

    public bool getActionPanelStatus()
    {
        return isActionPanelUp;
    }

    public bool getActionPanel02Status()
    {
        return isActionPanel02Up;
    }

    public void showSavingPanel()
    {
        StartCoroutine(showSavingPanelCoroutine());
    }

    // oculus and windows public debug methods
    public void oculusDebugText(string speed)
    {
        //oculusDebug.text = speed;
    }

    public void setWindowsDebug(string txt)
    {
        //windowDebug.text = txt;
    }
}