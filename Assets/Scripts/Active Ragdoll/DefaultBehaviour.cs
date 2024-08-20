using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ActiveRagdoll;
using UnityEngine.UI;

/// <summary> Default behaviour of an Active Ragdoll </summary>
public class DefaultBehaviour : MonoBehaviour {
    // Author: Sergio Abreu García | https://sergioabreu.me

    [Header("Modules")]
    [SerializeField] private ActiveRagdoll.ActiveRagdoll _activeRagdoll;
    [SerializeField] private PhysicsModule _physicsModule;
    [SerializeField] private AnimationModule _animationModule;
    [SerializeField] private GripModule _gripModule;
    [SerializeField] private MotionModule _motionModule;
    [SerializeField] private InputModule _inputModule;
    [SerializeField] private CameraModule _cameraModule;
    [SerializeField] private WeaponModule _weaponModule;

    [Header("Movement")]
    [SerializeField] private bool _enableMovement = true;
    [SerializeField] private float _turningRatio = 0.8f;
    [SerializeField] private float _moveImpulseScale = 0.5f;
    private Vector2 _movement;

    private Vector3 _aimDirection;

    private void OnValidate() {
        if (_activeRagdoll == null) _activeRagdoll = GetComponent<ActiveRagdoll.ActiveRagdoll>();
        if (_physicsModule == null) _physicsModule = GetComponent<PhysicsModule>();
        if (_animationModule == null) _animationModule = GetComponent<AnimationModule>();
        if (_gripModule == null) _gripModule = GetComponent<GripModule>();
        if (_weaponModule == null) _weaponModule = GetComponent<WeaponModule>();
        if (_motionModule == null) _motionModule = GetComponent<MotionModule>();
        if (_cameraModule == null) _cameraModule = GetComponent<CameraModule>();
    }

    private void Start() {
        // Link all the functions to its input to define how the ActiveRagdoll will behave.
        // This is a default implementation, where the input player is binded directly to
        // the ActiveRagdoll actions in a very simple way. But any implementation is
        // possible, such as assigning those same actions to the output of an AI system.

        _activeRagdoll.Input.OnMoveDelegates += MovementInput;
        _activeRagdoll.Input.OnMoveDelegates += _physicsModule.ManualTorqueInput;
        _activeRagdoll.Input.OnFloorChangedDelegates += ProcessFloorChanged;

        _activeRagdoll.Input.OnLeftArmDelegates += _animationModule.UseLeftArm;
        _activeRagdoll.Input.OnLeftArmDelegates += _gripModule.UseLeftGrip;
        // _activeRagdoll.Input.OnLeftArmDelegates += _weaponModule.GunShoot;
        _activeRagdoll.Input.OnShootDelegates += _weaponModule.GunShoot;
        
        /*_activeRagdoll.Input.OnRightArmDelegates += _animationModule.UseRightArm;
        _activeRagdoll.Input.OnRightArmDelegates += _gripModule.UseRightGrip;*/
        /*_activeRagdoll.Input.OnAimDelegates += _animationModule.UseAimming;
        _activeRagdoll.Input.OnAimDelegates += _animationModule.UseRightArm;
        _activeRagdoll.Input.OnAimDelegates += _gripModule.UseRightGrip;*/

        _activeRagdoll.Input.OnChangeDelegates += _weaponModule.ChangeWeapon;
        
        _activeRagdoll.Input.onAttachDelegates += _gripModule.UseAttachGrip; // must put last

        _activeRagdoll.Input.onAttackDelegates += _motionModule.LeftAttack;
        // _activeRagdoll.Input.onAttackDelegates += _motionModule.RightAttack;

        _activeRagdoll.Input.OnCrouchDelegates += CrouchAction;
        
        _activeRagdoll.Input.OnJumpDelegates += JumpAction;

        _activeRagdoll.Input.OnAimDelegates += _weaponModule.Aim;

    }

    private void Update() {
        _aimDirection = _cameraModule.Camera.transform.forward;
        _animationModule.AimDirection = _aimDirection;

        UpdateMovement();

#if UNITY_EDITOR
        // TEST
        /*if (Input.GetKeyDown(KeyCode.F1))
            Debug.Break();*/
#endif
    }

