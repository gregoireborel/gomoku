using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gomoku
{
    class Player
    {
        private int _id;
        public int  _tokens;

        public Player()
        {
            _id = 0;
            _tokens = 0;
        }

        public int getTokens()
        {
            return (this._tokens);
        }

        public void setTokens(int nbr)
        {
            this._tokens = nbr;
        }

        public int getId()
        {
            return (this._id);
        }

        public void setId(int nbr)
        {
            this._id = nbr;
        }
    }
}
