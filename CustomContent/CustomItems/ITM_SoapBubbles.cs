using BBTimes.CustomComponents.NpcSpecificComponents;
using UnityEngine;
using System.Collections;
using BBTimes.CustomComponents;
using BBTimes.CustomContent.NPCs;
using BBTimes.Extensions;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_SoapBubbles : Item, IItemPrefab
	{
		public void SetupPrefab()
		{
			bubPre = Resources.FindObjectsOfTypeAll<Bubble>()[0];
			audFill = Resources.FindObjectsOfTypeAll<Bubbly>()[0].audFillUp;
			audFill = Instantiate(audFill);
			audFill.color = Color.white;
		}
		public void SetupPrefabPost() { }

		public string Name { get; set; } public string TexturePath => this.GenerateDataPath("items", "Textures");
		public string SoundPath => this.GenerateDataPath("items", "Audios");
		public ItemObject ItmObj { get; set; }
		public override bool Use(PlayerManager pm)
		{
			this.pm = pm;
			SpitBubbleAtDirection(Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward);

			if (itmObjToReplace != null)
			{
				pm.itm.SetItem(itmObjToReplace, pm.itm.selectedItem);
				return false;
			}
			return true;
		}

		internal void SpitBubbleAtDirection(Vector3 dir)
		{
			var b = Instantiate(bubPre);
			b.Spawn(pm.ec, pm.plm.Entity, pm.transform.position, dir, 30f);
			b.entity.SetHeight(5);
			StartCoroutine(FillupBubble(b));
		}

		IEnumerator FillupBubble(Bubble b)
		{
			Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audFill);
			float scale = 0f;
			b.entity.SetFrozen(true);
			MovementModifier moveMod = new(Vector3.zero, 0.6f);
			pm.Am.moveMods.Add(moveMod);


			float speed = Random.Range(2.6f, 3.5f);
			while (true)
			{
				scale += (1.03f - scale) * speed * pm.ec.EnvironmentTimeScale * Time.deltaTime;
				if (scale >= 1f)
					break;
				b.renderer.transform.localScale = Vector3.one * scale;
				b.entity.Teleport(pm.transform.position);
				yield return null;
			}
			b.renderer.transform.localScale = Vector3.one;
			b.entity.SetFrozen(false);

			b.Initialize();

			pm.Am.moveMods.Remove(moveMod);
			Destroy(gameObject);

			yield break;
		}

		[SerializeField]
		internal Bubble bubPre;

		[SerializeField]
		internal SoundObject audFill;

		[SerializeField]
		internal ItemObject itmObjToReplace;
	}
}
