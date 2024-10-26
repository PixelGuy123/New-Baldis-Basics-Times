using MTM101BaldAPI.Components;
using PixelInternalAPI.Extensions;
using System.Collections;
using UnityEngine;

namespace BBTimes.CustomComponents.EventSpecificComponents.NatureEventFlowers
{
	public class SunFlower : Plant
	{
		protected override void TriggerEnterNPC(NPC npc)
		{
			base.TriggerEnterNPC(npc);
			audMan.PlaySingle(audTouch);
			Despawn(true, false);
			StartCoroutine(BlindNPC(npc));
		}

		protected override void TriggerEnterPlayer(PlayerManager pm)
		{
			base.TriggerEnterPlayer(pm);
			audMan.PlaySingle(audTouch);
			Despawn(true, false);
			StartCoroutine(BlindPlayer(pm));
		}

		IEnumerator BlindNPC(NPC npc)
		{
			ValueModifier valMod = new(0f, 0f);
			var cont = npc.GetNPCContainer();
			cont.AddLookerMod(valMod);
			float time = Random.Range(minBlindDelay, maxBlindDelay);
			while (time > 0f)
			{
				time -= ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}
			cont?.RemoveLookerMod(valMod);
			Destroy(gameObject);
		}

		IEnumerator BlindPlayer(PlayerManager pm)
		{
			blindCanvas.gameObject.SetActive(true);
			blindCanvas.worldCamera = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).canvasCam;
			float time = Random.Range(minBlindDelay, maxBlindDelay);
			while (time > 0f)
			{
				time -= ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}
			Destroy(gameObject);
		}

		[SerializeField]
		internal Canvas blindCanvas;

		[SerializeField]
		internal VisualAttacher attPre;

		[SerializeField]
		internal float minBlindDelay = 10f, maxBlindDelay = 15f;

		[SerializeField]
		internal SoundObject audTouch;
	}
}
