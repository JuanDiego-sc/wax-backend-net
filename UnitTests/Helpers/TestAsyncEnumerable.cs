using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace UnitTests.Helpers;

public class TestAsyncEnumerable<T>(Expression expression) : EnumerableQuery<T>(expression), IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable) : this(enumerable.AsQueryable().Expression) { }

    public static TestAsyncEnumerable<T> Create(IEnumerable<T> items)
    {
        return new TestAsyncEnumerable<T>(items.AsQueryable().Expression);
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}

public class TestAsyncEnumerator<T>(IEnumerator<T> inner) : IAsyncEnumerator<T>
{
    public T Current => inner.Current;

    public ValueTask DisposeAsync()
    {
        inner.Dispose();
        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> MoveNextAsync()
    {
        return new ValueTask<bool>(inner.MoveNext());
    }
}

public class TestAsyncQueryProvider<TEntity>(IQueryProvider inner) : IAsyncQueryProvider
{
    public IQueryable CreateQuery(Expression expression)
    {
        var type = typeof(TestAsyncEnumerable<>).MakeGenericType(expression.Type.GetGenericArguments()[0]);
        return (IQueryable)Activator.CreateInstance(type, [expression])!;
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new TestAsyncEnumerable<TElement>(expression);
    }

    public object? Execute(Expression expression)
    {
        return inner.Execute(expression);
    }

    public TResult Execute<TResult>(Expression expression)
    {
        return inner.Execute<TResult>(expression);
    }

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        var expectedResultType = typeof(TResult).GetGenericArguments()[0];
        
        // Use the original EnumerableQuery provider for the actual execution
        // but we must be careful with the expression tree.
        
        var executionResult = typeof(IQueryProvider)
            .GetMethods()
            .First(m => m.Name == nameof(Execute) && m.IsGenericMethod)
            .MakeGenericMethod(expectedResultType)
            .Invoke(inner, [expression]);

        var task = typeof(Task).GetMethod(nameof(Task.FromResult))!
            .MakeGenericMethod(expectedResultType)
            .Invoke(null, [executionResult])!;

        return (TResult)task;
    }
}
