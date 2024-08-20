using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class CharacterAiming : MonoBehaviour
{
    public Camera camera;
    public Rig gunAimLayer;
    public Rig bodyAimLayer;
    public float aimDuration = 0.3f;
    public bool aimed = false;
    public bool aiming = false;
    public ActiveRagdoll.ActiveRagdoll _activeRagdoll;

    public ActiveRagdoll.AnimationModule _animationModule;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (aiming)
        {
            Aim();
        }
        else
        {
            Rest();
        }

        /*if (_activeRagdoll._currentControlID != _activeRagdoll.currentControlIDfromInput)
        {
            return;
        }
        else
        {
            if (Input.GetMouseButton(1) || Input.GetMouseButton(0))
            {
                aiming = true;
                // Aim();
            }
            else
            {
                aiming = false;
                // Rest();
            }
            
            
        }*/
    }

    public void Aim()
    {
        /*if (_activeRagdoll._currentControlID != _activeRagdoll.currentControlIDfromInput)
        {
            return;
        }*/
        gunAimLayer.weight += Time.deltaTime / aimDuration;
        bodyAimLayer.weight += Time.deltaTime / aimDuration;
        if (gunAimLayer.weight >= 0.99) aimed = true;
        _animationModule.RightArmGunAim();

    }

    public void Rest()
    {
        
        gunAimLayer.weight -= Time.deltaTime / aimDuration;
        bodyAimLayer.weight -= Time.deltaTime / aimDuration;
        if (gunAimLayer.weight <= 0.99) aimed = false;
        _animationModule.RightArmGunRest();

    }

    private void FixedUpdate()
    {
        // float yawCamera = camera.transform.rotation.eulerAngles.y;
        
    }
}
