using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ScraperOne;

public class UiStatusMessages : ObservableCollection<StatusMessage>
{
    private static UiStatusMessages s_instance;

    public UiStatusMessages()
    {
        Add(new StatusMessage("test1 message"));
        Add(new StatusMessage("test2 message"));
        Add(new StatusMessage("test3 message"));
        s_instance = this;
    }

    public static void AddMsg(StatusMessage msg)
    {
        s_instance.Add(msg);
    }
}

public class StatusMessage : INotifyPropertyChanged
{
    private string i_message;

    public StatusMessage(string msg)
    {
        i_message = msg;
    }

    public string Message
    {
        get => i_message;
        set => SetField(ref i_message, value);
    }


    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}