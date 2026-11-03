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
        private Vector2 touchInput;

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
            if (touchInput.sqrMagnitude > .01f) input = touchInput;
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
            if (touchInput.sqrMagnitude > .01f) input = touchInput;
            if (input.sqrMagnitude > 1f) input.Normalize();
            body.linearVelocity = new Vector3(input.x * MoveSpeed, body.linearVelocity.y, input.y * MoveSpeed);
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

        private void OnGUI()
        {
            if (!IsHome || !IsSelected) return;

            GUI.Label(new Rect(12f, 12f, 280f, 28f), "PROJECT FOOTBALL  |  PLAYER 10");

            float left = 22f;
            float bottom = Screen.height - 150f;
            float size = 58f;
            bool up = GUI.RepeatButton(new Rect(left + size, bottom, size, size), "▲");
            bool down = GUI.RepeatButton(new Rect(left + size, bottom + size * 1.45f, size, size), "▼");
            bool goLeft = GUI.RepeatButton(new Rect(left, bottom + size * .72f, size, size), "◀");
            bool goRight = GUI.RepeatButton(new Rect(left + size * 2f, bottom + size * .72f, size, size), "▶");

            touchInput = new Vector2((goRight ? 1f : 0f) - (goLeft ? 1f : 0f),
                                     (up ? 1f : 0f) - (down ? 1f : 0f));

            if (GUI.Button(new Rect(Screen.width - 128f, Screen.height - 118f, 108f, 94f), "KICK"))
                KickBall();
        }
    }
}
