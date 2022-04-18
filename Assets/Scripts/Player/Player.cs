using UnityEngine;

public class Player : MonoBehaviour
{
    private string buildPlatform;

    // game , audio and UI managers
    private GameManager GM;
    private AudioManager AM;
    private UIInGameManager UIInGame;
    private SandStorm sandStorm;

    // UI InGame Pause Manager (welcomeUI has been used)
    private UIWelcomeManager UIWelcomeManager;

    private bool isDay;

    [Header("Container")]
    [SerializeField]
    private GameObject playersContainer;

    // player controllers
    private UnityStandardAssets.Characters.FirstPerson.FirstPersonController FPSController;
    private CharacterController FPSPlayer;
    private OVRPlayerController OVRPlayer;

    private Vector3 lastPosition;

    // speed and jump default values
    private float defaultSpeed = 4f;
    private float runnningSpeed = 20f;
    private float highSpeed = 40f;
    private float jumpForce = 0;

    private float walkingStepInterval = 5f;
    private float runningStepInterval = 10f;

    private float defaultOVRSpeed = 4f;

    // difficulty related
    private float energyMax = 100f;
    private float energyLoss;
    private float energyLossEasy = 600f;
    private float energyLossNormal = 400f;
    private float energyLossHard = 200f;
    private float energyLossNightFactor = 2f;
    private float energyLossInStormy = -0.1f;
    private float enegeyGainInRestZone = 0.5f;

    private string difficultyLevel;

    private float baseRunSpeed;
    private float baseWalkSpeed;

    // average player speed calculation
    private int playerSpeedAverageCount;
    private int playerSpeedAverageLength = 10;
    private float playerSpeedAverage;
    private float playerSpeed;
    private float playerWithDepaRunningTime = 0.0f;
    private bool runningWithDepaTipHasBennSaid = false;

    // oculus footsteps sound
    private Vector3 lastFootstepPosition;

    // particle systems
    [Header("Particle Systems")]
    [SerializeField]
    private ParticleSystem OVRRunParticleSystem;
    [SerializeField]
    private ParticleSystem FPSRunParticleSystem;
    private ParticleSystem runParticleSystem;

    // player stats
    private float playerEnergy;
    private Vector3 playerPosition;
    private bool isInRestZone;

    // sansStorm
    private bool isInStormy = false;

    // audio sources
    [Header("Audio Sources")]
    [SerializeField]
    private AudioSource FPSAudioSource;
    [SerializeField]
    private AudioSource FPSRunAudioSource;
    [SerializeField]
    private AudioSource OVRAudioSource;
    [SerializeField]
    private AudioSource OVRRunAudioSource;
    private AudioSource playerAudioSource;
    private AudioSource runAudioSource;

    // audio sources
    [Header("Audio Clips")]
    [SerializeField]
    private AudioClip footstepOculus;

    private Coroutine runningSpeedFadeIn;
    private Coroutine runningSpeedFadeOut;

    // factor from UI sliders (0 to 5)
    private float slidersFactor = 5.0f;

    private void Start()
    {
        // fing the GameManager
        GameObject gameManagerGameObject = GameObject.Find("GameManager");
        GM = gameManagerGameObject.GetComponent<GameManager>();

        // fing the GameManager
        GameObject audioManagerGameObject = GameObject.Find("AudioManager");
        AM = audioManagerGameObject.GetComponent<AudioManager>();

        // fing the UIInGameManger
        GameObject UIInGameGameObject = GameObject.Find("UI");
        UIInGame = UIInGameGameObject.GetComponentInChildren<UIInGameManager>();

        // find the SandStorm
        GameObject sandStormGameObject = GameObject.Find("SandStorm_ParticleSystem");
        sandStorm = sandStormGameObject.GetComponent<SandStorm>();

        // fing the UIWelcomeManger
        GameObject UIWelcomeManagerGameObject = GameObject.Find("UIPauseManager");
        UIWelcomeManager = UIWelcomeManagerGameObject.GetComponentInChildren<UIWelcomeManager>();

        // get the build platform
        buildPlatform = GM.getBuildPlaform();

        // get player stats from save
        playerEnergy = GM.getPlayerEnergy();
        playerPosition = GM.getPlayerPosition();

        // move the players container to the saved position
        playersContainer.transform.position = playerPosition;

        lastPosition = transform.position;

        if (buildPlatform == "OculusQuest")
        {
            OVRPlayer = GetComponent<OVRPlayerController>();
            runParticleSystem = OVRRunParticleSystem;

            OVRPlayer.JumpForce = jumpForce;
            SetNewSpeed(defaultSpeed);

            baseRunSpeed = defaultSpeed;
            baseWalkSpeed = highSpeed;

            playerAudioSource = OVRAudioSource;
            runAudioSource = OVRRunAudioSource;

            lastFootstepPosition = transform.position;
        }
        else if (buildPlatform == "MacWindows")
        {
            FPSPlayer = GetComponent<CharacterController>();
            FPSController = GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>();
            baseRunSpeed = FPSController.m_RunSpeed;
            baseWalkSpeed = FPSController.m_WalkSpeed;

            runParticleSystem = FPSRunParticleSystem;

            playerAudioSource = FPSAudioSource;
            runAudioSource = FPSRunAudioSource;
        }

        // update player energy on the UI 
        UIInGame.UpdateHP(playerEnergy);

        // set running audio to 0
        runAudioSource.volume = 0.0f;
    }

