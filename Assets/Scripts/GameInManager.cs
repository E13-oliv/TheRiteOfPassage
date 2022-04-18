using UnityEngine;

public class GameInManager : MonoBehaviour
{
    [Header("Player controllers")]
    [SerializeField]
    private GameObject macWindowsPlayer;
    [SerializeField]
    private GameObject oculusPlayer;

    [Header("Pause UI Panels")]
    [SerializeField]
    private GameObject macWindowsPauseUIPanel;
    [SerializeField]
    private GameObject oculusPauseUIPanel;

    [Header("Mac Windows UI Canvas")]
    [SerializeField]
    private GameObject macWindowsUI;

    [Header("Village Collider")]
    [SerializeField]
    private GameObject villageCollider;

    [Header("Introduction AudioClip")]
    [SerializeField]
    private AudioClip introduction;

    // game and audio managers
    private GameManager GM;

    private UnityStandardAssets.Characters.FirstPerson.FirstPersonController macWindowsController;

    private void Awake()
    {
        // fing the GameManager
        GameObject gameManagerGameObject = GameObject.Find("GameManager");
        GM = gameManagerGameObject.GetComponent<GameManager>();

        macWindowsController = macWindowsPlayer.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>();

        GM.setMacWindowsPlayer(macWindowsPlayer);
        GM.setOculusPlayer(oculusPlayer);
        GM.setMacWindowsPauseUIPanel(macWindowsPauseUIPanel);
        GM.setOculusPauseUIPanel(oculusPauseUIPanel);
        GM.setMacWindowsController(macWindowsController);
        GM.setVillageCollider(villageCollider);
        GM.setMacWindowsUI(macWindowsUI);
        GM.setIntroductionAudioClip(introduction);

        GM.loadSaveFile();

        GM.gameStart();

        Time.timeScale = 1;
    }
}
