#pragma warning disable 649

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace ActiveRagdoll {

    [RequireComponent(typeof(InputModule))]
    public class ActiveRagdoll : MonoBehaviour
    {
        public int LeftArmAttack { get; set; }
        public int weaponID = 0; // 0 empty, 1 hammer, 2 gun

        [Header("--- GENERAL ---")]
        [SerializeField] public int _currentControlID = -1;
        public int currentControlIDfromInput = -1;
        
        [SerializeField] private int _solverIterations = 12;
        [SerializeField] private int _velSolverIterations = 4;
        [SerializeField] private float _maxAngularVelocity = 50;

        [SerializeField] public Controller _controller;
        public int SolverIterations { get { return _solverIterations; } }
        public int VelSolverIterations { get { return _velSolverIterations; } }
        public float MaxAngularVelocity { get { return _maxAngularVelocity; } }

        public InputModule Input { get; private set; }

        public class HitState
        {
            public bool hit = false;
            public Vector3 initialVelocity = Vector3.zero;
        }

        public HitState _hitState = new HitState();

        /// <summary> The unique ID of this Active Ragdoll instance. </summary>
        public uint ID { get; private set; }
        private static uint _ID_COUNT = 0;

        [Header("--- BODY ---")]
        [SerializeField] private Transform _animatedTorso;
        [SerializeField] private Rigidbody _physicalTorso;
        [SerializeField] private Transform _animatedChest;
        [SerializeField] private Rigidbody _physicalChest;
        [SerializeField] private GameObject _physCharacter; 
        [SerializeField] private GameObject _animCharacter;
        public GameObject PhysCharacter { get { return _physCharacter; } }
        public GameObject AnimCharacter { get { return _animCharacter; } }
        
        public Transform AnimatedTorso { get { return _animatedTorso; } }
        public Rigidbody PhysicalTorso { get { return _physicalTorso; } }
        public Transform AnimatedChest { get { return _animatedChest; } }
        public Rigidbody PhysicalChest { get { return _physicalChest; } }

        public bool onFloor = true;

        public Transform[] AnimatedBones { get; private set; }
        public List<Transform> MaskedBonesPhys { get; private set; }
        public List<Transform> MaskedBonesAnim { get; private set; }
        public List<Transform> AnimatedBonesKine { get; private set; }
        
        private ConfigurableJoint[] joints = null;

        public ConfigurableJoint[] Joints
        {
            get { return joints; }
            set { joints = value; }
        }
        
        [SerializeField]
        private List<ConfigurableJoint> jointsKine = null;
        public List<ConfigurableJoint> JointsKine
        {
            get { return jointsKine; }
            private set { jointsKine = value; }
        }
        
        [SerializeField]
        private List<bool> jointsMask = null;
        public List<bool> JointsMask
        {
            get { return jointsMask; }
            private set { jointsMask = value; }
        }

        
        public Rigidbody[] Rigidbodies { get; private set; }

        [SerializeField] private List<BodyPart> _bodyParts;
        public List<BodyPart> BodyParts { get { return _bodyParts; } }

        [Header("--- ANIMATORS ---")]
        [SerializeField] private Animator _animatedAnimator;
        [SerializeField] private Animator _physicalAnimator;

        public bool beaten = false;
        public bool isGettingUp = false;
        public bool isGettingUpAnimated = false;
        
        public Animator AnimatedAnimator {
            get { return _animatedAnimator; }
            private set { _animatedAnimator = value; }
        }

        [SerializeField] private bool _moveAnimator = false;
        
        public AnimatorHelper AnimatorHelper { get; private set; }
        /// <summary> Whether to constantly set the rotation of the Animated Body to the Physical Body's.</summary>
        public bool SyncTorsoPositions { get; set; } = true;
        public bool SyncTorsoRotations { get; set; } = true;

        public Vector3 floorNormal = Vector3.up;
        
        private void OnValidate() {

            // Automatically retrieve the necessary references
            var animators = GetComponentsInChildren<Animator>();
            if (animators.Length >= 2)
            {
                if (_animatedAnimator == null) _animatedAnimator = animators[0];
                if (_physicalAnimator == null) _physicalAnimator = animators[1];

                if (_animatedTorso == null)
                    _animatedTorso = _animatedAnimator.GetBoneTransform(HumanBodyBones.Hips);
                if (_physicalTorso == null)
                    _physicalTorso = _physicalAnimator.GetBoneTransform(HumanBodyBones.Hips).GetComponent<Rigidbody>();
            }

            if (_bodyParts.Count == 0)
                GetDefaultBodyParts();
        }

        private void Awake() {
            ID = _ID_COUNT++;

            if (AnimatedBones == null) AnimatedBones = _animatedTorso?.GetComponentsInChildren<Transform>();
            AnimatedBonesKine = new List<Transform>();
            MaskedBonesAnim = new List<Transform>();
            MaskedBonesPhys = new List<Transform>();
            Joints = _physicalTorso?.GetComponentsInChildren<ConfigurableJoint>();
            JointsKine = new List<ConfigurableJoint>();
            JointsMask = new List<bool>();

            if (Rigidbodies == null) Rigidbodies = _physicalTorso?.GetComponentsInChildren<Rigidbody>();

            foreach (Rigidbody rb in Rigidbodies) {
                rb.solverIterations = _solverIterations;
                rb.solverVelocityIterations = _velSolverIterations;
                rb.maxAngularVelocity = _maxAngularVelocity;
                
            }

            foreach (BodyPart bodyPart in _bodyParts)
                bodyPart.Init();
            
            AnimatorHelper = _animatedAnimator.gameObject.AddComponent<AnimatorHelper>();
            if (TryGetComponent(out InputModule temp))
                Input = temp;
#if UNITY_EDITOR
            else
                Debug.LogError("InputModule could not be found. An ActiveRagdoll must always have" +
                                "a peer InputModule.");
#endif
            GatherAllKineBonesAndJoints();

            // DisableAllRigging();
        }

        public bool curPhysState = true;

        private void Start()
        {
            SetAndChangeWeaponState(0);
        }


        public GameObject gunGO;
        public GameObject hamGO;
        
        
        public void ChangeWeaponState()
        {
            weaponID += 1;
            weaponID %= 3;

            if (weaponID == 0)
            {
                // gun -> empty
                if (gunGO != null) gunGO.SetActive(false);
                if (hamGO != null) hamGO.SetActive(false);
            }
            
            else if (weaponID == 1)
            {
                // empty -> hammer
                // do nothing temp
                if (gunGO != null) gunGO.SetActive(false);
                if (hamGO != null) hamGO.SetActive(true);
            }
            
            else if (weaponID == 2)
            {
                // hammer -> gum
                if (gunGO != null) gunGO.SetActive(true);
                if (hamGO != null) hamGO.SetActive(false);
            }
            
            /*if (curPhysState)
            {
                DeVisualizePhys();
                EnableAllRigging();
            }
            else
            {
                VisualizePhys();
                DisableAllRigging();
            }                


            curPhysState = !curPhysState;*/
        }
        
        public void SetAndChangeWeaponState(int id)
        {
            weaponID = id;

            if (weaponID == 0)
            {
                // gun -> empty
                if (gunGO != null) gunGO.SetActive(false);
                if (hamGO != null) hamGO.SetActive(false);
            }
            
            else if (weaponID == 1)
            {
                // empty -> hammer
                // do nothing temp
                if (gunGO != null) gunGO.SetActive(false);
                if (hamGO != null) hamGO.SetActive(true);
            }
            
            else if (weaponID == 2)
            {
                // hammer -> gum
                if (gunGO != null) gunGO.SetActive(true);
                if (hamGO != null) hamGO.SetActive(false);
            }
            
            /*if (curPhysState)
            {
                DeVisualizePhys();
                EnableAllRigging();
            }
            else
            {
                VisualizePhys();
                DisableAllRigging();
            }


            curPhysState = !curPhysState;*/
        }

        private void DisableAllRigging()
        {
            var rigLayerBodyAimTrans = _animCharacter.transform.Find("RigLayer_BodyAim");
            rigLayerBodyAimTrans.gameObject.GetComponent<Rig>().weight = 0;
            var rigLayerGunAimTrans = _animCharacter.transform.Find("RigLayer_GunAiming");
            rigLayerGunAimTrans.gameObject.GetComponent<Rig>().weight = 0;
            var rigLayerGunPoseTrans = _animCharacter.transform.Find("RigLayer_GunPose");
            rigLayerGunPoseTrans.gameObject.GetComponent<Rig>().weight = 0;
            var rigLayerHandIKTrans = _animCharacter.transform.Find("RigLayer_HandIK");
            rigLayerHandIKTrans.gameObject.GetComponent<Rig>().weight = 0;
            //_animCharacter.GetComponent<CharacterAiming>().enabled = false;
        }
        
        private void EnableAllRigging()
        {
            var rigLayerBodyAimTrans = _animCharacter.transform.Find("RigLayer_BodyAim");
            rigLayerBodyAimTrans.gameObject.GetComponent<Rig>().weight = 1;
            /*var rigLayerGunAimTrans = _physCharacter.transform.Find("RigLayer_GunAiming");
            rigLayerGunAimTrans.gameObject.GetComponent<Rig>().weight = 1;*/
            var rigLayerGunPoseTrans = _animCharacter.transform.Find("RigLayer_GunPose");
            rigLayerGunPoseTrans.gameObject.GetComponent<Rig>().weight = 1;
            var rigLayerHandIKTrans = _animCharacter.transform.Find("RigLayer_HandIK");
            rigLayerHandIKTrans.gameObject.GetComponent<Rig>().weight = 1;
            //_animCharacter.GetComponent<CharacterAiming>().enabled = true;

        }
        
        private void DeVisualizePhys()
        {
            var bodyPhyVisMeshTrans = _physCharacter.transform.Find("Body");
            bodyPhyVisMeshTrans.gameObject.SetActive(false);     
            var bodyAniVisMeshTrans = _animCharacter.transform.Find("Body");
            bodyAniVisMeshTrans.gameObject.SetActive(true);     
            var gunAniVisMeshTrans = _animCharacter.transform.Find("GunPivot/StylizedPistol");
            gunAniVisMeshTrans.gameObject.SetActive(true);     
        }
        
        private void VisualizePhys()
        {
            var bodyPhyVisMeshTrans = _physCharacter.transform.Find("Body");
            bodyPhyVisMeshTrans.gameObject.SetActive(true);     
            var bodyAniVisMeshTrans = _animCharacter.transform.Find("Body");
            bodyAniVisMeshTrans.gameObject.SetActive(false);     
            var gunAniVisMeshTrans = _animCharacter.transform.Find("GunPivot/StylizedPistol");
            gunAniVisMeshTrans.gameObject.SetActive(false);     
        }

        
        
        private void GatherAllKineBonesAndJoints()
        {
            AnimatedBonesKine = new List<Transform>();
            AnimatedBonesKine.Add(_physicalTorso.transform);

            List<HumanBodyBones> bones = new List<HumanBodyBones>
            {
                HumanBodyBones.Head, HumanBodyBones.Neck, 
                HumanBodyBones.Spine, HumanBodyBones.Chest, HumanBodyBones.UpperChest,
                HumanBodyBones.LeftShoulder, HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand,
                HumanBodyBones.RightShoulder, HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand,
                HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot,
                HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot
            };
            
            List<HumanBodyBones> bonesAnime = new List<HumanBodyBones>
            {
                /*HumanBodyBones.Head, HumanBodyBones.Neck, 
                HumanBodyBones.Spine, HumanBodyBones.Chest, HumanBodyBones.UpperChest,
                HumanBodyBones.LeftShoulder, HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand,
                HumanBodyBones.RightShoulder, HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand,*/
            };
            
            foreach (var bone in bones)
            {
                Transform boneTransform = _physicalAnimator.GetBoneTransform(bone);
                Transform boneTransformAnim = _animatedAnimator.GetBoneTransform(bone);
                if (boneTransform != null && (boneTransform.TryGetComponent(out ConfigurableJoint joint)))
                {
                    jointsKine.Add(joint);
                    if (bonesAnime.Contains(bone))
                    {
                        jointsMask.Add(false);
                        MaskedBonesPhys.Add(boneTransform);
                        MaskedBonesAnim.Add(boneTransformAnim);
                        var rb = boneTransform.parent.GetComponent<Rigidbody>();
                        rb.isKinematic = false;
                    }
                    else
                    {
                        jointsMask.Add(true);
                    }
                }
            }
            
            foreach (var bone in bones)
            {
                Transform boneTransform = _animatedAnimator.GetBoneTransform(bone);
                if (boneTransform != null)
                {
                    AnimatedBonesKine.Add(boneTransform);
                }
            }
        }
        
        
        
        private void GetDefaultBodyParts()
        {
            
            _bodyParts.Add(new BodyPart("Head Neck",
                TryGetJoints(HumanBodyBones.Head, HumanBodyBones.Neck)));
            _bodyParts.Add(new BodyPart("Torso",
               TryGetJoints(HumanBodyBones.Spine, HumanBodyBones.Chest, HumanBodyBones.UpperChest)));
            _bodyParts.Add(new BodyPart("Left Arm",
                TryGetJoints(HumanBodyBones.LeftShoulder, HumanBodyBones.LeftUpperArm, 
                    HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand)));
            _bodyParts.Add(new BodyPart("Right Arm",
                TryGetJoints(HumanBodyBones.RightShoulder, HumanBodyBones.RightUpperArm, 
                    HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand)));
            _bodyParts.Add(new BodyPart("Left Leg",
                TryGetJoints(HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot)));
            _bodyParts.Add(new BodyPart("Right Leg",
                TryGetJoints(HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot)));
        }
        

        private List<ConfigurableJoint> TryGetJoints(params HumanBodyBones[] bones) {
            List<ConfigurableJoint> jointList = new List<ConfigurableJoint>();
            foreach (HumanBodyBones bone in bones) {
                Transform boneTransform = _physicalAnimator.GetBoneTransform(bone);
                if (boneTransform != null && (boneTransform.TryGetComponent(out ConfigurableJoint joint)))
                {
                    jointList.Add(joint);
                    // AnimatedBonesKine.Add(boneTransform);
                }
            }
            
            return jointList;
        }

        private void FixedUpdate() {
            SyncAnimatedBody();

            /*if (beaten && curPhysState)
            {
                ChangeWeaponState();
            }*/
        }

        /// <summary> Updates the rotation and position of the animated body's root
        /// to match the ones of the physical.</summary>
        private void SyncAnimatedBody() {
            // This is needed for the IK movements to be synchronized between
            // the animated and physical bodies. e.g. when looking at something,
            // if the animated and physical bodies are not in the same spot and
            // with the same orientation, the head movement calculated by the IK
            // for the animated body will be different from the one the physical body
            // needs to look at the same thing, so they will look at totally different places.
            // if (beaten) return;

            /*if (SyncTorsoPositions && _moveAnimator)
            {
                _animatedAnimator.transform.position = _physicalTorso.position
                                + (_animatedAnimator.transform.position - _animatedTorso.position);

            }
            if (SyncTorsoRotations && _moveAnimator)
            {
            }*/

            _animatedAnimator.transform.position = _physicalTorso.position
                                + (_animatedAnimator.transform.position - _animatedTorso.position);
            _animatedAnimator.transform.rotation = _physicalTorso.rotation;

        }

        public float GetTorsoAngle()
        {
            float angle = Vector3.Angle(PhysicalTorso.transform.up, Vector3.up);
            if (angle > 90) angle = 90;
            return angle;
        }


        // ------------------- GETTERS & SETTERS -------------------

        /// <summary> Gets the transform of the given ANIMATED BODY'S BONE </summary>
        /// <param name="bone">Bone you want the transform of</param>
        /// <returns>The transform of the given ANIMATED bone</returns>
        public Transform GetAnimatedBone(HumanBodyBones bone) {
            return _animatedAnimator.GetBoneTransform(bone);
        }

        /// <summary> Gets the transform of the given PHYSICAL BODY'S BONE </summary>
        /// <param name="bone">Bone you want the transform of</param>
        /// <returns>The transform of the given PHYSICAL bone</returns>
        public Transform GetPhysicalBone(HumanBodyBones bone) {
            return _physicalAnimator.GetBoneTransform(bone);
        }

        public BodyPart GetBodyPart(string name) {
            foreach (BodyPart bodyPart in _bodyParts)
                if (bodyPart.bodyPartName == name) return bodyPart;

            return null;
        }

        public void SetStrengthScaleForAllBodyParts (float scale) {
            foreach (BodyPart bodyPart in _bodyParts)
                bodyPart.SetStrengthScale(scale);
        }

        private void Update()
        {
            // Debug.Log("paused: " + Controller.instance.paused);
        }
    }
} // namespace ActiveRagdoll