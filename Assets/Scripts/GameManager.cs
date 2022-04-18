using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    private bool firstLoad = true;
    private bool newGame = false;

    // build platform
    enum buildPlatforms
    {
        OculusQuest = 0,
        MacWindows = 1
    };

    [Header("Build Plaform")]
    [SerializeField]
    private buildPlatforms buildPlatform;
    private string platform;
    private GameObject macWindowsUI;

    private Scene activeScene;
    private string activeSceneName;

    private GameObject player;

    [Header("UI Cameras")]
    [SerializeField]
    private GameObject macWindowsUICamera;
    [SerializeField]
    private GameObject oculusUICamera;

    [Header("World Cameras")]
    [SerializeField]
    private Camera macWindowsUIWorldCamera;
    [SerializeField]
    private Camera oculusUIWorldCamera;

    [Header("Weclome UI")]
    [SerializeField]
    private Canvas welcomeUICanvas;
    [SerializeField]
    private EventSystem welcomeUIEventSytem;

    private GameObject villageCollider;

    private GameObject macWindowsPauseUIPanel;
    private GameObject oculusPauseUIPanel;
    private GameObject pauseUIPanel;

    private GameObject macWindowsPlayer;
    private UnityStandardAssets.Characters.FirstPerson.FirstPersonController macWindowsController;
    private GameObject oculusPlayer;

    // UI InGame Pause Manager && UI HUD manager
    private UIWelcomeManager UIWelcomeManager;
    private UIInGameManager UIHudManager;

    private bool isPlayerInRestZone = false;

    // set dawn : 3:36AM
    private float dawn = .15f;
    // set dusk : 8:24PM
    private float dusk = .85f;
    private float dayLength;
    // set how long before dusk and after dawn the lights are lit : 1h
    private float lightsBeforeDuskAfterDawn = .04f;
    // is day 
    private bool isDay;
    // dawn & dusk are coming
    private bool dawnIsComing;
    private bool duskIsComing;

    // never put 0, at leat 0.01
    //private float startTime = .50f; // 1 = 1 full day
    private float startTime;

    // time setting
    // ie 1f –> 1s = 1 hour
    // private float inGameHourDuration = 1f;
    private float inGameHourDuration = 12f;
    private float inGameDayDuration;
    private float dawnDuskDuration;
    private float inGameTime;

    private bool isGamePaused = false;

    // congif manager
    private XmlManager xmlManager;
    private float gameDaysDuration;

    // audio
    private AudioManager AM;
    private AudioSource SFXAudioSource;
    private AudioClip introduction;
    private float sfxVolume;
    private float voiceOverVolume;

    protected override void Awake()
    {
        // call superclass awake method to keep singleton alive
        base.Awake();

        // access to xml
        xmlManager = new XmlManager();
        // load config
        xmlManager.loadFile("config");

        // get the name of the active scene
        activeScene = SceneManager.GetActiveScene();
        activeSceneName = activeScene.name;

        inGameDayDuration = inGameHourDuration * 24;

        if (activeSceneName == "MifunoDesert")
        {
            loadSaveFile();
            startTime = getSaveTime();
        }

        dayLength = dusk - dawn;
        inGameTime = startTime;

        // check if it's day or night
        if (inGameTime >= dawn && inGameTime <= dusk)
        {
            isDay = true;
        }
        else
        {
            isDay = false;
        }

        platform = getBuildPlaform();

        // if in intro scene
        if (activeSceneName == "Intro")
        {
            if (platform == "OculusQuest")
            {
                Debug.Log("Oculus");
                // activate Oculus camera and input module
                oculusUICamera.SetActive(true);
                macWindowsUICamera.SetActive(false);
                welcomeUIEventSytem.GetComponent<OVRInputModule>().enabled = true;
                welcomeUIEventSytem.GetComponent<OVRRaycaster>().enabled = true;
                welcomeUIEventSytem.GetComponent<StandaloneInputModule>().enabled = false;

                // set event camera for the canvas
                welcomeUICanvas.GetComponent<Canvas>().worldCamera = oculusUIWorldCamera;
                welcomeUICanvas.GetComponent<OVRRaycaster>().enabled = true;
                welcomeUICanvas.GetComponent<BaseRaycaster>().enabled = false;
            }
            else
            {
                // activate Mac/Windows camera and input module
                oculusUICamera.SetActive(false);
                macWindowsUICamera.SetActive(true);
                welcomeUIEventSytem.GetComponent<OVRInputModule>().enabled = false;
                welcomeUIEventSytem.GetComponent<OVRRaycaster>().enabled = false;
                welcomeUIEventSytem.GetComponent<StandaloneInputModule>().enabled = true;

                // set event camera for the canvas
                welcomeUICanvas.GetComponent<Canvas>().worldCamera = macWindowsUIWorldCamera;
                welcomeUICanvas.GetComponent<OVRRaycaster>().enabled = false;
                welcomeUICanvas.GetComponent<BaseRaycaster>().enabled = true;
            }
        }
    }

    private void Start()
    {
        // fing the GameManager
        GameObject audioManagerGameObject = GameObject.Find("AudioManager");
        AM = audioManagerGameObject.GetComponent<AudioManager>();

        if (activeSceneName == "Intro")
        {
            GameObject SFX_AudioSourceGameObject = GameObject.Find("SFX_AudioSource");
            SFXAudioSource = SFX_AudioSourceGameObject.GetComponent<AudioSource>();

            playMusicOnIntro();
        }

        // audio management
        // set volumes acording to prefs
        sfxVolume = getSfxVolume();
        voiceOverVolume = getVoiceOverVolume();

        platform = getBuildPlaform();
    }

    public void gameStart()
    {
        // fing the UIWelcomeManger
        GameObject UIWelcomeManagerGameObject = GameObject.Find("UIPauseManager");
        UIWelcomeManager = UIWelcomeManagerGameObject.GetComponentInChildren<UIWelcomeManager>();

        GameObject UIInGameManagerGameObject;

        // fing the UIWelcomeManger
        if (platform == "MacWindows")
        {
            UIInGameManagerGameObject = GameObject.Find("UIInGameManager");
        }
        else
        {
            UIInGameManagerGameObject = GameObject.Find("UIInGameManagerVR");
        }

        UIHudManager = UIInGameManagerGameObject.GetComponent<UIInGameManager>();

        // fing the GameManager
        GameObject audioManagerGameObject = GameObject.Find("AudioManager");
        AM = audioManagerGameObject.GetComponent<AudioManager>();

        // audio management
        // set volumes acording to prefs
        sfxVolume = getSfxVolume();
        voiceOverVolume = getVoiceOverVolume();

        startTime = getSaveTime();
        inGameTime = startTime;

        dawnDuskDuration = lightsBeforeDuskAfterDawn * 2.0f * inGameDayDuration;

        // set the correct player controller
        if (platform == "OculusQuest")
        {
            oculusPlayer.SetActive(true);
            macWindowsPlayer.SetActive(false);
            macWindowsUI.SetActive(false);
            pauseUIPanel = oculusPauseUIPanel;
        }
        else
        {
            oculusPlayer.SetActive(false);
            macWindowsPlayer.SetActive(true);
            macWindowsUI.SetActive(true);
            pauseUIPanel = macWindowsPauseUIPanel;
        }

        // check if it's day or night
        if (inGameTime >= dawn && inGameTime <= dusk)
        {
            isDay = true;
        }
        else
        {
            isDay = false;
        }

        AM.setAudioSourceVolume("SFX", sfxVolume);
        AM.setAudioSourceVolume("VoiceOver", voiceOverVolume);

        if (newGame == true)
        {
            // start game intro
            StartCoroutine(gameIntroCoroutine());

        }

        gameDaysDuration = Mathf.Floor(getGameDuration());
    }

    private void Update()
    {
        firstLoad = false;

        // get the name of the active scene
        activeScene = SceneManager.GetActiveScene();
        activeSceneName = activeScene.name;

        if (activeSceneName == "Intro")
        {
            if (Time.timeSinceLevelLoad == 0)
            {
                GameObject SFX_AudioSourceGameObject = GameObject.Find("SFX_AudioSource");
                SFXAudioSource = SFX_AudioSourceGameObject.GetComponent<AudioSource>();

                playMusicOnIntro();
                Time.timeScale = 1;
            }
        }

        inGameDayDuration = inGameHourDuration * 24;

        // restart in game hour if more than on day 
        if (inGameTime >= 1)
        {
            // add 1 day to the game duration
            gameDaysDuration += 1.0f;
            // reset time to 0
            inGameTime = 0;
        }

        // dawn and dusk are coming
        if (inGameTime > dawn - lightsBeforeDuskAfterDawn && inGameTime < dawn)
        {
            dawnIsComing = true;
        }
        else if (inGameTime > dusk - lightsBeforeDuskAfterDawn && inGameTime < dusk)
        {
            duskIsComing = true;
        }
        else
        {
            dawnIsComing = false;
            duskIsComing = false;
        }

        // pause game management
        if (activeSceneName == "MifunoDesert")
        {
            // incerement time in game
            inGameTime += Time.deltaTime / inGameHourDuration / 24;

            // check if it's day or night
            if (inGameTime >= dawn && inGameTime <= dusk)
            {
                isDay = true;
            }
            else
            {
                isDay = false;
            }

            // if escape is pressed
            if (Input.GetKeyUp(KeyCode.Escape) && Time.timeScale != 0 && platform == "MacWindows")
            {
                pauseGame();
            }
            else if (Input.GetKeyUp(KeyCode.Escape) && isGamePaused == true && platform == "MacWindows")
            {
                resumeGame();
            }

            // if oculus menu button is pressed
            if (OVRInput.GetDown(OVRInput.RawButton.Start) && Time.timeScale != 0 && platform == "OculusQuest")
            {
                pauseGame();
                UIWelcomeManager.activateOVRInGamePointer();
            }
        }
    }

    // play intro audio and stuck player for a few seconds
    private IEnumerator gameIntroCoroutine()
    {
        UIHudManager.showIntroPanel();

        macWindowsController.enabled = false;

        StartCoroutine(AM.playAudioClipCoroutine("VoiceOver", 0, introduction, 1.0f));

        yield return new WaitForSeconds(9.0f);

        Color titleStartColor = new Color(0.7215f, 0.0f, 0.0f, 1.0f);
        Color titleEndColor = new Color(0.7215f, 0.0f, 0.0f, 0.0f);

        Text mifuno = UIHudManager.getTitleMifunoText();
        Text desert = UIHudManager.getTitleDesertText();

        float counter = 0.0f;
        float fadeDuration = 3.0f;

        while (counter < fadeDuration)
        {
            counter += Time.deltaTime;

            mifuno.color = Color.Lerp(titleStartColor, titleEndColor, counter / fadeDuration);
            desert.color = Color.Lerp(titleStartColor, titleEndColor, counter / fadeDuration);

            yield return null;
        }

        yield return new WaitForSeconds(2.0f);

        macWindowsController.enabled = true;

        yield return new WaitForSeconds(2.0f);
        UIHudManager.hideIntroPanel();
    }

    private void hideCursor()
    {
        // hide and deactivate cursor
        macWindowsController.m_MouseLook.XSensitivity = 0;
        macWindowsController.m_MouseLook.YSensitivity = 0;
        macWindowsController.m_MouseLook.lockCursor = false;
        macWindowsController.m_MouseLook.m_cursorIsLocked = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void restoreCursor()
    {
        // restore cursor options
        macWindowsController.m_MouseLook.XSensitivity = 2;
        macWindowsController.m_MouseLook.YSensitivity = 2;
        macWindowsController.m_MouseLook.lockCursor = true;
        macWindowsController.m_MouseLook.m_cursorIsLocked = true;
        macWindowsController.m_MouseLook.UpdateCursorLock();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // public get methods
    public void playMusicOnIntro()
    {
        // start music
        StartCoroutine(AM.audioFadeCoroutine(SFXAudioSource, 0.0f, sfxVolume / 5, 4.0f));

    }

    public void stopMusicOnIntro()
    {
        // start music
        StartCoroutine(AM.audioFadeCoroutine(SFXAudioSource, sfxVolume / 5, 0.0f, 0.9f));

    }

    public void setGameSpeed(float speed)
    {
        Time.timeScale = speed;
    }

    public bool getFirstLoad()
    {
        return firstLoad;
    }

    public void pauseGame()
    {
        isGamePaused = true;

        // pause game
        Time.timeScale = 0;

        hideCursor();

        // show pause UI panel
        pauseUIPanel.SetActive(true);
        UIWelcomeManager.setIsPrefsOpen(true);
    }

    // public get methods
    public void pauseWinOrDeathGame()
    {
        // pause game
        Time.timeScale = 0;

        isGamePaused = true;

        hideCursor();

        if (platform == "OculusQuest")
        {
            UIWelcomeManager.activateOVRInGamePointer();
        }
    }

    public void resumeGame()
    {
        isGamePaused = false;

        // resume game
        Time.timeScale = 1;

        restoreCursor();

        // get the parent of the pause panel
        GameObject panelParent = pauseUIPanel.transform.parent.gameObject;

        // hide all pause UI panel children
        for (int i = 0; i < panelParent.transform.childCount; i++)
        {
            var child = panelParent.transform.GetChild(i).gameObject;

            if (child != null)
            {
                child.SetActive(false);
            }
        }

        UIWelcomeManager.setIsMapOpen(false);
        UIWelcomeManager.setIsPrefsOpen(false);

        // hide pause UI panel
        pauseUIPanel.SetActive(false);
    }

    public string getBuildPlaform()
    {
        return buildPlatform.ToString();
    }

    public void setMacWindowsPlayer(GameObject player)
    {
        macWindowsPlayer = player;
    }

    public void setMacWindowsController(UnityStandardAssets.Characters.FirstPerson.FirstPersonController controller)
    {
        macWindowsController = controller;
    }

    public void setOculusPlayer(GameObject player)
    {
        oculusPlayer = player;
    }

    public void setMacWindowsPauseUIPanel(GameObject panel)
    {
        macWindowsPauseUIPanel = panel;
    }

    public void setOculusPauseUIPanel(GameObject panel)
    {
        oculusPauseUIPanel = panel;
    }

    public bool getIsGamePaused()
    {
        return isGamePaused;
    }

    public void setMacWindowsUI(GameObject macUI)
    {
        macWindowsUI = macUI;
    }

    public float getStartTime()
    {
        return startTime;
    }

    public float getSunStartPosition()
    {
        float sunStartPosition = 0.0f;

        // calculations to compensate the fact that night goes faster
        if (inGameTime < dawn)
        {
            sunStartPosition = inGameTime / dayLength; 
        }
        else if (inGameTime >= dawn && inGameTime <= dusk)
        {
            sunStartPosition = dawn / dayLength + (inGameTime - dawn) * dayLength;
        }
        else
        {
            sunStartPosition = dawn / dayLength + (inGameTime - dawn) * dayLength + (inGameTime - dawn - dayLength) / dayLength;
        }

        return sunStartPosition;
    }
    
    public float getLightsBeforeDuskAfterDawn()
    {
        return lightsBeforeDuskAfterDawn;
    }

    public float getDayLength()
    {
        return dayLength;
    }

    public bool getIsDay()
    {
        return isDay;
    }

    public float getDawn()
    {
        return dawn;
    }

    public float getDusk()
    {
        return dusk;
    }

    public bool getDawnIsComing()
    {
        return dawnIsComing;
    }

    public bool getDuskIsComing()
    {
        return duskIsComing;
    }

    public float getDawnDuskDuration()
    {
        return dawnDuskDuration;
    }

    public float getInGameHourDuration()
    {
        return inGameHourDuration;
    }

    public float getInGameDayDuration()
    {
        return inGameDayDuration;
    }

    public float getInGameTime()
    {
        return inGameTime;
    }

    public float getSfxVolume()
    {
        return xmlManager.getSFXVolume();
    }

    public void setSfxVolume(float value)
    {
        xmlManager.setSFXVolume(value);
        xmlManager.saveFile("config");
    }

    public float getVoiceOverVolume()
    {
        return xmlManager.getVoicesVolume();
    }

    public void setVoicesVolume(float value)
    {
        xmlManager.setVoicesVolume(value);
        xmlManager.saveFile("config");
    }

    public string getDifficulty()
    {
        return xmlManager.getDifficulty();
    }

    public void setDifficulty(string difficulty)
    {
        xmlManager.setDifficulty(difficulty);
        xmlManager.saveFile("config");
    }

    public bool getFireStatus(int fireID)
    {
        return xmlManager.getFireStatus(fireID);
    }

    public void setFireStatus(int fireNum)
    {
        xmlManager.setFireStatus(fireNum);
    }

    public bool[] getStelePiecesStatus()
    {
        return xmlManager.getStelePiecesStatus();
    }

    public bool getSteleStatus(int steleID)
    {
        return xmlManager.getSteleStatus(steleID);
    }

    public void setOnSteleStatus(int steleID)
    {
        xmlManager.setOnSteleStatus(steleID);
    }

    public bool getOnSteleStatus(int steleID)
    {
        return xmlManager.getOnSteleStatus(steleID);
    }

    public void setSteleStatus(int steleID)
    {
        xmlManager.setSteleStatus(steleID);
    }

    public bool[] getMapCamps()
    {
        return xmlManager.getMapCamps();
    }

    public bool getMapCamp(int mapCampID)
    {
        return xmlManager.getMapCamp(mapCampID);
    }

    public void setMapCamp(int mapCampID)
    {
        xmlManager.setMapCamp(mapCampID);
    }

    public bool[] getMapCampsAlpha()
    {
        return xmlManager.getMapCampsAlpha();
    }

    public bool getMapCampAlpha(int mapCampID)
    {
        return xmlManager.getMapCampAlpha(mapCampID);
    }

    public void setMapCampAlpha(int mapCampID)
    {
        xmlManager.setMapCampAlpha(mapCampID);
    }

    public bool[] getMapStelePieces()
    {
        return xmlManager.getMapStelePieces();
    }

    public bool getMapStelePieces(int mapSteleID)
    {
        return xmlManager.getMapStelePieces(mapSteleID);
    }

    public void setMapStelePieces(int mapSteleID)
    {
        xmlManager.setMapStelePieces(mapSteleID);
    }

    public bool[] getMapPiecesOnStele()
    {
        return xmlManager.getMapPiecesOnStele();
    }

    public bool getMapPiecesOnStele(int mapSteleID)
    {
        return xmlManager.getMapPiecesOnStele(mapSteleID);
    }

    public void setMapPiecesOnStele(int mapSteleID)
    {
        xmlManager.setMapPiecesOnStele(mapSteleID);
    }

    public bool[] getMapLandMarks()
    {
        return xmlManager.getMapLandMarks();
    }

    public bool getMapLandMark(int mapLandMarkId)
    {
        return xmlManager.getMapLandMark(mapLandMarkId);
    }

    public void setMapLandMark(int mapLandMarkId)
    {
        xmlManager.setMapLandMark(mapLandMarkId);
    }

    public bool getEventVoiceOverOncePlayed(int eventVoiceOverOncePlayedID)
    {
        return xmlManager.getEventVoiceOverOncePlayed(eventVoiceOverOncePlayedID);
    }

    public void setEventVoiceOverOncePlayed(int eventVoiceOverOncePlayedID)
    {
        xmlManager.setEventVoiceOverOncePlayed(eventVoiceOverOncePlayedID);
    }

    public bool getNewGame()
    {
        return xmlManager.getNewGame();
    }

    public void setNewGame(bool isNewGame)
    {
        newGame = isNewGame;
        xmlManager.setNewGame(isNewGame);
    }

    public float getPlayerEnergy()
    {
        return xmlManager.getPlayerEnergy();
    }

    public void setPlayerEnergy(float energy)
    {
        xmlManager.setPlayerEnergy(energy);
    }

    public bool getWithDepa()
    {
        return xmlManager.getWithDepa();
    }

    public void setWithDepa(bool withDepa)
    {
        xmlManager.setWithDepa(withDepa);
    }

    public float getSaveTime()
    {
        return xmlManager.getSaveTime();
    }

    public float getNewSaveTime()
    {
        return xmlManager.getNewSaveTime();
    }

    public void setSaveTime()
    {
        float timeToSave;
        // debug... to prevent dawn and dusk bug
        if (inGameTime >= 0.80f && inGameTime <= 0.90f)
        {
            timeToSave = 0.90f;
        }
        else if (inGameTime >= 0.10f && inGameTime <= 0.20f)
        {
            timeToSave = 0.10f;
        }
        else
        {
            timeToSave = inGameTime;
        }

        xmlManager.setSaveTime(timeToSave);
    }

    public float getGameDaysDuration()
    {
        return gameDaysDuration;
    }

    public float getGameDuration()
    {
        return xmlManager.getGameDuration();
    }

    public void setGameDuration()
    {
        float duration = gameDaysDuration + inGameTime;

        xmlManager.setGameDuration(duration);
    }

    public Vector3 getPlayerPosition()
    {
        return xmlManager.getPlayerPosition();
    }

    public void setPlayerPosition()
    {
        // get player game objet for position
        player = GameObject.FindGameObjectWithTag("Player");
        xmlManager.setPlayerPosition(player.transform.position);
    }

    public bool getIsPlayerInRestZone()
    {
        return isPlayerInRestZone;
    }

    public void setIsPlayerInRestZone(bool isHe)
    {
        isPlayerInRestZone = isHe;
    }

    public void setVillageCollider(GameObject collider)
    {
        villageCollider = collider;
    }

    public void openVillage()
    {
        villageCollider.GetComponent<CapsuleCollider>().isTrigger = true;
    }

    public bool doSaveExists()
    {
        return xmlManager.doSaveExists();
    }

    public void createNewSaveFile()
    {
        xmlManager.newSaveFile();
    }

    public void loadSaveFile()
    {
        xmlManager.loadFile("save");
    }

    public void saveSaveFile()
    {
        xmlManager.saveFile("save");
    }

    public void deleteSaveFile()
    {
        xmlManager.deleteFile("save");
    }

    public void setIntroductionAudioClip(AudioClip clip)
    {
        introduction = clip;
    }
}