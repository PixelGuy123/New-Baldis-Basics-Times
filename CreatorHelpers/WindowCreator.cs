using BBTimes.CustomComponents;
using MTM101BaldAPI;
using UnityEngine;
using static UnityEngine.Object;

namespace BBTimes.CreatorHelpers
{
	public static partial class CreatorExtensions
	{
		public static WindowObject CreateWindow(string name, Texture2D tex, Texture2D brokenTex, Texture2D mask = null, bool unbreakable = false)
		{
			var window = ObjectCreators.CreateWindowObject(name, tex, brokenTex, mask);
			window.windowPre = Instantiate(window.windowPre);
			window.windowPre.gameObject.SetActive(false);
			window.windowPre.name = name;
			var w = window.windowPre.gameObject.AddComponent<CustomWindowComponent>();
			w.unbreakable = unbreakable;
			DontDestroyOnLoad(window.windowPre);

			Destroy(window.windowPre.audMan.audioDevice); // I know you're existing

			return window;
		}
	}
}
