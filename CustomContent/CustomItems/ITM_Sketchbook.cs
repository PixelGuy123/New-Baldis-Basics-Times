using BBTimes.CustomComponents;
using BBTimes.CustomContent.Misc;
using BBTimes.Extensions;
using UnityEngine;
using MTM101BaldAPI.ObjectCreation;
using BBTimes.Manager;
using PixelInternalAPI.Extensions;
using PixelInternalAPI.Classes;
using System.Collections;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_Sketchbook : Item, IItemPrefab
	{
		public void SetupPrefab()
		{
			audSketch = this.GetSound("sketchBookNoises.wav", "Vfx_SketchBook_Drawing", SoundType.Effect, Color.white);

			potentialSketchVisuals = this.GetSpriteSheet(2, 2, 25f, "sketch.png");
			sketchNpcPre = new NPCBuilder<SketchEntity>(BBTimesManager.plug.Info)
				.AddLooker()
				.SetMaxSightDistance(9999f)
				.AddTrigger()
				.IgnorePlayerOnSpawn()
				.SetEnum(Character.Null)
				.SetForcedSubtitleColor(Color.white)
				.SetName("SketchEntity")
				.SetMinMaxAudioDistance(85f, 115f)
				.Build();
			sketchNpcPre.looker.layerMask = LayerStorage.principalLookerMask;
			sketchNpcPre.spriteRenderer[0].sprite = potentialSketchVisuals[0];
			sketchNpcPre.audMan = sketchNpcPre.GetComponent<AudioManager>();

			sketchNpcPre.audDoingTrouble = this.GetSound("sketchFightNoises.wav", "Vfx_SketchBook_FightNoise", SoundType.Effect, Color.white);

			var renders = ObjectCreationExtensions.CreateSpriteBillboard(null)
				.AddSpriteHolder(out var renderer, 0f);
			renderer.name = "Sprite";
			renderer.transform.localPosition = Vector3.back * 0.5f;
			renders.name = "FightRenderer";
			renders.transform.SetParent(sketchNpcPre.transform);
			renders.transform.localPosition = Vector3.zero;
			renders.gameObject.AddComponent<BillboardUpdater>();

			sketchNpcPre.animComp = renders.gameObject.AddComponent<AnimationComponent>();
			sketchNpcPre.animComp.renderers = [renderer];
			sketchNpcPre.animComp.animation = this.GetSpriteSheet(4, 3, 25f, "fightAnimation.png").ExcludeNumOfSpritesFromSheet(1); // add animation bruh
			sketchNpcPre.animComp.speed = 12f;
			sketchNpcPre.animComp.gameObject.SetActive(false); // Turned off by default
		}
		
		public void SetupPrefabPost() { }

		public string Name { get; set; }
		public string Category => "items";
		public ItemObject ItmObj { get; set; }

		public override bool Use(PlayerManager pm)
		{
			Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audSketch);
			this.pm = pm;
			StartCoroutine(SpawnDelay());
			return true;
		}

		IEnumerator SpawnDelay()
		{
			int max = Random.Range(minSketchEntities, maxSketchEntities + 1);
			for (int i = 0; i < max; i++)
			{
				var sketch = pm.ec.SpawnForeignNPC(sketchNpcPre, pm.transform.position);
				sketch.spriteRenderer[0].sprite = potentialSketchVisuals[Random.Range(0, potentialSketchVisuals.Length)];
				yield return null;
			}
			Destroy(gameObject);
		}

		[SerializeField]
		internal SketchEntity sketchNpcPre;

		[SerializeField]
		internal int minSketchEntities = 2, maxSketchEntities = 4;

		[SerializeField] 
		internal SoundObject audSketch;

		[SerializeField]
		internal Sprite[] potentialSketchVisuals;
	}
}