using BBTimes.CustomComponents.CustomDatas;
using System.Collections;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_Pencil : Item
	{
		public override bool Use(PlayerManager pm)
		{				
			if (Physics.Raycast(pm.transform.position, Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward, out var hit, pm.pc.reach))
			{
				if (hit.transform.CompareTag("NPC"))
				{
					var e = hit.transform.GetComponent<NPC>();
					if (e != null)
					{
						gameObject.SetActive(true);
						transform.position = e.transform.position;
						this.pm = pm;
						audMan.PlaySingle(audStab);
						pm.RuleBreak("stabbing", 2f, 0.6f);
						e.GetComponent<CustomNPCData>()?.Stabbed();

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
			tar.Navigator.Entity.IgnoreEntity(pm.plm.Entity, true);
			tar.Navigator.Entity.ExternalActivity.moveMods.Add(moveMod);

			float cooldown = 20f;
			while (cooldown > 0f)
			{
				cooldown -= pm.ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}

			tar.Navigator.Entity.IgnoreEntity(pm.plm.Entity, false);
			tar.Navigator.Entity.ExternalActivity.moveMods.Remove(moveMod);

			Destroy(gameObject);

			yield break;
		}

		[SerializeField]
		internal PropagatedAudioManager audMan;

		internal static SoundObject audStab;

		readonly MovementModifier moveMod = new(Vector3.zero, 0f);
	}
}