    private void FixedUpdate()
    {
        // get difficulty level
        difficultyLevel = GM.getDifficulty();

        // get isDay state
        isDay = GM.getIsDay();

        // get player rest zone state
        isInRestZone = GM.getIsPlayerInRestZone();

        // get SFW volume
        float SFXVolume = GM.getSfxVolume() / slidersFactor;

        // set the energy loss factor regarding to the difficulty level
        if (difficultyLevel == "Easy")
        {
            energyLoss = energyLossEasy;
        }
        else if (difficultyLevel == "Normal")
        {
            energyLoss = energyLossNormal;
        }
        // else –> hard
        else
        {
            energyLoss = energyLossHard;
        }

        // if night, player lose more energy
        if (isDay == false)
        {
            energyLoss = energyLoss / energyLossNightFactor;
        }

        // get velocity to calculate magnitude and average speed
        Vector3 playerVelocity = (transform.position - lastPosition) / Time.deltaTime;
        float distance3d = Vector3.Distance(transform.position, lastPosition) / Time.deltaTime;

        // test if value is NaN after pause
        if (float.IsNaN(playerVelocity.x))
        {
            playerVelocity = new Vector3(0.0f, 0.0f, 0.0f);
        }

        //playerSpeed = Mathf.Abs(playerVelocity.magnitude);
        float playerSpeedMagn = playerVelocity.magnitude;
        playerSpeed = distance3d;
        lastPosition = transform.position;

        // average speed
        playerSpeedAverageCount++;

        if (playerSpeedAverageCount > playerSpeedAverageLength)
        {
            playerSpeedAverage = playerSpeedAverage + (playerSpeed - playerSpeedAverage) / (playerSpeedAverageLength + 1);
        }
        else
        {
            playerSpeedAverage += playerSpeed;

            if (playerSpeedAverageCount == playerSpeedAverageLength)
            {
                playerSpeedAverage += playerSpeedAverage / playerSpeedAverageCount;
            }
        }

        // if player is in restzone by night
        if (isInRestZone == true && isDay == false)
        {
            // restore energy
            UpdateEnergy(enegeyGainInRestZone);
        }
        // if player is in restzone by bay
        else if (isInRestZone == true && isDay == true)
        {
            // energy stay still
        }
        // if player move but don't run with the Depa and he's not in the rest zone by night
        else
        {
            if (playerSpeedAverage < 30.0f)
            {
                float lostEnergy = playerSpeedAverage / energyLoss;
                UpdateEnergy(-lostEnergy);
            }
        }

        // if player is in the tempest -> lost energy over time
        isInStormy = sandStorm.getIsInStormy();
        if (isInStormy == true)
        {
            UpdateEnergy(energyLossInStormy);
        }

        // get player local velocity
        Vector3 localVelo = transform.InverseTransformDirection(playerVelocity);

        if (buildPlatform == "OculusQuest")
        {
            // speed management
            if (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) >= 0.2f)
            {
                if (localVelo.z > 1.0f)
                {
                    SetNewSpeed(runnningSpeed);
                }
            }
            else
            {
                SetNewSpeed(defaultSpeed);
            }

            float footstepDistance3d = Vector3.Distance(transform.position, lastFootstepPosition);

            // Oculus footsteps sound
            if (playerSpeedAverage < 6.0f)
            {
                if (footstepDistance3d > 2.0f)
                {
                    StartCoroutine(AM.playAudioClipCoroutine("SFX", 7, footstepOculus, 0.0f));
                    lastFootstepPosition = transform.position;
                }
            }
            else if (playerSpeedAverage < 30.0f)
            {
                if (footstepDistance3d > 8.0f)
                {
                    StartCoroutine(AM.playAudioClipCoroutine("SFX", 7, footstepOculus, 0.0f));
                    lastFootstepPosition = transform.position;
                }
            }
        }
        else
        {
            // if player run forward –> run speed
            // Debug... work with round
            if (localVelo.z > (baseWalkSpeed - 0.1f))
            {
                SetNewSpeed(baseRunSpeed);
                setStepInterval(runningStepInterval);
            }
            // else if he try to run in any other direction –> walk
            else
            {
                SetNewSpeed(baseWalkSpeed);
                setStepInterval(walkingStepInterval);
            }
        }

