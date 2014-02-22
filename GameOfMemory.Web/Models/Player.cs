using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GameOfMemory.Web.Models
{
    public class Player
    {

        public Player(string name, string hash)
        {
            this.Name = name;
            this.Hash = hash;
            this.Id = Guid.NewGuid().ToString("d");
            this.Matches = new List<int>();
        }
        
        public string ConnectionId { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
        public string Hash { get; set; }
        public string Group { get; set; }
        public bool IsPlaying { get; set; }
        public List<int> Matches { get; set; }
    }
}