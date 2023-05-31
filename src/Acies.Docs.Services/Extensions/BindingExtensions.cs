namespace Acies.Docs.Services.Extensions
{
    public static class BindingExtensions
    {
        public static string BindObjectProperties(this string text, IDictionary<string, object>? data)
        {
            if (data == null) return text;
            foreach (var item in ExtractParams(text))
            {
                text = text.Replace("{" + item + "}", GetPropValue(data, item)?.ToString());
            }
            return text;
        }

        private static object? GetPropValue(IDictionary<string, object>? data, string name)
        {
            object? obj = data;
            foreach (string part in name.Split('.'))
            {
                if (obj == null) { return null; }
                if (int.TryParse(part, out var i))
                {
                    obj = ((IEnumerable<object>)obj).AsEnumerable().ElementAtOrDefault(i);
                }
                else
                {
                    data = obj as IDictionary<string, object>;
                    if (data != null && data.ContainsKey(part))
                    {
                        obj = data[part];
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            return obj;
        }

        private static IEnumerable<string> ExtractParams(string str)
        {
            var splitted = str.Split('{', '}');
            for (int i = 1; i < splitted.Length; i += 2)
                yield return splitted[i];
        }
    }
}