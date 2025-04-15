namespace Finance_Tracker_WPF_API.Core.Patterns;

public interface IObserver<T>
{
    void Update(T data);
} 