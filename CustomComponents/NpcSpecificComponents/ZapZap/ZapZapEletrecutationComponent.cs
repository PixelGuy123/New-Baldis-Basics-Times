using UnityEngine;

namespace BBTimes.CustomComponents.NpcSpecificComponents.ZapZap
{
	public class ZapZapEletrecutationComponent : MonoBehaviour
	{
		public void AttachTo(ActivityModifier actMod, EnvironmentController ec, ZapZapEletricity initializer)
		{
			eletricity = initializer;

			moveMod.movementMultiplier = slowFactor;

			target = actMod;
			target.moveMods.Add(moveMod);
			this.ec = ec;

			audMan.maintainLoop = true;
			audMan.SetLoop(true);
			audMan.QueueAudio(audEletrecute);


			initialized = true;
		}

		void OnTriggerStay(Collider other)
		{
			if (!initialized || target.gameObject == other.gameObject || eletricity.EletrecutationComponents.Exists(el => el.Overrider?.gameObject == other.gameObject)) return;

			if (other.isTrigger && (other.CompareTag("Player") || other.CompareTag("NPC")))
			{
				var e = other.GetComponent<Entity>();
				if (e)
					eletricity.CreateEletricity(e.ExternalActivity);
			}
		}

		void Update()
		{
			if (!initialized)
				return;

			if (!target)
			{
				Despawn();
				return;
			}

			moveMod.movementAddend.x = Random.Range(-eletricityForce, eletricityForce);
			moveMod.movementAddend.z = Random.Range(-eletricityForce, eletricityForce);

			transform.position = target.transform.position;

			timer -= ec.EnvironmentTimeScale * Time.deltaTime;
			if (timer < 0f)
				Despawn();
			
		}

		public void Despawn()
		{
			target?.moveMods.Remove(moveMod);
			eletricity.EletrecutationComponents.Remove(this);
			Destroy(gameObject);
		}

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal SoundObject audEletrecute;

		[SerializeField]
		internal float timer = 15f, eletricityForce = 16f;

		[SerializeField]
		[Range(0f, 1f)]
		internal float slowFactor = 0.65f;

		readonly MovementModifier moveMod = new(Vector3.zero, 1f);

		ZapZapEletricity eletricity;

		bool initialized = false;
		ActivityModifier target;
		public ActivityModifier Overrider => target;

		EnvironmentController ec;
	}
}
