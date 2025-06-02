using PixelInternalAPI.Classes;
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

			if (target.TryGetComponent<PlayerManager>(out var pm))
				gauge = Singleton<CoreGameManager>.Instance.GetHud(pm.playerNumber).gaugeManager.ActivateNewGauge(gaugeSprite, lifeTime);


			audMan.maintainLoop = true;
			audMan.SetLoop(true);
			audMan.QueueAudio(audEletrecute);

			lifeTime = timer;
			initialized = true;
		}

		void OnTriggerStay(Collider other)
		{
			if (!initialized || target.gameObject == other.gameObject) return;

			if (other.isTrigger && (other.CompareTag("Player") || other.CompareTag("NPC")))
			{
				var e = other.GetComponent<Entity>();
				if (e)
				{
					ray.origin = transform.position;
					ray.direction = (other.transform.position - transform.position).normalized;
					if (Physics.Raycast(ray, out hit, rayCastRadius, raycastLayer, QueryTriggerInteraction.Ignore) && hit.transform == other.transform)
						eletricity.CreateEletricity(e.ExternalActivity);
				}
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

			lifeTime -= ec.EnvironmentTimeScale * Time.deltaTime;
			gauge?.SetValue(timer, lifeTime);
			if (lifeTime < 0f)
				Despawn();

		}

		public void Despawn()
		{
			target?.moveMods.Remove(moveMod);
			gauge?.Deactivate();
			eletricity.EletrecutationComponents.Remove(this);
			Destroy(gameObject);
		}

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal SoundObject audEletrecute;

		[SerializeField]
		internal float timer = 15f, eletricityForce = 16f, rayCastRadius = 50f;

		[SerializeField]
		[Range(0f, 1f)]
		internal float slowFactor = 0.65f;

		[SerializeField]
		internal LayerMask raycastLayer = LayerStorage.principalLookerMask;

		[SerializeField]
		internal Sprite gaugeSprite;

		HudGauge gauge;

		readonly MovementModifier moveMod = new(Vector3.zero, 1f);

		ZapZapEletricity eletricity;

		bool initialized = false;
		ActivityModifier target;
		public ActivityModifier Overrider => target;

		EnvironmentController ec;
		Ray ray = new();
		RaycastHit hit;
		float lifeTime;
	}
}
