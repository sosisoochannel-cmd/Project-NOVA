using UnityEngine;

namespace ProjectFootball
{
    public sealed class FootballBootstrap : MonoBehaviour
    {
        [Header("Pitch dimensions (metres)")]
        [SerializeField] private float pitchLength = 105f;
        [SerializeField] private float pitchWidth = 68f;

        private void Start()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            BuildLighting();
            BuildPitch();
            BuildGoals();
            BuildBall();
            BuildTeams();
            BuildCamera();
            var match = new GameObject("MatchManager").AddComponent<MatchManager>();
            match.Initialize();
        }

        private static Material Material(Color color)
        {
            var shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
            var material = new Material(shader) { color = color };
            return material;
        }

        private static void BuildLighting()
        {
            RenderSettings.ambientLight = new Color(.48f, .52f, .58f);
            var sun = new GameObject("Sun");
            var light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            sun.transform.rotation = Quaternion.Euler(48f, -30f, 0f);
        }

        private void BuildPitch()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Pitch";
            ground.transform.localScale = new Vector3(pitchWidth / 10f, 1f, pitchLength / 10f);
            ground.GetComponent<Renderer>().material = Material(new Color(.075f, .32f, .13f));

            const float lineY = .025f;
            Strip("Touchline_Left", new Vector3(-pitchWidth / 2f, lineY, 0f), new Vector3(.12f, .025f, pitchLength));
            Strip("Touchline_Right", new Vector3(pitchWidth / 2f, lineY, 0f), new Vector3(.12f, .025f, pitchLength));
            Strip("GoalLine_North", new Vector3(0f, lineY, pitchLength / 2f), new Vector3(pitchWidth, .025f, .12f));
            Strip("GoalLine_South", new Vector3(0f, lineY, -pitchLength / 2f), new Vector3(pitchWidth, .025f, .12f));
            Strip("HalfwayLine", new Vector3(0f, lineY, 0f), new Vector3(pitchWidth, .025f, .12f));

            var circle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            circle.name = "CentreCircle";
            circle.transform.position = new Vector3(0f, .012f, 0f);
            circle.transform.localScale = new Vector3(18.3f, .012f, 18.3f);
            circle.GetComponent<Renderer>().material = Material(new Color(.09f, .36f, .15f));
            Destroy(circle.GetComponent<Collider>());
        }

        private static void Strip(string name, Vector3 position, Vector3 scale)
        {
            var item = GameObject.CreatePrimitive(PrimitiveType.Cube);
            item.name = name;
            item.transform.position = position;
            item.transform.localScale = scale;
            item.GetComponent<Renderer>().material = Material(Color.white);
            Destroy(item.GetComponent<Collider>());
        }

        private void BuildGoals()
        {
            CreateGoal("HomeGoal", pitchLength / 2f, true);
            CreateGoal("AwayGoal", -pitchLength / 2f, false);
        }

        private static void CreateGoal(string name, float z, bool homeGoal)
        {
            var root = new GameObject(name);
            root.transform.position = new Vector3(0f, 0f, z);
            const float halfWidth = 3.66f;
            const float height = 2.44f;
            GoalPost(root.transform, new Vector3(-halfWidth, height / 2f, 0f), new Vector3(.14f, height, .14f));
            GoalPost(root.transform, new Vector3(halfWidth, height / 2f, 0f), new Vector3(.14f, height, .14f));
            GoalPost(root.transform, new Vector3(0f, height, 0f), new Vector3(7.32f, .14f, .14f));

            var trigger = root.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(7.32f, 2.44f, 2.2f);
            root.AddComponent<GoalTrigger>().Initialize(homeGoal);
        }

        private static void GoalPost(Transform parent, Vector3 position, Vector3 scale)
        {
            var post = GameObject.CreatePrimitive(PrimitiveType.Cube);
            post.transform.SetParent(parent, false);
            post.transform.localPosition = position;
            post.transform.localScale = scale;
            post.GetComponent<Renderer>().material = Material(Color.white);
        }

        private static void BuildBall()
        {
            var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball.name = "MatchBall";
            ball.transform.position = new Vector3(0f, .35f, 0f);
            ball.transform.localScale = Vector3.one * .44f;
            ball.GetComponent<Renderer>().material = Material(Color.white);
            var body = ball.AddComponent<Rigidbody>();
            body.mass = .43f;
            body.linearDamping = .08f;
            body.angularDamping = .05f;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            ball.AddComponent<FootballBall>();
        }

        private static void BuildTeams()
        {
            Vector3[] home = {
                new Vector3(0,0,-48), new Vector3(-23,0,-36), new Vector3(-8,0,-38),
                new Vector3(8,0,-38), new Vector3(23,0,-36), new Vector3(-19,0,-17),
                new Vector3(-6,0,-20), new Vector3(6,0,-20), new Vector3(19,0,-17),
                new Vector3(-8,0,-5), new Vector3(8,0,-5)
            };
            Vector3[] away = {
                new Vector3(0,0,48), new Vector3(-23,0,36), new Vector3(-8,0,38),
                new Vector3(8,0,38), new Vector3(23,0,36), new Vector3(-19,0,17),
                new Vector3(-6,0,20), new Vector3(6,0,20), new Vector3(19,0,17),
                new Vector3(-8,0,5), new Vector3(8,0,5)
            };

            for (int i = 0; i < 11; i++)
            {
                CreatePlayer("Home", i + 1, home[i], new Color(.08f, .24f, .88f), true);
                CreatePlayer("Away", i + 1, away[i], new Color(.82f, .12f, .10f), false);
            }
        }

        private static void CreatePlayer(string team, int number, Vector3 position, Color color, bool isHome)
        {
            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = team + "_Player_" + number.ToString("00");
            player.transform.position = position + Vector3.up;
            player.GetComponent<Renderer>().material = Material(color);
            var body = player.AddComponent<Rigidbody>();
            body.mass = 75f;
            body.constraints = RigidbodyConstraints.FreezeRotation;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            player.AddComponent<FootballPlayer>().Initialize(isHome, number);
        }

        private static void BuildCamera()
        {
            var cameraObject = new GameObject("BroadcastCamera");
            var camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 48f;
            camera.nearClipPlane = .1f;
            camera.farClipPlane = 250f;
            cameraObject.AddComponent<AudioListener>();
            cameraObject.AddComponent<BroadcastCamera>();
        }
    }
}
