using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveItems : MonoBehaviour
{
    // game and audio managers
    private GameManager GM;
    private AudioManager AM;

    private UIInGameManager UIInGame;
    private UIWelcomeManager UIWelcomeManager;

    private Player player;
    private GameObject depa;

    enum Interactions
    {
        Choose = 0,
        getEnergy = 1,
        fireCamp = 2,
        startPath = 3,
        stelePiece = 4,
        stele = 5,
        audioEvent = 6,
        restZone = 7,
        finishZone = 8,
        fire = 9,
        village = 10,
        extinguishedFire = 11,
        addToMap = 12,
        villageClose = 13,
        villageCloser = 14,
        easterEgg = 15,
        easterEggAction = 16
    };
    [Header("General")]
    [SerializeField]
    private Interactions interactions;
    private string interaction;

    [SerializeField]
    private bool automaticAction = false;

    [Header("CampFire")]
    [SerializeField]
    private int campFireID;

    [SerializeField]
    private GameObject[] pathsToActivate;

    private bool campFireStatus;
    private bool isDay;
    private bool isLastFrameDay;
    private bool isWithDepa;
    private int fastTimeFactor = 10;

    private bool isInRestZone;

    [Header("Energy")]
    [SerializeField]
    private float energy;

    [Header("Start Path")]
    [SerializeField]
    private GameObject path;
    [SerializeField]
    private GameObject[] otherStartPath;

    [Header("Stele Pieces")]
    [SerializeField]
    private int stelePieceID;
    [SerializeField]
    private GameObject steleEffect;
    private int noStelePieces = 6;
    [SerializeField]
    private AudioClip stelePieceDiscoverVoiceOver;

    [Header("Stele")]
    [SerializeField]
    private GameObject[] onStelePieces;
    private int alreadyOnStelePieces;

    [Header("Audio Events")]
    [SerializeField]
    private AudioClip[] audioEventClips;

    enum IconTypes
    {
        Choose = 0,
        Camp = 1,
        Landmark = 2
    };
    [Header("Add To Map")]
    [SerializeField]
    private IconTypes iconTypes;
    private string iconType;
    [SerializeField]
    private int itemID;
    [SerializeField]
    private int[] nearCampIDs;

    private void Start()
    {
        // fing the GameManager
        GameObject gameManagerGameObject = GameObject.Find("GameManager");
        GM = gameManagerGameObject.GetComponent<GameManager>();

        // fing the GameManager
        GameObject audioManagerGameObject = GameObject.Find("AudioManager");
        AM = audioManagerGameObject.GetComponent<AudioManager>();

        // fing the UIInGameManager
        GameObject UIInGameGameObject = GameObject.Find("UI");
        UIInGame = UIInGameGameObject.GetComponentInChildren<UIInGameManager>();

        // fing the UIInGameManger
        GameObject UIWelcomeManagerGameObject = GameObject.Find("UIPauseManager");
        UIWelcomeManager = UIWelcomeManagerGameObject.GetComponentInChildren<UIWelcomeManager>();

        // find the depa game object
        depa = GameObject.FindGameObjectWithTag("Depa");

        isWithDepa = GM.getWithDepa();

        // get type of interaction
        string startInteraction = interactions.ToString();

        // deactivate stele pieces if it is already taken
        if (startInteraction == "stelePiece")
        {
            bool isTaken = GM.getSteleStatus(stelePieceID);

            if (isTaken == true)
            {
                gameObject.SetActive(false);
                steleEffect.SetActive(false);
            }
        }
        else if (startInteraction == "stele")
        {
            placeOnStelePieces(true);

            alreadyOnStelePieces = alreadyOnStele();
        }
    }

    private void Update()
    {
        isDay = GM.getIsDay();
        isWithDepa = GM.getWithDepa();
        isInRestZone = GM.getIsPlayerInRestZone();

        // for all actions
        bool isActionPanelUp = UIInGame.getActionPanelStatus();
        // for special actions
        bool isActionPanelUp2 = UIInGame.getActionPanel02Status();

        // speed up the game action
        if (isActionPanelUp2 == true)
        {
            if (interaction == "fireCamp")
            {
                if (campFireStatus == true)
                {
                    string panel02Text = "speed up thE time";

                    UIInGame.showActionPanel02(panel02Text);

                    // if G key is pressed during the night
                    if (Input.GetKey(KeyCode.G) == true || OVRInput.Get(OVRInput.RawButton.B))
                    {
                        //Debug.Log("Fast !!!");
                        // faster game speed
                        GM.setGameSpeed(fastTimeFactor);
                    }

                    // if G key is released
                    if (Input.GetKeyUp(KeyCode.G) == true || OVRInput.GetUp(OVRInput.RawButton.B) || OVRInput.GetUp(OVRInput.RawButton.X))
                    {
                        // restore game speed to default
                        GM.setGameSpeed(1.0f);
                    }
                }
            }
        }

        // if player get out of the rest zone -> stop time acceleration
        if (isInRestZone == false)
        {
            if (GM.getIsGamePaused() == false)
            {
                // restore game speed to default
                GM.setGameSpeed(1.0f);
            }
        }

        // all other actions
        if (isActionPanelUp == true)
        {
            if (interaction == "fireCamp")
            {
                if (campFireStatus == false)
                {
                    if (Input.GetKeyDown(KeyCode.Space) == true || OVRInput.GetDown(OVRInput.RawButton.A))
                    {
                        campFireStatus = true;
                        GM.setFireStatus(campFireID);

                        string panelText = "save";

                        UIInGame.showActionPanel(panelText);

                        activateDepaPaths();

                        string panel02Text = "speed up thE time";

                        UIInGame.showActionPanel02(panel02Text);
                    }
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.Space) == true || OVRInput.GetDown(OVRInput.RawButton.A))
                    {
                        if (UIWelcomeManager.getIsMapOpen() == false && UIWelcomeManager.getIsPrefsOpen() == false)
                        {
                            GM.setPlayerPosition();
                            GM.setSaveTime();
                            GM.setGameDuration();
                            GM.saveSaveFile();
                            UIInGame.showSavingPanel();
                        }
                    }
               }
            }
            else if (interaction == "stelePiece")
            {
                if (Input.GetKeyDown(KeyCode.Space) == true || OVRInput.GetDown(OVRInput.RawButton.A))
                {
                    GM.setSteleStatus(stelePieceID);
                    GM.setMapStelePieces(stelePieceID);
                    UIInGame.updateStelePieces(stelePieceID);
                    this.gameObject.SetActive(false);
                    steleEffect.SetActive(false);
                    UIInGame.hideActionPanel();
                    AM.playEventVoiceOver(0, 4, 0.0f, "stelePiecesPickUp");
                }
            }
            else if (interaction == "stele")
            {
                if (Input.GetKeyDown(KeyCode.Space) == true || OVRInput.GetDown(OVRInput.RawButton.A))
                {
                    placeOnStelePieces(false);
                }
            }
            else if (interaction == "easterEggAction")
            {
                if (Input.GetKeyDown(KeyCode.Space) == true || OVRInput.GetDown(OVRInput.RawButton.A))
                {
                    AM.playEventVoiceOver(0, 12, 0.0f, "easterEgg");
                }
            }
        }

        // activate and deactivate start path relative to the fire
        if (interactions.ToString() == "fireCamp")
        {
            // to prevent test from automatic fire
            if (campFireID != 999)
            {
                bool fireStatus = GM.getFireStatus(campFireID);

                if (fireStatus == true)
                {
                    if (isDay == true && isLastFrameDay == false)
                    {
                        activateDepaPaths();
                    }
                    else if (isDay == false && isLastFrameDay == true) 
                    {
                        deactivateDepaPaths();
                    }
                }
            }
        }

        bool withDepa = GM.getWithDepa();

        // activate and deactivate start path relative to the fire
        if (withDepa == true && (interactions.ToString() == "villageClose" || interactions.ToString() == "villageCloser"))
        {
            gameObject.SetActive(false);
        }

        isLastFrameDay = isDay;
    }

    private void activateDepaPaths()
    {
        if (isWithDepa == false)
        {
            // activate linked depa start paths
            if (pathsToActivate.Length >= 1 && isDay == true)
            {
                for (int i = 0; i < pathsToActivate.Length; i++)
                {
                    pathsToActivate[i].SetActive(true);
                }
            }
        }
    }

    private void deactivateDepaPaths()
    {
        // deactivate linked depa start paths
        if (pathsToActivate.Length >= 1)
        {
            for (int i = 0; i < pathsToActivate.Length; i++)
            {
                pathsToActivate[i].SetActive(false);
            }
        }
    }

    private void deactivateAllDepaPaths()
    {
        GameObject[] allPaths = GameObject.FindGameObjectsWithTag("StartPath");

        // deactivate all depa start paths
        if (allPaths.Length >= 1)
        {
            for (int i = 0; i < allPaths.Length; i++)
            {
                allPaths[i].SetActive(false);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            // get type of interaction
            interaction = interactions.ToString();

            player = other.GetComponent<Player>();

            if (interaction == "getEnergy" && automaticAction == true)
            {
                player.UpdateEnergy(energy);
                AM.playEventVoiceOver(0, 11, 0.0f, "backPackTips");
                gameObject.SetActive(false);
            }
            else if (interaction == "fireCamp")
            {
                // check if the fire has already been prepared to be lit
                campFireStatus = checkCampFireStatus(campFireID);

                if (campFireStatus == true)
                {
                    string panelText = "save";

                    UIInGame.showActionPanel(panelText);

                    string panel02Text = "speed up thE time";

                    UIInGame.showActionPanel02(panel02Text);

                    // save option
                    if (isDay == true)
                    {
                        activateDepaPaths();
                    }
                }
                else
                {
                    string panelText = "prepare thE fire";

                    UIInGame.showActionPanel(panelText);
                }
            }
            else if (interaction == "extinguishedFire")
            {
                activateDepaPaths();
            }
            else if (interaction == "startPath")
            {
                // set a new path and make Depa pop
                depa.GetComponent<Depa>().setNewPath(path);
                // display other camp start path
                if (otherStartPath.Length >= 2)
                {
                    for (int i = 0; i < otherStartPath.Length; i++)
                    {
                        otherStartPath[i].SetActive(true);
                    }
                }
                // hide this start path
                gameObject.SetActive(false);
            }
            else if (interaction == "stelePiece")
            {
                string panelText;

                panelText = "pick up";

                UIInGame.showActionPanel(panelText);

                // play voice over if it exists
                if (stelePieceDiscoverVoiceOver != null)
                {
                    StartCoroutine(AM.playAudioClipCoroutine("VoiceOver", 0, stelePieceDiscoverVoiceOver, 0.0f));
                }
            }
            else if (interaction == "stele")
            {
                if (checkOnStelePieces())
                {
                    string panelText = "placE thE piecEs";

                    UIInGame.showActionPanel(panelText);
                }
            }
            else if (interaction == "audioEvent")
            {
                StartCoroutine(AM.playAudioClipCoroutine("VoiceOver", 0, audioEventClips[0], 0.0f));
            }
            else if (interaction == "restZone")
            {
                GM.setIsPlayerInRestZone(true);
            }
            else if (interaction == "village")
            {
                GM.pauseWinOrDeathGame();
                UIWelcomeManager.showWinPanel();
            }
            else if (interaction == "villageClose")
            {
                Debug.Log("Village close");
                // play village close tip
                AM.playEventVoiceOver(0, 7, 0.5f, "villageCloseTips");
            }
            else if (interaction == "villageCloser")
            {
                Debug.Log("Village closer");
                // play village closer tip
                AM.playEventVoiceOver(0, 8, 0.0f, "villageCloserTips");
            }
            else if (interaction == "addToMap")
            {
                iconType = iconTypes.ToString();
                addToMap(iconType, itemID);
            }
            else if (interaction == "easterEgg")
            {
                AM.playEventVoiceOver(0, 12, 0.0f, "easterEgg");
            }
            else if (interaction == "easterEggAction")
            {
                string panelText = "pet";

                UIInGame.showActionPanel(panelText);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            UIInGame.hideActionPanel();
            UIInGame.hideActionPanel02();

            if (interaction == "restZone")
            {
                GM.setIsPlayerInRestZone(false);

                if (isDay == false)
                {
                    AM.playEventVoiceOver(0, 10, 0.0f, "nightOutOfCampTips");
                }
            }
        }

        interaction = null;
    }

    private bool checkCampFireStatus(int campFireID)
    {
        return GM.getFireStatus(campFireID);
    }

    private bool checkOnStelePieces()
    {
        bool piecesToBePlaced = false;

        for (int i = 0; i < noStelePieces; i++)
        {
            bool pieceOnPlayer = GM.getSteleStatus(i);
            bool pieceOnStele = GM.getOnSteleStatus(i);

            if (pieceOnPlayer == true && pieceOnStele == false)
            {
                piecesToBePlaced = true;
            }
        }

        return piecesToBePlaced;
    }

    private void placeOnStelePieces(bool isStart)
    {
        for (int i = 0; i < noStelePieces; i++)
        {
            bool pieceOnPlayer = GM.getSteleStatus(i);
            bool pieceOnStele = GM.getOnSteleStatus(i);

            // for in game
            if (isStart == false)
            {
                if (pieceOnPlayer == true && pieceOnStele == false)
                {
                    onStelePieces[i].SetActive(true);
                    GM.setOnSteleStatus(i);
                    GM.setMapPiecesOnStele(i);
                    alreadyOnStelePieces++;
                    UIInGame.updateOnStelePieces(i);
                }
            }
            // for the start
            else
            {
                if (pieceOnPlayer == true && pieceOnStele == true)
                {
                    onStelePieces[i].SetActive(true);
                    GM.setOnSteleStatus(i);
                    UIInGame.updateOnStelePieces(i);
                }
            }
        }

        UIInGame.hideActionPanel();

        if (alreadyOnStelePieces == noStelePieces)
        {
            // complete stele tip
            AM.playEventVoiceOver(0, 9, 0.5f, "completeSteleTips");

            // deactivate all startpath
            deactivateAllDepaPaths();

            // give access to village
            GM.openVillage();

            // set the player to always run at depa speed
            GM.setWithDepa(true);
        }
    }

    private void addToMap(string iconType, int itemID)
    {
        if (iconType == "Camp")
        {
            GM.setMapCamp(itemID);

            // add near camp to be displayed on map (with alpha)
            for (int i = 0; i < nearCampIDs.Length; i++)
            {
                GM.setMapCampAlpha(nearCampIDs[i]);
            }
        }
        else
        {
            GM.setMapLandMark(itemID);
        }
    }

    private int alreadyOnStele()
    {
        alreadyOnStelePieces = 0;

        for (int i = 0; i < noStelePieces; i++)
        {
            if (GM.getOnSteleStatus(i))
            {
                alreadyOnStelePieces++;
            }
        }

        // if all of them -> open the village
        if (alreadyOnStelePieces == noStelePieces)
        {
            GM.openVillage();
        }

        return alreadyOnStelePieces;
    }

    public string getInteractionType()
    {
        return interactions.ToString();
    }

    public int getCampFireID()
    {
        return campFireID;
    }
}