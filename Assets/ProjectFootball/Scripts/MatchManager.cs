using UnityEngine;

namespace ProjectFootball
{
    public sealed class MatchManager : MonoBehaviour
    {
        public int HomeScore { get; private set; }
        public int AwayScore { get; private set; }

        public void Initialize()
        {
            Debug.Log("PROJECT FOOTBALL: match initialized with 22 player bodies.");
        }

        public void RegisterGoal(bool homeGoal)
        {
            if (homeGoal) HomeScore++;
            else AwayScore++;

            Debug.Log($"GOAL — Home {HomeScore} : {AwayScore} Away");
            var ball = FindFirstObjectByType<FootballBall>();
            if (ball == null) return;

            var body = ball.GetComponent<Rigidbody>();
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            ball.transform.position = new Vector3(0f, .35f, 0f);
        }

        private void OnGUI()
        {
            GUI.Box(new Rect(12f, 12f, 230f, 42f), $"HOME  {HomeScore}  -  {AwayScore}  AWAY");
            GUI.Label(new Rect(12f, 58f, 360f, 24f), "WASD / Arrow keys: Move     SPACE: Kick");
        }
    }
}
