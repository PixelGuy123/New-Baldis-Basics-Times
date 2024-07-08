using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_Beartrap : Item, IEntityTrigger
	{
		public override bool Use(PlayerManager pm)
		{
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
					active = true;
					target = e;
					target.ExternalActivity.moveMods.Add(moveMod);
					audMan.PlaySingle(audTrap);
					renderer.sprite = closedTrap;
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
		internal Entity entity;

		[SerializeField]
		internal SoundObject audTrap;

		[SerializeField]
		internal SpriteRenderer renderer;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal Sprite closedTrap;

		readonly MovementModifier moveMod = new(Vector3.zero, 0f);

		bool active = false;

		float cooldown = 10f;

		Entity target;

		GameObject owner;
	}
}
