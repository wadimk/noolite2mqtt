using System;

namespace Noolite2Mqtt.Core.Plugins.Utils
{
    public class ObjectRegistry<T> : BaseRegistry<T, T>
    {
        protected override T Add(string key, T value) => value;

        protected override T Update(string key, T data, T value) => throw new Exception($"duplicated key {key} ({value})");
    }
}
