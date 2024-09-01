using BBTimes.CustomComponents;
using BBTimes.Extensions;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using PixelInternalAPI.Extensions;
using BBTimes.Manager;
using MTM101BaldAPI.Components;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_SuperCamera : Item, IItemPrefab
	{
		public void SetupPrefab()
		{
			canvas = ObjectCreationExtensions.CreateCanvas();
			canvas.transform.SetParent(transform);
			canvas.transform.localPosition = Vector3.zero; // I don't know if I really need this but whatever
			canvas.name = "SuperCamOverlay";

			image = ObjectCreationExtensions.CreateImage(canvas, BBTimesManager.man.Get<Sprite>("whiteScreen"));
			audShoot = this.GetSoundNoSub("photo.wav", SoundType.Effect);
		}

		public void SetupPrefabPost() { }

		public string Name { get; set; }
		public string TexturePath => this.GenerateDataPath("items", "Textures");
		public string SoundPath => this.GenerateDataPath("items", "Audios");
		public ItemObject ItmObj { get; set; }


		public override bool Use(PlayerManager pm)
		{
			Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audShoot);
			canvas.worldCamera = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).canvasCam;
			this.pm = pm;
			ec = pm.ec;
			StartCoroutine(Photo());

			for (int i = 0; i < ec.Npcs.Count; i++)
				if (ec.Npcs[i].looker.IsVisible && ec.Npcs[i].looker.PlayerInSight(pm))
					StartCoroutine(StunNPC(ec.Npcs[i]));

			return true;
		}

		IEnumerator Photo()
		{
			Color color = image.color;
			if (Singleton<PlayerFileManager>.Instance.reduceFlashing)
			{
				color.a = 0f;
				image.color = color;
				while (true)
				{
					color.a += ec.EnvironmentTimeScale * Time.deltaTime * 3f;
					if (color.a >= 1f)
						break;
					
					image.color = color;
					yield return null;
				}
				color.a = 1f;
				image.color = color;
			}

			float delay = 1f;
			while (delay > 0f)
			{
				delay -= Time.deltaTime * ec.EnvironmentTimeScale;
				yield return null;
			}

			while (true)
			{
				color.a -= ec.EnvironmentTimeScale * Time.deltaTime * 3f;
				if (color.a <= 0f)
				{
					canvas.gameObject.SetActive(false);
					if (stuns == 0)
						Destroy(gameObject);
					yield break;
				}
				image.color = color;
				yield return null;
			}
		}

		IEnumerator StunNPC(NPC npc)
		{
			stuns++;
			MovementModifier moveMod = new(Vector3.zero, 0f);
			npc.Navigator.Entity.IgnoreEntity(pm.plm.Entity, true);
			npc.Navigator.Am.moveMods.Add(moveMod);
			ValueModifier lookerMod = new(0f);
			var cont = npc.GetNPCContainer();
			cont.AddLookerMod(lookerMod);
			float cool = 10f;
			while (cool > 0f)
			{
				cool -= Time.deltaTime * ec.EnvironmentTimeScale;
				yield return null;
			}
			if (npc)
			{
				npc.Navigator.Entity.IgnoreEntity(pm.plm.Entity, false);
				npc.Navigator.Am.moveMods.Remove(moveMod);
				cont.RemoveLookerMod(lookerMod);
			}
			if (--stuns <= 0)
				Destroy(gameObject);
			yield break;
		}

		int stuns = 0;
		EnvironmentController ec;

		[SerializeField]
		internal Canvas canvas;

		[SerializeField]
		internal Image image;

		[SerializeField]
		internal SoundObject audShoot;
	}
}
