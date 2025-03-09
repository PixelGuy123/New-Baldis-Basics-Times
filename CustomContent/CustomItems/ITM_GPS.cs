using BBTimes.CustomComponents;
using BBTimes.Extensions;
using System.Collections;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_GPS : Item, IItemPrefab
	{
		public void SetupPrefab() =>
			aud_beep = this.GetSound("gps_beep.wav", "Vfx_GPS_Beep", SoundType.Effect, Color.white);
		
		public void SetupPrefabPost() { }

		public string Name { get; set; } public string Category => "items";
		
		public ItemObject ItmObj { get; set; }


		public override bool Use(PlayerManager pm)
		{
			if (usedGps || pm.ec.npcsLeftToSpawn.Count != 0 || pm.ec.Npcs.Count == 0)
			{
				Destroy(gameObject);
				return false;
			}
			usedGps = true;

			Singleton<CoreGameManager>.Instance.audMan.PlaySingle(aud_beep);
			this.pm = pm;

			StartCoroutine(Timer());

			return true;
		}

		IEnumerator Timer()
		{
			for (int i = 0; i < pm.ec.Npcs.Count; i++)
				if (pm.ec.Npcs[i])
					pm.ec.map.AddArrow(pm.ec.Npcs[i].Navigator.Entity, Color.yellow);
				
			
			float cooldown = timerSeconds;

			while (cooldown >= 0f)
			{
				cooldown -= pm.PlayerTimeScale * Time.deltaTime;
				yield return null;
			}

			for (int i = 0; i < pm.ec.Npcs.Count; i++)
			{
				if (pm.ec.Npcs[i])
				{
					int idx = pm.ec.map.arrowTargets.IndexOf(pm.ec.Npcs[i].Navigator.Entity);
					if (idx != -1)
						pm.ec.map.arrowTargets[idx] = null; // The map will automatically remove it
				}
			}
			

			Destroy(gameObject);
			yield break;
		}

		void OnDestroy() => usedGps = false;

		[SerializeField]
		internal SoundObject aud_beep;

		[SerializeField]
		internal float timerSeconds = 30f;

		static bool usedGps = false;
	}
}
