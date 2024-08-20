using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActiveRagdoll {
    // Author: Sergio Abreu García | https://sergioabreu.me

    public class GripModule : Module {
        [Tooltip("What's the minimum weight the arm IK should have over the whole " +
        "animation to activate the grip")]
        public float leftArmWeightThreshold = 0.5f, rightArmWeightThreshold = 0.5f;
        public JointMotionsConfig defaultMotionsConfig;

        [Tooltip("Whether to only use Colliders marked as triggers to detect grip collisions.")]
        public bool onlyUseTriggers = false;
        public bool canGripYourself = false;

        private Gripper _leftGrip, _rightGrip;

        private bool _leftTriggered, _rightTriggered;


        private void Start() {
            var leftHand = _activeRagdoll.GetPhysicalBone(HumanBodyBones.LeftHand).gameObject;
            var rightHand = _activeRagdoll.GetPhysicalBone(HumanBodyBones.RightHand).gameObject;

            (_leftGrip = leftHand.AddComponent<Gripper>()).GripMod = this;
            (_rightGrip = rightHand.AddComponent<Gripper>()).GripMod = this;

            _leftTriggered = false;
            _rightTriggered = false;
        }


        public void UseLeftGrip(float weight)
        {
            if (!_activeRagdoll.curPhysState) return;
            if (_activeRagdoll.weaponID != 0) return;
            // TODO beaton
            bool triggered = weight > leftArmWeightThreshold;
            _leftTriggered = triggered;
            // _leftGrip.enabled = triggered;
        }

        public void UseRightGrip(float weight)
        {
            if (!_activeRagdoll.curPhysState) return;
            if (_activeRagdoll.weaponID != 0) return;

            bool triggered = weight > rightArmWeightThreshold;
            _rightTriggered = triggered;
            // _rightGrip.enabled = weight > rightArmWeightThreshold;
        }

        public void UseAttachGrip(float weight) {
            bool attachedLeftNow = _leftGrip.AttachedObjects();
            if (_leftTriggered && weight > leftArmWeightThreshold)
            {
                if (!attachedLeftNow) _leftGrip.enabled = true;
                // empty
            } 
            else if (!_leftTriggered && attachedLeftNow)
            {
                _leftGrip.enabled = false;
            }
            
            bool attachedRightNow = _rightGrip.AttachedObjects();
            if (_rightTriggered && weight > rightArmWeightThreshold)
            {
                if (!attachedRightNow) _rightGrip.enabled = true;
                // empty
            } 
            else if (!_rightTriggered && attachedRightNow)
            {
                _rightGrip.enabled = false;
            }
            
            
            /*bool attachedLeftNow = _leftGrip.AttachedObjects();
            if (_leftTriggered && weight > leftArmWeightThreshold || // arm up 
                !_leftTriggered && attachedLeftNow) //  arm down with grip
            {
                _leftGrip.enabled = !attachedLeftNow;
            } // arm down, nothing will happen
            
            bool attachedRightNow = _rightGrip.AttachedObjects();
            if (_rightTriggered && weight > rightArmWeightThreshold || // arm up 
                !_rightTriggered && attachedRightNow) //  arm down with grip
            {
                _rightGrip.enabled = !attachedRightNow;
            } // arm down, nothing will happen*/

        }

        public void LooseAll()
        {
            _leftGrip.enabled = false;
            _rightGrip.enabled = false;
        }
    }
} // namespace ActiveRagdoll