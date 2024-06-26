﻿using BBTimes.CustomComponents;
using BBTimes.Extensions;
using MTM101BaldAPI;
using UnityEngine;
using static UnityEngine.Object;

namespace BBTimes.Helpers
{
	public static partial class CreatorExtensions
	{
		public static WindowObject CreateWindow(string name, Texture2D tex, Texture2D brokenTex, Texture2D mask = null, bool unbreakable = false)
		{
			var window = ObjectCreators.CreateWindowObject(name, tex, brokenTex, mask);
			window.windowPre = window.windowPre.SafeInstantiate();
			window.windowPre.gameObject.ConvertToPrefab(true);
			window.windowPre.name = name;

			var w = window.windowPre.gameObject.AddComponent<CustomWindowComponent>();
			w.unbreakable = unbreakable;

			

			if (window.windowPre.audMan.audioDevice)
				Destroy(window.windowPre.audMan.audioDevice); // I know you're existing
			if (AudioManager.totalIds > 0)
				AudioManager.totalIds--;
			window.windowPre.audMan.sourceId = 0;


			return window;
		}
	}
}
