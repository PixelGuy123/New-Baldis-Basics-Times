using BBTimes.CustomContent.NPCs;
using HarmonyLib;
using MTM101BaldAPI.Registers;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class FakerCustomData : CustomNPCData
	{
		protected override Sprite[] GenerateSpriteOrder()
		{
			Sprite[] sprs = new Sprite[3];
			for (int i = 0; i < sprs.Length; i++)
				sprs[i] = GetSprite(31f, $"Faker{i + 1}.png");
			return sprs;
		}
		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var faker = (Faker)Npc;
			faker.forms = [.. storedSprites];
			faker.audMan = GetComponent<PropagatedAudioManager>();
			faker.renderer = faker.spriteRenderer[0];
		}
		protected override void SetupPrefabPost()
		{
			base.SetupPrefabPost();
			var faker = (Faker)Npc;
			List<SoundObject> sds = [];
			foreach (var npc in NPCMetaStorage.Instance.All()) // get literally every sound from npcs registered in the meta storage
			{
				foreach (var pre in npc.prefabs)
				{
					if (pre.Value is Faker)
						continue;
					

					foreach (var field in AccessTools.GetDeclaredFields(pre.Value.GetType()))
					{
						if (field.FieldType == typeof(SoundObject))
						{
							var obj = (SoundObject)field.GetValue(pre.Value);
							if (obj != null && !sds.Contains(obj))
								sds.Add(obj);
						}
						else if (field.FieldType == typeof(SoundObject[]))
						{
							var obj = (SoundObject[])field.GetValue(pre.Value);
							if (obj != null)
							{
								for (int i = 0; i < obj.Length; i++)
									if (obj[i] != null && !sds.Contains(obj[i]))
										sds.Add(obj[i]);
							}
						}
					}
				}

			}
			faker.soundsToEmit = [.. sds];
		}
	}
}
