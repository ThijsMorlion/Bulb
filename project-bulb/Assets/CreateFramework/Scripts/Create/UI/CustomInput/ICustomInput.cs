public interface ICustomInput<T>
{
    T Value { get; set; }
    bool IsSelected { get; }
}