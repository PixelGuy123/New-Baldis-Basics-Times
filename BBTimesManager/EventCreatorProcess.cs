using BBTimes.CustomContent.Events;
using BBTimes.Helpers;
using BBTimes.Plugin;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Registers;

namespace BBTimes.Manager
{
	internal static partial class BBTimesManager
	{
		static void CreateEvents()
		{
			// 0 - F1
			// 1 - F2
			// 2 - F3
			// 3 - END
			RandomEvent e;

			// Principal out
			e = new RandomEventBuilder<PrincipalOut>(plug.Info)
				.SetEnum("Principalout")
				.SetMinMaxTime(40f, 80f)
				.SetMeta(RandomEventFlags.CharacterSpecific)
				.AddRequiredCharacters([Character.Principal, .. GetReplacementNPCs(Character.Principal)])
				.SetName("Principalout")
				.Build()
				.SetupEvent();


			floorDatas[F1].Events.Add(new(e, 45));
			floorDatas[F2].Events.Add(new(e, 55));
			floorDatas[F3].Events.Add(new(e, 25));
			floorDatas[END].Events.Add(new(e, 50));

			// Freezing Event
			e = new RandomEventBuilder<FrozenEvent>(plug.Info)
				.SetEnum("Frozenschool")
				.SetMinMaxTime(85f, 110f)
				.SetName("FrozenEvent")
				.SetMeta(RandomEventFlags.None, Storage.ChristmasSpecial_TimesTag)
				.Build()
				.SetupEvent();

			floorDatas[F2].Events.Add(new(e, 75));
			floorDatas[F3].Events.Add(new(e, 25));
			floorDatas[END].Events.Add(new(e, 35));


			// Curtains Closed
			e = new RandomEventBuilder<CurtainsClosedEvent>(plug.Info)
				.SetEnum("Curtainsclosed")
				.SetMinMaxTime(60f, 80f)
				.SetName("CurtainsClosed")
				.SetMeta(RandomEventFlags.Permanent | RandomEventFlags.AffectsGenerator)
				.Build()
				.SetupEvent();


			floorDatas[F2].Events.Add(new(e, 55));
			floorDatas[F3].Events.Add(new(e, 45));
			floorDatas[F4].Events.Add(new(e, 32, LevelType.Factory));
			floorDatas[F5].Events.Add(new(e, 15, LevelType.Factory));
			floorDatas[END].Events.Add(new(e, 77));

			// Hologram Past
			e = new RandomEventBuilder<HologramPastEvent>(plug.Info)
				.SetEnum("Hologrampast")
				.SetMinMaxTime(165f, 200f)
				.SetName("HologramPast")
				.Build()
				.SetupEvent();


			floorDatas[F2].Events.Add(new(e, 55));
			floorDatas[F3].Events.Add(new(e, 45));
			floorDatas[F4].Events.Add(new(e, 30, LevelType.Laboratory));
			floorDatas[F5].Events.Add(new(e, 25, LevelType.Laboratory));
			floorDatas[END].Events.Add(new(e, 77));

			// Skateboard Day
			e = new RandomEventBuilder<SkateboardDayEvent>(plug.Info)
				.SetEnum("Skateboardday")
				.SetMinMaxTime(50f, 75f)
				.SetName("SkateboardDay")
				.Build()
				.SetupEvent();


			floorDatas[F2].Events.Add(new(e, 75));
			floorDatas[F3].Events.Add(new(e, 75));
			floorDatas[F4].Events.Add(new(e, 55));
			floorDatas[F5].Events.Add(new(e, 45));
			floorDatas[END].Events.Add(new(e, 45));

			// Earthquake
			e = new RandomEventBuilder<Earthquake>(plug.Info)
				.SetEnum("Earthquake")
				.SetMinMaxTime(55f, 85f)
				.SetName("Earthquake")
				.Build()
				.SetupEvent();


			floorDatas[F2].Events.Add(new(e, 25));
			floorDatas[F3].Events.Add(new(e, 45));
			floorDatas[F4].Events.Add(new(e, 35));
			floorDatas[F5].Events.Add(new(e, 25));
			floorDatas[END].Events.Add(new(e, 35));

			// Super Fans
			e = new RandomEventBuilder<SuperFans>(plug.Info)
				.SetEnum("Superfans")
				.SetMinMaxTime(75f, 100f)
				.SetName("SuperFans")
				.SetMeta(RandomEventFlags.Permanent | RandomEventFlags.AffectsGenerator)
				.Build()
				.SetupEvent();


			floorDatas[F2].Events.Add(new(e, 60));
			floorDatas[F3].Events.Add(new(e, 25));
			floorDatas[F4].Events.Add(new(e, 12, LevelType.Factory));
			floorDatas[F5].Events.Add(new(e, 16, LevelType.Factory));
			floorDatas[END].Events.Add(new(e, 45));

			// Thunderstorm
			e = new RandomEventBuilder<LightningEvent>(plug.Info)
				.SetEnum("LightningEvent")
				.SetMinMaxTime(75f, 100f)
				.SetName("LightningEvent")
				.Build()
				.SetupEvent();


			floorDatas[F2].Events.Add(new(e, 50));
			floorDatas[F3].Events.Add(new(e, 45));
			floorDatas[END].Events.Add(new(e, 15));

			//Super Mystery Room
			e = new RandomEventBuilder<SuperMysteryRoom>(plug.Info)
				.SetEnum("SuperMysteryRoom")
				.SetMinMaxTime(60f, 120f)
				.SetName("SuperMysteryRoom")
				.SetMeta(RandomEventFlags.Permanent | RandomEventFlags.AffectsGenerator | RandomEventFlags.RoomSpecific)
				.Build()
				.SetupEvent();


			floorDatas[F2].Events.Add(new(e, 35));
			floorDatas[END].Events.Add(new(e, 15));

			//Nature Event
			e = new RandomEventBuilder<NatureEvent>(plug.Info)
				.SetEnum("NatureEvent")
				.SetMinMaxTime(60f, 90f)
				.SetName("NatureEvent")
				.SetMeta(RandomEventFlags.Permanent | RandomEventFlags.AffectsGenerator)
				.Build()
				.SetupEvent();


			floorDatas[F2].Events.Add(new(e, 15));
			floorDatas[F3].Events.Add(new(e, 55));
			floorDatas[END].Events.Add(new(e, 25));
		}




	}
}
