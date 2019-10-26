using System;
using System.Collections.Generic;
using System.Text;

namespace otor.msixhero.lib.BusinessLayer.Models
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
