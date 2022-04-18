using System.Collections;
using UnityEngine;

public class Depa : MonoBehaviour
{
    // game manager
    private GameManager GM;
    private AudioManager AM;

    [Header("Depa objects")]
    [SerializeField]
    private GameObject depaPrefab;
    [SerializeField]
    private Animator depaAnimator;
    [SerializeField]
    private GameObject depaWaiting;
    [SerializeField]
    private GameObject depaRunning;

    [Header("Particle Systems")]
    [SerializeField]
    private GameObject depaSandTrail;
    [SerializeField]
    private ParticleSystem depaPopParticleSystem;

    [Header("Audio Sources")]
    [SerializeField]
    private AudioSource depaAmbianceAudioSource;
    [SerializeField]
    private AudioSource depaPopAudioSource;

    private float colliderSize;

    private GameObject playerGO;
    private CharacterController player;

    private bool isPlayerClose = false;

    // terrain height management
    private float groundHeight;
    private float heightFromGround = 0.2f;
    private Terrain activeTerrain;

    private bool isDay;

    // path management
    private GameObject runningPath;
    private GameObject[] runningPoints;
    private int startPoint;
    private int currentPoint;
    private bool isDepaOnAPoint;
    private GameObject currentTarget;
    private int endPoint;
    private int nextPoint = 0;
    private GameObject nextTarget;
    private int numOfTargets;
    private float onPathSpeed = 0;
    private bool onPathForward = true;

    private void Start()
    {
        // fing the GameManager
        GameObject gameManagerGameObject = GameObject.Find("GameManager");
        GM = gameManagerGameObject.GetComponent<GameManager>();

        // fing the GameManager
        GameObject audioManagerGameObject = GameObject.Find("AudioManager");
        AM = audioManagerGameObject.GetComponent<AudioManager>();

        playerGO = GameObject.FindGameObjectWithTag("Player");
        activeTerrain = playerGO.GetComponent<HideTiles>().GetDepaActiveTerrain();

        if (runningPath != null)
        {
            newPath(runningPath);
        }

        colliderSize = this.GetComponent<SphereCollider>().radius;
    }

    private void FixedUpdate()
    {
        // get ground height
        activeTerrain = playerGO.GetComponent<HideTiles>().GetDepaActiveTerrain();
        groundHeight = activeTerrain.SampleHeight(transform.position);

        isDay = GM.getIsDay();

        // if it's night the Depa pop out
        if (isDay == false)
        {
            // if the Depa is active –> deactive him
            if (depaPrefab.activeSelf == true)
            {
                DepaPopOut("night");
            }
        }
        // else it's day
        else
        {
            // if player is in the shere collider (trigger)
            if (isPlayerClose == true)
            {
                DepaGOLookAt();
                depaLookAt(nextTarget);

                float playerDistance = Vector3.Distance(playerGO.transform.position, transform.position);

                Vector3 localVelo = transform.InverseTransformDirection(player.velocity);
                player.GetComponent<Player>().SetNewSpeed(40.0f);
                player.GetComponent<Player>().setRunningSpeed(42.0f);

                // if the player get to far
                if (playerDistance / colliderSize > 0.9f)
                {
                    onPathSpeed = 0.0f;
                    depaSandTrail.SetActive(false);
                }
                else
                {
                    depaAnimator.SetBool("isRunning", true);

                    depaRunning.SetActive(true);
                    depaWaiting.SetActive(false);

                    depaSandTrail.SetActive(true);

                    if (playerDistance / colliderSize > 0.6f)
                    {
                        onPathSpeed = 38.0f;
                    }
                    else if (playerDistance / colliderSize > 0.4f)
                    {
                        onPathSpeed = 42.0f;
                    }
                    else
                    {
                        onPathSpeed = 55.0f;
                    }
                }
            }
            // else the player is far
            else
            {
                depaAnimator.SetBool("isRunning", false);

                depaRunning.SetActive(false);
                depaWaiting.SetActive(true);

                depaLookAt(playerGO);
            }

            if (runningPath != null)
            {
                // distance between the Dépa and the next point
                float depaNextTargetDistance = Vector3.Distance(transform.position, nextTarget.transform.position);
                // distance between the Dépa and the next point
                float depaCurrentTargetDistance = Vector3.Distance(transform.position, currentTarget.transform.position);

                if (depaCurrentTargetDistance <= 1.0f || depaNextTargetDistance <= 1.0f)
                {
                    isDepaOnAPoint = true;
                }
                else
                {
                    isDepaOnAPoint = false;
                }

                // if the Dépa is very close from the target –> aim to the next
                if (depaNextTargetDistance <= 1.0f)
                {
                    // if it's the last target point
                    if (nextPoint == endPoint)
                    {
                        // remove the path
                        runningPath = null;

                        // pop out the Dépa
                        DepaPopOut("destination");
                    }
                    else
                    {
                        // set the current point
                        currentPoint = nextPoint;

                        // set the next target and rotate the Dépa making him facing it
                        SetNextPoint();
                    }
                }

                // adapt y pos with ground height
                Vector3 terrainHeightV3 = depaPrefab.transform.up;
                terrainHeightV3.y = (depaPrefab.transform.position.y - groundHeight - heightFromGround) * -1;
                // have to set them to 0 if not the depaPrefab was going backward...
                terrainHeightV3.x = 0;
                terrainHeightV3.z = 0;
                depaPrefab.transform.Translate(terrainHeightV3, Space.Self);

                // move the Dépa toward the target at the right speed
                transform.Translate(Vector3.forward * Time.deltaTime * onPathSpeed, Space.Self);
            }
        }
    }

