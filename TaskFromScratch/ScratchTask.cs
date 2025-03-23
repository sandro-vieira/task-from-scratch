using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

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

    public static ScratchTask Delay(TimeSpan delay)
    {
        ScratchTask task = new();

        new Timer(_ => task.SetResult()).Change(delay, Timeout.InfiniteTimeSpan);

        return task;
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

    public void Wait()
    {
        ManualResetEventSlim manualResetEventSlim = null;

        lock (_lock)
        {
            if (!_completed)
            {
                manualResetEventSlim = new ManualResetEventSlim();
                ContinueWith(() => manualResetEventSlim.Set());

            }
        }

        manualResetEventSlim?.Wait();

        if (_exception is not null)
        {
            ExceptionDispatchInfo.Throw(_exception);
        }
    }

    public ScratchTaskAwaiter GetAwaiter() => new(this);

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

public readonly struct ScratchTaskAwaiter : INotifyCompletion
{
    private readonly ScratchTask _task;
    internal ScratchTaskAwaiter(ScratchTask task) => _task = task;

    public bool IsCompleted => _task.IsCompleted;

    public void OnCompleted(Action continuation) => _task.ContinueWith(continuation);

    public ScratchTaskAwaiter GetAwait() => this;

    public void GetResult() => _task.Wait();
}
