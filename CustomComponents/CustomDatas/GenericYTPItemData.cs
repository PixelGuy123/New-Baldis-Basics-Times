namespace BBTimes.CustomComponents.CustomDatas
{
	public class GenericYTPItemData : CustomItemData
	{
		protected override SoundObject[] GenerateSoundObjects() =>
			[GetSoundNoSub("audYtpPickup.wav", SoundType.Effect)];

		public override void SetupPrefab()
		{
			base.SetupPrefab();
			myItmObj.audPickupOverride = soundObjects[0];
		}
	}
}
