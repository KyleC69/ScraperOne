using System.Diagnostics;

using ReactiveUI;



namespace ScraperOne;
using System.Reactive.Concurrency;





public class MyCoolObservableExceptionHandler : IObserver<Exception>
{
    public void OnNext(Exception value)
    {
        if (Debugger.IsAttached) Debugger.Break();
/*
        Logger.Analytics.Current.TrackEvent("MyRxHandler", new Dictionary<string, string>()
                                                {
                                                    {"Type", value.GetType().ToString()},
                                                    {"Message", value.Message},
                                                });

        RxApp.MainThreadScheduler.Schedule(() => { throw value; });
        */
    }

    public void OnError(Exception error)
    {
       if (Debugger.IsAttached) Debugger.Break();
      /*

        ScraperOne.Logger.Analytics.Current.TrackEvent("MyRxHandler Error", new Dictionary<string, string>()
        {
            {"Type", error.GetType().ToString()},
            {"Message", error.Message},
        });

        RxApp.MainThreadScheduler.Schedule(() => { throw error; });*/
    }

    public void OnCompleted()
    {
        if (Debugger.IsAttached) Debugger.Break();
        RxApp.MainThreadScheduler.Schedule(() => { throw new NotImplementedException(); });
    }
}