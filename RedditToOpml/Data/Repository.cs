using System.Text.Json;

namespace RedditToOpml.Data;

public class Repository
{
    private bool _isInitialized = false;
    private List<Subscription> _subscriptions = [];
    public List<Subscription> Subscriptions => _isInitialized ? _subscriptions : Load();
    
    private List<Subscription> Load()
    {
        var jsonFile = new FileInfo("Store/store.json");
        if (jsonFile.Exists)
        {
            var json =  File.ReadAllText(jsonFile.FullName);
            _subscriptions = JsonSerializer.Deserialize<List<Subscription>>(json);
        }
        else
        {
            _subscriptions = [];
            Save();
        }

        _isInitialized = true;
        return _subscriptions;
    }

    public void Save()
    {
        var jsonFile = new FileInfo("Store/store.json");
        var json = JsonSerializer.Serialize(_subscriptions, new JsonSerializerOptions(){WriteIndented = true});
        File.WriteAllText(jsonFile.FullName, json);
    }
    
}