using BBTimes.CustomContent.NPCs;
using BBTimes.Extensions;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomComponents
{
	public class Eletricity : MonoBehaviour
	{
		void OnTriggerEnter(Collider other)
		{
			if (other.gameObject == owner.gameObject) return;
			var player = other.CompareTag("Player");
			if (other.isTrigger && (other.CompareTag("NPC") || player))
			{
				var e = other.GetComponent<Entity>();
				if (e)
				{
					if (player)
					{
						var pm = e.GetComponent<PlayerManager>().GetAttribute();
						if (pm.HasAttribute("boots"))
							return;
						pms.Add(new(e.ExternalActivity, pm));
					}
					mods.Add(e.ExternalActivity);
					e.ExternalActivity.moveMods.Add(moveMod);
				}
			}
		}

		void OnTriggerExit(Collider other)
		{
			var player = other.CompareTag("Player");
			if (other.isTrigger && (other.CompareTag("NPC") || player))
			{
				var e = other.GetComponent<Entity>();
				if (e && mods.Contains(e.ExternalActivity))
				{
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

		void OnDestroy()
		{
			while (mods.Count > 0)
			{
				mods[0].moveMods.Remove(moveMod);
				mods.RemoveAt(0);
			}
		}

		void Update()
		{
			if (ec)
			{
				frame += 15f * ec.EnvironmentTimeScale * Time.deltaTime;
				frame %= animation.Length;
				renderer.sprite = animation[Mathf.FloorToInt(frame)];
			}

			for (int i = 0; i < pms.Count; i++)
			{
				if (pms[i].Value.HasAttribute("boots"))
				{
					pms[i].Key.moveMods.Remove(moveMod);
					pms.RemoveAt(i--);
				}
			}
		}

		public void Initialize(EnvironmentController ec, Cell cell, RollingBot owner)
		{
			this.ec = ec;
			transform.position = cell.FloorWorldPosition;
			this.owner = owner;
		}

		readonly List<ActivityModifier> mods = [];
		readonly List<KeyValuePair<ActivityModifier, PlayerAttributesComponent>> pms = [];

		readonly MovementModifier moveMod = new(Vector3.zero, 0.7f);

		[SerializeField]
		internal Sprite[] animation;

		[SerializeField]
		internal SpriteRenderer renderer;

		EnvironmentController ec;
		RollingBot owner;
		float frame = 0f;
	}
}
