namespace Luminance.Helpers
{
    public class AppSettings
    {
        private static readonly AppSettings _instance = new();
        public static AppSettings Instance => _instance;

        private readonly Dictionary<string, string> _settings = new();

        private AppSettings() { }

        public void Set(string key, string value) => _settings[key] = value;

        public string Get(string key) => _settings.TryGetValue(key, out var val) ? val : string.Empty;

        public T Get<T>(string key, T defaultValue = default!)
        {
            if (_settings.TryGetValue(key, out var strVal))
            {
                try { return (T)Convert.ChangeType(strVal, typeof(T)); }
                catch { }
            }
            return defaultValue;
        }
    }

}
