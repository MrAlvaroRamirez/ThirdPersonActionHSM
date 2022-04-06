using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStateMachine : MonoBehaviour
{
    #region Variables

    [Space(20)]
    [Header("Object References")]
    [Space(20)]

    //Variables: referencias
    //Variables: referencias
    public Animator animator;
    public Transform thirdPersonCamera;
    public Transform playerModel;
    public PlayerLookAtTransform lookTarget;
    [HideInInspector] public ActionCamera actionCamera;
    [HideInInspector] public ParticleHelper particleHelper;
    [HideInInspector] public CharacterController characterController;
    public ParticleSystem windParticles;
    [HideInInspector] public Vector3 characterCenter;
    private CharacterInputHandler inputHandler;

    [Space(20)]
    [Header("Movement")]
    [Space(20)]

    //Variables: movimiento
    public float walkSpeed = 15.0f;
    [SerializeField] private float rotationSpeed = 15.0f;
    public float tiltSpeed = 15.0f;
    public float runMultiplier = 3.0f;
    public float currentSpeed = 0;
    [HideInInspector] public float targetSpeed = 0;
    [SerializeField] private float airResistance = 10;
    [SerializeField] private float groundFriction = 40;

    //Variables: input del jugador
    [HideInInspector] public Vector2 rawMovementInput = Vector2.zero;
    [HideInInspector] public Vector2 currentMovementInput = Vector2.zero;
    [HideInInspector] public Vector2 lastMovementInput = new Vector2(0,1);
     public Vector3 currentMovement;
    [HideInInspector] public float blendSmooth = 0;

    //Rotacion
    [HideInInspector] public Quaternion targetRotation = Quaternion.identity;
    [HideInInspector] public Quaternion currentRotation = Quaternion.identity;
    private Vector3 playerModelRotation;
    [HideInInspector] public float targetTilt;

    //Easing
    [HideInInspector] public float velocity2 = 0;
    [SerializeField] private float easeFactor = 0.2f;

    //Estados
    public bool isSkilling = false; //(replacement for ??)
    [HideInInspector] public bool isMovementPressed;
    public bool isDashing;
    private bool isSpeedDashing;
    [HideInInspector] public bool isJumpPressed = false;
    [HideInInspector] public bool requireNewJump = false;
    [HideInInspector] public bool isRunPressed;
    public bool isFloorBelow;
    private IEnumerator resetMovement;
    public delegate void UnlockMovement();
    public event UnlockMovement movementUnlocked;

    [Space(20)]
    [Header("Jump")]
    [Space(20)]

    //Salto
    [HideInInspector] public float initialJumpVelocity;
    [SerializeField] private float maxJumpHeight = 15.0f;
    [SerializeField] private float maxJumpTime = 2f;
    [HideInInspector] public bool canDoubleJump = true;
    public DashSetting doubleJumpDash;
    private bool m_isChargingBoost = false;
    public bool isChargingBoost {
        get{
            return m_isChargingBoost;
        }
        set{
            if(m_isChargingBoost != value)
            {
                m_isChargingBoost = value;
                gravity *= value? .1f : 10f;
            }
        }
    }
    public Transform airBoostFx;
    public Vector3 airBoostPositionOffset;

    //Gravedad
    [HideInInspector] public float gravity = -9.8f;
    public float groundedGravity = -.05f;

    //Grounded
    public Vector3 distanceToGround;
   public float groundedRadius;
    public LayerMask groundLayer;

    [Space(20)]
    [Header("Dash")]
    [Space(20)]

    //Dash/Roll
    public DashSetting sprintDash = new DashSetting();
    private IEnumerator dashCoroutine;
    public ParticleFX DashParticle;
    
    [Space(20)]
    [Header("Wall Run")]
    [Space(20)]

    //Wall Run
    public float wallRunSpeedMultiplier = 1.3f;  
    public float wallRunMinDistance = 10f;
    public float wallRunGravityMultiplier = 0.04f;
    [SerializeField] private float wallRunCooldown = 0.6f;
    private IEnumerator activateWallRun;
    [HideInInspector] public bool leftWallColliding = false, rightWallColliding = false;
    public bool canWallRun = true;

    [Space(20)]
    [Header("Ledge Climb")]
    [Space(20)]

    //Wall detection
    public LayerMask ledgeLayer;
    public Vector3 wallDetectionBoxExtents;
    public Vector3 wallDetectionBoxPosition;
    public Vector3 ledgeStartHeight;
    [SerializeField] private Vector3 roofCheckPos;
    [SerializeField] private float roofCheckLenght;
    public float ledgeMaxHeight;
    public float ledgeSpread;
    [HideInInspector] public bool isCollidingWall = false;
    private bool isRoof = false;
    [HideInInspector] public Vector3 climbPos;
    public float maxSurfaceTilt;
    public Vector3 hangPosOffset;

    [Space(20)]
    [Header("Slope Slide")]
    [Space(20)]

    //Sliding Slopes
    public float slopeSpeed;
    public float maxSlopeSpeed;
    [HideInInspector] public Vector3 hitPointNormal;
    [HideInInspector] public float hitPointAngle;
    private Vector3 collisionPoint;
    [HideInInspector] public Vector3 slopeDirection;
    [HideInInspector] public float smoothXaxis;
    [HideInInspector] public float xVelocity = 0.0f;
    [HideInInspector] public bool isSliding;

    [Space(20)]
    [Header("Combat")]
    [Space(20)]

    //Combate
    public int currentCombo = 0;
    public int comboMax = 3;
    public int currentAirCombo = 0;
    public int airComboMax = 2;
    [HideInInspector] public bool canAttack = true;
    [HideInInspector] public bool airComboSeekingRestart = false;
    public DashSetting[] comboDashes;
    public DashSetting[] airComboDashes;
    public float enemyBoundsOffset;
    public Vector3 enemyTargetBoxExtents;
    public float airHitBoxVerticalValue = 4;
    public LayerMask enemyLayerMask;
    public float enemyTargetOffset = 2;
    [HideInInspector] public IEnumerator comboReset;
    [HideInInspector] public Tween attackTween;
    public float longAttackDistance = 1;
    public EnemyBase enemyTarget;
    public LayerMask collisionLayer;
    [HideInInspector] public bool justHit = false;
    public DashSetting kncockbackDash;

    [Space(20)]
    [Header("FX")]
    [Space(20)]

    //Particles
    public ParticleFX hitFX;
    public Transform fistFX;
    public ParticlePosition[] comboParticles;
    public ParticlePosition[] airComboParticles;
    public ParticleFX damageFX;

    //state
    public PlayerBaseState CurrentState;
    PlayerStateFactory _states;

    #endregion

    #region Initialization
    private void Start()
    {
        //TEMPORAL!! Hide mouse
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        Time.timeScale = 1f;
        Application.targetFrameRate = -1;
        QualitySettings.vSyncCount = 1;

        //Assign references
        characterController = GetComponent<CharacterController>();
        particleHelper = GetComponent<ParticleHelper>();
        actionCamera = FindObjectOfType<ActionCamera>();

        //setup state
        _states = new PlayerStateFactory(this);
        CurrentState = _states.Grounded();
        CurrentState.EnterState();

        //Character Input
        inputHandler = CharacterInputHandler.instance;

        inputHandler.input.CharacterControls.Run.started += OnRun;
        inputHandler.input.CharacterControls.Run.canceled += OnRun;
        inputHandler.input.CharacterControls.Roll.performed += context => OnDash();
        inputHandler.input.CharacterControls.Jump.started += OnJump;
        inputHandler.input.CharacterControls.Jump.started += context => OnDoubleJump();
        inputHandler.input.CharacterControls.Jump.canceled += OnJump;
        inputHandler.input.CharacterControls.Attack.performed += context => OnAttack();

        setupJumpVariables();
    }
    private void setupJumpVariables()
    {
        float timeToApex = maxJumpTime / 2;
        gravity = (-2 * maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        initialJumpVelocity = (2 * maxJumpHeight) / timeToApex;
    }

    #endregion

    #region Update
    // Update is called once per frame
    void Update()
    {
        //Check ground
        isFloorBelow = Physics.CheckSphere(transform.position + distanceToGround, groundedRadius, groundLayer);

        //TODO: GET SET PERFORMANCE
        animator.SetBool("isFalling", !isFloorBelow);

        //input
        OnMovementInput(inputHandler.input.CharacterControls.Move.ReadValue<Vector2>());
        //Set Character center point
        characterCenter = transform.position + (Vector3.up * 1.04f);
        //Apply rotacion
        HandleRotation();
        //Speed
        HandleSpeed();

        //Update State
        CurrentState.UpdateStates();

        characterController.Move(currentMovement * Time.deltaTime);

        if(airComboSeekingRestart && isFloorBelow)
        {
            currentAirCombo = 0;
            airComboSeekingRestart = false;
            animator.SetInteger("airCombo", currentAirCombo);
        }
    }

    #endregion

    #region Input
    void OnMovementInput(Vector2 context)
    {
        //Obtain Raw Data
        rawMovementInput = context.normalized;

        //Obtain movement from input
        currentMovementInput = context;

        //Sliding Limits to only X Axis
        //if(isSliding)
            //currentMovementInput.y = 0;

        //Obtener direccion de la camara
        var cameraForward = thirdPersonCamera.transform.forward;
        cameraForward.y = 0;
        cameraForward = cameraForward.normalized;
        var cameraRight = thirdPersonCamera.transform.right;
        cameraRight.y = 0;
        cameraRight = cameraRight.normalized;

        //Make input relaitve to camera
        currentMovementInput = new Vector2(cameraForward.x * currentMovementInput.y + cameraRight.x * currentMovementInput.x,
            cameraForward.z * currentMovementInput.y + cameraRight.z * currentMovementInput.x);

        isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;

        //Register last current input
        if (currentMovementInput != Vector2.zero)
            lastMovementInput = currentMovementInput;
    }

    //Get realtime input direction
    public Vector3 InputDirection() {
        Vector3 direction = new Vector3(currentMovementInput.x, 0, currentMovementInput.y);
        if (direction == Vector3.zero)
            direction = transform.forward;
                return direction;
    }

    private void OnRun(InputAction.CallbackContext context)
    {
        isRunPressed = context.ReadValueAsButton();
    }

    //Jump
    private void OnJump(InputAction.CallbackContext context)
    {
        isJumpPressed = context.ReadValueAsButton();

        if(characterController.isGrounded || (leftWallColliding || rightWallColliding))
        {
            requireNewJump = false;
        }
    }


    //Called when "Dash"
    private void OnDash()
    {
        CurrentState.Trigger("dash");

        //TODO: Move This

        //Execute the dash
        //ExecuteDash(sprintDash, false); FIX

        //WindParticleBurst(10);
    }

    private void OnDoubleJump()
    {
        if(!characterController.isGrounded && canDoubleJump)
        {
            CurrentState.Trigger("boost");

            //TODO: Move This
            //animator.SetTrigger("doubleJump");
            //FreezeMovement(); FIX
            //ExecuteDash(doubleJumpDash, true);
            //canDoubleJump = false;
            //lookTarget.Freeze(.1f);

            //Enable WallRun
            //WallRunCooldown(wallRunCooldown); FIX

            //Camera FOV
            //actionCamera.Zoom(1.2f, 74);
        }
    }

    private void OnAttack()
    {
        CurrentState.Trigger("attack");
    }

    #endregion

    #region Handlers
    void HandleSpeed()
    {
        //Interpolation
        if(Mathf.Abs(currentSpeed - targetSpeed) < .4f)
        {
            currentSpeed = targetSpeed;
        } else if(currentSpeed < targetSpeed)
        {
            //Acceleration
            currentSpeed += easeFactor * Time.deltaTime;
        } else if(((currentSpeed > targetSpeed)))
        {
            //Deceleration
            currentSpeed -= (characterController.isGrounded ? groundFriction : airResistance) * Time.deltaTime;
        }

        if(currentSpeed < 0)
            currentSpeed = 0;
    }

    void HandleRotation()
    {
        if(isSliding || isDashing) return;
        
        //Actual rotation
        currentRotation = transform.rotation;

        if (isMovementPressed)
        {
            //New smooth rotation
            targetRotation = Quaternion.LookRotation(InputDirection());

            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationSpeed * Time.deltaTime);

            playerModelRotation = playerModel.localRotation.eulerAngles;
        }
        else
        {
            targetRotation = transform.rotation;
        }

        //Apply tilt to character
        float tilt =  Mathf.Lerp(playerModel.localRotation.z, targetTilt, Time.deltaTime * tiltSpeed);
        playerModel.localRotation = Quaternion.Euler(0, 0, tilt);
    }

    public void QuickRotation(Vector3 direction)
    {
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        targetTilt = 0;
        transform.rotation = targetRotation;
    }

    //Dash
    public void ExecuteDash(DashSetting setting, bool relativeToInput, Vector3 direction = default(Vector3))
    {
        if(direction == default(Vector3))
        {
            direction = new Vector3(lastMovementInput.x, 0, lastMovementInput.y);
        }

        if (dashCoroutine != null)
            StopCoroutine(dashCoroutine);

        dashCoroutine = Dash(setting.dashTime, setting.dashForce, setting.dashCurve, direction);
        StartCoroutine(dashCoroutine);
    }

    public IEnumerator Dash (float duration, float force, AnimationCurve curve, Vector3 direction)
    {
        isDashing = true;

        QuickRotation(new Vector3(lastMovementInput.x, 0, lastMovementInput.y));

        //Jump
        if (direction.y != 0)
            currentMovement.y = direction.y;

        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            //Generate dash vector
            Vector3 dashVector;
            dashVector = direction * force;

            //Normalize elapsed time (0 - 1)
            float elapsedRange = elapsedTime / duration;

            //Use curve to get the current value
            Vector3 dashMovement = Vector3.Lerp(Vector3.zero, dashVector, curve.Evaluate(elapsedRange));

            currentMovement = new Vector3(dashMovement.x, currentMovement.y, dashMovement.z);

            yield return null;
        }

        //actionCamera.Shake(1, 0, 1);
        isDashing = false;
        CurrentState.Trigger("endDash");
    }

    public void ResetCombo()
    {
        if (comboReset != null)
            StopCoroutine(comboReset);

        comboReset = comboStop((isFloorBelow ? comboDashes[currentCombo-1] : airComboDashes[currentAirCombo-1]).recoverTime);
        StartCoroutine(comboReset);

        IEnumerator comboStop (float time)
        {
            yield return new WaitForSeconds(time);

            currentCombo = 0;
            animator.SetInteger("combo", currentCombo);
            airComboSeekingRestart = true;
        }
    }

    public void ResetMovement()
    {
        if (resetMovement != null)
            StopCoroutine(resetMovement);

        resetMovement = ResetMov((isFloorBelow ? comboDashes[currentCombo-1] : airComboDashes[currentAirCombo-1]).dashTime);
        StartCoroutine(resetMovement);

        IEnumerator ResetMov (float time)
        {
            yield return new WaitForSeconds(time);

            StateTrigger("endDash");
        }
    }

    #endregion

    public void StateTrigger(string trigger) {
        CurrentState.Trigger(trigger);
    }

    public void StateTrigger(int damage, Vector3 position) {
        CurrentState.Trigger(damage, position);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit) {
        //hitPointNormal = hit.normal;
        collisionPoint = hit.point;
        //collisionPoint = (collisionPoint - transform.position + Vector3.up * .1f);
        var point = hit.point;
        var dir = -hit.normal; // you need vector pointing TOWARDS the collision, not away from it
        // step back a bit
        point -= dir + Vector3.up * .2f;

        // cast a ray twice as far as your step back. This seems to work in all
        // situations, at least when speeds are not ridiculously big
        if (hit.collider.Raycast(new Ray(isSliding ? point : transform.position + distanceToGround, dir), out RaycastHit hitInfo, 2))
        {
            // this is the collider surface normal
            var angle = Vector3.Angle(hitInfo.normal, Vector3.up);
            if (angle < 80)
            {
                hitPointNormal = hitInfo.normal;
                hitPointAngle = angle;
            }
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.DrawSphere(collisionPoint, 0.1f);
    }
}
