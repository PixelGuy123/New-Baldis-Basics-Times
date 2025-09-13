using System.Collections;
using BBTimes.CustomComponents;
using BBTimes.Extensions;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_FryingPan : Item, IItemPrefab
	{
		public void SetupPrefab()
		{
			audMan = gameObject.CreatePropagatedAudioManager(65f, 85f);
			audHit = this.GetSound("pan_hit.wav", "BB_Hit", SoundType.Effect, Color.white);
		}
		public void SetupPrefabPost() { }

		public string Name { get; set; }
		public string Category => "items";

		public ItemObject ItmObj { get; set; }



		public override bool Use(PlayerManager pm)
		{
			this.pm = pm;
			if (Physics.Raycast(pm.transform.position, Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward, out var hit, pm.pc.reach))
			{
				if (hit.transform.CompareTag("NPC"))
				{
					var e = hit.transform.GetComponent<NPC>();
					if (e != null)
					{
						transform.position = e.transform.position;
						this.pm = pm;
						audMan.PlaySingle(audHit);
						pm.RuleBreak("Bullying", 2f, 0.6f);

						StartCoroutine(Timer(e));
						return true;
					}
				}
			}
			Destroy(gameObject);
			return false;
		}

		IEnumerator Timer(NPC tar)
		{
			tar.Entity.ExternalActivity.moveMods.Add(moveMod);
			tar.Entity.AddForce(new(Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.right * (Random.value >= 0.5f ? 1 : -1), hitForce, -hitForce * hitDecreaseFactor));

			float cooldown = 10f;
			while (cooldown > 0f)
			{
				cooldown -= pm.ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}

			tar.Entity.ExternalActivity.moveMods.Remove(moveMod);

			Destroy(gameObject);

			yield break;
		}

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal SoundObject audHit;

		[SerializeField]
		internal float hitForce = 40f;

		[SerializeField]
		[Range(0f, 1f)]
		internal float hitDecreaseFactor = 0.95f;

		readonly MovementModifier moveMod = new(Vector3.zero, 0.1f);

	}
}
