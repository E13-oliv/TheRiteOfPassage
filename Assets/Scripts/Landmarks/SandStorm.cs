using System.Collections;
using UnityEngine;

public class SandStorm : MonoBehaviour
{
    // game and audio managers
    private GameManager GM;
    private AudioManager AM;

    private Player player;
    private HideTiles hideTiles;
    private Terrain activeTerrain;
    private TerrainAmbiance terrainAmbiance;

    private float inGameTime;
    private float inGameDayDuration;
    private bool isDay;
    private bool dawnIsComing;
    private bool duskIsComing;
    private float dawnDuskDuration;

    // sandstorm particle system
    private ParticleSystem sandStormParticleSystem;
    private ParticleSystem.ShapeModule shape;
    private ParticleSystem.ColorOverLifetimeModule sandStormColor;
    private ParticleSystem.ColorOverLifetimeModule stormySandStormColor;
    //second particle system for stormy sandstorms
    [Header("Stormy ParticleSystem")]
    [SerializeField]
    private GameObject stormySandStorm;
    private ParticleSystem stormySandStormParticleSystem;

    // particle system rotation management
    private float particleShapeBaseAngle = 90.0f;
    private Vector3 particleShapeAngle = new Vector3(0.0f, 0.0f, 0.0f);

    // default color (color over time)
    private Color dayColorStart = new Color(1.0f, 0.68f, 0.0f, 1.0f);
    private Color dayColorEnd = new Color(1.0f, 0.73f, 0.0f, 1.0f);
    private Color nightColorStart = new Color(0.32f, 0.2285f, 0.0f, 1.0f);
    private Color nightColorEnd = new Color(0.16f, 0.11f, 0.0f, 1.0f);

    private bool isColorChanging = false;

    // default alpha gradient values (color overt time)
    private GradientAlphaKey gradientAlphaKey01 = new GradientAlphaKey(0.0f, 0.0f);
    private GradientAlphaKey gradientAlphaKey02 = new GradientAlphaKey(1.0f, 0.129f);
    private GradientAlphaKey stormyGradientAlphaKey02 = new GradientAlphaKey(0.53f, 0.129f);
    private GradientAlphaKey gradientAlphaKey03 = new GradientAlphaKey(0.0f, 0.0f);
    private GradientAlphaKey[] gradientAlphaKeys;
    private GradientAlphaKey[] stormyGradientAlphaKeys;

    // storm intensity
    [Header("Stome Level")]
    // default particle number
    private float actualParticle = 250.0f;
    private float particleMin;
    private float particleMax;
    private float quiteParticleMin = 50.0f;
    private float quiteParticleMax = 150.0f;
    private float normalParticleMin = 150.0f;
    private float normalParticleMax = 225.0f;
    private float stormyParticleMin = 350.0f;
    private float stormyParticleMax = 500.0f;
    // for the stormy particleSytem
    private float stormyStormyMax = 400.0f;

    private string stormLevel;
    private string actuelStormLevel;
    private bool isStormyParticleActive = false;

    private float stormLevelChangeDuration = 4.0f;

    // audio management
    private float quietVolumeFactor = 0.25f;
    private float normalVolumeFactor = 0.5f;
    private float stormyVolumeFactor = 1.0f;

    private void Start()
    {
        // get the GameManager
        GameObject gameManagerGameObject = GameObject.Find("GameManager");
        GM = gameManagerGameObject.GetComponent<GameManager>();

        // fing the GameManager
        GameObject audioManagerGameObject = GameObject.Find("AudioManager");
        AM = audioManagerGameObject.GetComponent<AudioManager>();

        inGameTime = GM.getInGameTime();
        inGameDayDuration = GM.getInGameDayDuration();
        dawnDuskDuration = GM.getDawnDuskDuration();
        isDay = GM.getIsDay();

        // get the Player GameObject
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        player = playerGO.GetComponent<Player>();
        hideTiles = playerGO.GetComponent<HideTiles>();

        sandStormParticleSystem = GetComponent<ParticleSystem>();
        stormySandStormParticleSystem = stormySandStorm.GetComponent<ParticleSystem>();

        // storm color management
        sandStormColor = sandStormParticleSystem.colorOverLifetime;
        stormySandStormColor = stormySandStormParticleSystem.colorOverLifetime;

        // alpha gradient for color overtime
        gradientAlphaKeys = new GradientAlphaKey[] { gradientAlphaKey01, gradientAlphaKey02, gradientAlphaKey03 };
        stormyGradientAlphaKeys = new GradientAlphaKey[] { gradientAlphaKey01, stormyGradientAlphaKey02, gradientAlphaKey03 };

        if (isDay == true)
        {
            setDayOrNightSandStorm(false, 0.01f);
        }
        else
        {
            setDayOrNightSandStorm(true, 0.01f);
        }

        if (GM.getBuildPlaform() == "MacWindows")
        {
            // storm level management
            activeTerrain = hideTiles.GetPlayerActiveTerrain();
            terrainAmbiance = activeTerrain.GetComponent<TerrainAmbiance>();

            stormLevel = terrainAmbiance.getStormLevel();
            changeStormLevel(stormLevel, 0.01f);

            actuelStormLevel = stormLevel;
        }
    }

