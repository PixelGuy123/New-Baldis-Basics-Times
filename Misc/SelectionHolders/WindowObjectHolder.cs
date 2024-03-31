using PixelInternalAPI.Classes;
namespace BBTimes.Misc.SelectionHolders
{
    public class WindowObjectHolder(WindowObject window, int weight, RoomCategory[] categories) : SelectionHolder<WindowObject, RoomCategory>(window, weight, categories)
    {
    }
}
