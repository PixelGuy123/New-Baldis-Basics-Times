
using BBTimes.CustomComponents;
using BBTimes.CustomComponents.EventSpecificComponents;
using BBTimes.Extensions;
using MTM101BaldAPI;
using PixelInternalAPI.Extensions;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.Events
{
    public class CurtainsClosedEvent : RandomEvent, IObjectPrefab
	{
		public void SetupPrefab()
		{
			
			eventIntro = this.GetSound("baldi_curtains.wav", "Event_CurtClosed0", SoundType.Effect, Color.green);
			eventIntro.additionalKeys = [new() { time = 6.28f, key = "Event_CurtClosed1" }, new() { time = 11.742f, key = "Event_CurtClosed2" }];

			var curt = new GameObject("Curtain").AddComponent<Curtains>();
			curt.gameObject.ConvertToPrefab(true);
			curt.audClose = this.GetSound("curtainClose.wav", "Vfx_Curtain_Slide", SoundType.Voice, Color.white);
			curt.audOpen = this.GetSound("curtainOpen.wav", "Vfx_Curtain_Slide", SoundType.Voice, Color.white);

			var storedSprites = this.GetSpriteSheet(2, 1, 35f, "curtains.png");
			curt.sprClosed = storedSprites[0];
			curt.sprOpen = storedSprites[1];
			curt.audMan = curt.gameObject.CreatePropagatedAudioManager(65, 85);
			curt.collider = curt.gameObject.AddBoxCollider(Vector3.zero, new(5f, 10f, 0.7f), false);
			curt.collider.enabled = false;

			curt.renderers = [CurtFace(0.01f), CurtFace(-0.01f)];

			SpriteRenderer CurtFace(float offset)
			{
				var curtFace = ObjectCreationExtensions.CreateSpriteBillboard(storedSprites[1], false);
				curtFace.transform.SetParent(curt.transform);
				curtFace.transform.localPosition = Vector3.forward * offset + Vector3.up * 5f;
				return curtFace;
			}

			curtPre = curt;
		}
		public void SetupPrefabPost() { }
		public string Name { get; set; } public string TexturePath => this.GenerateDataPath("events", "Textures");
		public string SoundPath => this.GenerateDataPath("events", "Audios");
		// ---------------------------------------------------

		public override void AfterUpdateSetup()
		{
			base.AfterUpdateSetup();
			foreach (var window in FindObjectsOfType<Window>())
			{
				if (window.aTile.Null || window.bTile.Null) // Avoid these!!
					continue;

				var curt = Instantiate(curtPre);
				curt.transform.SetParent(window.aTile.ObjectBase);
				curt.transform.position = window.transform.position + window.direction.ToVector3() * 5f;
				curt.transform.rotation = window.direction.ToRotation();
				curt.AttachToWindow(window);
				curtains.Add(curt);
			}
		}

		public override void Begin()
		{
			base.Begin();
			ec.FreezeNavigationUpdates(true);
			curtains.ForEach(x => x.Close(true));
			ec.FreezeNavigationUpdates(false);
		}

		public override void End()
		{
			base.End();
			curtains.ForEach(x => x.TimedClose(false, Random.Range(1f, 15f)));
		}

		readonly List<Curtains> curtains = [];

		[SerializeField]
		internal Curtains curtPre;
	}
}
