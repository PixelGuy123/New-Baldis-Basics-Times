using UnityEngine;

namespace BBTimes.CustomContent.Builders
{
    public class Structure_WaterCreator : StructureBuilder
    {
        [SerializeField]
        public WaterMover waterPrefab;
        public override void OnGenerationFinished(LevelBuilder lb)
        {
            base.OnGenerationFinished(lb);

            WaterMover inst = Instantiate<WaterMover>(waterPrefab);
            inst.name = "water";
            inst.ec = ec;
            inst.transform.SetParent(ec.transform, false);
            inst.transform.position = new Vector3(0, 0, 0);
            inst.transform.localScale = new Vector3(2500, 1, 2500);

        }
    }

    public class WaterMover : MonoBehaviour
    {
        Vector2 LimitPos = new Vector2(50, 50);
        Vector3 Speed = new Vector3(0.45f, 0, 0.45f);

        [SerializeField]
        public EnvironmentController ec;

        void Start()
        {
            
        }
        void Update()
        {
            transform.position += Speed * Time.deltaTime * ec.EnvironmentTimeScale;
            if (transform.position.x > LimitPos.x)
            {
                transform.position -= Vector3.right * LimitPos.x;
            }
            if (transform.position.z > LimitPos.y)
            {
                transform.position -= Vector3.forward * LimitPos.y;
            }
        }
    }
}