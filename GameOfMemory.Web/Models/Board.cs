using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GameOfMemory.Web.Models
{
    public class Board
    {
        private List<Card> _pieces = new List<Card>();
        public List<Card> Pieces
        {
            get { return _pieces; }
            set { _pieces = value; }
        }

        public Board()
        {
            int imgIndex = 1;

            for (var i = 1; i <= 30; i++)
            {

                if(IsOdd(i))
                {
                    _pieces.Add(new Card {
                    Id = i,
                    Pair = i + 1,
                    Name = "card-" + i.ToString(),
                    Image = string.Format("/content/img/{0}.png", imgIndex)
                    });
                }else{
                    _pieces.Add(new Card {
                    Id = i,
                    Pair = i-1,
                    Name = "card-" + i.ToString(),
                    Image = string.Format("/content/img/{0}.png", imgIndex)
                    });
                    imgIndex++;
                }

            }

            _pieces.Shuffle();
        }

        private bool IsOdd(int i)
        {
            return (i % 2 != 0);
        }
    }
}