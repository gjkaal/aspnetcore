using Microsoft.JSInterop;

namespace Company.RazorClassLibrary1;

// This class provides an example of how JavaScript functionality can be wrapped
// in a .NET class for easy consumption. The associated JavaScript module is
// loaded on demand when first needed.
//
// This class can be registered as scoped DI service and then injected into Blazor
// components for use.

public class ExampleJsInterop : IAsyncDisposable
{
    public CancellationToken Cancellation { get; }
    private readonly AsyncLazy<IJSObjectReference> moduleTask;

    public ExampleJsInterop(IJSRuntime jsRuntime)
    {
        moduleTask = new AsyncLazy<IJSObjectReference>(
            () => jsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/Flightplan.RazorLibrary/exampleJsInterop.js").AsTask(),
            Cancellation);
    }

    public async ValueTask<string> PromptAsync(string message)
    {
        var module = await moduleTask.Value;
        return await module.InvokeAsync<string>("showPrompt", message);
    }

    public async ValueTask DisposeAsync()
    {
        if (moduleTask.IsValueCreated)
        {
            var module = await moduleTask.Value;
            await module.DisposeAsync();
        }
        GC.SuppressFinalize(this);
    }
}

public class AsyncLazy<T> : Lazy<Task<T>>
{
    public AsyncLazy(Func<T> valueFactory, CancellationToken cancellation) :
        base(() => Task.Factory.StartNew(valueFactory, cancellation, TaskCreationOptions.PreferFairness, TaskScheduler.Current))
    { }

    public AsyncLazy(Func<Task<T>> taskFactory, CancellationToken cancellation) :
        base(() => Task.Factory.StartNew(() => taskFactory(), cancellation, TaskCreationOptions.PreferFairness, TaskScheduler.Current).Unwrap())
    { }
}
