using UnityEngine;

public class CarController : MonoBehaviour
{
    // Reference to the auto-generated Input Actions class
    private CarInputActions inputActions;

    // Stores movement input (X = Steering, Y = Acceleration/Reverse)
    private Vector2 moveInput;

    // Stores handbrake state (1 = Pressed, 0 = Released)
    private float handbrakeInput;

    [Header("Wheel Colliders")]
    [SerializeField] private WheelCollider frontLeftWheelCollider;
    [SerializeField] private WheelCollider frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider;
    [SerializeField] private WheelCollider rearRightWheelCollider;

    [Header("Wheel Transforms (Visuals)")]
    [SerializeField] private Transform frontLeftWheelTransform;
    [SerializeField] private Transform frontRightWheelTransform;
    [SerializeField] private Transform rearLeftWheelTransform;
    [SerializeField] private Transform rearRightWheelTransform;

    [Header("Vehicle Performance Settings")]
    [SerializeField] private float motorForce = 2000f;       // Acceleration force
    [SerializeField] private float brakeForce = 4000f;       // Braking force
    [SerializeField] private float maxSteerAngle = 30f;      // Maximum steering angle in degrees
    [SerializeField] private float handbrakeForce = 5000f;   // Handbrake resistance force

    private void Awake()
    {
        // Instantiate the generated C# input class
        inputActions = new CarInputActions();
    }

    private void OnEnable()
    {
        // Enable the 'Vehicle' action map when the object becomes active
        inputActions.Vehicle.Enable();
    }

    private void OnDisable()
    {
        // Disable the 'Vehicle' action map when the object is disabled
        inputActions.Vehicle.Disable();
    }

    private void Update()
    {
        // Read the 2D vector for movement (W/S for Y, A/D for X)
        moveInput = inputActions.Vehicle.Move.ReadValue<Vector2>();

        // Read the handbrake button state (Spacebar)
        handbrakeInput = inputActions.Vehicle.Handbrake.ReadValue<float>();
    }

    private void FixedUpdate()
    {
        // Extract direction and throttle values
        float steeringAxis = moveInput.x;   // Range: -1 (Left) to 1 (Right)
        float throttleAxis = moveInput.y;   // Range: -1 (Reverse) to 1 (Forward)
        bool isHandbraking = handbrakeInput > 0f;

        // Apply physics to your WheelColliders
        HandleMotor(throttleAxis, isHandbraking);
        HandleSteering(steeringAxis);
        UpdateWheelVisuals();
    }

    private void HandleMotor(float throttle, bool isHandbraking)
    {
        // If handbraking, cut motor torque completely; otherwise apply throttle force
        float currentMotorTorque = isHandbraking ? 0f : (throttle * motorForce);

        // Apply motor torque to rear wheels (Rear-Wheel Drive setup)
        rearLeftWheelCollider.motorTorque = currentMotorTorque;
        rearRightWheelCollider.motorTorque = currentMotorTorque;

        // Determine braking torque based on user inputs
        float currentBrakeTorque = 0f;

        if (isHandbraking)
        {
            currentBrakeTorque = handbrakeForce;
        }
        else if (throttle == 0f)
        {
            // Natural engine braking when no throttle is applied
            currentBrakeTorque = brakeForce;
        }

        // Apply brake torque across all wheels
        frontLeftWheelCollider.brakeTorque = currentBrakeTorque;
        frontRightWheelCollider.brakeTorque = currentBrakeTorque;
        rearLeftWheelCollider.brakeTorque = currentBrakeTorque;
        rearRightWheelCollider.brakeTorque = currentBrakeTorque;
    }

    private void HandleSteering(float steerAngle)
    {
        // Apply calculated steering angle to the front wheels
        float currentSteerAngle = steerAngle * maxSteerAngle;
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }

    private void UpdateWheelVisuals()
    {
        // Update visual transform for each wheel using GetWorldPose
        UpdateSingleWheelVisual(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheelVisual(frontRightWheelCollider, frontRightWheelTransform);
        UpdateSingleWheelVisual(rearLeftWheelCollider, rearLeftWheelTransform);
        UpdateSingleWheelVisual(rearRightWheelCollider, rearRightWheelTransform);
    }

    private void UpdateSingleWheelVisual(WheelCollider wheelCollider, Transform wheelTransform)
    {
        if (wheelTransform == null) return;

        // Get world position and rotation from the physics WheelCollider
        wheelCollider.GetWorldPose(out Vector3 position, out Quaternion rotation);

        // Apply position and rotation to the visual 3D mesh object
        wheelTransform.position = position;
        wheelTransform.rotation = rotation;
    }
}