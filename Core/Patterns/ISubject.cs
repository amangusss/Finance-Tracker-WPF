namespace Finance_Tracker_WPF_API.Core.Patterns;

public interface ISubject<T>
{
    void Attach(IObserver<T> observer);
    void Detach(IObserver<T> observer);
    void Notify();
} 