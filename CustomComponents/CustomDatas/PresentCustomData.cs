using BBTimes.CustomContent.CustomItems;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;
using PixelInternalAPI.Extensions;
using System.Collections.Generic;
using System.IO;

namespace BBTimes.CustomComponents.CustomDatas
{
	public class PresentCustomData : CustomItemData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
			[ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(SoundPath, "prs_unbox.wav")), "Vfx_PRS_Unbox", SoundType.Effect, UnityEngine.Color.white)];

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			var present = GetComponent<ITM_Present>();
			present.aud_unbox = soundObjects[0];
			present.audMan = gameObject.CreateAudioManager(gameObject.CreateAudioSource(45f, 75f), false, [], true);
			present.audMan.MakeAudioManagerNonPositional();
		}

		public override void SetupPrefabPost()
		{
			base.SetupPrefabPost();
			// GetComponent<Present>().items = [.. ItemMetaStorage.Instance.FindAll(x => x.id != myEnum).ConvertAll(x => x.value)];

			// Another workaround for this stupid bug
			List<ItemObject> list = new(ItemMetaStorage.Instance.FindAll(x => x.id != myEnum).ConvertAll(x => x.value));

			HashSet<ItemObject> duplicates = [];
			for (int i = 0; i < list.Count; i++)
			{
				if (duplicates.Contains(list[i]))
				{
					list.RemoveAt(i);
					i--;
					continue;
				}
				duplicates.Add(list[i]);
			}
			GetComponent<ITM_Present>().items = [.. list];
		}
	}
}
