namespace BBTimes.Manager.SelectionHolders
{
	public class SelectionHolder<T, C>(T sel, int weight, C[] selectionLimiter)
	{
		readonly WeightedSelection<T> selection = new() { selection = sel, weight = weight };
		public WeightedSelection<T> Selection => selection;

		readonly C[] selectionLimiters = selectionLimiter;
		public C[] SelectionLimiters => selectionLimiters;
	}
}
