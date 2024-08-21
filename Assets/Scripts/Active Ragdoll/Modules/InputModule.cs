using System;
using System.Net.Http.Headers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ActiveRagdoll {

    /// <summary> Tells the ActiveRagdoll what it should do. Input can be external (like the
    /// one from the player or from another script) and internal (kind of like sensors, such as
    /// detecting if it's on floor). </summary>
    public class InputModule : Module {
        
        // ---------- EXTERNAL INPUT ----------
        
        public int currentPlayerControl = -1;
        public bool onlyOneDevice = false;
        public delegate void onMoveDelegate(Vector2 movement);
        public onMoveDelegate OnMoveDelegates { get; set; }
        public void OnMove(InputValue value) {
            //if (_activeRagdoll._currentControlID != currentPlayerControl) return;

            OnMoveDelegates?.Invoke(value.Get<Vector2>());
        }

        public void OnMoveP2(InputValue value)
        {
           // if (_activeRagdoll._currentControlID != currentPlayerControl) return;

            OnMoveDelegates?.Invoke(value.Get<Vector2>());
        }

        public delegate void onJumpDelegate(float movement);
        public onJumpDelegate OnJumpDelegates { get; set; }
        public void OnJump(InputValue value)
        {
            //if (_activeRagdoll._currentControlID != currentPlayerControl) return;
            OnJumpDelegates?.Invoke(value.Get<float>()); 
        }

        public void OnJumpP2(InputValue value)
        {
            //if (_activeRagdoll._currentControlID != currentPlayerControl) return;
            OnJumpDelegates?.Invoke(value.Get<float>());
        }

        public delegate void onChangeDelegate(float movement);
        public onChangeDelegate OnChangeDelegates { get; set; }
        public void OnChange(InputValue value)      
        {
            //Debug.Log(this.transform.name + " p1 " + _activeRagdoll._currentControlID + " " + currentPlayerControl);
            //if (_activeRagdoll._currentControlID != currentPlayerControl) return;
            OnChangeDelegates?.Invoke(value.Get<float>()); 
        }

        public void OnChangeP2(InputValue value)
        {
            //Debug.Log(this.transform.name + " p2 " + _activeRagdoll._currentControlID + " " + currentPlayerControl);
            //if (_activeRagdoll._currentControlID != currentPlayerControl) return;
            OnChangeDelegates?.Invoke(value.Get<float>());
        }

        public void OnQuit(InputValue value)
        {
            if (value.Get<float>() > 0.5)
            {
                Controller.instance.askQuit = true;
            }
            else
            {
                Controller.instance.askQuit = false;

            }
        }

        public delegate void onAttackDelegate(float attackWeight);
        public onAttachDelegate onAttackDelegates { get; set; }
        public void OnAttack(InputValue value)
        {
            //if (_activeRagdoll._currentControlID != currentPlayerControl) return;
            onAttackDelegates?.Invoke(value.Get<float>());
        }
        public void OnAttackP2(InputValue value)
        {
            //if (_activeRagdoll._currentControlID != currentPlayerControl) return;
            onAttackDelegates?.Invoke(value.Get<float>());
        }
        public delegate void onAttachDelegate(float attachWeight);
        public onAttachDelegate onAttachDelegates { get; set; }
        public void OnAttach(InputValue value)
        {
            //if (_activeRagdoll._currentControlID != currentPlayerControl) return;
            onAttachDelegates?.Invoke(value.Get<float>());
        }

        public void OnAttachP2(InputValue value)
        {
            //if (_activeRagdoll._currentControlID != currentPlayerControl) return;
            onAttachDelegates?.Invoke(value.Get<float>());
        }

        public delegate void onLeftArmDelegate(float armWeight);
        public onLeftArmDelegate OnLeftArmDelegates { get; set; }
        public void OnLeftArm(InputValue value) {
            //if (_activeRagdoll._currentControlID != currentPlayerControl) return;
            OnLeftArmDelegates?.Invoke(value.Get<float>());
        }

        public void OnLeftArmP2(InputValue value)
        {
            //if (_activeRagdoll._currentControlID != currentPlayerControl) return;
            OnLeftArmDelegates?.Invoke(value.Get<float>());
        }

        public delegate void onShootDelegate(float armWeight);
        public onShootDelegate OnShootDelegates { get; set; }
        public void OnShoot(InputValue value) {
            //if (_activeRagdoll._currentControlID != currentPlayerControl) return;
            OnShootDelegates?.Invoke(value.Get<float>());
        }

        public void OnShootP2(InputValue value)
        {
           // if (_activeRagdoll._currentControlID != currentPlayerControl) return;
            OnShootDelegates?.Invoke(value.Get<float>());
        }

        public delegate void onRightArmDelegate(float armWeight);
        public onRightArmDelegate OnRightArmDelegates { get; set; }
        public void OnRightArm(InputValue value) {
            if (_activeRagdoll._currentControlID != currentPlayerControl) return;
            OnRightArmDelegates?.Invoke(value.Get<float>());
        }
        
        public delegate void onAimDelegate(float armWeight);
        public onAimDelegate OnAimDelegates { get; set; }
        public void OnAim(InputValue value) {
           // if (_activeRagdoll._currentControlID != currentPlayerControl) return;
            OnAimDelegates?.Invoke(value.Get<float>());
        }
        public void OnAimP2(InputValue value)
        {
            //if (_activeRagdoll._currentControlID != currentPlayerControl) return;
            OnAimDelegates?.Invoke(value.Get<float>());
        }

        

        public delegate void OnCrouchDelegate(float movement);
        public OnCrouchDelegate OnCrouchDelegates { get; set; }
        public void OnCrouch(InputValue value)
        {
            // if (_activeRagdoll._currentControlID != currentPlayerControl) return;
            OnCrouchDelegates?.Invoke(value.Get<float>());
        }
        public void OnCrouchP2(InputValue value)
        {
            // if (_activeRagdoll._currentControlID != currentPlayerControl) return;
            OnCrouchDelegates?.Invoke(value.Get<float>());
        }

        public void OnSwitch(InputValue value)
        {
            if (onlyOneDevice)
            {
                currentPlayerControl = value.Get<float>() < 0 ? -1 : 1;
                _activeRagdoll.currentControlIDfromInput = currentPlayerControl;
            }
        }

        public bool isRunning = false;
        public void OnRunP2(InputValue value)
        {
            if (value.Get<float>() > 0.5f)
            {
                isRunning = true;
            }
            else
            {
                isRunning = false;
            }
        }

        // ---------- INTERNAL INPUT ----------

        [Header("--- FLOOR ---")]
        public float floorDetectionDistance = 20.0f;
        public float maxFloorSlope = 60;

        private bool _isOnFloor = true;
        public bool IsOnFloor { get { return _isOnFloor; } }

        Rigidbody _rightFoot, _leftFoot;


        void Start() {
            if (!onlyOneDevice)
            {
                if (this.gameObject.name == "Character1")
                {
                    currentPlayerControl = -1;
                }
                else
                {
                    currentPlayerControl = 1;
                }
            }

            _rightFoot = _activeRagdoll.GetPhysicalBone(HumanBodyBones.RightFoot).GetComponent<Rigidbody>();
            _leftFoot = _activeRagdoll.GetPhysicalBone(HumanBodyBones.LeftFoot).GetComponent<Rigidbody>();
            
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            // currentPlayerControl = -1;
        }

        void Update() {
            UpdateOnFloor();
        }

        public delegate void onFloorChangedDelegate(bool onFloor);
        public onFloorChangedDelegate OnFloorChangedDelegates { get; set; }
        private void UpdateOnFloor() {
            bool lastIsOnFloor = _isOnFloor;

            _isOnFloor = CheckRigidbodyOnFloor(_rightFoot, out Vector3 foo)
                         || CheckRigidbodyOnFloor(_leftFoot, out foo);

            if (_isOnFloor != lastIsOnFloor)
                OnFloorChangedDelegates(_isOnFloor);

            _activeRagdoll.floorNormal = foo;
        }

        /// <summary>
        /// Checks whether the given rigidbody is on floor
        /// </summary>
        /// <param name="bodyPart">Part of the body to check</param
        /// <returns> True if the Rigidbody is on floor </returns>
        public bool CheckRigidbodyOnFloor(Rigidbody bodyPart, out Vector3 normal) {
            // Raycast
            bool onFloor;
            if (!Controller.instance.AfterResetDown) onFloor = true;
            else
            {
                Ray ray = new Ray(bodyPart.position, Vector3.down);
                onFloor = Physics.Raycast(ray, out RaycastHit info, floorDetectionDistance, ~(1 << bodyPart.gameObject.layer));

                /*// Additional checks
                onFloor = onFloor && Vector3.Angle(info.normal, Vector3.up) <= maxFloorSlope;

                if (onFloor && info.collider.gameObject.TryGetComponent<Floor>(out Floor floor))
                        onFloor = floor.isFloor;

                normal = info.normal;*/

            }
            normal = Vector3.up;
            return onFloor;
        }
    }
} // namespace ActiveRagdoll