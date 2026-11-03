using UnityEngine;

namespace ProjectFootball
{
    public sealed class GoalTrigger : MonoBehaviour
    {
        private bool isHomeGoal;

        public void Initialize(bool homeGoal)
        {
            isHomeGoal = homeGoal;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<FootballBall>() == null) return;
            var match = FindFirstObjectByType<MatchManager>();
            if (match != null) match.RegisterGoal(isHomeGoal);
        }
    }
}
