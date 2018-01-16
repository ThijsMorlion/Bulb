using System.ComponentModel;

namespace Create.Helpers.ImageLoader
{
    public interface ILoader
    {
        event PropertyChangedEventHandler PropertyChanged;
        bool IsLoading { get; }
    }
}