    // set new path for the Depa
    private void newPath(GameObject path)
    {
        // make the Depa pop in
        DepaPopIn();
        // set new running path
        runningPath = path;
        // get the path points array
        runningPoints = runningPath.GetComponent<DepaPath>().GetPoints();
        // get the path start point
        startPoint = runningPath.GetComponent<DepaPath>().GetStartPoint();
        // get the path end point
        endPoint = runningPath.GetComponent<DepaPath>().GetEndPoint();
        // bet the number of points in the path
        numOfTargets = runningPoints.Length;
        // get the current point
        currentPoint = startPoint;
        // get the current gameobject
        currentTarget = runningPoints[startPoint];
        // move the Dépa to the first target position
        transform.position = currentTarget.transform.position;
        // set on a point 
        isDepaOnAPoint = true;
        // set the next target and rotate the Dépa making him facing it
        SetNextPoint();
    }

    private void OnTriggerEnter(Collider other)
    {
        // if the player get in of the trigger zone
        if (other.tag == "Player")
        {
            player = playerGO.GetComponent<CharacterController>();
            //player.GetComponent<FirstPersonControllerExtend>().speedUpdate(40.0f);
            isPlayerClose = true;

            SetNextPoint();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // if the player get out of the trigger zone
        if (other.tag == "Player")
        {
            isPlayerClose = false;
            onPathSpeed = 0f;
            player.GetComponent<Player>().SetNewSpeed(20.0f);
            player.GetComponent<Player>().setRunningSpeed(20.0f);
        }
    }

    // depa pop in
    private void DepaPopIn()
    {
        // reset the local position of the prefab
        depaPrefab.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);

        // reset running tips state
        playerGO.GetComponent<Player>().setRunningWithDepaTipHasBennSaid(false);

        // fade out the depa sound
        StartCoroutine(AM.audioFadeCoroutine(depaAmbianceAudioSource, 0f, GM.getSfxVolume() / 5, 2.0f));
        depaPrefab.SetActive(true);
        this.GetComponent<SphereCollider>().enabled = true;

        // play pop sound
        playDepaPopSoundAndParticle("popIn");

        // play voiceOver tip
        AM.playEventVoiceOver(0, 0, 0.5f, "depaPopTips");
    }

    // depa pop out
    private void DepaPopOut(string reason)
    {
        if (reason == "destination")
        {
            // play storm tip
            AM.playEventVoiceOver(0, 6, 0.0f, "depaPopOutTips");

            playDepaPopSoundAndParticle("popOutDestination");
        }
        else if (reason == "night")
        {
            playDepaPopSoundAndParticle("popOutNight");
        }
        // reset player speed and close state
        isPlayerClose = false;
        if (player)
        {
            player.GetComponent<Player>().SetNewSpeed(20.0f);
            player.GetComponent<Player>().setRunningSpeed(20.0f);
        }

        // fade out the depa sound
        StartCoroutine(AM.audioFadeCoroutine(depaAmbianceAudioSource, depaAmbianceAudioSource.volume, 0f, 2.0f));
        // reset animator state
        depaAnimator.SetBool("isRunning", false);

        depaRunning.SetActive(false);
        depaWaiting.SetActive(true);

        // stop sand trail particle
        depaSandTrail.SetActive(false);
        // set Depa speed
        onPathSpeed = 0.0f;
        // disable collider
        GetComponent<SphereCollider>().enabled = false;
        // hide Depa
        // Coroutine to have a short delay to let the animator to be updated
        StartCoroutine(deactivateDepaCoroutine());
    }

    private void playDepaPopSoundAndParticle(string reason)
    {
        if (reason != "popOutNight")
        {
            depaPopAudioSource.Play();
        }

        depaPopParticleSystem.Play();
    }

    private IEnumerator deactivateDepaCoroutine()
    {
        yield return new WaitForSeconds(0.2f);
        depaPrefab.SetActive(false);
    }

    // set the next point on the path and rotate the Dépa
    private void SetNextPoint()
    {
        // get next and previous point ID on the path
        int nextIDPt;
        int prevIDPt;
        int prevModifier = 0;
        int nextModifier = 0;

        // adat cuurent ID point with direction and is on a point or not
        if (isDepaOnAPoint == false)
        {
            if (onPathForward == true)
            {
                prevModifier = -1;
                nextModifier = 0;
            } else
            {
                prevModifier = 0;
                nextModifier = -1;
            }
        }

        if (currentPoint == 0)
        {
            nextIDPt = currentPoint + 1;
            prevIDPt = numOfTargets - 1;
        } else if (currentPoint == numOfTargets - 1)
        {
            nextIDPt = 0;
            prevIDPt = currentPoint - 1;

        } else
        {
            nextIDPt = currentPoint + 1 + nextModifier;
            prevIDPt = currentPoint - 1 + prevModifier;
        }

        // check from witch point the player closer is
        float playerNextIDPtDistance = Vector3.Distance(playerGO.transform.position, runningPoints[nextIDPt].transform.position);
        float depaNextIDPtDistance = Vector3.Distance(transform.position, runningPoints[nextIDPt].transform.position);

        if (depaNextIDPtDistance < playerNextIDPtDistance)
        {
            onPathForward = true;
            nextPoint = nextIDPt;
            nextTarget = runningPoints[nextPoint];
        } else
        {
            onPathForward = false;
            nextPoint = prevIDPt;
            nextTarget = runningPoints[nextPoint];
        }

        DepaGOLookAt();
        depaLookAt(nextTarget);
    }

    private void DepaGOLookAt()
    {
        transform.LookAt(nextTarget.transform.position);
    }

    // make the Dépa look at the player if he (the Dépa) don't move
    private void depaLookAt(GameObject target)
    {
        depaPrefab.transform.LookAt(target.transform.position);
    }

    // public set methods
    public void setNewPath(GameObject runningPath)
    {
        newPath(runningPath);
    }
}