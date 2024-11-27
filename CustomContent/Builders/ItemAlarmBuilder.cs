using BBTimes.CustomComponents;
using BBTimes.CustomContent.Objects;
using BBTimes.Extensions;
using MTM101BaldAPI;
using PixelInternalAPI.Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BBTimes.CustomContent.Builders
{
	public class ItemAlarmBuilder : ObjectBuilder, IObjectPrefab
	{

		public void SetupPrefab()
		{
			var alarmObj = ObjectCreationExtensions.CreateSpriteBillboard(this.GetSprite(25f, "itemDetectorAlarm.png"))
				.AddSpriteHolder(out var renderer, 0.7f);
			alarmObj.name = "ItemAlarm";
			renderer.name = "ItemAlarm_Renderer";
			alarmObj.gameObject.ConvertToPrefab(true);
			alarmPre = alarmObj.gameObject.AddComponent<ItemAlarm>();
			alarmPre.audMan = alarmObj.gameObject.CreatePropagatedAudioManager(125f, 185f);
			alarmPre.audAlarm = this.GetSound("itemDetectorAlarmNoise.wav", "Vfx_ItemAlarm_Alarm", SoundType.Voice, Color.white);
			alarmPre.renderer = renderer.transform;
		}
		public void SetupPrefabPost() { }

		public string Name { get; set; }
		public string TexturePath => this.GenerateDataPath("objects", "Textures");
		public string SoundPath => this.GenerateDataPath("objects", "Audios");

		// Prefab stuff above ^^
		public override void Build(EnvironmentController ec, LevelBuilder builder, RoomController room, System.Random cRng)
		{
			base.Build(ec, builder, room, cRng);
			this.cRng = cRng;
			this.ec = ec;
		}

		public void ActuallySpawnAlarms()
		{
			var existingAlarms = FindObjectsOfType<ItemAlarm>();

			var potentialPickups = new List<Pickup>(ec.items);
			potentialPickups.RemoveAll(pic =>
				!pic.free ||
				existingAlarms.Any(al => al.LinkedPickup == pic)); // If any ItemAlarm is found in their children, it means there is an item alarm there already (they attach to the pickup)

			if (potentialPickups.Count == 0)
			{
				Debug.LogWarning("ItemAlarmBuilder failed to find any good spots for alarms");
				return;
			}

			var alarm = Instantiate(alarmPre, transform);
			alarm.AttachToPickup(potentialPickups[cRng.Next(potentialPickups.Count)]);
			alarm.Ec = ec;
		}

		System.Random cRng;
		EnvironmentController ec;

		[SerializeField]
		internal ItemAlarm alarmPre;
	}
}
