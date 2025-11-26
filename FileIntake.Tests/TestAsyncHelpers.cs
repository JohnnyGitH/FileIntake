// Helper for mocking IAsyncEnumerator
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace FileIntake.Tests;
internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public T Current => _inner.Current;

    public ValueTask DisposeAsync() => new ValueTask(Task.CompletedTask);

    // This is the key method that simulates the asynchronous move
    public ValueTask<bool> MoveNextAsync()
    {
        return new ValueTask<bool>(_inner.MoveNext());
    }
}

// Helper for mocking IAsyncQueryProvider
internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    internal TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(Expression expression)
    {
        return new TestAsyncEnumerable<TEntity>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new TestAsyncEnumerable<TElement>(expression);
    }

    public object Execute(Expression expression)
    {
        return _inner.Execute(expression);
    }

    public TResult Execute<TResult>(Expression expression)
    {
        return _inner.Execute<TResult>(expression);
    }

    // This is the key method for executing async queries
    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
    {
        var expectedResultType = typeof(TResult).GetGenericArguments()[0];
        var executionResult = _inner.Execute(expression);
        
        // Wrap the result in a Task (e.g., Task<List<T>>)
        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
            .MakeGenericMethod(expectedResultType)
            .Invoke(null, new[] { executionResult })!;
    }
}

// This class links the provider to the data and supports IAsyncEnumerable
internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable)
        : base(enumerable)
    {
        // Initialize with the custom async provider
        Provider = new TestAsyncQueryProvider<T>(this);
    }

    public TestAsyncEnumerable(Expression expression)
        : base(expression)
    {
        // Initialize with the custom async provider
        Provider = new TestAsyncQueryProvider<T>(this);
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        // This is where it calls the final class in the chain
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    public new IQueryProvider Provider { get; }
}