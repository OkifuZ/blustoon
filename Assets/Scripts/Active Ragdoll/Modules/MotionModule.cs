#pragma warning disable 649

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace ActiveRagdoll {

    public class MotionModule : Module
    {

        
        public GameObject leftArm;
        public float falldownAngle = 55;

        
        
        private GripModule _gripModule;
        private AnimationModule _animationModule;
        private PhysicsModule _physicsModule;
        private CameraModule _cameraModule;

        private PhysicsModule.BALANCE_MODE oldBalanceMode;
        

        public void Start()
        {
            _gripModule = GetComponent<GripModule>();
            _animationModule = GetComponent<AnimationModule>();
            _physicsModule = GetComponent<PhysicsModule>();
            _cameraModule = GetComponent<CameraModule>();
        }

        public void LeftAttack(float weight)
        {

            /*Rigidbody leftArmRigid = leftArm.GetComponent<Rigidbody>();
            if (leftArmRigid == null)
            {
                Debug.Log("left arm lost");
                return;
            }

            ConfigurableJoint leftShoulderJoint = leftArm.GetComponent<ConfigurableJoint>();*/

            
            if (BodyInPassiveState())  
                Standup();
        }


        private bool BodyInPassiveState()
        {
            return _activeRagdoll.isGettingUp ||
                   _activeRagdoll.beaten ||
                   _activeRagdoll.isGettingUpAnimated;
        }
        
        private void FixedUpdate()
        {
            // 监听身体状态
            if (BodyInPassiveState()) return;
            
            float torsoAngle = _animationModule.GetTorsoAngle();
            _activeRagdoll._hitState.hit |= torsoAngle > falldownAngle;
            
            if (_activeRagdoll._hitState.hit)
            {
                Beaten(_activeRagdoll._hitState.initialVelocity);
            }
            
            _activeRagdoll._hitState.initialVelocity.Set(0, 0, 0);
            _activeRagdoll._hitState.hit = false;
            
        }
        
        public void RightAttack(float weight)
        {
            // Debug.Log("right attack");
        }


        public void Beaten(Vector3 velocity)
        {
            if (_activeRagdoll.beaten || _activeRagdoll.isGettingUp) return;
            
            // if (!_activeRagdoll.curPhysState) _activeRagdoll.ChangeWeaponState();
            
            _activeRagdoll.AnimatedAnimator.enabled = false;
            _activeRagdoll.beaten = true;
            _gripModule.LooseAll();
            oldBalanceMode = _physicsModule.BalanceMode;
            _physicsModule.SetBalanceMode(PhysicsModule.BALANCE_MODE.NONE);
            _animationModule.DisableJoints();
            _animationModule._enableIK = false;
            _cameraModule.SetBeatenState();
            
            
            _activeRagdoll.SetAndChangeWeaponState(0);
            
            _activeRagdoll.PhysicalTorso.velocity = velocity;
        }
        
        IEnumerator PlayAnimationAndWait()
        {
            // Play the animation
            _activeRagdoll.isGettingUpAnimated = true;
            _activeRagdoll.AnimatedAnimator.enabled = true;

            string animateName = _activeRagdoll.PhysicalTorso.transform.forward.y > 0 ? "Kickup" : "Standup";
            
            _animationModule.ResetAnimatorToState(animateName);
            // Wait for the animation to finish
            _physicsModule.SetBalanceMode(oldBalanceMode);
            
            yield return new WaitUntil(() => 
                IsAnimationPlaying(_activeRagdoll.AnimatedAnimator, animateName) == false);
        
            _cameraModule.ResetState();

            _activeRagdoll.isGettingUpAnimated = false;
            _animationModule._enableIK = true;
        }
        
        private bool IsAnimationPlaying(Animator animator, string stateName)
        {
            return animator.GetCurrentAnimatorStateInfo(0).IsName(stateName) &&
                   animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f;
        }
        
        public void Standup()
        {
            if (_activeRagdoll.isGettingUp) return;
            
            _animationModule.ResetJoints();
            _activeRagdoll.beaten = false;
            _physicsModule.IncreaseStrength();

            StartCoroutine(PlayAnimationAndWait());

        }
    }
} // namespace ActiveRagdoll