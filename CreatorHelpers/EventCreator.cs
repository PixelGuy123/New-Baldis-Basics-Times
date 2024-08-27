using BBTimes.CustomComponents;
using MTM101BaldAPI.ObjectCreation;
using BBTimes.Plugin;

namespace BBTimes.Helpers
{
    public static partial class CreatorExtensions
	{

		public static RandomEvent SetupEvent(this RandomEvent ev)
		{
			var data = ev.gameObject.GetComponent<IObjectPrefab>();
			data.Name = ev.name;
			data.SetupPrefab();
			BasePlugin._cstData.Add(data);
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
