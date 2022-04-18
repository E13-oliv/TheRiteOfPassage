using System.Collections;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Header("Base SkyBox")]
    [SerializeField]
    private Material desertSkybox;

    [Header("Desert bounds elements")]
    [SerializeField]
    private Material boundsCloudsMat;

    [SerializeField]
    private GameObject[] desertBound;

    [SerializeField]
    private GameObject[] desertBoundSprites;

    [Header("Sky elements")]
    [SerializeField]
    private Light sun;
    [SerializeField]
    private Light moon;
    [SerializeField]
    private ParticleSystem stars;

    private GameManager GM;
    private float inGameHourDuration;
    private float inGameDayDuration;
    private float inGameTime;
    private float startPosition;
    private bool isDay;
    private bool isDayLastFrame;
    private float dayLength;
    private float dawn;
    private float dusk;
    private float lightsBeforeDuskAfterDawn;
    private float duskLightChange;
    private float dawnLightChange;
    private float dawnDuskDuration;

    // skyboxes colors and attributes
    private Color daySunColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    private float daySunStrenght = 2.0f;
    private Color dayAtmosphereColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    private Color dayFogColor = new Color(1.0f, 0.8275f, 0.0f, 1.0f);
    private Color nightSunColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    private float nightSunStrenght = 10.0f;
    private Color nightAtmosphereColor = new Color(0.0f, 0.05f, 0.22f, 1.0f);
    private Color nightFogColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);

    // not needed at the moment
    //private Color dayGroundColor = new Color(1.0f, 0.85f, 0.0f, 1.0f);
    //private float dayHdrExposure = 1.0f;
    //private Color nightGroundColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);
    //private float nightHdrExposure = 0.7f;

    // to prevent to change at the same time
    private bool changeSkybox = false;

    // defaut sun rotation degrees
    private float degPerHour = 1.0f;

    void Start()
    {
        // fing the GameManager
        GameObject gameManagerGameObject = GameObject.Find("GameManager");
        GM = gameManagerGameObject.GetComponent<GameManager>();

        // get GM values
        isDay = GM.getIsDay();
        inGameHourDuration = GM.getInGameHourDuration();
        inGameDayDuration = GM.getInGameDayDuration();
        inGameTime = GM.getInGameTime();

        startPosition = GM.getSunStartPosition();
        dayLength = GM.getDayLength();
        dawn = GM.getDawn();
        dusk = GM.getDusk();
        lightsBeforeDuskAfterDawn = GM.getLightsBeforeDuskAfterDawn();

        // set when dawn and dusk happend
        duskLightChange = dusk - lightsBeforeDuskAfterDawn;
        dawnLightChange = dawn - lightsBeforeDuskAfterDawn;

        dawnDuskDuration = lightsBeforeDuskAfterDawn * 2 * inGameDayDuration;

        // set opposite virtual last frame to force change on start
        isDayLastFrame = !isDay;

        //if (isDay == true)
        if (inGameTime > dawn && inGameTime < dusk - lightsBeforeDuskAfterDawn)
        {
            // start new day
            RenderSettings.sun = sun;
            StartCoroutine(updateSkyboxCoroutine(desertSkybox, nightSunColor, daySunColor, nightSunStrenght, daySunStrenght, nightAtmosphereColor, dayAtmosphereColor, nightFogColor, dayFogColor, 0.01f));
            StartCoroutine(updateBoundsCloudsSpritesCoroutine(false, 0.01f));
        }
        else
        {
            // start new night
            RenderSettings.sun = moon;
            StartCoroutine(updateSkyboxCoroutine(desertSkybox, daySunColor, nightSunColor, daySunStrenght, nightSunStrenght, dayAtmosphereColor, nightAtmosphereColor, dayFogColor, nightFogColor, 0.01f));
            StartCoroutine(updateStarsCoroutine(true));
            StartCoroutine(updateBoundsCloudsSpritesCoroutine(true, 0.01f));
        }

        // sun start position
        // 90.0f is midnight
        float sunStartAngle = 360 - startPosition * 360 - 90;
        // get sun rotation (convert to euler)
        Vector3 sunRotation = sun.transform.rotation.eulerAngles;
        sunRotation.x = sunStartAngle;
        sun.transform.rotation = Quaternion.Euler(sunRotation);

        // moon start position
        // -90.0f is midnight
        float moonStartAngle = 180 - startPosition * 360 - 90;
        // get moon rotation (convert to euler)
        Vector3 moonRotation = moon.transform.rotation.eulerAngles;
        moonRotation.x = moonStartAngle;
        moon.transform.rotation = Quaternion.Euler(moonRotation);
    }

    void Update()
    {
        inGameTime = GM.getInGameTime();
        isDay = GM.getIsDay();

        // update some time information due to the night speed change option
        inGameDayDuration = GM.getInGameDayDuration();
        dawnDuskDuration = lightsBeforeDuskAfterDawn * 2.0f * inGameDayDuration;
        inGameHourDuration = GM.getInGameHourDuration();

        float angle = 0f;

        // if it is the day
        if (isDay == true)
        {
            // set sun rotation angle
            degPerHour = 180 / dayLength / 24;

            // if it was night before
            if (isDayLastFrame == false)
            {
                // new day
                isDayLastFrame = true;
            }

            if (inGameTime > duskLightChange && changeSkybox == false)
            {
                // new dusk
                StartCoroutine(updateSkyboxCoroutine(desertSkybox, daySunColor, nightSunColor, daySunStrenght, nightSunStrenght, dayAtmosphereColor, nightAtmosphereColor, dayFogColor, nightFogColor, dawnDuskDuration));
                StartCoroutine(updateStarsCoroutine(true));
                StartCoroutine(updateBoundsCloudsSpritesCoroutine(true, dawnDuskDuration));
            }
        }
        // else if it is night
        else if (isDay == false)
        {
            // set moon rotation angle
            degPerHour = 180 / (1 - dayLength) / 24;

            // if it was day before
            if (isDayLastFrame == true)
            {
                // new night
                isDayLastFrame = false;
            }

            // inGameTime < 0.5 : to prevent dawn to happend before sunrise
            if (inGameTime > dawnLightChange && inGameTime < 0.5 && changeSkybox == false)
            {
                // new dawn
                StartCoroutine(updateSkyboxCoroutine(desertSkybox, nightSunColor, daySunColor, nightSunStrenght, daySunStrenght, nightAtmosphereColor, dayAtmosphereColor, nightFogColor, dayFogColor, dawnDuskDuration));
                StartCoroutine(updateStarsCoroutine(false));
                StartCoroutine(updateBoundsCloudsSpritesCoroutine(false, dawnDuskDuration));
            }
        }

        float deltaFactor = Time.deltaTime / inGameHourDuration;
        angle = degPerHour * deltaFactor;

        // sun and moon rotation
        sun.transform.RotateAround(Vector3.zero, Vector3.forward, angle);
        moon.transform.RotateAround(Vector3.zero, Vector3.forward, angle);


        // micro-settings to improve dawn and dusk
        if (inGameTime > duskLightChange + 0.08)
        {
            RenderSettings.sun = moon;
        }
        else if (inGameTime > dawnLightChange + 0.075)
        {
            RenderSettings.sun = sun;
        }
    }

    private IEnumerator updateSkyboxCoroutine(Material skybox, Color sunStartColor, Color sunEndColor, float sunStartStrength, float sunEndStrength, Color atmStartColor, Color atmEndColor, Color fogStartColor, Color fogEndColor, float duration)
    {
        float counter = 0.0f;
        changeSkybox = true;

        while (counter < duration)
        {
            counter += Time.deltaTime;

            skybox.SetColor("_Color", Color.Lerp(atmStartColor, atmEndColor, counter / duration));
            skybox.SetColor("_SunTint", Color.Lerp(sunStartColor, sunEndColor, counter / duration));
            skybox.SetFloat("_SunStrength", Mathf.Lerp(sunStartStrength, sunEndStrength, counter / duration));
            RenderSettings.fogColor = Color.Lerp(fogStartColor, fogEndColor, counter / duration);

            yield return null;
        }

        yield return new WaitForSeconds(duration);
        changeSkybox = false;
    }

    // start or stop stars particule system
    private IEnumerator updateStarsCoroutine(bool lightTheStars)
    {
        if (lightTheStars == true)
        {
            stars.Play();
        } else
        {
            stars.Stop();
        }
        yield return null;
    }

    // update bound clouds sprites
    private IEnumerator updateBoundsCloudsSpritesCoroutine(bool toNight, float duration)
    {
        float counter = 0.0f;

        float startAlpha;
        float endAlpha;
        float newAlpha;
        Color spriteColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);

        if (toNight == true)
        {
            startAlpha = 1.0f;
            endAlpha = 0.0f;
        }
        else
        {
            startAlpha = 0.0f;
            endAlpha = 1.0f;
        }

        while (counter < duration)
        {
            counter += Time.deltaTime;

            newAlpha = Mathf.Lerp(startAlpha, endAlpha, counter / duration);

            spriteColor.a = newAlpha;

            for (int i = 0; i < desertBoundSprites.Length; i++)
            {
                desertBoundSprites[i].GetComponent<SpriteRenderer>().color = spriteColor;
            }

            yield return null;
        }
    }
}