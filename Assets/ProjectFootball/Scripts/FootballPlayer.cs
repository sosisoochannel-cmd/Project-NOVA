using UnityEngine;

namespace ProjectFootball
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class FootballPlayer : MonoBehaviour
    {
        public bool IsHome { get; private set; }
        public int ShirtNumber { get; private set; }
        public bool IsSelected { get; private set; }

        private Rigidbody body;
        private static FootballPlayer selectedPlayer;
        private const float MoveSpeed = 6.8f;

        public void Initialize(bool isHome, int shirtNumber)
        {
            IsHome = isHome;
            ShirtNumber = shirtNumber;
            body = GetComponent<Rigidbody>();
            if (isHome && shirtNumber == 10) Select();
        }

        private void Update()
        {
            if (!IsHome || !IsSelected) return;

            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (input.sqrMagnitude > 1f) input.Normalize();
            Vector3 direction = new Vector3(input.x, 0f, input.y);

            if (direction.sqrMagnitude > .01f)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, Quaternion.LookRotation(direction), 10f * Time.deltaTime);
            }

            if (Input.GetKeyDown(KeyCode.Space)) KickBall();
        }

        private void FixedUpdate()
        {
            if (!IsHome || !IsSelected || body == null) return;
            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (input.sqrMagnitude > 1f) input.Normalize();
            Vector3 velocity = new Vector3(input.x * MoveSpeed, body.linearVelocity.y, input.y * MoveSpeed);
            body.linearVelocity = velocity;
        }

        private void Select()
        {
            if (selectedPlayer != null) selectedPlayer.IsSelected = false;
            selectedPlayer = this;
            IsSelected = true;
        }

        private void KickBall()
        {
            var ball = FindFirstObjectByType<FootballBall>();
            if (ball == null || Vector3.Distance(transform.position, ball.transform.position) > 3.2f) return;
            Vector3 forward = transform.forward;
            if (forward.sqrMagnitude < .1f) forward = Vector3.forward;
            ball.Kick(forward * 12.5f + Vector3.up * 2.2f);
        }
    }
}
