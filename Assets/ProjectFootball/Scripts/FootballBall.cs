using UnityEngine;

namespace ProjectFootball
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class FootballBall : MonoBehaviour
    {
        private Rigidbody body;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
        }

        public void Kick(Vector3 impulse)
        {
            body.AddForce(impulse, ForceMode.Impulse);
        }

        private void FixedUpdate()
        {
            Vector3 planarVelocity = new Vector3(body.linearVelocity.x, 0f, body.linearVelocity.z);
            if (planarVelocity.sqrMagnitude < .02f)
                body.linearVelocity = new Vector3(0f, body.linearVelocity.y, 0f);
        }
    }
}
