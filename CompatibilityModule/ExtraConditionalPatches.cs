using BepInEx.Bootstrap;
using MTM101BaldAPI;

namespace BBTimes.CompatibilityModule
{
	internal class ConditionalPatchModByVersion(string guid, string expectedVersion, bool includePrevVersions = false, bool includePostVersions = false, bool invertCondition = false) : ConditionalPatchMod(guid)
	{
		protected string guid = guid;
		protected System.Version version = new(expectedVersion);
		protected bool includePrevVersions = includePrevVersions;
		protected bool includePostVersions = includePostVersions;
		protected bool invertCondition = invertCondition;
		public override bool ShouldPatch()
		{
			bool guidMatch = base.ShouldPatch();
			if (!guidMatch)
				return false;

			var modVersion = Chainloader.PluginInfos[guid].Metadata.Version;
			bool flag = (includePrevVersions && modVersion < version) || (includePostVersions && modVersion > version) || version == modVersion;
			if (invertCondition)
				flag = !flag;
			return flag;
		}
	}
}
