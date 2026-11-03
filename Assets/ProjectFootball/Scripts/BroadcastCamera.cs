using UnityEngine;

namespace ProjectFootball
{
    [RequireComponent(typeof(Camera))]
    public sealed class BroadcastCamera : MonoBehaviour
    {
        private Transform target;
        private readonly Vector3 offset = new Vector3(0f, 25f, -34f);

        private void Start()
        {
            var players = FindObjectsByType<FootballPlayer>(FindObjectsSortMode.None);
            foreach (var player in players)
            {
                if (player.IsHome && player.ShirtNumber == 10)
                {
                    target = player.transform;
                    break;
                }
            }

            if (target == null)
            {
                var ball = GameObject.Find("MatchBall");
                if (ball != null) target = ball.transform;
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;
            Vector3 desiredPosition = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, 2.4f * Time.deltaTime);
            Vector3 lookPoint = Vector3.Lerp(Vector3.zero, target.position, .28f);
            Quaternion desiredRotation = Quaternion.LookRotation(lookPoint - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, 3f * Time.deltaTime);
        }
    }
}
