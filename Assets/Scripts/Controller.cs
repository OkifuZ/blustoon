using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public class Controller : MonoBehaviour
{
    public GameObject canvas;
    // public GameObject Infocanvas;
    public bool lockCursor = true; // If true, the mouse will be locked to screen center and hidden
    public bool inMenu = false;
    public int layerNum = 3;
    private int _layerNum = 3;

    public int timePerLayer = 10;

    public float distanceLayer = 10.0f;

    public float pieceFadeTime = 1.5f;
    
    public bool paused = false;

    public bool askQuit = false;
    /*public List<Camera> cameras;
    public GameObject ResumeButton;
    public GameObject RestartButton;
    public GameObject QuitButton;
    public GameObject PauseImage;
    public GameObject TitleGO;
    public TMP_Text Title;*/

    public static Controller instance;

    public bool shouldGG = false;

    [SerializeField] private GameObject _environmentLayer;
    public GameObject _currentEnvironment;

    public delegate void OnDamage();

    public OnDamage onDamageDelegates { get; set; }

    public void onDamage()
    {
        onDamageDelegates?.Invoke();
    }

    public delegate void AfterDamage();

    public AfterDamage afterDamageDelegates { get; set; }

    public void afterDamage()
    {
        afterDamageDelegates?.Invoke();
    }

    public Vector3 _initPosition;

    public int winner = 0;

    public bool inTutorial = false;
    public GameObject _tutorialInfo;

    void Awake()
    {
        if (instance)
        {
            _layerNum = layerNum;
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            AfterResetDown = true;

            DontDestroyOnLoad(gameObject);
            _layerNum = layerNum;

            _initPosition = this.transform.position;

            if (_currentEnvironment == null)
            {
                MakeEnvDisappear();
                GenerateNewEnv();
            }
        }
        // DontDestroyOnLoad(this.gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        // Cursors
        if (!inMenu)
        {
            Cursor.lockState = lockCursor ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = lockCursor ? false : true;
        }

        onDamageDelegates += GeneratePieces;
        onDamageDelegates += MakeEnvDisappear;
        afterDamageDelegates += GenerateNewEnv;

        //canvas.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {
        if (paused)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }

        if (!inMenu && (askQuit || Input.GetKeyDown(KeyCode.Escape)))
        {
            if (inTutorial)
            {
                _tutorialInfo.SetActive(false);
                inTutorial = false;
            }
            else {
                if (canvas.active)
                {
                    canvas.SetActive(false);
                    /*Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;*/
                    AfterResume();
                }
                else
                {
                    canvas.SetActive(true);
                    /*Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;*/
                    AfterPause();
                }
            }

        }

        // Floor Damage Logic
        if (!inMenu && !paused && Input.GetKeyDown(KeyCode.P))
        {
            _layerNum -= 1;
            onDamage();
            afterDamage();
        }
        else if (Input.GetKeyDown(KeyCode.M) && !inMenu && !paused)
        {
            shouldGG = false;
            JumpToComplete();
        }

        if (!inMenu && shouldGG && !paused)
        {
            _layerNum -= 1;
            shouldGG = false;
            if (_layerNum == 0)
            {
                winner = WinnerControl.WhoWins();
                JumpToComplete();
            }
            else
            {
                TimeController tc = GameObject.Find("InfoCanvas").GetComponent<TimeController>();
                onDamage();
                afterDamage();
                tc.countDownTimer = timePerLayer;
                tc.SetLayer(layerNum - _layerNum + 1);
            }
        }
    }

    public void JumpToComplete()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        inMenu = true;
        paused = false;
        // MakeEnvDisappear();
        canvas.gameObject.SetActive(false);
        SceneManager.LoadScene(2);

        GameObject go1 = GameObject.Find("Winner");
        GameObject go2 = GameObject.Find("Looser");
    }

    public bool AfterResetDown = true;

    public void AfterReset()
    {
        inMenu = false;
        paused = false;

        canvas.gameObject.SetActive(false);
        AfterResetDown = true;
        /*PauseMenu pauseMenu = canvas.GetComponent<PauseMenu>();
        pauseMenu.cameras[0] = GameObject.Find("Camera1").GetComponent<Camera>();
        pauseMenu.cameras[1] = GameObject.Find("Camera2").GetComponent<Camera>();*/

        this.transform.position = _initPosition;
        MakeEnvDisappear();
        GenerateNewEnv();
        
        TimeController tc = GameObject.Find("InfoCanvas").GetComponent<TimeController>();
        tc.countDownTimer = timePerLayer;
    }

    public void AfterMenu()
    {
        inMenu = true;
        paused = false;
        MakeEnvDisappear();
        canvas.gameObject.SetActive(false);
    }


    public void AfterResume()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        paused = false;
    }

    public void AfterPause()
    {
        paused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void AfterPlay()
    {
        //Debug.Log("after play");
        canvas.gameObject.SetActive(false);
        
        inMenu = false;
        paused = false;

        this.transform.position = _initPosition;
        MakeEnvDisappear();
        GenerateNewEnv();
        
        TimeController tc = GameObject.Find("InfoCanvas").GetComponent<TimeController>();
        tc.countDownTimer = timePerLayer;
        /*PauseMenu pauseMenu = canvas.GetComponent<PauseMenu>();
        pauseMenu.cameras[0] = GameObject.Find("Camera1").GetComponent<Camera>();
        pauseMenu.cameras[1] = GameObject.Find("Camera2").GetComponent<Camera>();*/
    }

    private void GeneratePieces()
    {
        if (_currentEnvironment)
        {
            var floorTrans = _currentEnvironment.transform.Find("BattleField/Floor");
            var btfTrans = _currentEnvironment.transform.Find("BattleField");
            var damageFloor = floorTrans.gameObject.GetComponent<DamageFloor>();
            if (damageFloor != null)
            {
                var pieces = Instantiate(damageFloor.pieces);
                var fa = pieces.AddComponent<FadeAfter>();
                fa.delay = pieceFadeTime;
                pieces.transform.position = floorTrans.position;
                pieces.transform.rotation = floorTrans.rotation;
            }
        }
    }
    
    private void MakeEnvDisappear()
    {
        if (this._currentEnvironment != null)
        {
            Destroy(this._currentEnvironment);
            this._currentEnvironment = null;
        }
    }

    private void GenerateNewEnv()
    {
        this._currentEnvironment = Instantiate(_environmentLayer, this.transform.position, this.transform.rotation);
        
        Vector3 lowerPos = this.transform.position;
        lowerPos.y -= distanceLayer;
        this.transform.position = lowerPos;
    }
    
    
    
    
    
    
    
}
