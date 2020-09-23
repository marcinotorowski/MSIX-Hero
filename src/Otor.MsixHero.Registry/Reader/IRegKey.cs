using System;
using System.Collections.Generic;

namespace Otor.MsixHero.Registry.Reader
{
    public interface IRegKey : IDisposable
    {
        string Name { get; }

        string Path { get; }

        IRegKey Parent { get; }

        IEnumerable<IRegKey> Keys { get; }

        IEnumerable<IRegValue> Values { get; }
        
        bool RemoveSubKey(string name);

        bool RemoveValue(string name);

        IRegKey GetSubKey(string name);
        
        IRegKey AddSubKey(string name);

        IRegValue SetValue(string name, byte[] binaryValue);

        IRegValue SetValue(string name, int dwordValue);

        IRegValue SetValue(string name, long qwordValue);

        IRegValue SetValue(string name, string stringValue, bool expanded = false);

        IRegValue SetValue(string name, string[] multiStringValue);

    }
}
