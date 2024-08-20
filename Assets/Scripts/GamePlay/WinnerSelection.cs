using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinnerControl1 : MonoBehaviour
{
    public Material bodyA;
    public Material bodyB;
    
    // Start is called before the first frame update
    void Start()
    {
        if (Controller.instance != null)
        {
            int winner = Controller.instance.winner;
            GameObject winnerCube = GameObject.Find("CubeWinner");
            GameObject looserCube = GameObject.Find("CubeLooser");
            if (winner == 1) // A wins
            {
                winnerCube.GetComponent<SkinnedMeshRenderer>().material = bodyA;
                looserCube.GetComponent<SkinnedMeshRenderer>().material = bodyB;
            }
            else // B wins
            {
                winnerCube.GetComponent<SkinnedMeshRenderer>().material = bodyB;
                looserCube.GetComponent<SkinnedMeshRenderer>().material = bodyA;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
