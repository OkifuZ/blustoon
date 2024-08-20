using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageFloor : MonoBehaviour, IDamagable
{

    [SerializeField] public GameObject pieces;

    private void Awake()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        // Controller.instance.onDamageDelegates += OnDead;
        // Controller.instance._currentEnvironment = this.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /*void OnDead()
    {
        Instantiate(pieces, transform.position, transform.rotation);
        // Destroy(gameObject);
    }*/

    
}
