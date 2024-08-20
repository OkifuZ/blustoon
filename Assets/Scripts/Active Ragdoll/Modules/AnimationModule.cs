using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace ActiveRagdoll {
    // Author: Sergio Abreu García | https://sergioabreu.me

    public class AnimationModule : Module {
        [Header("--- BODY ---")]
        /// <summary> Required to set the target rotations of the joints </summary>
        private Quaternion[] _initialJointsRotation;
        private ConfigurableJoint[] _joints;
        private float[] jointsXStrength;
        private float[] jointsYZStrength;
        
        private Quaternion[] _initialJointsRotationKine;
        private List<ConfigurableJoint> _jointsKine;
        private List<bool> _jointsMask;
        private float[] jointsXStrengthKine;
        private float[] jointsYZStrengthKine;
        private Transform[] _animatedBones;
        private List<Transform> _animatedBonesKine;
        private AnimatorHelper _animatorHelper;
        public Animator Animator { get; private set; }

        [Header("--- INVERSE KINEMATICS ---")]
        public bool _enableIK = true;

        [Tooltip("Those values define the rotation range in which the target direction influences arm movement.")]
        public float minTargetDirAngle = - 30,
                     maxTargetDirAngle = 60;

        [Space(10)]
        [Tooltip("The limits of the arms direction. How far down/up should they be able to point?")]
        public float minArmsAngle = -70;
        public float maxArmsAngle = 100;
        [Tooltip("The limits of the look direction. How far down/up should the character be able to look?")]
        public float minLookAngle = -50, maxLookAngle = 60;

        [Space(10)]
        [Tooltip("The vertical offset of the look direction in reference to the target direction.")]
        public float lookAngleOffset;
        [Tooltip("The vertical offset of the arms direction in reference to the target direction.")]
        public float armsAngleOffset;
        [Tooltip("Defines the orientation of the hands")]
        public float handsRotationOffset = 0;

        [Space(10)]
        [Tooltip("How far apart to place the arms")]
        public float armsHorizontalSeparation = 0.75f;

        [Tooltip("The distance from the body to the hands in relation to how high/low they are. " +
                 "Allows to create more realistic movement patterns.")]
        public AnimationCurve armsDistance;
        
        public Vector3 AimDirection { get; set; }
        private Vector3 _armsDir, _lookDir, _targetDir2D;
        private Transform _animTorso, _chest;
        private float _targetDirVerticalPercent;

        public GameObject AnimationGun;
        public GameObject PhysicalGun;

        public CharacterAiming aimingComponent;


        private void Start() {
            aimingComponent = _activeRagdoll.AnimCharacter.gameObject.GetComponent<CharacterAiming>();
            
            _joints = _activeRagdoll.Joints;
            _jointsKine = _activeRagdoll.JointsKine;
            _jointsMask = _activeRagdoll.JointsMask;
            _animatedBones = _activeRagdoll.AnimatedBones;
            _animatedBonesKine = _activeRagdoll.AnimatedBonesKine;
            _animatorHelper = _activeRagdoll.AnimatorHelper;
            Animator = _activeRagdoll.AnimatedAnimator;

            _initialJointsRotation = new Quaternion[_joints.Length];
            jointsXStrength = new float[_joints.Length];
            jointsYZStrength = new float[_joints.Length];
            for (int i = 0; i < _joints.Length; i++) {
                _initialJointsRotation[i] = _joints[i].transform.localRotation;
                jointsXStrength[i] = _joints[i].angularXDrive.positionSpring;
                jointsYZStrength[i] = _joints[i].angularYZDrive.positionSpring;
            }
            
            _initialJointsRotationKine = new Quaternion[_jointsKine.Count];
            jointsXStrengthKine = new float[_jointsKine.Count];            
            jointsYZStrengthKine = new float[_jointsKine.Count];            

            for (int i = 0; i < _jointsKine.Count; i++)
            {
                _initialJointsRotationKine[i] = _jointsKine[i].transform.localRotation;
                jointsXStrengthKine[i] = _jointsKine[i].angularXDrive.positionSpring;
                jointsYZStrengthKine[i] = _jointsKine[i].angularYZDrive.positionSpring;
            }
        }

        void SyncGunPosRot()
        {
            if (PhysicalGun != null && AnimationGun != null)
            {
                PhysicalGun.transform.position = AnimationGun.transform.position;
                PhysicalGun.transform.rotation = AnimationGun.transform.rotation;
            }
        }
        
        void FixedUpdate() {
            if (!_activeRagdoll.beaten)
            {
                if (aimingComponent.aimed)
                {
                    SyncGunPosRot();
                }
                UpdateJointTargets();
                //UpdateLocalRotation();
            }
            else
            {
            }
            UpdateIK();

        }

        private void UpdateLocalRotation()
        {
            for (int i = 0; i < _activeRagdoll.MaskedBonesPhys.Count; i++)
            {
                var animeTrans = _activeRagdoll.MaskedBonesAnim[i];
                var physTrans = _activeRagdoll.MaskedBonesPhys[i];
                
                physTrans.localRotation = animeTrans.localRotation;
                physTrans.localPosition = animeTrans.localPosition;
            }
        }

        /// <summary> Makes the physical bones match the rotation of the animated ones </summary>
        private void UpdateJointTargets() {
            var leftArmBp = _activeRagdoll.GetBodyPart("Left Arm");
            var shoulderJoint = leftArmBp._joints[0];
            
            /*for (int i = 0; i < _joints.Length; i++)
            {
                Quaternion targetRotation = _animatedBones[i + 1].localRotation;
                //Quaternion targetRotation = _animatedBonesKine[i + 1].localRotation;
                /*if (_activeRagdoll.LeftArmAttack > 0)
                {
                    if (shoulderJoint == _joints[i])
                    {
                        targetRotation = Quaternion.Euler(0, -180, 0) * targetRotation;
                        _activeRagdoll.LeftArmAttack -= 1;
                    }
                }#1#
                ConfigurableJointExtensions.SetTargetRotationLocal(_joints[i], targetRotation, _initialJointsRotation[i]);
            }*/
            
            for (int i = 0; i < _jointsKine.Count; i++)
            {
                /*if (!_jointsMask[i])
                {
                    _jointsKine[i].angularXMotion = ConfigurableJointMotion.Free;
                    _jointsKine[i].angularYMotion = ConfigurableJointMotion.Free;
                    _jointsKine[i].angularZMotion = ConfigurableJointMotion.Free;
                    continue;
                }*/
                
                // Quaternion targetRotation = _animatedBones[i + 1].localRotation;
                Quaternion targetRotation = _animatedBonesKine[i + 1].localRotation;
                ConfigurableJointExtensions.SetTargetRotationLocal(_jointsKine[i], targetRotation, _initialJointsRotationKine[i]);
            }
        }

        public void DisableJoints()
        {
            if (_activeRagdoll.isGettingUp) return;
            /*for (int i = 0; i < _joints.Length; i++)
            {
                Quaternion targetRotation = _initialJointsRotation[i];
                ConfigurableJointExtensions.SetTargetRotationLocal(_joints[i], targetRotation, _initialJointsRotation[i]);
                var angularXDrive = _joints[i].angularXDrive;
                angularXDrive.positionSpring = 10.0f;
                var angularYZDrive = _joints[i].angularYZDrive;
                angularYZDrive.positionSpring = 10.0f;

                _joints[i].angularXDrive = angularXDrive;
                _joints[i].angularYZDrive = angularYZDrive;
            }*/
            
            for (int i = 0; i < _jointsKine.Count; i++)
            {
                //_jointsKine[i].angularXMotion = ConfigurableJointMotion.Free;
                //_jointsKine[i].angularYMotion = ConfigurableJointMotion.Free;
                //_jointsKine[i].angularZMotion = ConfigurableJointMotion.Free;
                /*if (!_jointsMask[i])
                {
                    _jointsKine[i].angularXMotion = ConfigurableJointMotion.Free;
                    _jointsKine[i].angularYMotion = ConfigurableJointMotion.Free;
                    _jointsKine[i].angularZMotion = ConfigurableJointMotion.Free;
                    continue;
                }
                
                _jointsKine[i].angularXMotion = ConfigurableJointMotion.Locked;
                _jointsKine[i].angularYMotion = ConfigurableJointMotion.Locked;
                _jointsKine[i].angularZMotion = ConfigurableJointMotion.Locked;*/

                Quaternion targetRotation = _initialJointsRotationKine[i];
                ConfigurableJointExtensions.SetTargetRotationLocal(_jointsKine[i], targetRotation, _initialJointsRotationKine[i]);
                var angularXDrive = _jointsKine[i].angularXDrive;
                angularXDrive.positionSpring = 20.0f;
                var angularYZDrive = _jointsKine[i].angularYZDrive;
                angularYZDrive.positionSpring = 20.0f;

                _jointsKine[i].angularXDrive = angularXDrive;
                _jointsKine[i].angularYZDrive = angularYZDrive;
            }
        }

        public void ResetJoints()
        {
            if (!_activeRagdoll.isGettingUp )
            {
                StartCoroutine(DoSomethingOverFrames());
                _activeRagdoll.isGettingUp = true;
            }
            /*for (int i = 0; i < _joints.Length; i++)
            {
                Quaternion targetRotation = _initialJointsRotation[i];
                ConfigurableJointExtensions.SetTargetRotationLocal(_joints[i], targetRotation, _initialJointsRotation[i]);
                var angularXDrive = _joints[i].angularXDrive;
                angularXDrive.positionSpring = jointsXStrength[i];
                var angularYZDrive = _joints[i].angularYZDrive;
                angularYZDrive.positionSpring = jointsYZStrength[i];

                _joints[i].angularXDrive = angularXDrive;
                _joints[i].angularYZDrive = angularYZDrive;
            }*/
            /*for (int i = 0; i < _jointsKine.Count; i++)
            {
                /*if (!_jointsMask[i])
                {
                    _jointsKine[i].angularXMotion = ConfigurableJointMotion.Free;
                    _jointsKine[i].angularYMotion = ConfigurableJointMotion.Free;
                    _jointsKine[i].angularZMotion = ConfigurableJointMotion.Free;
                    continue;
                }#1#
                Quaternion targetRotation = _initialJointsRotationKine[i];
                ConfigurableJointExtensions.SetTargetRotationLocal(_jointsKine[i], targetRotation, _initialJointsRotationKine[i]);
                var angularXDrive = _jointsKine[i].angularXDrive;
                angularXDrive.positionSpring = 20.0f;
                var angularYZDrive = _jointsKine[i].angularYZDrive;
                angularYZDrive.positionSpring = 20.0f;
                
                // Quaternion targetRotation = _animatedBones[i + 1].localRotation;
                ConfigurableJointExtensions.SetTargetRotationLocal(_jointsKine[i], targetRotation, _initialJointsRotationKine[i]);
            }*/
        }
        IEnumerator DoSomethingOverFrames()
        {
            float recoverProgress = 0.0f;
            while (recoverProgress < 1.0)
            {
                recoverProgress += 0.1f;
                
                /*for (int i = 0; i < _jointsKine.Count; i++)
                {
                    Quaternion targetRotation = _initialJointsRotationKine[i];
                    ConfigurableJointExtensions.SetTargetRotationLocal(_jointsKine[i], targetRotation, _initialJointsRotationKine[i]);
                    var angularXDrive = _jointsKine[i].angularXDrive;
                    angularXDrive.positionSpring = jointsXStrength[i] * recoverProgress;
                    var angularYZDrive = _jointsKine[i].angularYZDrive;
                    angularYZDrive.positionSpring = jointsYZStrength[i] * recoverProgress;

                    _jointsKine[i].angularXDrive = angularXDrive;
                    _jointsKine[i].angularYZDrive = angularYZDrive;
                    yield return null;
                }*/
                for (int i = 0; i < _joints.Length; i++)
                {
                    Quaternion targetRotation = _initialJointsRotation[i];
                    ConfigurableJointExtensions.SetTargetRotationLocal(_joints[i], targetRotation, _initialJointsRotation[i]);
                    var angularXDrive = _joints[i].angularXDrive;
                    angularXDrive.positionSpring = jointsXStrength[i] * recoverProgress;
                    var angularYZDrive = _joints[i].angularYZDrive;
                    angularYZDrive.positionSpring = jointsYZStrength[i] * recoverProgress;

                    _joints[i].angularXDrive = angularXDrive;
                    _joints[i].angularYZDrive = angularYZDrive;
                    yield return null;
                }
            }

            // Reset the flag after the coroutine has finished
            _activeRagdoll.isGettingUp = false;
        }

        private void UpdateIK() {
            if (!_enableIK) {
                _animatorHelper.LeftArmIKWeight = 0;
                _animatorHelper.RightArmIKWeight = 0;
                _animatorHelper.LookIKWeight = 0;
                return;
            }
            _animatorHelper.LookIKWeight = 1;

            _animTorso = _activeRagdoll.AnimatedTorso;
            _chest = _activeRagdoll.GetAnimatedBone(HumanBodyBones.Spine);
            ReflectBackwards();
            _targetDir2D = Auxiliary.GetFloorProjection(AimDirection);
            CalculateVerticalPercent();

            UpdateLookIK();
            UpdateArmsIK();
        }

        /// <summary> Reflect the direction when looking backwards, avoids neck-breaking twists </summary>
        /// <param name=""></param>
        private void ReflectBackwards() {
            bool lookingBackwards = Vector3.Angle(AimDirection, _animTorso.forward) > 90;
            if (lookingBackwards) AimDirection = Vector3.Reflect(AimDirection, _animTorso.forward);
        }

        /// <summary> Calculate the vertical inlinacion percentage of the target direction
        /// (how much it is looking up) </summary>
        private void CalculateVerticalPercent() {
            float directionAngle = Vector3.Angle(AimDirection, Vector3.up);
            directionAngle -= 90;
            _targetDirVerticalPercent = 1 - Mathf.Clamp01((directionAngle - minTargetDirAngle) / Mathf.Abs(maxTargetDirAngle - minTargetDirAngle));
        }

        private void UpdateLookIK() {
            float lookVerticalAngle = _targetDirVerticalPercent * Mathf.Abs(maxLookAngle - minLookAngle) + minLookAngle;
            lookVerticalAngle += lookAngleOffset;
            _lookDir = Quaternion.AngleAxis(-lookVerticalAngle, _animTorso.right) * _targetDir2D;

            Vector3 lookPoint = _activeRagdoll.GetAnimatedBone(HumanBodyBones.Head).position + _lookDir;
            _animatorHelper.LookAtPoint(lookPoint);
        }

        private void UpdateArmsIK() {
            float armsVerticalAngle = _targetDirVerticalPercent * Mathf.Abs(maxArmsAngle - minArmsAngle) + minArmsAngle;
            armsVerticalAngle += armsAngleOffset;
            _armsDir = Quaternion.AngleAxis(-armsVerticalAngle, _animTorso.right) * _targetDir2D;

            float currentArmsDistance = armsDistance.Evaluate(_targetDirVerticalPercent);

            Vector3 armsMiddleTarget = _chest.position + _armsDir * currentArmsDistance;
            Vector3 upRef = Vector3.Cross(_armsDir, _animTorso.right).normalized;
            Vector3 armsHorizontalVec = Vector3.Cross(_armsDir, upRef).normalized;
            Quaternion handsRot = _armsDir != Vector3.zero? Quaternion.LookRotation(_armsDir, upRef)
                                                            : Quaternion.identity;

            _animatorHelper.LeftHandTarget.position = armsMiddleTarget + armsHorizontalVec * armsHorizontalSeparation / 2;
            _animatorHelper.RightHandTarget.position = armsMiddleTarget - armsHorizontalVec * armsHorizontalSeparation / 2;
            _animatorHelper.LeftHandTarget.rotation = handsRot * Quaternion.Euler(0, 0, 90 - handsRotationOffset);
            _animatorHelper.RightHandTarget.rotation = handsRot * Quaternion.Euler(0, 0, -90 + handsRotationOffset);

            var armsUpVec = Vector3.Cross(_armsDir, _animTorso.right).normalized;
            _animatorHelper.LeftHandHint.position = armsMiddleTarget + armsHorizontalVec * armsHorizontalSeparation - armsUpVec;
            _animatorHelper.RightHandHint.position = armsMiddleTarget - armsHorizontalVec * armsHorizontalSeparation - armsUpVec;
        }

        /// <summary> Plays an animation using the animator. The speed doesn't change the actual
        /// speed of the animator, but a parameter of the same name that can be used to multiply
        /// the speed of certain animations. </summary>
        /// <param name="animation">The name of the animation state to be played</param>
        /// <param name="speed">The speed to be set</param>
        public void PlayAnimation(string animation, float speed = 1) {
            Animator.Play(animation);
            Animator.SetFloat("speed", speed);
        }   
        
        public void UseLeftArm(float weight) {
            if (!_enableIK || !_activeRagdoll.curPhysState)
                return;

            if (_activeRagdoll.weaponID == 2) return; 
            _animatorHelper.LeftArmIKWeight = weight;
        }

        public void UseRightArm(float weight) {
            if (!_enableIK || !_activeRagdoll.curPhysState)
                return;

            if (_activeRagdoll.weaponID != 0) return;
            _animatorHelper.RightArmIKWeight = weight;
        } 
        
        public void RightArmGunAim()
        {
            if (!_enableIK || !_activeRagdoll.curPhysState) return;

            _animatorHelper.RightArmIKWeight += Time.deltaTime / 0.3f;
            if (_animatorHelper.RightArmIKWeight > 1) _animatorHelper.RightArmIKWeight = 1;
        }

        public void RightArmGunRest()
        {
            if (!_enableIK || !_activeRagdoll.curPhysState) return;

            _animatorHelper.RightArmIKWeight -= Time.deltaTime / 0.3f;
            if (_animatorHelper.RightArmIKWeight < 0) _animatorHelper.RightArmIKWeight = 0;
        }

        
        public void ResetAnimatorToState(string animeState)
        {
            if (_activeRagdoll.AnimatedAnimator != null)
            {
                _activeRagdoll.AnimatedAnimator.Play(animeState, 0, 0f);
                _activeRagdoll.AnimatedAnimator.Update(0f);
            }
        }

        public void UseAimming(float weight)
        {
            if (_activeRagdoll.isGettingUp || _activeRagdoll.beaten) return;
            _activeRagdoll.AnimatedAnimator.SetBool("aiming", weight > 0.5);
        }

        public float GetTorsoAngle()
        {
            float angle = Vector3.Angle(_activeRagdoll.PhysicalTorso.transform.up, Vector3.up);
            if (angle > 90) angle = 90;
            return angle;
        }
    }
} // namespace ActiveRagdoll