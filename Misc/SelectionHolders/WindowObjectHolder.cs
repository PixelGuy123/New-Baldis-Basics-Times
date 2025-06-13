using PixelInternalAPI.Classes;
namespace BBTimes.Misc.SelectionHolders
{
    [System.Serializable] // Surely this will help
    public class WindowObjectHolder(WindowObject window, int weight, RoomCategory[] categories) : SelectionHolder<WindowObject, RoomCategory>(window, weight, categories)
    {
    }
}
