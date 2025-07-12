using UnityEngine;

/// <summary>
/// Enhanced player controller with realistic movement mechanics.
/// Features: sprinting, crouching, jumping, footstep sounds, head bobbing.
/// </summary>
public class SimplePlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 80f;
    
    [Header("Advanced Movement")]
    [SerializeField] private float acceleration = 10f;
    // [SerializeField] private float deceleration = 15f; // Unused - removed to avoid warning
    [SerializeField] private float airControl = 0.3f;
    [SerializeField] private float slideSpeed = 10f;
    [SerializeField] private float slideDuration = 1f;
    
    [Header("Camera Settings")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float headBobFrequency = 2f;
    [SerializeField] private float headBobAmplitude = 0.1f;
    [SerializeField] private float crouchCameraOffset = -0.5f;
    [SerializeField] private float sprintCameraOffset = 0.2f;
    
    [Header("Audio")]
    [SerializeField] private AudioSource footstepAudio;
    [SerializeField] private AudioClip[] walkFootsteps;
    [SerializeField] private AudioClip[] sprintFootsteps;
    [SerializeField] private AudioClip[] crouchFootsteps;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip landSound;
    
    [Header("Effects")]
    [SerializeField] private ParticleSystem dustEffect;
    [SerializeField] private GameObject footstepDecal;
    
    // Movement state
    private Vector3 moveDirection;
    private Vector3 currentVelocity;
    private bool isGrounded;
    private bool isSprinting;
    private bool isCrouching;
    private bool isSliding;
    private float slideTimer;
    private float originalHeight;
    private float originalCameraY;
    
    // Camera
    private float mouseX;
    private float mouseY;
    private Vector3 originalCameraPosition;
    private float headBobTimer;
    
    // Components
    private CharacterController characterController;
    private Camera playerCamera;
    
    // Properties
    public Transform GetCameraTransform() => cameraTransform;
    public bool IsGrounded => isGrounded;
    public bool IsSprinting => isSprinting;
    public bool IsCrouching => isCrouching;
    public bool IsSliding => isSliding;
    public float CurrentSpeed => currentVelocity.magnitude;
    
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main?.transform;
        }
        
        if (cameraTransform != null)
        {
            playerCamera = cameraTransform.GetComponent<Camera>();
            originalCameraPosition = cameraTransform.localPosition;
            originalCameraY = cameraTransform.localPosition.y;
        }
        
        originalHeight = characterController.height;
        
        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGamePaused)
            return;
            
        HandleInput();
        HandleMovement();
        HandleCamera();
        HandleFootsteps();
        HandleEffects();
    }
    
    /// <summary>
    /// Handle player input
    /// </summary>
    private void HandleInput()
    {
        // Movement input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        moveDirection = new Vector3(horizontal, 0, vertical).normalized;
        
        // Mouse look
        mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        // Sprint
        if (Input.GetKey(KeyCode.LeftShift) && !isCrouching && moveDirection.magnitude > 0.1f)
        {
            isSprinting = true;
        }
        else
        {
            isSprinting = false;
        }
        
        // Crouch
        if (Input.GetKeyDown(KeyCode.C))
        {
            ToggleCrouch();
        }
        
        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }
        
        // Slide (while sprinting and crouching)
        if (Input.GetKeyDown(KeyCode.C) && isSprinting && !isCrouching)
        {
            StartSlide();
        }
    }
    
    /// <summary>
    /// Handle player movement with realistic physics
    /// </summary>
    private void HandleMovement()
    {
        // Check if grounded
        isGrounded = characterController.isGrounded;
        
        // Calculate target speed
        float targetSpeed = walkSpeed;
        if (isSprinting && !isCrouching)
        {
            targetSpeed = sprintSpeed;
        }
        else if (isCrouching)
        {
            targetSpeed = crouchSpeed;
        }
        
        // Apply slide speed
        if (isSliding)
        {
            targetSpeed = slideSpeed;
            slideTimer -= Time.deltaTime;
            if (slideTimer <= 0)
            {
                EndSlide();
            }
        }
        
        // Calculate movement
        Vector3 targetVelocity = transform.TransformDirection(moveDirection) * targetSpeed;
        
        // Apply gravity
        if (!isGrounded)
        {
            targetVelocity.y = currentVelocity.y - 9.8f * Time.deltaTime;
            targetVelocity.x *= airControl;
            targetVelocity.z *= airControl;
        }
        else
        {
            targetVelocity.y = -2f; // Small downward force when grounded
        }
        
        // Smooth movement
        float accelerationRate = isGrounded ? acceleration : acceleration * airControl;
        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, accelerationRate * Time.deltaTime);
        
        // Apply movement
        characterController.Move(currentVelocity * Time.deltaTime);
        
        // Handle head bobbing
        if (isGrounded && moveDirection.magnitude > 0.1f)
        {
            headBobTimer += Time.deltaTime * (isSprinting ? 2f : 1f);
        }
        else
        {
            headBobTimer = 0f;
        }
    }
    
    /// <summary>
    /// Handle camera movement and effects
    /// </summary>
    private void HandleCamera()
    {
        if (cameraTransform == null) return;
        
        // Rotate player horizontally
        transform.Rotate(Vector3.up * mouseX);
        
        // Rotate camera vertically
        mouseY = Mathf.Clamp(mouseY, -maxLookAngle, maxLookAngle);
        cameraTransform.Rotate(Vector3.left * mouseY);
        
        // Apply camera effects
        Vector3 cameraPosition = originalCameraPosition;
        
        // Crouch camera offset
        if (isCrouching)
        {
            cameraPosition.y += crouchCameraOffset;
        }
        
        // Sprint camera offset
        if (isSprinting && !isCrouching)
        {
            cameraPosition.y += sprintCameraOffset;
        }
        
        // Head bobbing
        if (isGrounded && moveDirection.magnitude > 0.1f)
        {
            float bobX = Mathf.Sin(headBobTimer * headBobFrequency) * headBobAmplitude;
            float bobY = Mathf.Sin(headBobTimer * headBobFrequency * 2f) * headBobAmplitude * 0.5f;
            cameraPosition += new Vector3(bobX, bobY, 0);
        }
        
        // Smooth camera movement
        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, cameraPosition, 10f * Time.deltaTime);
    }
    
    /// <summary>
    /// Handle footstep sounds
    /// </summary>
    private void HandleFootsteps()
    {
        if (footstepAudio == null || !isGrounded || moveDirection.magnitude < 0.1f) return;
        
        float footstepInterval = isSprinting ? 0.3f : (isCrouching ? 0.8f : 0.5f);
        
        if (Time.time % footstepInterval < Time.deltaTime)
        {
            PlayFootstep();
        }
    }
    
    /// <summary>
    /// Handle visual effects
    /// </summary>
    private void HandleEffects()
    {
        // Dust effect when moving
        if (dustEffect != null && isGrounded && moveDirection.magnitude > 0.1f)
        {
            if (!dustEffect.isPlaying)
            {
                dustEffect.Play();
            }
        }
        else if (dustEffect != null && dustEffect.isPlaying)
        {
            dustEffect.Stop();
        }
        
        // Footstep decals
        if (footstepDecal != null && isGrounded && moveDirection.magnitude > 0.1f)
        {
            if (Time.time % 0.5f < Time.deltaTime)
            {
                CreateFootstepDecal();
            }
        }
    }
    
    /// <summary>
    /// Toggle crouch state
    /// </summary>
    private void ToggleCrouch()
    {
        isCrouching = !isCrouching;
        
        if (isCrouching)
        {
            // Crouch
            characterController.height = originalHeight * 0.6f;
            characterController.center = new Vector3(0, -0.2f, 0);
        }
        else
        {
            // Stand up
            characterController.height = originalHeight;
            characterController.center = Vector3.zero;
        }
        
        Debug.Log($"Crouch: {isCrouching}");
    }
    
    /// <summary>
    /// Start sliding
    /// </summary>
    private void StartSlide()
    {
        isSliding = true;
        slideTimer = slideDuration;
        
        // Play slide sound
        if (footstepAudio != null)
        {
            footstepAudio.PlayOneShot(jumpSound);
        }
        
        Debug.Log("Started sliding");
    }
    
    /// <summary>
    /// End sliding
    /// </summary>
    private void EndSlide()
    {
        isSliding = false;
        Debug.Log("Ended sliding");
    }
    
    /// <summary>
    /// Perform jump
    /// </summary>
    private void Jump()
    {
        currentVelocity.y = jumpForce;
        
        // Play jump sound
        if (footstepAudio != null && jumpSound != null)
        {
            footstepAudio.PlayOneShot(jumpSound);
        }
        
        Debug.Log("Jumped");
    }
    
    /// <summary>
    /// Play footstep sound
    /// </summary>
    private void PlayFootstep()
    {
        if (footstepAudio == null) return;
        
        AudioClip[] footsteps = walkFootsteps;
        if (isSprinting)
        {
            footsteps = sprintFootsteps;
        }
        else if (isCrouching)
        {
            footsteps = crouchFootsteps;
        }
        
        if (footsteps.Length > 0)
        {
            AudioClip randomFootstep = footsteps[Random.Range(0, footsteps.Length)];
            footstepAudio.PlayOneShot(randomFootstep);
        }
    }
    
    /// <summary>
    /// Create footstep decal
    /// </summary>
    private void CreateFootstepDecal()
    {
        if (footstepDecal == null) return;
        
        Vector3 footstepPosition = transform.position + Vector3.down * 0.1f;
        GameObject decal = Instantiate(footstepDecal, footstepPosition, Quaternion.identity);
        Destroy(decal, 10f);
    }
    
    /// <summary>
    /// Set mouse sensitivity
    /// </summary>
    public void SetMouseSensitivity(float sensitivity)
    {
        mouseSensitivity = sensitivity;
    }
    
    /// <summary>
    /// Get current movement state
    /// </summary>
    public string GetMovementState()
    {
        if (isSliding) return "Sliding";
        if (isSprinting) return "Sprinting";
        if (isCrouching) return "Crouching";
        if (!isGrounded) return "Airborne";
        if (moveDirection.magnitude > 0.1f) return "Walking";
        return "Idle";
    }
    
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Handle landing
        if (!isGrounded && hit.normal.y > 0.7f)
        {
            if (footstepAudio != null && landSound != null)
            {
                footstepAudio.PlayOneShot(landSound);
            }
        }
    }
} 