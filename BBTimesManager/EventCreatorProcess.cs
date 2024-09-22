using BBTimes.CustomContent.Events;
using BBTimes.Helpers;
using BepInEx;
using MTM101BaldAPI.ObjectCreation;

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
				.AddRequiredCharacters([Character.Principal, ..GetReplacementNPCs(Character.Principal)])
				.SetName("Principalout")
				.Build()
				.SetupEvent();


			floorDatas[0].Events.Add(new() { selection = e, weight = 45 });
			floorDatas[1].Events.Add(new() { selection = e, weight = 55 });
			floorDatas[3].Events.Add(new() { selection = e, weight = 50 });

			// BlackOut
			e = new RandomEventBuilder<BlackOut>(plug.Info)
				.SetEnum("Blackout")
				.SetMinMaxTime(55f, 90f)
				.SetName("Blackout")
				.Build()
				.SetupEvent();

			floorDatas[2].Events.Add(new() { selection = e, weight = 45 });

			// Freezing Event
			e = new RandomEventBuilder<FrozenEvent>(plug.Info)
				.SetEnum("Frozenschool")
				.SetMinMaxTime(85f, 110f)
				.SetName("FrozenEvent")
				.Build()
				.SetupEvent();


			
			floorDatas[1].Events.Add(new() { selection = e, weight = 75 });
			floorDatas[2].Events.Add(new() { selection = e, weight = 25 });
			floorDatas[3].Events.Add(new() { selection = e, weight = 35 });


			// Curtains Closed
			e = new RandomEventBuilder<CurtainsClosedEvent>(plug.Info)
				.SetEnum("Curtainsclosed")
				.SetMinMaxTime(60f, 80f)
				.SetName("CurtainsClosed")
				.Build()
				.SetupEvent();


			floorDatas[1].Events.Add(new() { selection = e, weight = 55 });
			floorDatas[2].Events.Add(new() { selection = e, weight = 45 });
			floorDatas[3].Events.Add(new() { selection = e, weight = 77 });

			// Hologram Past
			e = new RandomEventBuilder<HologramPastEvent>(plug.Info)
				.SetEnum("Hologrampast")
				.SetMinMaxTime(165f, 200f)
				.SetName("HologramPast")
				.Build()
				.SetupEvent();


			floorDatas[1].Events.Add(new() { selection = e, weight = 55 });
			floorDatas[2].Events.Add(new() { selection = e, weight = 45 });
			floorDatas[3].Events.Add(new() { selection = e, weight = 77 });

			// Skateboard Day
			e = new RandomEventBuilder<SkateboardDayEvent>(plug.Info)
				.SetEnum("Skateboardday")
				.SetMinMaxTime(95f, 120f)
				.SetName("SkateboardDay")
				.Build()
				.SetupEvent();


			floorDatas[1].Events.Add(new() { selection = e, weight = 75 });
			floorDatas[2].Events.Add(new() { selection = e, weight = 75 });
			floorDatas[3].Events.Add(new() { selection = e, weight = 45 });

			// Earthquake
			e = new RandomEventBuilder<Earthquake>(plug.Info)
				.SetEnum("Earthquake")
				.SetMinMaxTime(55f, 85f)
				.SetName("Earthquake")
				.Build()
				.SetupEvent();


			floorDatas[1].Events.Add(new() { selection = e, weight = 25 });
			floorDatas[2].Events.Add(new() { selection = e, weight = 45 });
			floorDatas[3].Events.Add(new() { selection = e, weight = 35 });

			// Shuffling Chaos
			e = new RandomEventBuilder<ShufflingChaos>(plug.Info)
				.SetEnum("Shufflingchaos")
				.SetMinMaxTime(120f, 155f)
				.SetName("ShufflingChaos")
				.Build()
				.SetupEvent();


			floorDatas[1].Events.Add(new() { selection = e, weight = 10 });
			floorDatas[2].Events.Add(new() { selection = e, weight = 35 });
			floorDatas[3].Events.Add(new() { selection = e, weight = 55 }); //55

			// Super Fans
			e = new RandomEventBuilder<SuperFans>(plug.Info)
				.SetEnum("Superfans")
				.SetMinMaxTime(75f, 100f)
				.SetName("SuperFans")
				.Build()
				.SetupEvent();


			floorDatas[1].Events.Add(new() { selection = e, weight = 60 });
			floorDatas[2].Events.Add(new() { selection = e, weight = 25 });
			floorDatas[3].Events.Add(new() { selection = e, weight = 45 });

			// Thunderstorm
			e = new RandomEventBuilder<LightningEvent>(plug.Info)
				.SetEnum("LightningEvent")
				.SetMinMaxTime(75f, 100f)
				.SetName("LightningEvent")
				.Build()
				.SetupEvent();


			floorDatas[1].Events.Add(new() { selection = e, weight = 50 });
			floorDatas[2].Events.Add(new() { selection = e, weight = 45 });
			floorDatas[3].Events.Add(new() { selection = e, weight = 15 });

			//Super Mystery Room
			e = new RandomEventBuilder<SuperMysteryRoom>(plug.Info)
				.SetEnum("SuperMysteryRoom")
				.SetMinMaxTime(60f, 120f)
				.SetName("SuperMysteryRoom")
				.Build()
				.SetupEvent();


			floorDatas[1].Events.Add(new() { selection = e, weight = 35 });
			floorDatas[2].Events.Add(new() { selection = e, weight = 40 });
			floorDatas[3].Events.Add(new() { selection = e, weight = 15 });
		}

		

		
	}
}
