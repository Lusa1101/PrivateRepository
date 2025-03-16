using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ObservableDictionary : INotifyPropertyChanged
{
    private Dictionary<string, string?> _fields = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    public string? this[string key]
    {
        get => _fields.ContainsKey(key) ? _fields[key] : null;
        set
        {
            if (_fields.ContainsKey(key) && _fields[key] == value) return;

            _fields[key] = value;
            OnPropertyChanged(key); // Notify UI about the change for the specific key
        }
    }

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
