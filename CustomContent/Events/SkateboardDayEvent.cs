using BBTimes.CustomComponents;
using BBTimes.CustomComponents.EventSpecificComponents;
using BBTimes.Extensions;
using MTM101BaldAPI.Registers;
using PixelInternalAPI.Extensions;
using MTM101BaldAPI;
using System.Collections.Generic;
using UnityEngine;


namespace BBTimes.CustomContent.Events
{
    public class SkateboardDayEvent : RandomEvent, IObjectPrefab
	{
		public void SetupPrefab()
		{
			eventIntro = this.GetSound("SkateboardDay.wav", "Event_SkateboardDay1", SoundType.Effect, Color.green);
			eventIntro.additionalKeys = [new() { time = 3.156f, key = "Event_SkateboardDay2" }];

			var skRender = ObjectCreationExtensions.CreateSpriteBillboard(null).AddSpriteHolder(out var skRenderer, 0f);
			skRender.gameObject.ConvertToPrefab(true);
			skRenderer.CreateAnimatedSpriteRotator(new SpriteRotationMap() { angleCount = 8, spriteSheet = this.GetSpriteSheet(4, 2, 15f, "skate.png") });

			var sk = skRender.gameObject.AddComponent<Skateboard>();
			sk.entity = sk.gameObject.CreateEntity(3.5f, 1f, skRender.transform);
			((CapsuleCollider)sk.entity.collider).height = 10f; // Should stop it from going below anything
			sk.name = "Skateboard";
			sk.audMan = sk.gameObject.CreatePropagatedAudioManager(45f, 65f);
			sk.audRoll = this.GetSound("skateNoises.wav", "Vfx_Ska_Noise", SoundType.Voice, Color.white);

			skatePre = sk;

		}
		public void SetupPrefabPost() { }
		public string Name { get; set; } public string TexturePath => this.GenerateDataPath("events", "Textures");
		public string SoundPath => this.GenerateDataPath("events", "Audios");
		// ---------------------------------------------------
		public override void Begin()
		{
			base.Begin();
			for (int i = 0; i < ec.Npcs.Count; i++)
			{
				if (ec.Npcs[i].Navigator.isActiveAndEnabled && ec.Npcs[i].GetMeta().flags.HasFlag(NPCFlags.Standard))
				{
					var s = Instantiate(skatePre);
					boards.Add(s);

					s.OverrideNavigator(ec.Npcs[i].Navigator);
					s.Initialize(ec.Npcs[i].Navigator.Entity, ec);
				}
			}

			for (int i = 0; i < ec.Players.Length; i++)
			{
				if (ec.Players[i] != null)
				{
					var s = Instantiate(skatePre);
					boards.Add(s);

					s.Initialize(ec.Players[i].plm.Entity, ec);
				}
			}
		}

		public override void End()
		{
			base.End();
			boards.ForEach(x =>
			{
				StartCoroutine(GameExtensions.TimerToDestroy(x.gameObject, ec, Random.Range(2f, 6f)));

				Destroy(x);
			});
			boards.Clear();
		}

		readonly List<Skateboard> boards = [];

		[SerializeField]
		internal Skateboard skatePre;
	}
}
