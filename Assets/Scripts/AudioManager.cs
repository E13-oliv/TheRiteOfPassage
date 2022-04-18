using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    // game managers
    private GameManager GM;

    private bool isDay;
    private float inGameTime;
    private float dawn;
    private float dusk;
    private bool dawnIsComing;
    private bool duskIsComing;
    private bool dawnHasPassed = false;
    private bool duskHasPassed = false;

    private Scene activeScene;
    private string activeSceneName;

    // factor from UI sliders (0 to 5)
    private float slidersFactor = 5.0f;

    [Header("SFX")]
    [SerializeField]
    private AudioSource[] SFXAudioSources;

    [Header("SFX Day Clips")]
    [SerializeField]
    private AudioClip[] SFXDayClips;

    private Coroutine daySounds;
    private bool daySoundsIsRunning = false;
    private float daySoundsMinRythm = 5.0f;
    private float daySoundsMaxRythm = 10.0f;
    private float daySoundsChanceToPlay = 3.0f;

    [Header("SFX Night Clips")]
    [SerializeField]
    private AudioClip[] SFXNightClips;

    private Coroutine nightSounds;
    private bool nightSoundsIsRunning = false;
    private float nightSoundsMinRythm = 5.0f;
    private float nightSoundsMaxRythm = 10.0f;
    private float nightSoundsChanceToPlay = 3.0f;

    [Header("Ambiance")]
    [SerializeField]
    private AudioSource[] ambianceAudioSources;
    private float ambianceVolumeFactor = 0.5f;
    private float actualAmbianceVolumeFactor;

    [Header("VoiceOver")]
    [SerializeField]
    private AudioSource[] voiceOverAudioSources;
    // eventVoiceOverOncePlayed IDs
    //  0 : intro
    //  1 : ...
    [SerializeField]
    private AudioClip[] eventVoiceOverAudioClips;
    private float defaultTipsChanceToPlay = 10.0f;

    [Header("VoiceOverTips")]
    [SerializeField]
    private AudioClip[] depaPopTips;
    private float depaPopTipsChanceToPlay = 10.0f;
    [SerializeField]
    private AudioClip[] stormTips;
    private float stormTipsChanceToPlay = 5.0f;

    [SerializeField]
    private AudioClip[] dawnTips;
    private float dawnTipsChanceToPlay = 1.0f;

    [SerializeField]
    private AudioClip[] duskTips;
    private float duskTipsChanceToPlay = 1.0f;

    [SerializeField]
    private AudioClip[] runningWithDepaTips;
    private float runningWithDepaTipsChanceToPlay = 7.0f;

    [SerializeField]
    private AudioClip[] depaPopOutTips;
    private float depaPopOutTipsChanceToPlay = 7.0f;

    [SerializeField]
    private AudioClip[] villageCloseTips;
    private float villageCloseTipsChanceToPlay = 2.0f;

    [SerializeField]
    private AudioClip[] villageCloserTips;
    private float villageCloserTipsChanceToPlay = 1.0f;

    [SerializeField]
    private AudioClip[] completeSteleTips;
    private float completeSteleTipsChanceToPlay = 1.0f;

    [SerializeField]
    private AudioClip[] nightOutOfCampTips;
    private float nightOutOfCampTipsChanceToPlay = 7.0f;

    [SerializeField]
    private AudioClip[] backPackTips;
    private float backPackTipsChanceToPlay = 7.0f;

    [SerializeField]
    private AudioClip[] easterEgg;
    private float easterEggChanceToPlay = 1.0f;

    [Header("VoiceOverStelePiecesPickUp")]
    [SerializeField]
    private AudioClip[] stelePiecesPickup;
    private float stelePiecesPickupChanceToPlay = 2.0f;

    // volume levels
    private float SFXVolume;
    private float VoicesVolume;

    private void Start()
    {
        // get the GameManager
        GameObject gameManagerGameObject = GameObject.Find("GameManager");
        GM = gameManagerGameObject.GetComponent<GameManager>();

        dawn = GM.getDawn();
        dusk = GM.getDusk();
    }

    private void Update()
    {
        // get the name of the active scene
        activeScene = SceneManager.GetActiveScene();
        activeSceneName = activeScene.name;

        if (activeSceneName == "MifunoDesert")
        {
            isDay = GM.getIsDay();
            inGameTime = GM.getInGameTime();

            // if ambiance factor change
            if (ambianceVolumeFactor != actualAmbianceVolumeFactor)
            {
                float actualVolume = ambianceAudioSources[0].volume;
                float newVolume = ambianceAudioSources[0].volume * ambianceVolumeFactor / actualAmbianceVolumeFactor;

                StartCoroutine(audioFadeCoroutine(ambianceAudioSources[0], actualVolume, newVolume, 3.5f));

                actualAmbianceVolumeFactor = ambianceVolumeFactor;
            }

            if (isDay == true)
            {
                if (daySoundsIsRunning == false)
                {
                    daySounds = StartCoroutine(daySoundsCoroutine());
                    daySoundsIsRunning = true;
                }

                if (nightSoundsIsRunning == true)
                {
                    StopCoroutine(nightSounds);
                    nightSoundsIsRunning = false;
                }
            }
            else
            {
                if (daySoundsIsRunning == true)
                {
                    StopCoroutine(daySounds);
                    daySoundsIsRunning = false;
                }

                if (nightSoundsIsRunning == false)
                {
                    nightSounds = StartCoroutine(nightSoundsCoroutine());
                    nightSoundsIsRunning = true;
                }
            }

            // get volume levels
            SFXVolume = GM.getSfxVolume();
            VoicesVolume = GM.getVoiceOverVolume();
        }

        dawnIsComing = GM.getDawnIsComing();
        duskIsComing = GM.getDuskIsComing();

        // dawm tip
        if (dawnIsComing == true && dawnHasPassed == false)
        {
            // add if temple undiscovered

            // play dusk tip
            playEventVoiceOver(0, 2, 0.0f, "dawnTips");

            dawnHasPassed = true;
            duskHasPassed = false;
        }

        // dusk tip
        if (duskIsComing == true && duskHasPassed == false)
        {
            // play dusk tip
            playEventVoiceOver(0, 3, 0.0f, "duskTips");

            dawnHasPassed = false;
            duskHasPassed = true;
        }
    }

    // public methods
    public void setAudioSourceVolume(string source, float volume)
    {
        if (source == "SFX")
        {
            for (int i = 0; i < SFXAudioSources.Length; i++)
            {
                SFXAudioSources[i].volume = volume / slidersFactor;
            }

            for (int i = 0; i < ambianceAudioSources.Length; i++)
            {
                ambianceAudioSources[i].volume = volume / slidersFactor * ambianceVolumeFactor;
            }

            actualAmbianceVolumeFactor = ambianceVolumeFactor;

            SFXVolume = volume;
        }
        else if (source == "VoiceOver")
        {
            for (int i = 0; i < voiceOverAudioSources.Length; i++)
            {
                voiceOverAudioSources[i].volume = volume / slidersFactor;
            }

            VoicesVolume = volume;
        }
    }

    // eventVoiceOverOncePlayed clipIDs
    //  0 : depaPop
    //  1 : storm
    //  2 : dawn
    //  3 : dusk
    //  4 : stelePiecesPickUp
    //  5 : runningWithDepa
    //  6 : depaPopOut
    //  7 : villageClose
    //  8 : villageCloser
    //  9 : completeStele
    // 10 : nightOutOfCamp
    // 11 : backPack
    // 12 : easterEgg
    // 13 : easterEggAction
    public void playEventVoiceOver(int audioSourceID, int clipID, float delay, string type)
    {
        // if no other voiceOverEnvet is playing
        if (voiceOverAudioSources[audioSourceID].isPlaying == false)
        {
            bool hasBeenPlayed = GM.getEventVoiceOverOncePlayed(clipID);

            // set change to play and clip regarding the type
            float chanceToPlay;
            int clipToPlayID;
            AudioClip clipToPlay;

            switch (type)
            {
                case "depaPopTips":
                    chanceToPlay = depaPopTipsChanceToPlay;
                    clipToPlayID = Random.Range(0, depaPopTips.Length);
                    clipToPlay = depaPopTips[clipToPlayID];
                    break;

                case "stormTips":
                    chanceToPlay = stormTipsChanceToPlay;
                    clipToPlayID = Random.Range(0, stormTips.Length);
                    clipToPlay = stormTips[clipToPlayID];
                    break;

                case "dawnTips":
                    chanceToPlay = dawnTipsChanceToPlay;
                    clipToPlayID = Random.Range(0, dawnTips.Length);
                    clipToPlay = dawnTips[clipToPlayID];
                    break;

                case "duskTips":
                    chanceToPlay = duskTipsChanceToPlay;
                    clipToPlayID = Random.Range(0, duskTips.Length);
                    clipToPlay = duskTips[clipToPlayID];
                    break;

                case "runningWithDepaTips":
                    chanceToPlay = runningWithDepaTipsChanceToPlay;
                    clipToPlayID = Random.Range(0, runningWithDepaTips.Length);
                    clipToPlay = runningWithDepaTips[clipToPlayID];
                    break;

                case "stelePiecesPickUp":
                    bool[] stelePiecesStatus = GM.getStelePiecesStatus();

                    int foundPiecesCount = 0;

                    for (int i = 0; i < stelePiecesStatus.Length; i++)
                    {
                        if (stelePiecesStatus[i] == true)
                            foundPiecesCount++;
                    }

                    int steleID = foundPiecesCount - 1;

                    chanceToPlay = stelePiecesPickupChanceToPlay;
                    clipToPlay = stelePiecesPickup[steleID];
                    break;

                case "depaPopOutTips":
                    chanceToPlay = depaPopOutTipsChanceToPlay;
                    clipToPlayID = Random.Range(0, depaPopOutTips.Length);
                    clipToPlay = depaPopOutTips[clipToPlayID];
                    break;

                case "villageCloseTips":
                    chanceToPlay = villageCloseTipsChanceToPlay;
                    clipToPlayID = Random.Range(0, villageCloseTips.Length);
                    clipToPlay = villageCloseTips[clipToPlayID];
                    break;

                case "villageCloserTips":
                    chanceToPlay = villageCloserTipsChanceToPlay;
                    clipToPlayID = Random.Range(0, villageCloserTips.Length);
                    clipToPlay = villageCloserTips[clipToPlayID];
                    break;

                case "completeSteleTips":
                    chanceToPlay = completeSteleTipsChanceToPlay;
                    clipToPlayID = Random.Range(0, completeSteleTips.Length);
                    clipToPlay = completeSteleTips[clipToPlayID];
                    break;

                case "nightOutOfCampTips":
                    chanceToPlay = nightOutOfCampTipsChanceToPlay;
                    clipToPlayID = Random.Range(0, nightOutOfCampTips.Length);
                    clipToPlay = nightOutOfCampTips[clipToPlayID];
                    break;

                case "backPackTips":
                    chanceToPlay = backPackTipsChanceToPlay;
                    clipToPlayID = Random.Range(0, backPackTips.Length);
                    clipToPlay = backPackTips[clipToPlayID];
                    break;

                case "easterEgg":
                    chanceToPlay = easterEggChanceToPlay;
                    clipToPlayID = Random.Range(0, easterEgg.Length);
                    clipToPlay = easterEgg[clipToPlayID];
                    break;

                default:
                    chanceToPlay = defaultTipsChanceToPlay;
                    clipToPlay = null;
                    break;
            }

            int chance = Mathf.CeilToInt(Random.Range(0, chanceToPlay));

            // if clip has never been played
            if (hasBeenPlayed == false)
            {
                StartCoroutine(playAudioClipCoroutine("VoiceOver", audioSourceID, clipToPlay, delay));

                GM.setEventVoiceOverOncePlayed(clipID);
            }
            else
            {
                if (chance == chanceToPlay)
                {
                    StartCoroutine(playAudioClipCoroutine("VoiceOver", audioSourceID, clipToPlay, delay));
                }
            }
        }
    }

    // night sound play
    private IEnumerator daySoundsCoroutine()
    {
        bool dayKeepPlaying = true;

        while (dayKeepPlaying)
        {
            float rythm = Random.Range(daySoundsMinRythm, daySoundsMaxRythm);

            int chance = Mathf.CeilToInt(Random.Range(0, daySoundsChanceToPlay));

            if (chance == daySoundsChanceToPlay)
            {
                int clipID = Random.Range(0, SFXDayClips.Length);

                SFXAudioSources[1].clip = SFXDayClips[clipID];
                SFXAudioSources[1].Play();
            }

            yield return new WaitForSeconds(rythm);
        }
    }

    // night sound play
    private IEnumerator nightSoundsCoroutine()
    {
        bool nightKeepPlaying = true;

        while (nightKeepPlaying)
        {
            float rythm = Random.Range(nightSoundsMinRythm, nightSoundsMaxRythm);

            int chance = Mathf.CeilToInt(Random.Range(0, nightSoundsChanceToPlay));

            if (chance == nightSoundsChanceToPlay)
            {
                int clipID = Random.Range(0, SFXNightClips.Length);

                SFXAudioSources[1].clip = SFXNightClips[clipID];
                SFXAudioSources[1].Play();
            }

            yield return new WaitForSeconds(rythm);
        }
    }

    // audio clip play
    public IEnumerator playAudioClipCoroutine(string source, int sourceID, AudioClip clip, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (source == "VoiceOver")
        {
            voiceOverAudioSources[sourceID].clip = clip;
            voiceOverAudioSources[sourceID].Play();
        }
        else if (source == "SFX")
        {
            SFXAudioSources[sourceID].clip = clip;
            SFXAudioSources[sourceID].Play();
        }
    }

    // audio play and volume fade
    public IEnumerator audioFadeCoroutine(AudioSource audioSource, float startVolume, float endVolume, float fadeDuration)
    {
        float counter = 0.0f;

        while (counter < fadeDuration)
        {
            counter += Time.deltaTime;

            audioSource.volume = Mathf.Lerp(startVolume, endVolume, counter / fadeDuration);

            yield return null;
        }
    }

    // audio play and fade in
    public IEnumerator audioFadeInCoroutine(AudioSource audioSource, float fadeDuration)
    {
        audioSource.Play();

        while (audioSource.volume < 1)
        {
            audioSource.volume += Time.deltaTime / fadeDuration;

            yield return null;
        }
    }

    // audio fade out and stop
    public IEnumerator audioFadeOutCoroutine(AudioSource audioSource, float fadeDuration)
    {
        while (audioSource.volume > 0)
        {
            audioSource.volume -= Time.deltaTime / fadeDuration;

            yield return null;
        }

        yield return new WaitForSeconds(fadeDuration);
        audioSource.Stop();
    }

    public void setAmbianceVolumeFactor(float factor)
    {
        ambianceVolumeFactor = factor;
    }
}