    private void JumpAction(float value)
    {
        if (_activeRagdoll.beaten) return;
        _animationModule.Animator.SetBool("jumping", value > 0.5);
    }
    

    private void UpdateMovement() {
        if (_activeRagdoll.beaten) return;

        // update forward
        float angleOffset = Vector2.SignedAngle(_movement, Vector2.up);
        Vector3 targetForward = Quaternion.AngleAxis(angleOffset * _turningRatio, Vector3.up) * Auxiliary.GetFloorProjection(_aimDirection);
        _physicsModule.TargetDirection = targetForward;
        
        if (_movement == Vector2.zero || !_enableMovement) {
            _animationModule.Animator.SetBool("moving", false);
            _animationModule.Animator.SetBool("running", false);
            return;
        }

        // bool running = _inputModule.isRunning;
        bool running = Input.GetKey("left shift") || _inputModule.isRunning;
        _animationModule.Animator.SetBool("moving", true);
        _animationModule.Animator.SetBool("running", running);
        _animationModule.Animator.SetFloat("speed", _movement.magnitude);

        float scale = running ? 2.0f : 1.0f;
        Vector3 vel = scale * Vector3.Normalize(targetForward) * _moveImpulseScale * Time.deltaTime;
        vel.y = (running ? -0.5f : 0)  * _moveImpulseScale * Time.deltaTime;
        _activeRagdoll.PhysicalTorso.velocity += vel;
        
    }

    private void CrouchAction(float value)
    {
        if (_activeRagdoll.beaten) return;
        _animationModule.Animator.SetBool("crouching", value > 0.5);
    }

    private IEnumerator teleport(ActiveRagdoll.ActiveRagdoll ac, Vector3 target_position)
    {

        yield return new WaitForSeconds(2);
        Vector3 diff = target_position - ac.PhysicalTorso.transform.position +  new Vector3(0, 3, 0);
        Vector3 diff2 = ac._currentControlID  == -1 ? new Vector3(1, 3, 1) : new Vector3(-1, 3, -1);
        var rbs = ac.PhysicalTorso.GetComponentsInChildren<Rigidbody>();
        foreach (var rb in rbs)
        {
            rb.position = rb.position + diff;
        }
        _activeRagdoll.onFloor = true;
        _activeRagdoll.SetAndChangeWeaponState(0);

    }

    private void ProcessFloorChanged(bool onFloor) {
        if (onFloor) {
            _activeRagdoll.onFloor = true;
            /*_physicsModule.SetBalanceMode(PhysicsModule.BALANCE_MODE.UPRIGHT_TORQUE);
            _enableMovement = true;
            _activeRagdoll.GetBodyPart("Head Neck")?.SetStrengthScale(1);
            _activeRagdoll.GetBodyPart("Right Leg")?.SetStrengthScale(1);
            _activeRagdoll.GetBodyPart("Left Leg")?.SetStrengthScale(1);
            _animationModule.PlayAnimation("Idle");*/
        }
        else {
            _activeRagdoll.onFloor = false;
            StartCoroutine(teleport(_activeRagdoll, Controller.instance._currentEnvironment.transform.position));
            /*Vector3 diff = Controller.instance._currentEnvironment.transform.position - _activeRagdoll.PhysicalTorso.transform.position + new Vector3(0, 3, 0);
            var rbs = _activeRagdoll.PhysicalTorso.GetComponentsInChildren<Rigidbody>();
            foreach (var rb in rbs)
            {
                rb.position = rb.position + diff;
            }*/

            /*_physicsModule.SetBalanceMode(PhysicsModule.BALANCE_MODE.MANUAL_TORQUE);
            _enableMovement = false;
            _activeRagdoll.GetBodyPart("Head Neck")?.SetStrengthScale(0.1f);
            _activeRagdoll.GetBodyPart("Right Leg")?.SetStrengthScale(0.05f);
            _activeRagdoll.GetBodyPart("Left Leg")?.SetStrengthScale(0.05f);
            _animationModule.PlayAnimation("InTheAir");*/
        }
    }

    /// <summary> Make the player move and rotate </summary>
    private void MovementInput(Vector2 movement) {
        if (_activeRagdoll.beaten)
        {
            _movement.Set(0, 0);
            return;
        }

        _movement = movement;
    }
}
