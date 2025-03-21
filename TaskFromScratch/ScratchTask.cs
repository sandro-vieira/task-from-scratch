namespace TaskFromScratch;

public class ScratchTask
{
    private readonly Lock _lock = new();

    private bool _completed;
    private Exception? _exception;
    private Action? _action;
    private ExecutionContext? _context;

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

    public ScratchTask ContinueWith(Action action)
    {
        ScratchTask task = new();

        lock (_lock)
        {
            if (_completed)
            {
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
            }
            else
            {
                _action = action;
                _context = ExecutionContext.Capture();
            }
        }

        return task;
    }

    public void SetResult() => CompleteTask(null);

    public void SetException(Exception exception) => CompleteTask(exception);

    private void CompleteTask(Exception? exception)
    {
        lock (_lock)
        {
            if (_completed)
            {
                throw new InvalidOperationException("ScratchTask already completed. Cannot set result of a completed ScratchTask");
            }

            _completed = true;
            _exception = exception;

            if (_action is not null)
            {
                if (_context is null)
                {
                    _action.Invoke();
                }
                else
                {
                    ExecutionContext.Run(_context, state => ((Action?)state)?.Invoke(), _action);
                }
            }
        }
    }
}
