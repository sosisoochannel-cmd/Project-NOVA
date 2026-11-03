using UnityEngine;

namespace ProjectFootball
{
    /// <summary>
    /// Dynamic high-sideline broadcast camera.
    /// Tracks the ball first, predicts its short-term movement and widens the
    /// shot when nearby play spreads out. All movement is smoothed in LateUpdate.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class BroadcastCamera : MonoBehaviour
    {
        [Header("Broadcast position")]
        [SerializeField, Min(35f)] private float sidelineDistance = 60f;
        [SerializeField, Min(20f)] private float baseHeight = 29.5f;
        [SerializeField, Min(0f)] private float trailingDistance = 7f;
        [SerializeField, Range(0f, 0.6f)] private float lateralTracking = 0.35f;

        [Header("Framing")]
        [SerializeField, Range(0.1f, 0.8f)] private float predictionLeadTime = 0.28f;
        [SerializeField, Range(0f, 0.5f)] private float activePlayerInfluence = 0.18f;
        [SerializeField, Range(30f, 50f)] private float minimumFieldOfView = 39f;
        [SerializeField, Range(40f, 60f)] private float maximumFieldOfView = 49f;
        [SerializeField, Min(5f)] private float wideShotPlayerRadius = 24f;

        [Header("Camera motion")]
        [SerializeField, Min(0.05f)] private float positionSmoothTime = 0.38f;
        [SerializeField, Min(0.1f)] private float rotationSharpness = 4.5f;
        [SerializeField, Min(0.1f)] private float zoomSmoothTime = 0.65f;

        private Camera sceneCamera;
        private FootballBall trackedBall;
        private Rigidbody ballBody;
        private FootballPlayer selectedPlayer;
        private FootballPlayer[] players = System.Array.Empty<FootballPlayer>();

        private Vector3 positionVelocity;
        private float zoomVelocity;
        private float refreshTimer;
        private bool initialized;

        private void Awake()
        {
            sceneCamera = GetComponent<Camera>();
            sceneCamera.allowHDR = true;
            sceneCamera.allowMSAA = true;
            sceneCamera.nearClipPlane = 0.3f;
            sceneCamera.farClipPlane = 400f;
            sceneCamera.useOcclusionCulling = true;
        }

        private void Start()
        {
            RefreshTargets();
        }

        private void LateUpdate()
        {
            if (sceneCamera == null) return;

            refreshTimer -= Time.deltaTime;
            if (refreshTimer <= 0f)
            {
                RefreshTargets();
                refreshTimer = 1f;
            }

            if (trackedBall == null && selectedPlayer == null) return;

            Vector3 ballPosition = trackedBall != null
                ? trackedBall.transform.position
                : selectedPlayer.transform.position;

            Vector3 ballVelocity = ballBody != null
                ? ballBody.linearVelocity
                : Vector3.zero;
            ballVelocity.y = 0f;
            ballVelocity = Vector3.ClampMagnitude(ballVelocity, 28f);

            // A short prediction keeps fast passes and clearances from escaping the frame.
            Vector3 predictedBall = ballPosition + ballVelocity * predictionLeadTime;

            // Keep the selected player in the composition without allowing them
            // to pull the camera away from the actual ball.
            Vector3 focus = predictedBall;
            if (selectedPlayer != null && trackedBall != null)
            {
                Vector3 playerPosition = selectedPlayer.transform.position;
                float separation = PlanarDistance(predictedBall, playerPosition);
                float influence = activePlayerInfluence *
                    (1f - Mathf.InverseLerp(5f, 22f, separation));
                focus = Vector3.Lerp(predictedBall, playerPosition, influence);
            }

            focus.x = Mathf.Clamp(focus.x, -32f, 32f);
            focus.z = Mathf.Clamp(focus.z, -50f, 50f);
            focus.y = Mathf.Clamp(focus.y, 0f, 2f);

            float actionSpread = MeasureActionSpread(focus);
            float wideShot = Mathf.InverseLerp(8f, wideShotPlayerRadius, actionSpread);
            float distanceToGoalLine = Mathf.Max(0f, 52.5f - Mathf.Abs(focus.z));
            float goalContext = 1f - Mathf.InverseLerp(8f, 25f, distanceToGoalLine);

            // The camera stays outside the touchline and moves along it.
            // This creates a stable, familiar TV broadcast angle instead of a
            // player-follow chase camera.
            float lateralOffset = focus.x * lateralTracking;
            Vector3 desiredPosition = new Vector3(
                -sidelineDistance + lateralOffset,
                baseHeight + wideShot * 4f + goalContext * 1.2f,
                focus.z - trailingDistance - ballVelocity.z * 0.10f);

            // Aim slightly above the ball so the ball sits below centre,
            // leaving more room to read player movement and passing lanes.
            Vector3 lookPoint = new Vector3(
                focus.x * lateralTracking,
                2.2f,
                focus.z);

            float desiredFov = Mathf.Lerp(minimumFieldOfView, maximumFieldOfView, wideShot)
                               + goalContext * 1.2f;
            desiredFov = Mathf.Clamp(desiredFov, minimumFieldOfView, maximumFieldOfView);

            if (!initialized)
            {
                transform.position = desiredPosition;
                transform.rotation = Quaternion.LookRotation(lookPoint - transform.position, Vector3.up);
                sceneCamera.fieldOfView = desiredFov;
                initialized = true;
                return;
            }

            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref positionVelocity,
                positionSmoothTime,
                Mathf.Infinity,
                Time.deltaTime);

            Vector3 lookDirection = lookPoint - transform.position;
            if (lookDirection.sqrMagnitude > 0.001f)
            {
                Quaternion desiredRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
                float rotationBlend = 1f - Mathf.Exp(-rotationSharpness * Time.deltaTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationBlend);
            }

            sceneCamera.fieldOfView = Mathf.SmoothDamp(
                sceneCamera.fieldOfView,
                desiredFov,
                ref zoomVelocity,
                zoomSmoothTime);
        }

        private void RefreshTargets()
        {
            trackedBall = FindFirstObjectByType<FootballBall>();
            ballBody = trackedBall != null ? trackedBall.GetComponent<Rigidbody>() : null;
            players = FindObjectsByType<FootballPlayer>(FindObjectsSortMode.None);

            selectedPlayer = null;
            foreach (FootballPlayer player in players)
            {
                if (player != null && player.IsHome && player.IsSelected)
                {
                    selectedPlayer = player;
                    break;
                }
            }
        }

        private float MeasureActionSpread(Vector3 focus)
        {
            float spread = 8f;
            float radiusSquared = wideShotPlayerRadius * wideShotPlayerRadius;

            foreach (FootballPlayer player in players)
            {
                if (player == null) continue;

                float distanceSquared = PlanarDistanceSquared(focus, player.transform.position);
                if (distanceSquared <= radiusSquared)
                {
                    spread = Mathf.Max(spread, Mathf.Sqrt(distanceSquared) * 0.72f);
                }
            }

            return spread;
        }

        private static float PlanarDistance(Vector3 a, Vector3 b)
        {
            return Mathf.Sqrt(PlanarDistanceSquared(a, b));
        }

        private static float PlanarDistanceSquared(Vector3 a, Vector3 b)
        {
            float dx = a.x - b.x;
            float dz = a.z - b.z;
            return dx * dx + dz * dz;
        }
    }
}
