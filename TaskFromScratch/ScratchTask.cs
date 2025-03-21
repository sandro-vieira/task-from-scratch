namespace TaskFromScratch;

public class ScratchTask
{
    private readonly Lock _lock = new();

    private bool _completed;
    private Exception? _exception;

    public bool IsCompleted
    {
        get
        {
            lock (_lock)
            {
                return _completed;
            }
        }
    }

    public static ScratchTask Run(Action action)
    {
        ScratchTask task = new();

        ThreadPool.QueueUserWorkItem(_ =>
        {
            try
            {
                action();
                task.SetResult();
            }
            catch (Exception e)
            {
                task.SetException(e);
            }
        });

        return task;
    }

    public void SetResult()
    {
        lock (_lock)
        {
            if (_completed)
            {
                throw new InvalidOperationException("ScratchTask already completed. Cannot set result of a completed ScratchTask");
            }

            _completed = true;
        }
    }

    public void SetException(Exception e)
    {
        lock (_lock)
        {
            if (_completed)
            {
                throw new InvalidOperationException("ScratchTask already completed. Cannot set result of a completed ScratchTask");
            }
        }

        _exception = e;
    }
}
