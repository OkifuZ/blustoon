using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class TimeController : MonoBehaviour
{
    [SerializeField] private float timeCounter;
    [SerializeField] public float countDownTimer = 60.0f;
    [SerializeField] private bool isCountDown;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI LayerText;

    public int flipInterval = 15;
    private int _flipInterval = 15;
    private bool shown = true;
    public Color colorFlip;
    
    // Start is called before the first frame update
    void Start()
    {
        _flipInterval = flipInterval;
    }

    public void SetLayer(int i)
    {
        LayerText.text = string.Format("Layer " + i);
    }
    
    // Update is called once per frame
    void Update()
    {
        if (Controller.instance.paused) return;
        if (countDownTimer < 0)
        {
            EndGameLayer();
            countDownTimer = 1234;
            return;
        }
        
        
        if (isCountDown && countDownTimer > 0)
        {
            countDownTimer -= Time.deltaTime;
        }
        else if (!isCountDown)
        {
            timeCounter += Time.deltaTime;
        }

        int minutes = Mathf.FloorToInt(isCountDown ? countDownTimer / 60.0f : timeCounter / 60f);
        int seconds = Mathf.FloorToInt(isCountDown ? countDownTimer - minutes * 60 : timeCounter - minutes * 60);


        if (countDownTimer < 5)
        {
            _flipInterval -= 1;
            if (_flipInterval == 0)
            {
                if (shown)
                {
                    timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
                    timerText.color = Color.Lerp(Color.red, Color.yellow, 0.5f);
                    
                }
                else
                {
                    timerText.text = "";
                }

                shown = !shown;
                _flipInterval = flipInterval;
            }
        }
        else
        {
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
        
    }

    void EndGameLayer()
    {
        Controller.instance.shouldGG = true;
    }
}
