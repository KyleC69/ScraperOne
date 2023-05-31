


namespace ScraperOne.DataModels;


public interface IPostQueue<T>
{

    void Add(T post);

    Task<T> ReceiveAsync();

    Task CompleteAdding();

    Task<bool> OutputAvailableAsync(CancellationToken cancellationToken);

int Count {
    get;
}

}
