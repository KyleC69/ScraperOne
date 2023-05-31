


using System.Threading.Tasks.Dataflow;

namespace ScraperOne.DataModels
{
    public class PostQueue<T> : IPostQueue<T>
    {
        private readonly BufferBlock<T> i_postQueue;


        public PostQueue()
        {
            i_postQueue = new BufferBlock<T>();
        }


        public void Add(T post)
        {
            _ = i_postQueue.Post(post);
        }

        public int Count
        {
            get
            {
                return i_postQueue.Count;
            }
        }
        
        

        public async Task<T> ReceiveAsync()
        {
            return await i_postQueue.ReceiveAsync();
        }


        public Task CompleteAdding()
        {
            i_postQueue.Complete();
            return i_postQueue.Completion;
        }


        public async Task<bool> OutputAvailableAsync(CancellationToken cancellationToken)
        {
            return await i_postQueue.OutputAvailableAsync(cancellationToken);
        }
    }
}