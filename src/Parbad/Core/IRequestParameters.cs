namespace Parbad.Core
{
    internal interface IRequestParameters
    {
        object Get(string key, bool caseSensitive = false);

        void Set(string key, object value);
    }
}