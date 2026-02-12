using System;
using System.Collections;
using CustomInspector;
using DG.Tweening;
// using FMOD.Studio;
// using FMODUnity;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }
    public enum CrouchState
    {
        Standing,
        Crouching,
        Crawling,
        Transitioning
    }

    [ReadOnly] public CrouchState crouchState = CrouchState.Standing;
    [ReadOnly] public bool isMoving;
    [ReadOnly] public bool isSprinting;
    [ReadOnly] public bool isFirstForWalkSound = true;
    [ReadOnly] public bool disableSprint = false;
    [ReadOnly] public bool isClimbingLadder = false;

    [HorizontalLine(message: "References", color: FixedColor.IceWhite, thickness: 3, spacing: 30)]
    [SerializeField] private CinemachineInputAxisController cinemachineFPSCamController;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private CharacterController controller;
    [SerializeField] private Transform eyes;
    // [SerializeField] private EventReference playerWalkSound;
    // [SerializeField] private EventReference playerSprintSound;

    [HorizontalLine(message: "Movement", color: FixedColor.IceWhite, thickness: 3, spacing: 30)]
    [SerializeField, Range(0f, 60f)] private float mouseSensitivity = 10.0f;
    [SerializeField, Range(0f, 10f)] private float playerSpeed = 2.0f;
    [SerializeField, Range(0f, 10f)] private float crouchSpeed = 1.0f;
    [SerializeField, Range(0f, 10f)] private float sprintSpeed = 5.0f;
    [SerializeField, Range(0f, 100f)] private float acceleration = 5.0f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundedOffset = -0.1f;
    [SerializeField] private float groundedCheckRadius = 0.3f;
    [SerializeField, Range(0f, 4f)] private float defaultHeight = 2f;

    [HorizontalLine(message: "Crouch", color: FixedColor.IceWhite, thickness: 3, spacing: 30)]
    [SerializeField, Range(0f, 2f)] private float crouchHeight = 1f;
    [SerializeField] private AnimationCurve crouchCurve;
    [SerializeField] private bool useCrawling = true;
    [SerializeField, Range(0f, 1.5f), ShowIf(nameof(useCrawling))] private float crawlHeight = 1f;
    [SerializeField, Range(0f, 2f), ShowIf(nameof(useCrawling))] private float crouchDuration = 0.5f;
    [SerializeField, Range(0f, 2f), ShowIf(nameof(useCrawling))] private float crawlCheckDistance = 1f;
    [SerializeField, Range(0f, 2f), ShowIf(nameof(useCrawling))] private float crawlCheckRadius = .3f;

    [HorizontalLine(message: "Ladder Climbing", color: FixedColor.IceWhite, thickness: 3, spacing: 30)]
    [Tooltip("Layer for ladder objects")]
    [SerializeField] private LayerMask ladderLayer;
    [Tooltip("Sphere check forward distance for detecting ladder")]
    [SerializeField, Range(0, 2)] private float ladderDetectDistance = 0.6f;
    [Tooltip("Climb speed in units/sec")]
    [SerializeField, Range(0, 5)] private float climbSpeed = 2.0f;
    [SerializeField, Range(0, 3)] private float climbCooldown = 1f;

    private InputManager inputManager;
    private Transform cameraTransform;
    private Vector3 move;
    // private PlayerWeapons playerWeapons;
    private float currentSpeed;
    private float verticalVelocity;
    private bool isGrounded;
    private bool canMove = true;
    private Vector3 lastPosition;
    private float minMovementThreshold = 0.05f;
    private Coroutine crouchRoutine;
    float eyesOffsetFromTop;

    private float defaultRadius = 0f;

    private bool isNearLadder = false;
    private Transform currentLadder = null;
    private bool canClimb = true;


    // private EventInstance playerWalk;
    // private EventInstance playerSprint;


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        inputManager = InputManager.Instance;
        cameraTransform = Camera.main.transform;

        eyesOffsetFromTop = controller.height - eyes.localPosition.y;

        // playerWalk = AudioManager.Instance.CreateInstance(playerWalkSound);
        // playerSprint = AudioManager.Instance.CreateInstance(playerSprintSound);

        //* Setting sens here
        float sens = PlayerPrefs.GetFloat("mouseSens", mouseSensitivity);
        cinemachineFPSCamController.Controllers.ForEach(controller =>
        {
            if (controller.Name == "Look X (Pan)")
            {
                controller.Input.Gain = sens;
            }
            else if (controller.Name == "Look Y (Tilt)")
            {
                controller.Input.Gain = -sens;
            }
        });

        //* Setting FOV here

        //TODO: Uncomment this later
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        defaultRadius = controller.radius;
        // Debug.unityLogger.logEnabled = false;
    }

    private void Update()
    {
        // Cursor.lockState = CursorLockMode.None;
        // Cursor.visible = true;
        if (!canMove) return;

        DetectLadder();

        if (isClimbingLadder)
        {
            ClimbMovement();
            return;
        }

        GroundCheck();
        CalculateMovement();
        ApplyGravity();
        PerformMovement();
        Crouching();
    }

    private void GroundCheck()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y + groundedOffset, transform.position.z);
        isGrounded = Physics.CheckSphere(spherePosition, groundedCheckRadius, groundLayer, QueryTriggerInteraction.Ignore);

        if (isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }
    }

    private void CalculateMovement()
    {
        Vector2 input = inputManager.GetPlayerMovement();
        isMoving = input.sqrMagnitude > 0.01f;
        isSprinting = inputManager.IsPlayerSprinting() && input.y > 0 && crouchState == CrouchState.Standing;
        if (inputManager.GetPlayerAim()) isSprinting = false;
        if (disableSprint) isSprinting = false;

        if (isMoving && isSprinting && crouchState == CrouchState.Standing)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, sprintSpeed, Time.deltaTime * acceleration);
        }
        else if (isMoving && crouchState == CrouchState.Standing)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, playerSpeed, Time.deltaTime * acceleration);
        }
        else if (isMoving && (crouchState == CrouchState.Crouching || crouchState == CrouchState.Crawling))
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, crouchSpeed, Time.deltaTime * acceleration);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, Time.deltaTime * acceleration);
        }

        move = cameraTransform.forward * input.y + cameraTransform.right * input.x;
        move.y = 0f;
    }

    private void ApplyGravity()
    {
        verticalVelocity += gravity * Time.deltaTime;
    }

    private void PerformMovement()
    {
        if (move != Vector3.zero)
            move = move.normalized;

        Quaternion targetRotation = Quaternion.Euler(0f, cameraTransform.eulerAngles.y, 0f);
        transform.rotation = targetRotation;

        Vector3 totalMovement = move * currentSpeed + Vector3.up * verticalVelocity;
        controller.Move(totalMovement * Time.deltaTime);

        Vector3 displacement = transform.position - lastPosition;
        float distanceMoved = displacement.magnitude / Time.deltaTime;

        if (isFirstForWalkSound)
        {
            distanceMoved = 0f;
            isFirstForWalkSound = false;
        }
        // bool isActuallyMoving = distanceMoved > minMovementThreshold;

        // UpdateSounds(isActuallyMoving);

        // lastPosition = transform.position;
    }

    // private void UpdateSounds(bool isActuallyMoving)
    // {
    //     PLAYBACK_STATE walkPlaybackState;
    //     playerWalk.getPlaybackState(out walkPlaybackState);

    //     PLAYBACK_STATE sprintPlaybackState;
    //     playerSprint.getPlaybackState(out sprintPlaybackState);

    //     if (isActuallyMoving && isGrounded)
    //     {
    //         if (isSprinting)
    //         {
    //             if (sprintPlaybackState == PLAYBACK_STATE.STOPPED)
    //                 playerSprint.start();
    //             playerWalk.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    //         }
    //         else
    //         {
    //             if (walkPlaybackState == PLAYBACK_STATE.STOPPED)
    //                 playerWalk.start();
    //             playerSprint.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    //         }
    //     }
    //     else
    //     {
    //         playerWalk.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    //         playerSprint.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    //     }
    // }

    #region Crouching
    private void Crouching()
    {
        if (inputManager.GetPlayerCrouch())
        {
            // animator.SetBool(PlayerConstants.CROUCH, true);
            if (crouchState == CrouchState.Standing)
            {
                StartCrouch();
            }
            else if ((crouchState == CrouchState.Crouching || crouchState == CrouchState.Crawling) && CanStand())
            {
                StopCrouch();
            }
        }

        if (useCrawling)
        {
            if (crouchState == CrouchState.Crouching || crouchState == CrouchState.Crawling)
            {
                if (CanCrawlForward() && crouchState != CrouchState.Crawling && crouchState != CrouchState.Transitioning)
                {
                    StartCrawl();
                }
                else if (!CanCrawlForward() && CanStand() && crouchState != CrouchState.Crouching && crouchState != CrouchState.Transitioning && crouchState == CrouchState.Crawling)
                {
                    StartCrouch();
                }
            }
        }
    }

    private void StartCrouch()
    {
        StartHeightChange(crouchHeight);
    }

    private void StartCrawl()
    {
        StartHeightChange(crawlHeight);
    }

    private void StopCrouch()
    {
        StartHeightChange(defaultHeight);
    }

    private void StartHeightChange(float targetHeight)
    {
        if (crouchRoutine != null)
            StopCoroutine(crouchRoutine);

        crouchRoutine = StartCoroutine(HeightRoutine(targetHeight));
    }

    private IEnumerator HeightRoutine(float targetHeight)
    {
        crouchState = CrouchState.Transitioning;

        float startHeight = controller.height;
        float startCenterY = controller.center.y;

        float targetCenterY = targetHeight * 0.5f;

        float targetRadius = defaultRadius;

        float elapsed = 0f;

        if (targetHeight <= crawlHeight)
        {
            targetRadius -= .2f;
        }

        eyes.DOLocalMoveY(targetHeight - eyesOffsetFromTop, crouchDuration).SetEase(crouchCurve);

        while (elapsed < crouchDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / crouchDuration;

            float curveT = crouchCurve.Evaluate(t);

            controller.radius = Mathf.Lerp(controller.radius, targetRadius, curveT);
            controller.height = Mathf.Lerp(startHeight, targetHeight, curveT);
            controller.center = new Vector3(0f,
                Mathf.Lerp(startCenterY, targetCenterY, curveT),
                0f);

            yield return null;
        }

        eyes.localPosition = new Vector3(0f, targetHeight - eyesOffsetFromTop, 0f);
        controller.radius = targetRadius;
        controller.height = targetHeight;
        controller.center = new Vector3(0f, targetCenterY, 0f);

        if (targetHeight == defaultHeight)
        {
            crouchState = CrouchState.Standing;
        }
        else if (targetHeight == crouchHeight)
        {
            crouchState = CrouchState.Crouching;
        }
        else if (targetHeight == crawlHeight)
        {
            crouchState = CrouchState.Crawling;
        }
    }

    private bool CanStand()
    {
        float radius = controller.radius;
        float castDistance = defaultHeight - controller.height;

        Vector3 origin = transform.position + Vector3.up * (controller.height * .5f);

        bool didHit = Physics.SphereCast(
            origin,
            radius,
            Vector3.up,
            out RaycastHit hit,
            castDistance,
            groundLayer
        );

        return !didHit;
    }

    private bool CanCrawlForward()
    {
        float halfHeight = crawlHeight * 0.5f;
        float radius = crawlCheckRadius;

        bool blocked = true;

        bool checkLow = Physics.CheckSphere(transform.position + Vector3.up * halfHeight + transform.forward * crawlCheckDistance, radius, groundLayer, QueryTriggerInteraction.Ignore);

        bool checkHigh = Physics.CheckSphere(transform.position + Vector3.up * crawlHeight + transform.forward * crawlCheckDistance, radius, groundLayer, QueryTriggerInteraction.Ignore);

        if (checkHigh && !checkLow)
        {
            blocked = false;
        }

        return !blocked;
    }

    #endregion

    #region Ladder climbing
    private void DetectLadder()
    {
        // Cast a small sphere in front of the player to detect ladder(s)
        Vector2 input = inputManager.GetPlayerMovement();
        Vector3 checkOrigin;
        float detectDistance = ladderDetectDistance;

        if (input.y > 0)
        {
            checkOrigin = transform.position + Vector3.up * .1f;
        }
        else if (input.y < 0)
        {
            checkOrigin = transform.position + Vector3.up * (controller.height * 0.5f);
            detectDistance = ladderDetectDistance * 2;
        }
        else
        {
            checkOrigin = transform.position + Vector3.up * .1f;
        }
        isNearLadder = Physics.Raycast(checkOrigin, transform.forward, out RaycastHit hit, detectDistance, ladderLayer, QueryTriggerInteraction.Collide);
        currentLadder = isNearLadder ? hit.transform : null;

        if (isNearLadder)
        {
            if (!isClimbingLadder && canClimb)
                StartClimb();
        }
    }


    private void StopClimb()
    {
        isClimbingLadder = false;
        currentLadder = null;
    }

    private void StartClimb()
    {
        if (currentLadder == null) return;

        Vector3 ladderCenter = currentLadder.localPosition;
        isMoving = false;
        transform.DOMove(new Vector3(ladderCenter.x, transform.position.y, ladderCenter.z + controller.radius + .1f), .25f);
        // Debug.Log(ladderCenter);
        // Debug.Log(currentLadder.name);

        isClimbingLadder = true;

        verticalVelocity = 0f;
    }

    private void ClimbMovement()
    {
        Vector2 input = inputManager.GetPlayerMovement();
        float verticalInput = input.y;

        if (input.y != 0)
        {
            isMoving = true;
            isSprinting = false;
        }
        else
        {
            isMoving = false;
            isSprinting = false;
        }

        float climbVel = verticalInput * climbSpeed;

        Vector3 climbDelta = Vector3.up * climbVel;

        controller.Move(climbDelta * Time.deltaTime);

        float checkDistance = ladderDetectDistance * 2;
        Vector3 checkOrigin = transform.position + Vector3.up * .1f;
        bool stillNear = Physics.Raycast(checkOrigin, transform.forward, out RaycastHit hit, checkDistance, ladderLayer, QueryTriggerInteraction.Collide);
        Debug.DrawLine(transform.position, checkOrigin, Color.red);
        if (!stillNear)
        {
            // Debug.Log("No longer near ladder");
            StopClimb();
            verticalVelocity = 4f;
            canClimb = false;
            StartCoroutine(ResetCanClimbCooldown());
        }

        if (verticalInput < 0f)
        {
            float ladderDropDistance = .1f;
            Debug.DrawRay(transform.position, Vector3.down * ladderDropDistance, Color.red);
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit floorRaycastHit, ladderDropDistance, ~ladderLayer.value))
            {
                // Debug.Log("Floor raycast hit " + floorRaycastHit.transform.name);
                StopClimb();
                canClimb = false;
                StartCoroutine(ResetCanClimbCooldown());
            }
        }

        verticalVelocity = 0f;
    }

    private IEnumerator ResetCanClimbCooldown()
    {
        yield return new WaitForSeconds(climbCooldown);
        canClimb = true;
    }

    #endregion
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y + groundedOffset, transform.position.z);
        Gizmos.DrawWireSphere(spherePosition, groundedCheckRadius);

        float halfHeight = crawlHeight * 0.5f;
        float radius = crawlCheckRadius;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * halfHeight + transform.forward * crawlCheckDistance, radius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * crawlHeight + transform.forward * crawlCheckDistance, radius);
    }

    public void SetCanMove(bool value)
    {
        canMove = value;
        cinemachineFPSCamController.enabled = value;
        if (!value)
        {
            controller.Move(Vector3.zero);
            // playerWalk.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            // playerSprint.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
    }

    public bool GetCanMove() => canMove;

    public void SetDisableSprint(bool value) => disableSprint = value;

    public Transform GetEyes()
    {
        return cinemachineFPSCamController.transform;
    }

    public void SetSensitivity(float sens)
    {
        cinemachineFPSCamController.Controllers.ForEach(controller =>
        {
            if (controller.Name == "Look X (Pan)")
            {
                controller.Input.Gain = sens;
            }
            else if (controller.Name == "Look Y (Tilt)")
            {
                controller.Input.Gain = -sens;
            }
        });
    }
}
