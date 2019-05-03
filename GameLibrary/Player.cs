using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;

namespace GameLibrary
{
    [DataContract]
    public class Player
    {
        [DataMember] public string name { get; set; }
        [DataMember] public int score { get; set; }

        // True if it is currently this players turn
        public bool currentlyPlaying;


        public ICallback callback;

        public Player() { }

        public Player(string name, ICallback callback) { 
        
            this.name = name;
            this.callback = callback;
            score = 0;
        }
    }
}
