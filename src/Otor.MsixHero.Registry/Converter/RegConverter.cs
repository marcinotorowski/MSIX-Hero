using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Otor.MsixHero.Registry.Parser;
using Otor.MsixHero.Registry.Reader;
using Otor.MsixHero.Registry.Tokenizer;
using ValueType = Otor.MsixHero.Registry.Parser.ValueType;

namespace Otor.MsixHero.Registry.Converter
{
    public class RegConverter
    {
        private readonly RegistryTokenizer tokenizer = new RegistryTokenizer();

        public async Task ConvertFromRegToDat(string regFile, string file, RegistryRoot? root = null)
        {
            var regParser = new RegFileParser();
            var parsedKeys = regParser.Parse(regFile);

            IEnumerable<RegistryEntry> parsedFilteredKeys;
            if (root == null)
            {
                parsedFilteredKeys = parsedKeys;
            }
            else
            {
                parsedFilteredKeys = parsedKeys.Where(r => r.Root == root.Value);
            }

            var reader = new RawReader();

            using (var hive = await reader.Create())
            {
                var mustSave = false;
                foreach (var item in parsedFilteredKeys)
                {
                    var key = PrepareRegistryKey(item);

                    mustSave = true;

                    var k = this.EnsureRegistryKey(hive.Root, key);

                    if (!string.IsNullOrEmpty(item.Name))
                    {
                        switch (item.Type)
                        {
                            case ValueType.Default:
                                break;
                            case ValueType.String:
                                k.SetValue(item.Name, this.Tokenize((string) item.Value));
                                break;
                            case ValueType.DWord:
                                k.SetValue(item.Name, (int)Convert.ChangeType(item.Value, typeof(int)));
                                break;
                            case ValueType.QWord:
                                k.SetValue(item.Name, (long)Convert.ChangeType(item.Value, typeof(long)));
                                break;
                            case ValueType.Multi:
                                k.SetValue(item.Name, this.Tokenize((string[])item.Value));
                                break;
                            case ValueType.Expandable:
                                k.SetValue(item.Name, this.Tokenize((string)item.Value));
                                break;
                            case ValueType.Binary:
                                k.SetValue(item.Name, (byte[]) item.Value);
                                break;
                            case ValueType.DWordBigEndian:
                                k.SetValue(item.Name, (int)Convert.ChangeType(item.Value, typeof(int)));
                                break;
                        }
                    }
                }

                if (mustSave)
                {
                    await hive.Save(file);
                }
            }
        }

        private string Tokenize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            if (value.TrimStart().StartsWith('"'))
            {
                var findClosing = value.IndexOf('"', value.IndexOf('"') + 1);
                if (findClosing == -1)
                {
                    return value;
                }

                return value.Substring(0, value.IndexOf('"') + 1) + this.tokenizer.Tokenize(value) + value.Substring(findClosing);
            }

            var findSpace = value.IndexOf(' ');
            if (findSpace == -1)
            {
                return this.tokenizer.Tokenize(value);
            }

            return this.tokenizer.Tokenize(value.Substring(0, findSpace)) + value.Remove(0, findSpace);
        }

        private string[] Tokenize(string[] value)
        {
            if (value == null || !value.Any())
            {
                return value;
            }

            return value.Select(Tokenize).ToArray();
        }
        private IRegKey EnsureRegistryKey(IRegKey regKey, string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return regKey;
            }

            var pos = path.IndexOf('\\');

            if (pos == -1)
            {
                var child = regKey.GetSubKey(path);
                if (child != null)
                {
                    return child;
                }

                return regKey.AddSubKey(path);
            }
            else
            {
                var child = regKey.GetSubKey(path.Substring(0, pos));
                if (child == null)
                {
                    child = regKey.AddSubKey(path.Substring(0, pos));
                }

                return this.EnsureRegistryKey(child, path.Substring(pos + 1));
            }
        }

        private static string PrepareRegistryKey(RegistryEntry key)
        {
            var root = key.Root;

            switch (root)
            {
                case RegistryRoot.HKEY_CLASSES_ROOT:
                    return "REGISTRY\\MACHINE\\Classes\\" + key.Key;
                case RegistryRoot.HKEY_CURRENT_USER:
                    return "REGISTRY\\USER\\[{CurrentUserSID}]\\" + key.Key;
                case RegistryRoot.HKEY_USERS:
                    return "REGISTRY\\USER\\" + key.Key;
                case RegistryRoot.HKEY_LOCAL_MACHINE:
                    return "REGISTRY\\MACHINE\\" + key.Key;
                default:
                    return "REGISTRY\\MACHINE\\" + key.Key;
            }
        }
    }
}
