using UnityEngine;
using static UnityEngine.Object;

namespace BBTimes.Manager
{
	internal static partial class BBTimesManager
	{
		static void CreateSpriteBillboards()
		{
			//var billboard = CreateSpriteBillboard(null, false); <-- placeholder

			GameObject CreateSpriteBillboard(Sprite sprite, bool dontDestroyOnLoad = true)
			{
				var obj = Instantiate(man.Get<GameObject>("SpriteBillboardTemplate"));
				obj.GetComponent<SpriteRenderer>().sprite = sprite;
				if (dontDestroyOnLoad)
					DontDestroyOnLoad(obj);
				return obj;
			}
		}
	}
}
