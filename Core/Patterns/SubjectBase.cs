namespace Finance_Tracker_WPF_API.Core.Patterns;

public abstract class SubjectBase<T> : ISubject<T>
{
    private readonly List<IObserver<T>> _observers = new();
    protected T State { get; set; }

    public void Attach(IObserver<T> observer)
    {
        if (!_observers.Contains(observer))
        {
            _observers.Add(observer);
        }
    }

    public void Detach(IObserver<T> observer)
    {
        _observers.Remove(observer);
    }

    public void Notify()
    {
        foreach (var observer in _observers)
        {
            observer.Update(State);
        }
    }

    protected void UpdateState(T newState)
    {
        State = newState;
        Notify();
    }
} 