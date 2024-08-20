using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    // Start is called before the first frame update
    public Camera camera;
    public GameObject playButton;
    public GameObject quitButton;
    public GameObject tutorialButton;
    public GameObject TitleGO;
    public TMP_Text Title;

    public Controller controller = null;
    
    static Menu instance;

    private Color color;

    private void Awake()
    {
        if (instance)
        {
            instance.QuitPlayReenterMenu();
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    
    
    void Start()
    {
        Title = TitleGO.GetComponent<TMPro.TMP_Text>();
        color = camera.backgroundColor;
    }

    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (tutorialInfo.active)
            {
                tutorialInfo.SetActive(false);
            }
        }
    }

    public void QuitPlayReenterMenu()
    {
        if (camera == null || camera.IsDestroyed())
        {
            camera = GameObject.Find("Main Camera").GetComponent<Camera>();
        }

        if (camera)
        {
            camera.backgroundColor = color;
        }
        playButton.SetActive(true);
        quitButton.SetActive(true);
        tutorialButton.SetActive(true);
        TitleGO.SetActive(true);
        Title.text = "Blustoon";
        
        
    }
    
    public void OnPlayButton()
    {
        camera.backgroundColor = Color.black;
        playButton.SetActive(false);
        quitButton.SetActive(false);
        tutorialButton.SetActive(false);
        Title.text = "Loading ...";
        Rect rect = TitleGO.GetComponent<RectTransform>().rect;
        var pos = rect.center;
        //Debug.Log(pos);
        // pos.y = -261;
        rect.center = pos;
        
        //Debug.Log("here reached1");

        StartCoroutine(MyLoadSceneInMenu(1, PlayCallback));
        
        // SceneManager.LoadScene(1);
    }

    void PlayCallback()
    {
        var controllerGO = GameObject.FindGameObjectWithTag("Menu");
        if (controllerGO != null)
        {
            controller = controllerGO.GetComponent<Controller>();
        }
        //Debug.Log("controller is null ? "  + controller);
        if (controller != null)
        {
            controller.AfterPlay();
        }
        
        TitleGO.SetActive(false);
    }
    
    private IEnumerator MyLoadSceneInMenu(int id, System.Action processAction)
    {
        // The Application loads the Scene in the background as the current Scene runs.
        // This is particularly good for creating loading screens.
        // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
        // a sceneBuildIndex of 1 as shown in Build Settings.

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(id, LoadSceneMode.Single);
        
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
        
    }
    
    


    public void OnQuitButton()
    {
        Application.Quit();
    }


    public GameObject tutorialInfo;

    public void OnTutorialButton()
    {
        tutorialInfo.SetActive(true);

    }
}
