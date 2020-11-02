using System;

namespace Otor.MsixHero.Appx.Users
{
    [Serializable]
    public class User
    {
        public User(string name)
        {
            Name = name;
        }

        public User()
        {
        }

        public string Name { get; set; }
    }
}