    private void Update()
    {
        // storm shape and rotation management
        float playerAngle = player.getPlayerZRotation();
        float newAngle = particleShapeBaseAngle - playerAngle;

        particleShapeAngle.y = newAngle;

        // storm shape and rotation management
        shape = sandStormParticleSystem.shape;

        shape.rotation = particleShapeAngle;

        // storm color management
        inGameTime = GM.getInGameTime();
        dawnIsComing = GM.getDawnIsComing();
        duskIsComing = GM.getDuskIsComing();

        // if dawn is coming change to day colors
        if (dawnIsComing && isColorChanging == false)
        {
            setDayOrNightSandStorm(false, dawnDuskDuration);
        }

        // if dusk is coming change to night colors
        if (duskIsComing && isColorChanging == false)
        {
            setDayOrNightSandStorm(true, dawnDuskDuration);
        }

        // storm level management
        activeTerrain = hideTiles.GetPlayerActiveTerrain();
        terrainAmbiance = activeTerrain.GetComponent<TerrainAmbiance>();

        stormLevel = terrainAmbiance.getStormLevel();

        // if we get on a terrain that have a different level of storm
        if (actuelStormLevel != stormLevel)
        {
            actuelStormLevel = stormLevel;
            changeStormLevel(stormLevel, stormLevelChangeDuration);
        }
    }

    // change the color of the storm particule
    private void setDayOrNightSandStorm(bool toNight, float duration)
    {
        if (toNight == true)
        {
            // from day to night
            StartCoroutine(setDayOrNightSandStormCoroutine(dayColorStart, nightColorStart, dayColorEnd, nightColorEnd, duration));
        }
        else
        {
            // from night to day
            StartCoroutine(setDayOrNightSandStormCoroutine(nightColorStart, dayColorStart, nightColorEnd, dayColorEnd, duration));
        }
    }

    // change the color of the storm particule CoRoutine
    private IEnumerator setDayOrNightSandStormCoroutine(Color startColorStart, Color startColorEnd, Color endColorStart, Color endColorEnd, float duration)
    {
        float counter = 0.0f;
        isColorChanging = true;

        GradientColorKey[] newGradient;

        while (counter < duration)
        {
            counter += Time.deltaTime;

            Color startColor = Color.Lerp(startColorStart, startColorEnd, counter / duration);
            Color endColor = Color.Lerp(endColorStart, endColorEnd, counter / duration); ;

            newGradient = new GradientColorKey[] { new GradientColorKey(startColor, 0.0f), new GradientColorKey(endColor, 1.0f) };

            // set new color and alpha gradient
            Gradient grad = new Gradient();
            grad.SetKeys(newGradient, gradientAlphaKeys);
            sandStormColor.color = grad;
            grad.SetKeys(newGradient, stormyGradientAlphaKeys);
            stormySandStormColor.color = grad;

            yield return null;
        }

        yield return new WaitForSeconds(duration);
        isColorChanging = false;
    }

    // change the storm level (intensity of particule) and add second particule system for stormy sandStorm
    private void changeStormLevel(string level, float duration)
    {
        if (level == "Stormy")
        {
            particleMin = stormyParticleMin;
            particleMax = stormyParticleMax;

            // if previous was not stormy
            if (isStormyParticleActive == false)
            {
                // activate stormy
                StartCoroutine(stormyStormLevelCoroutine(true));
            }

            // set ambiance audio volume
            AM.setAmbianceVolumeFactor(stormyVolumeFactor);

            // play storm tip
            AM.playEventVoiceOver(0, 1, 0.5f, "stormTips");
        }
        else
        {
            if (level == "Quite")
            {
                particleMin = quiteParticleMin;
                particleMax = quiteParticleMax;

                // set ambiance audio volume
                AM.setAmbianceVolumeFactor(quietVolumeFactor);
            }
            else if (level == "Normal")
            {
                particleMin = normalParticleMin;
                particleMax = normalParticleMax;

                // set ambiance audio volume
                AM.setAmbianceVolumeFactor(normalVolumeFactor);
            }

            // if previous was stormy
            if (isStormyParticleActive == true)
            {
                // deactivate stormy
                StartCoroutine(stormyStormLevelCoroutine(false));
            }
        }

        float newParticle = Random.Range(particleMin, particleMax);

        StartCoroutine(changeStormLevelCoroutine(actualParticle, newParticle, duration));

        actualParticle = newParticle;
    }

    // change the storm level (intensity of particule) CoRoutine
    private IEnumerator changeStormLevelCoroutine(float startParticle, float endParticle, float duration)
    {
        float counter = 0.0f;

        var sandStormParticleSystemMain = sandStormParticleSystem.main;

        while (counter < duration)
        {
            counter += Time.deltaTime;

            sandStormParticleSystemMain.maxParticles = (int)Mathf.Lerp(startParticle, endParticle, counter / duration);

            yield return null;
        }
    }

    // activate second storm particle system CoRoutine
    private IEnumerator stormyStormLevelCoroutine(bool start)
    {
        float counter = 0.0f;
        float duration = stormLevelChangeDuration;

        float startParticle;
        float endParticle;

        if (start == true)
        {
            startParticle = 0.0f;
            endParticle = stormyStormyMax;

            stormySandStormParticleSystem.Play();
            isStormyParticleActive = true;
        }
        else
        {
            startParticle = stormyStormyMax;
            endParticle = 0.0f;

            stormySandStormParticleSystem.Stop();
            isStormyParticleActive = false;
        }

        var stormySandStormParticleSystemMain = stormySandStormParticleSystem.main;

        while (counter < duration)
        {
            counter += Time.deltaTime;

            stormySandStormParticleSystemMain.maxParticles = (int)Mathf.Lerp(startParticle, endParticle, counter / duration);

            yield return null;
        }
    }

    public bool getIsInStormy()
    {
        return isStormyParticleActive;
    }
}