using Cinemachine;
using LucidSightTools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Input = UnityEngine.Input;
using Random = UnityEngine.Random;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : ExampleNetworkedEntityView
{
    public bool isLeftClick = false;
    Vector3 initialPos = Vector3.zero;
    private CharacterController _characterController;

    private Quaternion cachedHeadRotation;
    private Vector2 currentLookRotation = Vector2.zero;
    private float gravityValue = -9.81f;
    private bool groundedPlayer;

    private float _animationBlend;
    public AudioClip LandingAudioClip;
    public AudioClip[] FootstepAudioClips;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;
    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    public float Gravity = -15.0f;
    [Range(0, 1)] public float FootstepAudioVolume = 0.5f;
    [Tooltip("Acceleration and deceleration")]
    public float SpeedChangeRate = 10.0f;

    [SerializeField]
    private CinemachineVirtualCamera cam = null;

    [SerializeField]
    private Transform headRoot = null;

    private bool isPaused;

    [SerializeField]
    private float jumpHeight = 1.0f;

    [SerializeField]
    private float lookSpeed = 3.0f;

    [SerializeField]
    private float playerSpeed = 2.0f;

    private Vector3 playerVelocity;

    public string userName;

    public bool isReady = false;
    public InputSyncData SyncData;

    private Animator _animator;
    private bool _hasAnimator;

    // timeout deltatime
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    // animation IDs
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;

    [Space(10)]
    [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
    public float JumpTimeout = 0.50f;

    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    public float FallTimeout = 0.15f;
    public bool hasGameBegun = false;

    protected override void Start()
    {
        initialPos = transform.position;
        base.Start();
        _hasAnimator = TryGetComponent(out _animator);
        if (ExampleManager.Instance.Avatar.ToString() == prefabName)
        {
            headRoot.gameObject.SetActive(true);
            cam.m_Priority = 11;
        }
        else
        {
            headRoot.gameObject.SetActive(false);
            cam.m_Priority = 10;
        }
        userName = string.Empty;
        _characterController = GetComponent<CharacterController>();
        SetPause(true);
        StartCoroutine("WaitForConnect");

        AssignAnimationIDs();

        // reset our timeouts on start
        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;
    }
    private void OnEnable()
    {
        ExampleRoomController.onBeginRound += BeginRound;
        ExampleRoomController.onSyncData += GetSyncData;
    }

    private void BeginRound()
    {
        SetPause(false);
        hasGameBegun = true;
    }

    private void GetSyncData(InputSyncData input)
    {
        if (IsMine) return;
        if (input == null) return;
        if (input.name == prefabName)
        {
            Vector3 pos = new Vector3(input.xPos, input.yPos, input.zPos);
            Quaternion rot = new Quaternion(input.xRot, input.yRot, input.zRot, input.wRot);
            transform.position = pos;
            transform.localRotation = rot;

            isLeftClick = input.leftClicked;

            ExampleManager.Instance.CurrentNetworkedEntity.timestamp = input.timestamp;

            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, !input.jump);
                _animator.SetBool(_animIDJump, input.jump);
                Vector3 move = GetMoveVector(input.left, input.right, input.up, input.down);

                float targetMultiplier = input.sprint ? 2 : 1;
                _animationBlend = Mathf.Lerp(_animationBlend, move.magnitude * targetMultiplier * playerSpeed, Time.deltaTime * SpeedChangeRate);
                if (_animationBlend < 0.01f) _animationBlend = 0f;

                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, move.magnitude);
            }
        }
    }
    private void OnDisable()
    {
        ExampleRoomController.onSyncData -= GetSyncData;
        ExampleRoomController.onBeginRound -= BeginRound;
    }
    private IEnumerator WaitForConnect()
    {
        if (ExampleManager.Instance.CurrentUser != null && !IsMine)
        {
            yield break;
        }
        while (!ExampleManager.Instance.IsInRoom)
        {
            yield return 0;
        }

        LSLog.LogImportant("HAS JOINED ROOM - CREATING ENTITY");
        ExampleManager.CreateNetworkedEntityWithTransform(transform.position, Quaternion.identity,
            new Dictionary<string, object> { ["userName"] = ExampleManager.Instance.UserName }, this, entity =>
            {
                userName = ExampleManager.Instance.UserName;
                ExampleManager.Instance.CurrentNetworkedEntity = entity;
                transform.position = initialPos;
            });
    }

    public override void OnEntityRemoved()
    {
        base.OnEntityRemoved();
        LSLog.LogImportant("REMOVING ENTITY", LSLog.LogColor.lime);
    }

    protected override void ProcessViewSync()
    {
        // This is the target playback time of this body
        double interpolationTime = ExampleManager.Instance.GetServerTime - interpolationBackTimeMs;

        // Use interpolation if the target playback time is present in the buffer
        if (proxyStates[0].timestamp > interpolationTime)
        {
            // The longer the time since last update add a delta factor to the lerp speed to get there quicker
            float deltaFactor = ExampleManager.Instance.GetServerTimeSeconds > proxyStates[0].timestamp
                ? (float)(ExampleManager.Instance.GetServerTimeSeconds - proxyStates[0].timestamp) * 0.2f
                : 0f;

            if (syncLocalPosition)
            {
                myTransform.localPosition = Vector3.Slerp(myTransform.localPosition, proxyStates[0].pos,
                    Time.deltaTime * (positionLerpSpeed + deltaFactor));
            }

            if (syncLocalRotation && Mathf.Abs(Quaternion.Angle(transform.localRotation, proxyStates[0].rot)) >
                snapIfAngleIsGreater)
            {
                myTransform.localRotation = proxyStates[0].rot;
            }

            if (syncLocalRotation)
            {
                myTransform.localRotation = Quaternion.Slerp(myTransform.localRotation, proxyStates[0].rot,
                    Time.deltaTime * (rotationLerpSpeed + deltaFactor));
                headRoot.localRotation = Quaternion.Slerp(headRoot.localRotation, cachedHeadRotation,
                    Time.deltaTime * (rotationLerpSpeed + deltaFactor));
            }
        }
        // Use extrapolation (If we didnt get a packet in the last X ms and object had velocity)
        else
        {
            EntityState latest = proxyStates[0];

            float extrapolationLength = (float)(interpolationTime - latest.timestamp);
            // Don't extrapolation for more than 500 ms, you would need to do that carefully
            if (extrapolationLength < extrapolationLimitMs / 1000f)
            {
                if (syncLocalPosition)
                {
                    myTransform.localPosition = latest.pos + latest.vel * extrapolationLength;
                }

                if (syncLocalRotation)
                {
                    myTransform.localRotation = latest.rot;
                }
            }
        }
    }

    protected override void UpdateViewFromState()
    {
        base.UpdateViewFromState();

        if (!HasInit || !IsMine)
        {
            return;
        }
        //catch the incoming head rotation. If it has xView, it will have the rest
        if (state.attributes.ContainsKey("xViewRot"))
        {
            cachedHeadRotation.x = float.Parse(state.attributes["xViewRot"]);
            cachedHeadRotation.y = float.Parse(state.attributes["yViewRot"]);
            cachedHeadRotation.z = float.Parse(state.attributes["zViewRot"]);
            cachedHeadRotation.w = float.Parse(state.attributes["wViewRot"]);
        }

        if (string.IsNullOrEmpty(userName) && state.attributes.ContainsKey("userName"))
        {
            userName = state.attributes["userName"];
        }

        if (state.attributes.ContainsKey("isReady"))
        {
            isReady = bool.Parse(state.attributes["isReady"]);
        }
    }

    protected override void UpdateStateFromView()
    {
        base.UpdateStateFromView();

        if (!HasInit || !IsMine)
        {
            return;
        }
        //Update the head rotation attributes
        Dictionary<string, string> updatedAttributes = new Dictionary<string, string>
        {
            {"xViewRot", headRoot.localRotation.x.ToString()},
            {"yViewRot", headRoot.localRotation.y.ToString()},
            {"zViewRot", headRoot.localRotation.z.ToString()},
            {"wViewRot", headRoot.localRotation.w.ToString()}
        };
        SetAttributes(updatedAttributes);
    }
    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

    protected override void Update()
    {
        base.Update();
        _hasAnimator = TryGetComponent(out _animator);

        if (!HasInit || !IsMine)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SetPause(!isPaused);
        }
        if (isPaused) return;
        if (!hasGameBegun)
            return;

        TakeInputs();
        HandleInput(SyncData.left, SyncData.right, SyncData.up, SyncData.down, SyncData.jump, SyncData.sprint);
        HandleLook(SyncData.mouseX, SyncData.mouseY);
        JumpAndGravity(SyncData.jump);
        isLeftClick = SyncData.leftClicked;
    }

    private void TakeInputs()
    {
        if (prefabName != "")
            SyncData.name = prefabName;

        SyncData.timestamp = (float)ExampleManager.Instance.CurrentNetworkedEntity.timestamp;
        SyncData.pause = Input.GetKeyDown(KeyCode.Escape);
        SyncData.mouseX = Input.GetAxis("Mouse X");
        SyncData.mouseY = Input.GetAxis("Mouse Y");
        SyncData.left = (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow));
        SyncData.right = (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow));
        SyncData.up = (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow));
        SyncData.down = (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow));
        SyncData.jump = Input.GetButtonDown("Jump");
        SyncData.sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        SyncData.leftClicked = Input.GetMouseButtonDown(0);

        SyncData.xPos = transform.position.x;
        SyncData.yPos = transform.position.y;
        SyncData.zPos = transform.position.z;

        SyncData.xRot = transform.localRotation.x;
        SyncData.yRot = transform.localRotation.y;
        SyncData.zRot = transform.localRotation.z;
        SyncData.wRot = transform.localRotation.w;
        SyncData.name = prefabName;

        ExampleManager.CustomServerMethod("setSyncInputs", new object[] { SyncData });
    }
    public void JumpAndGravity(bool jump)
    {
        if (isPaused) return;
        if (groundedPlayer)
        {
            // reset the fall timeout timer
            _fallTimeoutDelta = FallTimeout;

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDJump, false);
                _animator.SetBool(_animIDFreeFall, false);
            }

            // stop our velocity dropping infinitely when grounded
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            // Jump
            if (jump && _jumpTimeoutDelta <= 0.0f)
            {
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * Gravity);

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, true);
                }
            }

            // jump timeout
            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            // reset the jump timeout timer
            _jumpTimeoutDelta = JumpTimeout;

            // fall timeout
            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDFreeFall, true);
                }
            }
        }

        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += Gravity * Time.deltaTime;
        }
    }

    private void HandleInput(bool left, bool right, bool up, bool down, bool jump, bool sprint)
    {
        if (isPaused) return;

        groundedPlayer = _characterController.isGrounded;
        if (_hasAnimator)
        {
            _animator.SetBool(_animIDGrounded, groundedPlayer);
        }
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }
        float targetMultiplier = sprint ? 2 : 1;

        Vector3 move = GetMoveVector(left, right, up, down);
        if (move == Vector3.zero) targetMultiplier = 0.0f;

        _characterController.Move(move * Time.deltaTime * playerSpeed * targetMultiplier);

        if (jump && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        _characterController.Move(playerVelocity * Time.deltaTime);

        _animationBlend = Mathf.Lerp(_animationBlend, move.magnitude * playerSpeed * targetMultiplier, Time.deltaTime * SpeedChangeRate);
        if (_animationBlend < 0.01f) _animationBlend = 0f;

        if (_hasAnimator)
        {
            _animator.SetFloat(_animIDSpeed, _animationBlend);
            _animator.SetFloat(_animIDMotionSpeed, move.magnitude);
        }
    }

    private Vector3 GetMoveVector(bool left, bool right, bool up, bool down)
    {
        float horizontalAxis = left ? -1 : right ? 1 : 0;
        float verticalAxis = down ? -1 : up ? 1 : 0;
        Vector3 move = new Vector3(horizontalAxis, 0, verticalAxis).normalized;
        move = transform.TransformDirection(move);
        return move;
    }

    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            if (FootstepAudioClips.Length > 0)
            {
                var index = Random.Range(0, FootstepAudioClips.Length);
                AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_characterController.center), FootstepAudioVolume);
            }
        }
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_characterController.center), FootstepAudioVolume);
        }
    }
    private void HandleLook(float mouseX, float mouseY)
    {
        if (isPaused) return;
        currentLookRotation.y += mouseX;
        currentLookRotation.x += -mouseY;

        Vector3 bodyRot = transform.eulerAngles;
        Vector3 look = currentLookRotation * lookSpeed;
        //Player rotation
        bodyRot.y = look.y;
        transform.eulerAngles = bodyRot;
        //Tilt
        Vector3 head = headRoot.eulerAngles;
        head.x = Mathf.Clamp(look.x, -80, 80); //Clamp to prevent gimbal lock
        headRoot.eulerAngles = head;
    }

    public void SetCursorUnlocked(bool val)
    {
        Cursor.visible = val;
        Cursor.lockState = val ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public void SetPause(bool pause)
    {
        isPaused = pause;
        SetCursorUnlocked(isPaused);
    }

    public void UpdateReadyState(bool ready)
    {
        if (IsMine)
        {
            isReady = ready;
            SetAttributes(new Dictionary<string, string>() { { "isReady", isReady.ToString() } });
        }
    }
}