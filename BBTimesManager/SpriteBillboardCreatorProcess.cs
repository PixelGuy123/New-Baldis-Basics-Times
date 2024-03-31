using BBTimes.CustomContent.Misc;
using BBTimes.ModPatches;
using BBTimes.ModPatches.EnvironmentPatches;
using MTM101BaldAPI.AssetTools;
using BBTimes.Extensions.ObjectCreationExtensions;
using System.IO;

namespace BBTimes.Manager
{
	internal static partial class BBTimesManager
	{
		static void CreateSpriteBillboards() // Solo sprite bill boards btw, that isn't added to other categorized stuff like Room Function (that would be in RoomFunctionCreatorProcess)
		{
			// Decorations outside
			EnvironmentControllerMakeBeautifulOutside.decorations = [ObjectCreationExtension.CreateSpriteBillboard(AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "flower.png")), 15f), 2.6f)];
			// Fire Object
			SchoolFire.anim = new([AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "SchoolFire.png")), 15f), AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(Path.Combine(MiscPath, TextureFolder, "SchoolFire2.png")), 15f)], 2f);
			var fire = ObjectCreationExtension.CreateSpriteAnimator<SchoolFire>("SchoolFire");
			fire.gameObject.SetActive(false);
			fire.spriteRenderer.material.SetTexture("_LightMap", null); // Don't get affected by reddish from schoolhouse
			MainGameManagerPatches.fire = fire.gameObject;
		}	
	}
}