        // debug... to prevent running audio to play at the start
        if (playerSpeedAverage < 0.1f || Time.timeSinceLevelLoad < 1.0f)
        {
            runAudioSource.volume = 0;
        }

        // start running particle and the running sound if the player go fast
        if (playerSpeedAverage > 25.0f)
        {
            // set player running time
            playerWithDepaRunningTime += Time.deltaTime;

            // if tip has not been played –> chance to play it
            if (runningWithDepaTipHasBennSaid == false)
            {
                // play storm tip
                AM.playEventVoiceOver(0, 5, 1.0f, "runningWithDepaTips");

                runningWithDepaTipHasBennSaid = true;
            }

            RunParticle(true);

            // if running audio is not up
            if (runAudioSource.volume < SFXVolume)
            {
                // fade out foot steps audio source
                StartCoroutine(AM.audioFadeCoroutine(playerAudioSource, SFXVolume, 0.0f, 1.0f));

                // fade in running audio source
                float currentVolume = runAudioSource.volume;
                float fadeDuration = 1.5f * (SFXVolume - currentVolume) / SFXVolume;

                runningSpeedFadeIn = StartCoroutine(AM.audioFadeCoroutine(runAudioSource, currentVolume, SFXVolume, fadeDuration));
            }
        }
        else
        {
            // reset player running time
            playerWithDepaRunningTime = 0.0f;

            RunParticle(false);
            // if running audio is up
            if (runAudioSource.volume > 0.1f)
            {
                // fade in foot steps audio source
                StartCoroutine(AM.audioFadeCoroutine(playerAudioSource, 0.0f, SFXVolume, 2.0f));

                // fade out running audio source
                float currentVolume = runAudioSource.volume;
                float fadeDuration;

                if (currentVolume == SFXVolume)
                {
                    fadeDuration = 1.5f;
                }
                else
                {
                    fadeDuration = 1.5f * (SFXVolume - currentVolume) / SFXVolume;
                }

                runningSpeedFadeOut = StartCoroutine(AM.audioFadeCoroutine(runAudioSource, currentVolume, 0.0f, fadeDuration));
            }
        }

        // if the player has placed all stele pieces –> run always by day
        if (GM.getWithDepa() && isDay == true)
        {
            SetNewSpeed(highSpeed);
        }
    }

    private void RunParticle(bool start)
    {
        if (start == true)
        {
            runParticleSystem.Play();
        }
        else
        {
            runParticleSystem.Stop();
        }
    }

    private void setStepInterval(float interval)
    {
        FPSController.m_StepInterval = interval;
    }

    // public methods
    public void SetNewSpeed(float newSpeed)
    {
        if (buildPlatform == "OculusQuest")
        {
            OVRPlayer.SetMoveScaleMultiplier(newSpeed / defaultOVRSpeed);
        }
        else
        {
            FPSController.m_RunSpeed = newSpeed;
        }
    }

    // set running speed (OVR)
    public void setRunningSpeed(float speed)
    {
        runnningSpeed = speed;
    }

    public void setRunningWithDepaTipHasBennSaid(bool state)
    {
        runningWithDepaTipHasBennSaid = state;
    }

    public void UpdateEnergy(float energy)
    {
        playerEnergy += energy;

        if (playerEnergy > energyMax)
        {
            playerEnergy = energyMax;
        }
        else if (playerEnergy <= 0)
        {
            UIWelcomeManager.showDeathPanel();
            GM.pauseWinOrDeathGame();
        } 

        UIInGame.UpdateHP(Mathf.RoundToInt(playerEnergy));

        // set the ernergy in the save file
        GM.setPlayerEnergy(Mathf.RoundToInt(playerEnergy));
    }

    public float getPlayerZRotation()
    {
        return transform.eulerAngles.y;
    }

    public Vector3 getPlayerPos()
    {
        return transform.position;
    }
}