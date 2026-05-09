using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Szenen-Namen (müssen in Build Settings sein!)")]
    [SerializeField] private string beratungsraumSzene = "Beratungsraum";
    [SerializeField] private string kiTestSzene = "AITestScene_VR";

    public void StartBeratungsraum()
    {
        Debug.Log($"[MainMenu] Lade Szene: {beratungsraumSzene}");
        SceneManager.LoadScene(beratungsraumSzene);
    }

    public void StartKITest()
    {
        Debug.Log($"[MainMenu] Lade Szene: {kiTestSzene}");
        SceneManager.LoadScene(kiTestSzene);
    }

    public void Beenden()
    {
        Debug.Log("[MainMenu] Spiel beendet");
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}