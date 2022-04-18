using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIWelcomeManager : MonoBehaviour
{
    // game and audio managers
    private GameManager GM;
    private AudioManager AM;

    // player
    private Player player;

    private string platform;
    private bool firstLoad;

    [Header("Panels")]
    [SerializeField]
    private GameObject e13Panel;
    [SerializeField]
    private GameObject prefPanel;
    [SerializeField]
    private GameObject controlsPanel;
    [SerializeField]
    private GameObject macWindowsControls;
    [SerializeField]
    private GameObject oculusControls;
    [SerializeField]
    private GameObject tipsPanel;
    [SerializeField]
    private GameObject overPanel;
    [SerializeField]
    private GameObject deathPanel;
    [SerializeField]
    private GameObject winPanel;
    [SerializeField]
    private GameObject mapPanel;

    private bool isPrefsOpen = false;
    private bool isMapOpen = false;

    [Header("Buttons")]
    [SerializeField]
    private Button continueButton;

    [Header("Audio Settings")]
    [SerializeField]
    private Dropdown sfxVolumeDropdown;
    [SerializeField]
    private Slider sfxVolumeSlider;
    [SerializeField]
    private Dropdown voicesVolumeDropdown;
    [SerializeField]
    private Slider voicesVolumeSlider;

    [Header("Difficulty Settings")]
    [SerializeField]
    private Dropdown difficultyDropdown;
    [SerializeField]
    private Slider difficultySlider;

    [Header("Win Panel Texts")]
    [SerializeField]
    private Text daysText;
    [SerializeField]
    private Text hoursText;

    [Header("Map Panel")]
    [SerializeField]
    private GameObject map;
    [SerializeField]
    private GameObject playerIcon;
    private Vector3 playerInGamePos;
    private float playerInGameRotation;
    private Vector3 playerIconPos;
    private Vector3 playerIconRotation;
    private float desertWidth = 4500.0f;
    private float madWidth = 800.0f;
    private float desertMapRatio;

    [Header("Game Scene Name")]
    [SerializeField]
    private string gameStart;

    [Header("Oculus Pointer")]
    [SerializeField]
    private GameObject OVRInGamePointer;

    [Header("Debug Text Zone")]
    [SerializeField]
    private Text VRDebug;

    private void Start()
    {
        // fing the GameManager
        GameObject gameManagerGO = GameObject.Find("GameManager");
        GM = gameManagerGO.GetComponent<GameManager>();

        platform = GM.getBuildPlaform();
        firstLoad = GM.getFirstLoad();

        // find the AudioManager
        GameObject audioManagerGameObject = GameObject.Find("AudioManager");
        AM = audioManagerGameObject.GetComponent<AudioManager>();

        string activeScene = SceneManager.GetActiveScene().name;

        if (activeScene == "MifunoDesert")
        {
            // find the Player
            GameObject playerGameObject = GameObject.FindGameObjectWithTag("Player");
            player = playerGameObject.GetComponent<Player>();
        }
        else
        {
            // switch intro button states
            changeButtonsState();
        }

        // get preferences values
        int sfxVolume = (int)GM.getSfxVolume();
        int voiceOverVolume = (int)GM.getVoiceOverVolume();
        string difficulty = GM.getDifficulty() + "";

        // set the preference values as selected in the sliders
        sfxVolumeSlider.value = sfxVolume;
        voicesVolumeSlider.value = voiceOverVolume;

        switch (difficulty)
        {
            case "Easy":
                difficultySlider.value = 0;
                break;
            case "Normal":
                difficultySlider.value = 1;
                break;
            case "Hard":
                difficultySlider.value = 2;
                break;
            default:
                break;
        }

        if (platform == "MacWindows")
        {
            macWindowsControls.SetActive(true);
            oculusControls.SetActive(false);
        }
        else
        {
            macWindowsControls.SetActive(false);
            oculusControls.SetActive(true);
        }

        // set map ratio
        desertMapRatio = madWidth / desertWidth;

        //DEBUG VR
        if (activeScene != "MifunoDesert")
        {
            // VRDebug.text = "save time = " + GM.getSaveTime() + " - new save time = " + GM.getNewSaveTime();
        }

        // shows e13 logo on first start
        if (firstLoad == true)
        {
            StartCoroutine(e13Coroutine());
        }
    }

    private void Update()
    {
        string activeScene = SceneManager.GetActiveScene().name;

        if (activeScene == "MifunoDesert")
        {
            // get player position and rotation
            playerInGamePos = player.getPlayerPos();
            playerInGameRotation = player.getPlayerZRotation();

            // set player icon position
            playerIconPos.x = playerInGamePos.x * desertMapRatio;
            playerIconPos.y = playerInGamePos.z * desertMapRatio;
            playerIconPos.z = 0.0f;

            playerIcon.transform.localPosition = playerIconPos;

            // set player icon rotation
            playerIconRotation.z = playerInGameRotation * -1;
            playerIcon.transform.eulerAngles = playerIconRotation;

            // if m key is pressed –> map panel
            if (Input.GetKeyDown(KeyCode.M) == true && isPrefsOpen == false || OVRInput.GetDown(OVRInput.RawButton.X) && isPrefsOpen == false)
            {
                if (isMapOpen == false)
                {
                    showMap();
                    isMapOpen = true;
                }
                else
                {
                    hideMap();
                    isMapOpen = false;
                }
            }
        }
    }

    private void changeButtonsState()
    {
        Animator buttonAnimator = continueButton.GetComponentInChildren<Animator>();

        // check if save file exists
        if (!GM.doSaveExists() == true)
        {
            continueButton.GetComponent<Button>().interactable = false;


            buttonAnimator.SetBool("saveExists", false);
        }
        else
        {
            buttonAnimator.SetBool("saveExists", true);
        }
    }

    private IEnumerator e13Coroutine()
    {
        e13Panel.SetActive(true);
        yield return new WaitForSeconds(3.0f);
        e13Panel.SetActive(false);
    }

    // public methods
    public void showPref()
    {
        hidePanel(mapPanel);
        isMapOpen = false;
        prefPanel.SetActive(true);
        isPrefsOpen = true;
    }

    public void closePref()
    {
        prefPanel.SetActive(false);
        deactivateOVRInGamePointer();

        string activeScene = SceneManager.GetActiveScene().name;
        // is it's the in game pref panel
        if (activeScene == "MifunoDesert")
        {
            GM.resumeGame();
        }

        isPrefsOpen = false;
    }

    public void showMap()
    {
        GM.pauseGame();

        isMapOpen = true;
        isPrefsOpen = false;

        hidePanel(prefPanel);
        showPanel(mapPanel);

        if (platform == "OculusQuest")
        {
            activateOVRInGamePointer();
        }

        Color iconColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        Color inconAlphaColor = new Color(1.0f, 1.0f, 1.0f, 0.33f);
        Color iconPiecesColor = new Color(0.7215f, 0.0f, 0.0f, 1.0f);

        // near camps icons
        bool[] nearCampIcons = GM.getMapCampsAlpha();
        addIconsToMap(nearCampIcons, "Camp", inconAlphaColor);

        // camps icons
        bool[] campIcons = GM.getMapCamps();
        addIconsToMap(campIcons, "Camp", iconColor);

        // landMarks icons
        bool[] landMarksIcons = GM.getMapLandMarks();
        addIconsToMap(landMarksIcons, "LandMark", iconColor);

        // stele pieces icons
        bool[] stelePiecesIcons = GM.getMapStelePieces();
        addIconsToMap(stelePiecesIcons, "StelePiece", iconPiecesColor);

        // pieces on stele icons
        bool[] piecesOnSteleIcons = GM.getMapPiecesOnStele();
        addIconsToMap(piecesOnSteleIcons, "PieceOnStele", iconPiecesColor);
    }

    private void addIconsToMap(bool[] icons, string iconType, Color iconColor)
    {
        // add icons to the map
        if (icons.Length > 0)
        {
            for (int i = 0; i < icons.Length; i++)
            {
                string iconName = iconType + i;

                GameObject iconToShow = map.transform.Find(iconName).gameObject;

                if (icons[i] == true)
                {
                    iconToShow.SetActive(true);
                    iconToShow.GetComponent<Image>().color = iconColor;
                }
            }
        }
    }

    public void hideMap()
    {
        hidePanel(mapPanel);
        GM.resumeGame();
    }

    public void setSfxVolume(Dropdown dropdown)
    {
        float newVolume = float.Parse(dropdown.options[dropdown.value].text);

        GM.setSfxVolume(newVolume);
        AM.setAudioSourceVolume("SFX", newVolume);
    }

    public void setSfxVolumeSlider(Slider slider)
    {
        float newVolume = slider.value;

        GM.setSfxVolume(newVolume);
        AM.setAudioSourceVolume("SFX", newVolume);
    }

    public void setVoicesVolume(Dropdown dropdown)
    {
        float newVolume = float.Parse(dropdown.options[dropdown.value].text);

        GM.setVoicesVolume(newVolume);
        AM.setAudioSourceVolume("VoiceOver", newVolume);
    }

    public void setVoicesVolumeSlider(Slider slider)
    {
        float newVolume = slider.value;

        GM.setVoicesVolume(newVolume);
        AM.setAudioSourceVolume("VoiceOver", newVolume);
    }

    public void setDifficulty(Dropdown dropdown)
    {
        GM.setDifficulty(dropdown.options[dropdown.value].text);
    }

    public void setDifficultySlider(Slider slider)
    {
        float newDIfficulty = slider.value;

        switch (newDIfficulty)
        {
            case 0:
                GM.setDifficulty("Easy");
                break;
            case 1:
                GM.setDifficulty("Normal");
                break;
            case 2:
                GM.setDifficulty("Hard");
                break;
            default:
                break;
        }
    }

    public void goToMainMenu()
    {
        SceneManager.LoadScene("Intro");
    }

    public void showPanel(GameObject panel)
    {
        panel.SetActive(true);
    }

    public void hidePanel(GameObject panel)
    {
        panel.SetActive(false);
    }

    public void showOverPanel()
    {
        overPanel.SetActive(true);
    }

    public void showDeathPanel()
    {
        deathPanel.SetActive(true);
    }

    public void showWinPanel()
    {
        // set game duration
        string days = GM.getGameDaysDuration()+"";
        string hours = Mathf.Floor(GM.getInGameTime() * 24)+"";

        daysText.text = days;
        hoursText.text = hours;

        winPanel.SetActive(true);
    }

    public bool getIsPrefsOpen()
    {
        return isPrefsOpen;
    }

    public void setIsPrefsOpen(bool isIt)
    {
        isPrefsOpen = isIt;
    }

    public bool getIsMapOpen()
    {
        return isMapOpen;
    }

    public void setIsMapOpen(bool isIt)
    {
        isMapOpen = isIt;
    }

    // start game method
    public void startNewGame()
    {
        GM.setNewGame(true);

        //debug... to test
        //if (GM.doSaveExists())
        if (GM.doSaveExists() == true)
        {
            overPanel.SetActive(true);
        }
        else
        {
            StartCoroutine(startNewGameCoroutine());
        }
    }

    private IEnumerator startNewGameCoroutine()
    {
        GM.stopMusicOnIntro();

        yield return new WaitForSeconds(1.0f);

        GM.createNewSaveFile();
        GM.loadSaveFile();
        SceneManager.LoadScene(gameStart);
    }

    // continue game method
    public void continueGame()
    {
        StartCoroutine(continueGameCoroutine());
    }

    private IEnumerator continueGameCoroutine()
    {
        string activeScene = SceneManager.GetActiveScene().name;

        if (activeScene == "Intro")
        {
            GM.stopMusicOnIntro();

            yield return new WaitForSeconds(1.0f);
        }
        GM.setNewGame(false);

        if (activeScene == "MifunoDesert")
        {
            hidePanel(deathPanel);
        }

        GM.loadSaveFile();
        SceneManager.LoadScene(gameStart);

        yield return null;
    }

    // quit app
    public void quitGame()
    {
        Application.Quit();
    }

    // cancel button (on overwrite save file panel)
    public void cancelOverwriting()
    {
        overPanel.SetActive(false);
    }

    // erase button
    public void deleteSave()
    {
        changeButtonsState();
        overPanel.SetActive(false);

        StartCoroutine(startNewGameCoroutine());
    }

    public void activateOVRInGamePointer()
    {
        OVRInGamePointer.SetActive(true);
    }

    public void deactivateOVRInGamePointer()
    {
        OVRInGamePointer.SetActive(false);
    }
}