#pragma warning disable 649

using System;
using System.Net;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ActiveRagdoll {

    public class WeaponModule : Module
    {

        
        private CharacterAiming aimingComponent = null;
        [SerializeField] private ParticleSystem inkParticle = null;
        [SerializeField] private CameraModule _cameraModule = null;
        [SerializeField] private CharacterAiming _characterAiming = null;
        

        public void ChangeWeapon(float x) {
            /*if (_activeRagdoll.currentControlIDfromInput != _activeRagdoll._currentControlID)
            {
                return;
            }*/

            if (_activeRagdoll.beaten || _activeRagdoll.isGettingUp) return;

            
            if (x > 0.5)
            {
                _activeRagdoll.ChangeWeaponState();
            }
        }

        private bool enableGunShoot = false;
        private float curSpeed = 20;
        public void GunShoot(float x)
        {
            /*if (_activeRagdoll.currentControlIDfromInput != _activeRagdoll._currentControlID)
            {
                return;
            }*/

            // if (_activeRagdoll.curPhysState) return;
            
            if (_activeRagdoll.weaponID != 2) return;

            if (x > 0.5f)
            {
                enableGunShoot = true;
                _characterAiming.aiming = true;
            }
            else
            {
                enableGunShoot = false;
                _characterAiming.aiming = false;

            }
        }

        public void Aim(float x)
        {
            // if (_activeRagdoll.curPhysState) return;
            if (_activeRagdoll.weaponID != 2) return;
            if (x > 0.5f)
            {
                _characterAiming.aiming = true;
                
            }
            else
            {
                _characterAiming.aiming = false;

            }
        }


        private void OnValidate() {
            
        }

        void Start()
        {
            aimingComponent = _activeRagdoll.AnimCharacter.gameObject.GetComponent<CharacterAiming>();
        }

        void Update() {
            if ( aimingComponent.aimed && enableGunShoot)
            {
                inkParticle.Play();
                curSpeed += Time.deltaTime * 40;
                if (curSpeed > 200) curSpeed = 200;
                ChangeShootingDistance(curSpeed);
            }
            else
            {
                inkParticle.Stop();
                curSpeed = 20;
            }
        }

        public void ChangeShootingDistance(float start_speed = 50)
        {
            var main = inkParticle.main;
            main.startSpeed = start_speed;
        }

        

    }
} // namespace ActiveRagdoll
