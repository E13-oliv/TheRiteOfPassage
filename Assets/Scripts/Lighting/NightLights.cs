using System.Collections;
using UnityEngine;

public class NightLights : MonoBehaviour
{
    // game and audio manager
    private GameManager GM;
    private AudioManager AM;

    // light types
    enum LightTypes
    {
        fireCamp = 0,
        glowingRock = 1
    };
    [Header("Light Type")]
    [SerializeField]
    private LightTypes lightTypes;
    private string lightType;

    [Header("LightSource")]
    [SerializeField]
    private float lightIntensityFloat = 2.5f;
    private float lightIntensity;
    [SerializeField]
    private Light thisLight;
    private Vector3 lightPos;
    private Vector3 newLightPos;
    private float lightMovement = .05f;

    private float startTime;
    private float endTime;
    private bool isDay;

    private float lightingDuration = 1f;

    private float inGameHour;

    private bool lightOn = false;
    private bool lightCrakled = false;
    private Coroutine lightCrakling;

    private Coroutine glowVariations;

    [Header("Fire Particule Systems")]
    [SerializeField]
    private GameObject fireParticle;
    [SerializeField]
    private GameObject smokeParticle;

    [Header("Fire AudioSources")]
    [SerializeField]
    private AudioSource fireCraklingAudioSource;

    private int campFireID = 998;
    private bool lightStatus;

    private void Awake()
    {
        // fing the GameManager
        GameObject gameManagerGameObject = GameObject.Find("GameManager");
        GM = gameManagerGameObject.GetComponent<GameManager>();

        // fing the AudioManager
        GameObject audioManagerGameObject = GameObject.Find("AudioManager");
        AM = audioManagerGameObject.GetComponent<AudioManager>();

        float dawn = GM.getDawn();
        float dusk = GM.getDusk();
        float lightsBeforeDusk = GM.getLightsBeforeDuskAfterDawn();

        startTime = dusk - lightsBeforeDusk;
        endTime = dawn + lightsBeforeDusk;

        // get light type
        lightType = lightTypes.ToString();

        lightIntensity = lightIntensityFloat;

        // get light position
        lightPos.x = thisLight.transform.localPosition.x;
        lightPos.y = thisLight.transform.localPosition.y;
        lightPos.z = thisLight.transform.localPosition.z;
    }

    private void Start()
    {
        isDay = GM.getIsDay();

        // if it is a camp fire
        if (lightType == "fireCamp")
        {
            campFireID = GetComponent<InteractiveItems>().getCampFireID();

            checkCampFire();
        }
    }

    private void Update()
    {
        // get in game hour
        inGameHour = GM.getInGameTime();
        isDay = GM.getIsDay();

        // if day
        if (inGameHour > endTime && inGameHour < startTime)
        {
            if (lightOn == true)
            {
                StartCoroutine(lightTurOffCoroutine(lightingDuration));
                lightOn = false;
            }
        }
        // else if night and light off
        else if (lightOn == false && lightStatus == true)
        {
            StartCoroutine(lightTurnOnCoroutine(lightingDuration));
            lightOn = true;
            if (lightType == "fireCamp")
            {
                lightCrakling = StartCoroutine(lightCracklingCoroutine());
            }
            else if (lightType == "glowingRock")
            {
                glowVariations = StartCoroutine(glowVariationsCoroutine());
            }
        }

        checkCampFire();
    }

    // check if camp fire and if it is prepared
    private void checkCampFire()
    {
        // 999 = landmarks fire that are always light up
        if (campFireID == 999 || lightType == "glowingRock")
        {
            lightStatus = true;
        }
        else
        {
            lightStatus = GM.getFireStatus(campFireID);
        }

        if (lightStatus == true && lightType == "fireCamp")
        {
            if (isDay == true)
            {
                smokeParticle.SetActive(true);
            }
            else
            {
                fireParticle.SetActive(true);
                smokeParticle.SetActive(false);
            }
        }
    }

    // light fade in
    private IEnumerator lightTurnOnCoroutine(float duration)
    {
        if (lightType == "fireCamp")
        {
            fireParticle.SetActive(true);
            smokeParticle.SetActive(false);

            StartCoroutine(AM.audioFadeInCoroutine(fireCraklingAudioSource, lightingDuration));
        }

        float counter = 0.0f;

        while (counter < duration)
        {
            counter += Time.deltaTime;

            thisLight.GetComponent<Light>().intensity = Mathf.Lerp(0, lightIntensity, counter / duration);

            yield return null;
        }
    }

    // light fade out
    private IEnumerator lightTurOffCoroutine(float duration)
    {

        if (lightType == "fireCamp")
        {
            fireParticle.SetActive(false);
            checkCampFire();
            StartCoroutine(AM.audioFadeOutCoroutine(fireCraklingAudioSource, lightingDuration));
        }

        float counter = 0.0f;

        while (counter < duration)
        {
            counter += Time.deltaTime;

            thisLight.GetComponent<Light>().intensity = Mathf.Lerp(lightIntensity, 0, counter / duration);

            yield return null;
        }

        if (lightType == "fireCamp")
        {
            yield return new WaitForSeconds(lightingDuration);
            StopCoroutine(lightCrakling);
        }
        else if (lightType == "glowingRock")
        {
            StopCoroutine(glowVariations); 
        }
    }

    // flame crackling (movements)
    private IEnumerator lightCracklingCoroutine()
    {
        yield return new WaitForSeconds(lightingDuration);

        for (int i = 1; i > 0; i++)
        {
            yield return new WaitForSeconds(0.1f);

            if (lightCrakled == false)
            {
                newLightPos.x = Random.Range(-lightMovement, lightMovement);
                newLightPos.y = Random.Range(-lightMovement, lightMovement);
                newLightPos.z = Random.Range(-lightMovement, lightMovement);
                thisLight.transform.Translate(newLightPos, Space.Self);
                lightCrakled = true;
            } else
            {
                thisLight.transform.Translate(-newLightPos, Space.Self);
                lightCrakled = false;
            }
        }
    }

    private IEnumerator glowVariationsCoroutine()
    {
        yield return new WaitForSeconds(lightingDuration);

        float lowIntensity = lightIntensity - 0.025f;
        float highIntensity = lightIntensity + 0.025f;

        for (int i = 1; i > 0; i++)
        {
            yield return new WaitForSeconds(0.1f);

            thisLight.GetComponent<Light>().intensity = Random.Range(lowIntensity, highIntensity);
        }
    }
}