using BBTimes.CustomComponents.CustomDatas;
using MTM101BaldAPI.ObjectCreation;

namespace BBTimes.Helpers
{
    public static partial class CreatorExtensions
	{

		public static RandomEvent SetupEvent<C>(this RandomEvent ev) where C : CustomEventData
		{
			var cus = ev.gameObject.AddComponent<C>();
			cus.Name = ev.name;
			cus.GetAudioClips();
			cus.GetSprites();
			cus.SetupPrefab();
			

			return ev;
		}

		public static RandomEventBuilder<T> AddRequiredCharacters<T>(this RandomEventBuilder<T> r, params Character[] chars) where T : RandomEvent
		{
			for (int i = 0; i < chars.Length; i++)
				r.AddRequiredCharacter(chars[i]);
			return r;
		}


	}
}
