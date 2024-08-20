using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PauseMenu : MonoBehaviour
{
    // Start is called before the first frame update
    public List<Camera> cameras;
    public GameObject ResumeButton;
    public GameObject RestartButton;
    public GameObject QuitButton;
    public GameObject PauseImage;
    public GameObject TitleGO;
    public GameObject TutorialInfo;
    public TMP_Text Title;

    public Controller controller;

    static PauseMenu instance;

    void Start()
    {
        Title = TitleGO.GetComponent<TMPro.TMP_Text>();
        this.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        // DontDestroyOnLoad(this.gameObject);
    }

    public void OnRestartButton()
    {
        if (cameras[0] == null || cameras[1] == null)
        {
            cameras[0] = GameObject.Find("Camera1").GetComponent<Camera>();
            cameras[1] = GameObject.Find("Camera2").GetComponent<Camera>();

        }
        foreach (var camera in cameras)
        {
            camera.backgroundColor = Color.black;
        }

        /*foreach (Transform child in transform)
            child.gameObject.SetActive(false);*/
        Controller.instance.AfterResetDown = false;
        StartCoroutine(MyLoadSceneInPauseMenu(1, Controller.instance.AfterReset));
        /*ResumeButton.SetActive(false);
        RestartButton.SetActive(false);
        QuitButton.SetActive(false);
        PauseImage.SetActive(false);
        TitleGO.SetActive(false);*/
        /*Rect rect = TitleGO.GetComponent<RectTransform>().rect;
        var pos = rect.center;
        Debug.Log(pos);
        pos.y = -261;
        rect.center = pos;*/
        
        // SceneManager.LoadScene(1);
    }
    
    private IEnumerator MyLoadSceneInPauseMenu(int id, System.Action processAction)
    {
        // The Application loads the Scene in the background as the current Scene runs.
        // This is particularly good for creating loading screens.
        // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
        // a sceneBuildIndex of 1 as shown in Build Settings.

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(id);
        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        
        // Controller.instance.AfterReset();
        if (processAction != null)
        {
            processAction();
        }
        
        this.gameObject.SetActive(false);
    }
    
    public void OnResumeButton()
    {
        this.gameObject.SetActive(false);
        controller.AfterResume();
    }

    public void OnQuitButton()
    {
        StartCoroutine(MyLoadSceneInPauseMenu(0, controller.AfterMenu));
        // SceneManager.LoadScene(0);
        
    }

    public void OnTutorialButton()
    {
        TutorialInfo.SetActive(true);
        Controller.instance.inTutorial = true;
        Controller.instance._tutorialInfo = TutorialInfo;
    }
}
