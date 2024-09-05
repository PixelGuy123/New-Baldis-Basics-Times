using BBTimes.Extensions;
using System.Collections.Generic;
using UnityEngine;


namespace BBTimes.CustomComponents
{
	public class GlueObject : MonoBehaviour
	{
		void OnTriggerEnter(Collider other)
		{
			if (other.gameObject == ownerObject) return;
			var player = other.CompareTag("Player");
			if (other.isTrigger && (other.CompareTag("NPC") || player))
			{
				var e = other.GetComponent<Entity>();
				if (e && e.Grounded)
				{
					ActivityEnter(e.ExternalActivity);
					if (player)
					{
						var pm = e.GetComponent<PlayerManager>().GetAttribute();
						if (!ignoreBootsAttribute && pm.HasAttribute("boots"))
							return;
						pms.Add(new(e.ExternalActivity, pm));
					}
					mods.Add(e.ExternalActivity);
					e.ExternalActivity.moveMods.Add(moveMod);
				}
			}
		}
		protected virtual void ActivityEnter(ActivityModifier actMod) { }

		void OnTriggerExit(Collider other)
		{
			if (affectOwnerAfterExit && other.gameObject == ownerObject)
				ownerObject = null;

			var player = other.CompareTag("Player");
			if (other.isTrigger && (other.CompareTag("NPC") || player))
			{
				var e = other.GetComponent<Entity>();
				if (e && mods.Contains(e.ExternalActivity))
				{
					ActivityExit(e.ExternalActivity);
					if (player)
					{
						var at = e.GetComponent<PlayerManager>().GetAttribute();
						pms.RemoveAll(x => x.Value == at);
					}


					mods.Remove(e.ExternalActivity);
					e.ExternalActivity.moveMods.Remove(moveMod);
				}
			}
		}

		protected virtual void ActivityExit(ActivityModifier actMod) { }

		void OnDestroy()
		{
			while (mods.Count != 0)
			{
				mods[0].moveMods.Remove(moveMod);
				mods.RemoveAt(0);
			}
		}

		void Update()
		{
			if (Time.timeScale == 0) return;

			if (!ignoreBootsAttribute)
			{
				for (int i = 0; i < pms.Count; i++)
				{
					if (pms[i].Value.HasAttribute("boots"))
					{
						pms[i].Key.moveMods.Remove(moveMod);
						pms.RemoveAt(i--);
					}
				}
			}

			VirtualUpdate();
		}

		protected virtual void VirtualUpdate() { }

		public void Initialize(GameObject owner, Vector3 position, float modMultiplier, EnvironmentController ec)
		{
			this.ec = ec;
			ownerObject = owner;
			transform.position = position;
			moveMod.movementMultiplier = modMultiplier;
			Initialize();
		}

		protected virtual void Initialize() { }

		protected readonly List<ActivityModifier> mods = [];

		protected readonly List<KeyValuePair<ActivityModifier, PlayerAttributesComponent>> pms = [];

		GameObject ownerObject;

		protected readonly MovementModifier moveMod = new(Vector3.zero, 1f);

		protected EnvironmentController ec;

		[SerializeField]
		internal bool affectOwnerAfterExit = false, ignoreBootsAttribute = false;
	}
}
