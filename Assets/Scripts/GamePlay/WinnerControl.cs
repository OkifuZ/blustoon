using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinnerControl : MonoBehaviour
{
    public static int cnt_A;
    public static int cnt_B;
    
    // Start is called before the first frame update
    void Start()
    {
        cnt_A = cnt_B = 0;
    }

    // Update is called once per frame
    void Update()
    {
        /*Debug.Log("cnt A: " + cnt_A);
        Debug.Log("cnt B: " + cnt_B);*/
    }


    static public int WhoWins()
    {
        return cnt_A > cnt_B ? 1 : 2;
    }
}
