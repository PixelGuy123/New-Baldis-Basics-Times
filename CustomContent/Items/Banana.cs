using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class Banana : Item, IEntityTrigger
	{
		public override bool Use(PlayerManager pm)
		{
			gameObject.SetActive(true);
			this.pm = pm;
			entity.Initialize(pm.ec, pm.transform.position);
			pm.RuleBreak("littering", 5f, 0.8f);
			owner = pm.gameObject;

			return true;
		}

		public void EntityTriggerEnter(Collider other)
		{
			if (other.gameObject == owner || active)
				return;

			if (other.CompareTag("NPC") || other.CompareTag("Player"))
			{
				var e = other.GetComponent<Entity>();
				if (e != null && e.Grounded)
				{
					audMan.PlaySingle(aud_slip);
					rendererBase.gameObject.SetActive(false);
					target = e;
					moveMod.movementAddend = e.transform.forward * 85f;
					transform.SetParent(e.transform);
					transform.localPosition = Vector3.zero;

					e.ExternalActivity.moveMods.Add(moveMod);
					active = true;
				}
			}
		}

		public void EntityTriggerStay(Collider other)
		{
		}

		public void EntityTriggerExit(Collider other)
		{
			if (other.gameObject == owner)
				owner = null;
		}

		private void Update()
		{
			if (!active) return;

			cooldown -= pm.ec.EnvironmentTimeScale * Time.deltaTime;
			if (cooldown < 0f)
			{
				target.ExternalActivity.moveMods.Remove(moveMod);
				Destroy(gameObject);
			}
		}

		[SerializeField]
		public Transform rendererBase;

		[SerializeField]
		public Entity entity;

		[SerializeField]
		public SoundObject aud_slip;

		[SerializeField]
		public PropagatedAudioManager audMan;

		readonly MovementModifier moveMod = new(Vector3.zero, 0.1f, 1); // Higher priority than belt managers, lower than Bsodas

		bool active = false;

		float cooldown = 5f;

		Entity target;

		GameObject owner;
	}
}
