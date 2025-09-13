using System.Collections;
using BBTimes.CustomComponents;
using BBTimes.Extensions;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_Pencil : Item, IItemPrefab
	{
		public void SetupPrefab()
		{
			audMan = gameObject.CreatePropagatedAudioManager(65f, 85f);
			item = ItmObj.itemType;
		}
		public void SetupPrefabPost() { }

		public string Name { get; set; }
		public string Category => "items";

		public ItemObject ItmObj { get; set; }



		public override bool Use(PlayerManager pm)
		{
			if (Physics.Raycast(pm.transform.position, Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward, out var hit, pm.pc.reach))
			{
				if (hit.transform.CompareTag("NPC"))
				{
					var e = hit.transform.GetComponent<NPC>();
					if (e != null)
					{
						transform.position = e.transform.position;
						this.pm = pm;
						audMan.PlaySingle(audStab);
						pm.RuleBreak("stabbing", 2f, 0.6f);
						var acceptor = e.GetComponent<IItemAcceptor>();

						if (acceptor != null && acceptor.ItemFits(item))
							acceptor.InsertItem(pm, pm.ec);


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
			tar.Entity.IgnoreEntity(pm.plm.Entity, true);
			tar.Entity.ExternalActivity.moveMods.Add(moveMod);

			float cooldown = 20f;
			while (cooldown > 0f)
			{
				cooldown -= pm.ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}

			tar.Entity.IgnoreEntity(pm.plm.Entity, false);
			tar.Entity.ExternalActivity.moveMods.Remove(moveMod);

			Destroy(gameObject);

			yield break;
		}

		[SerializeField]
		internal Items item;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		internal static SoundObject audStab;

		readonly MovementModifier moveMod = new(Vector3.zero, 0f);
	}
}
