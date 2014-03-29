using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Gomoku
{
    public enum typeOfGame
    {
        JVSJ,
        JVSB
    }

    public enum playerTurn
    {
        J1,
        J2
    }

    public enum gameStatus
    {
        INGAME,
        WIN
    }

    class Referee
    {
        private int[][]     _map;
        public int[]        _lastShotTabX, _lastShotTabY;
        public playerTurn   _turn;
        public gameStatus   _status;
        public int          _winner;
        public int          _count;

        public              Referee()
        {
            _status = gameStatus.INGAME;
            _turn = playerTurn.J1;
            _winner = 0;
            _count = 0;
            _map = new int[19][];
            _lastShotTabX = new int[20]; //18 avant
            _lastShotTabY = new int[20];
            for (int i = 0; i < 19; ++i)
            {
                _map[i] = new int[19];
                for (int y = 0; y < 19; ++y)
                    _map[i][y] = 0;
            }
        }

        public List<Vector2> Update(float mouseX, float mouseY, ref int takeNb, int flag)
        {
            Vector2         coord;
            List<Vector2>   caseNb = new List<Vector2>();

            caseNb.Add(new Vector2(-1, -1));

            /*On recupere les coordonees de la case clique*/
            if (flag == 0)
                coord = convertCoor(mouseX, mouseY);
            else
            {
                coord.X = (int)mouseX;
                coord.Y = (int)mouseY;
            }
            if (coord.X == -1 || coord.Y == -1)
                return caseNb;
            else
                caseNb[0] = coord;
            
            /*On verifie les differentes regles*/
            if (_map[(int)coord.X][(int)coord.Y] != 0 && _map[(int)coord.X][(int)coord.Y] != 3) // si case deja prise / enlever le != 3
                return caseNb;

            if (takeRule(coord, ref caseNb) == false)
                if (checkDoubleFreeThree((int)coord.X, (int)coord.Y) == false) // rule three
                    return caseNb;
                       
            /*Si tout est en regle*/
            caseNb[0] = coord;
            _map[(int) coord.X][(int) coord.Y] = (int)_turn + 1;

            for (int i = 1; i < caseNb.Count(); ++i)
            {
                takeNb++;
                if (takeNb == 10)
                   _status = gameStatus.WIN;
                _map[(int) caseNb[i].X][(int) caseNb[i].Y] = 0;
            }

            if (_status == gameStatus.INGAME)
            {
                if (checkVictory((int)coord.X, (int)coord.Y) == 1)
                    _status = gameStatus.WIN;
            }
            if (_turn == playerTurn.J1)
                _turn = playerTurn.J2;
            else
                _turn = playerTurn.J1;
            return caseNb;
        }

        public void Draw(ref SpriteBatch spriteBatch, ref Texture2D blue, ref Texture2D red, ref SpriteFont font)
        {
            Vector2 vector = new Vector2(0, 0);
            String text = "";

            if (_turn == playerTurn.J1)
                text = "Tour Joueur 1";
            else
                text = "Tour Joueur 2";
            vector.X = 620; vector.Y = 26;
            spriteBatch.DrawString(font, text, vector, Color.Black);
            for (int i = 0; i < 19; ++i)
                for (int y = 0; y < 19; ++y)
                {
                    if (_map[i][y] == 1)
                    {
                        vector.X = 5 + i * 31; vector.Y = 5 + y * 31;
                        spriteBatch.Draw(blue, vector, Color.White);
                    }
                    if (_map[i][y] == 2)
                    {
                        vector.X = 5 + i * 31; vector.Y = 5 + y * 31;
                        spriteBatch.Draw(red, vector, Color.White);
                    }
                }
        }

        public void     clearData()
        {
          _status = gameStatus.INGAME;
          for (int i = 0; i < 19; ++i)
              for (int y = 0; y < 19; ++y)
                  _map[i][y] = 0;
          _turn = playerTurn.J1;
        }

        private bool    takeRule(Vector2 coord, ref List<Vector2> caseNb)
        {
            int         turn = (int)_turn + 1;
            int         notTurn;
            int         x = (int) coord.X;
            int         y = (int) coord.Y;
            bool        result = false;

            if (turn == 1)
                notTurn = 2;
            else
                notTurn = 1;

            /*Cas 1*/
            if ((y - 3) >= 0)
            {
                if (_map[x][y - 3] == turn && _map[x][y - 1] == notTurn && _map[x][y - 2] == notTurn)
                {
                    caseNb.Add(new Vector2(x, y - 1));
                    caseNb.Add(new Vector2(x, y - 2));
                    result = true;
                }
            }

            /*Cas 2*/
            if ((y - 3) >= 0 && (x + 3) < 19)
            {
                if (_map[x + 3][y - 3] == turn && _map[x + 1][y - 1] == notTurn && _map[x + 2][y - 2] == notTurn)
                {
                    caseNb.Add(new Vector2(x + 1, y - 1));
                    caseNb.Add(new Vector2(x + 2, y - 2));
                    result = true;
                }
            }

            /*Cas 3*/
            if ((x + 3) < 19)
            {
                if (_map[x + 3][y] == turn && _map[x + 1][y] == notTurn && _map[x + 2][y] == notTurn)
                {
                    caseNb.Add(new Vector2(x + 1, y));
                    caseNb.Add(new Vector2(x + 2, y));
                    result = true;
                }
            }

            /*Cas 4*/
            if ((y + 3) < 19 && (x + 3) < 19)
            {
                if (_map[x + 3][y + 3] == turn && _map[x + 1][y + 1] == notTurn && _map[x + 2][y + 2] == notTurn)
                {
                    caseNb.Add(new Vector2(x + 1, y + 1));
                    caseNb.Add(new Vector2(x + 2, y + 2));
                    result = true;
                }
            }

            /*Cas 5*/
            if ((y + 3) < 19)
            {
                if (_map[x][y + 3] == turn && _map[x][y + 1] == notTurn && _map[x][y + 2] == notTurn)
                {
                    caseNb.Add(new Vector2(x, y + 1));
                    caseNb.Add(new Vector2(x, y + 2));
                    result = true;
                }
            }

            /*Cas 6*/
            if ((y + 3) < 19 && (x - 3) >= 0)
            {
                if (_map[x - 3][y + 3] == turn && _map[x - 1][y + 1] == notTurn && _map[x - 2][y + 2] == notTurn)
                {
                    caseNb.Add(new Vector2(x - 1, y + 1));
                    caseNb.Add(new Vector2(x - 2, y + 2));
                    result = true;
                }
            }

            /*Cas 7*/
            if ((x - 3) >= 0)
            {
                if (_map[x - 3][y] == turn && _map[x - 1][y] == notTurn && _map[x - 2][y] == notTurn)
                {
                    caseNb.Add(new Vector2(x - 1, y));
                    caseNb.Add(new Vector2(x - 2, y));
                    result = true;
                }
            }

            /*Cas 8*/
            if ((y - 3) >= 0 && (x - 3) >= 0)
            {
                if (_map[x - 3][y - 3] == turn && _map[x - 1][y - 1] == notTurn && _map[x - 2][y - 2] == notTurn)
                {
                    caseNb.Add(new Vector2(x - 1, y - 1));
                    caseNb.Add(new Vector2(x - 2, y - 2));
                    result = true;
                }
            }
            return result;
        }

        private int checkFreeThreeDiagTwo(int posX, int posY, int turn, int noTurn)
        {
            int check = 0;
            int i = 1;

            while (i <= 3 && posX - i >= 0 && posY - i >= 0)
            {
                if (_map[posX - i][posY - i] == noTurn || posX - i == 0 || posY - i == 0 || _map[posX - i - 1][posY - i - 1] == noTurn)
                    return (0);
                else if (_map[posX - i][posY - i] == turn)
                    check++;
                i++;
            }

            i = 1;

            while (i <= 3 && posX + i <= 18 && posY + i <= 18)
            {
                if (_map[posX + i][posY + i] == noTurn || posX + i == 18 || posY + i == 18 || _map[posX + i + 1][posY + i + 1] == noTurn)
                    return (0);
                else if (_map[posX + i][posY + i] == turn)
                    check++; 
                i++;
            }

            if (check >= 2)
                return (1);
            else
                return 0;
        }


        private int checkFreeThreeDiagOne(int posX, int posY, int turn, int noTurn)
        {
            int check = 0;
            int i = 1;

            while (i <= 3 && posX - i >= 0 && posY + i <= 18)
            {
                if (_map[posX - i][posY + i] == noTurn || posX - i == 0 || posY + i == 18 || _map[posX - i - 1][posY + i + 1] == noTurn)
                    return (0);
                else if (_map[posX - i][posY + i] == turn)
                    check++;
                i++;
            }

            i = 1;

            while (i <= 3 && posX + i <= 18 && posY - i >= 0)
            {
                if (_map[posX + i][posY - i] == noTurn || posX + i == 18 || posY - i == 0 || _map[posX + i + 1][posY - i - 1] == noTurn)
                    return (0);
                else if (_map[posX + i][posY - i] == turn)
                    check++;
                i++;
            }

            if (check >= 2)
                return (1);
            else
                return 0;
        }

        private int checkFreeThreeVert(int posX, int posY, int turn, int noTurn)
        {
            int check = 0;
            int i = 1;

            while (i <= 3 && posY + i <= 18)
            {
                if (_map[posX][posY + i] == noTurn || posY + i == 18 || _map[posX][posY + i + 1] == noTurn)
                    return (0);
                else if (_map[posX][posY + i] == turn)
                    check++;
                i++;
            }

            i = 1;

            while (i <= 3 && posY - i >= 0)
            {
                if (_map[posX][posY - i] == noTurn || posY - i == 0 || _map[posX][posY - i - 1] == noTurn)
                    return (0);
                else if (_map[posX][posY - i] == turn)
                    check++;

                i++;
            }

            if (check >= 2)
                return (1);
            else
                return 0;

        }

        private int checkFreeThreeHoriz(int posX, int posY, int turn, int noTurn)
        {
            int check = 0;
            int i = 1;

            while (i <= 3 && posX - i >= 0)
            {
                if (_map[posX - i][posY] == noTurn || posX - i == 0 || _map[posX - i - 1][posY] == noTurn)
                    return (0);
                else if (_map[posX - i][posY] == turn)
                    check++;
                i++;
            }

            i = 1;

            while (i <= 3 && posX + i <= 18)
            {
                if (_map[posX + i][posY] == noTurn || posX + i == 18 || _map[posX + i + 1][posY] == noTurn)
                    return (0);
                else if (_map[posX + i][posY] == turn)
                    check++;
                i++;
            }

            if (check >= 2)
                return (1);
            else
                return 0;
        }

        private bool checkDoubleFreeThree(int posX, int posY)
        {
            int turn = (int)_turn + 1;
            int noTurn;
            int count = 0;

            if (turn == 1)
                noTurn = 2;
            else
                noTurn = 1;


            if (checkFreeThreeHoriz(posX, posY, turn, noTurn) == 1)
                count++;
            if (checkFreeThreeVert(posX, posY, turn, noTurn) == 1)
                count++;
            if (checkFreeThreeDiagOne(posX, posY, turn, noTurn) == 1)
                count++;
            if (checkFreeThreeDiagTwo(posX, posY, turn, noTurn) == 1)
                count++;
            if (count < 2)
                return (true);
            return (false);
        }

            private Vector2 convertCoor(float x, float y)
            {
                if (x <= 2 || y <= 2 || x >= 585 || y >= 585)
                {
                    Vector2 result = new Vector2(-1, -1);
                    return (result);
                }
                else
                {
                    int newX = (int)x / 31;
                    int newY = (int)y / 31;
                    Vector2 result = new Vector2(newX, newY);
                    return (result);
                }
            }

            private bool checkLastShotVertical(int posX, int posY, int turn, int noTurn)
            {
                if (posX < 19 && posX >= 0)
                {
                    if (posY - 1 >= 0 && posY + 2 < 19 && _map[posX][posY + 1] == turn && _map[posX][posY + 2] == 0 && _map[posX][posY - 1] == noTurn)
                        return true;
                    if (posY - 2 >= 0 && posY + 1 < 19 && _map[posX][posY - 1] == turn && _map[posX][posY - 2] == 0 && _map[posX][posY + 1] == noTurn)
                        return true;
                    if (posY + 2 < 19 && posY - 1 >= 0 && _map[posX][posY + 1] == turn && _map[posX][posY + 2] == noTurn && _map[posX][posY - 1] == 0)
                        return true;
                    if (posY - 2 >= 0 && posY + 1 < 19 && _map[posX][posY + 1] == 0 && _map[posX][posY - 1] == turn && _map[posX][posY - 2] == noTurn)
                        return true;
                }
                return false;
            }
            
            private int checkVert(int posX, int posY, int turn, int noTurn)
            {
                int i = 1, index = 1;
                int count = 1;
                int[] tabY = new int[20];
                int[] tabX = new int[20];
                tabY[0] = posY;
                for (int j = 0; j != tabX.Length; j++)
                    tabX[j] = posX;
                while (posY + i < 19 && _map[posX][posY + i] == turn)
                {
                    tabY[index++] = posY + i;
                    i++;
                    count++;
                }
                i = 1;
                while (posY - i >= 0 && _map[posX][posY - i] == turn)
                {
                    tabY[index++] = posY - i;
                    i++;
                    count++;
                }
                tabY[index] = 0;
                if (count >= 5)
                {
                    for (index = 0; index < 5; index++)
                        if (checkLastShotHorizontal(posX, tabY[index], turn, noTurn) == true)
                        {
                            _count = count;
                            tabY.CopyTo(_lastShotTabY, 0);
                            tabX.CopyTo(_lastShotTabX, 0);
                            return 2;
                        }
                    _winner = turn;
                    return 1;
                }
                return 0;
            }
            
        
        private bool checkLastShotHorizontal(int posX, int posY, int turn, int noTurn)
        {
            if (posY < 19 && posY >= 0)
            {
                if (((posX + 2 < 19 && posX - 1 >= 0) && _map[posX + 1][posY] == turn && _map[posX + 2][posY] == 0 && _map[posX - 1][posY] == noTurn)
                    || (posX - 2 >= 0 && posX + 1 < 19 && _map[posX - 1][posY] == turn && _map[posX - 2][posY] == 0 && _map[posX + 1][posY] == noTurn)
                    || (posX + 2 < 19 && posX - 1 >= 0 && _map[posX - 1][posY] == 0 && _map[posX + 1][posY] == turn && _map[posX + 2][posY] == noTurn)
                    || (posX - 2 >= 0 && posX + 1 < 19 && _map[posX - 1][posY] == turn && _map[posX - 2][posY] == noTurn && _map[posX + 1][posY] == 0))
                {
                  return true;
                }
            }
            return false;
        }

        private int checkHorizon(int posX, int posY, int turn, int noTurn)
            { 
                int i = 1, index = 1;
                int count = 1;
                int[] tabX = new int[19];
                int[] tabY = new int[19];
                tabX[0] = posX;

                for (int j = 0; j != tabY.Length; j++)
                    tabY[j] = posY;
                while (posX + i < 19 && _map[posX + i][posY] == turn)
                {
                    tabX[index++] = posX + i;
                    i++;
                    count++;
                }
                i = 1;
                while (posX - i >= 0 && _map[posX - i][posY] == turn)
                {
                    tabX[index++] = posX - i;
                    i++;
                    count++;
                }
                tabX[index] = 0;
               if (count >= 5)
                {
                    for (index = 0; index < 5; index++)
                        if (checkLastShotVertical(tabX[index], posY, turn, noTurn) == true) // s'il est cassable
                        {
                            _count = count;
                            tabX.CopyTo(_lastShotTabX, 0);              
                            tabY.CopyTo(_lastShotTabY, 0);
                            return 2;
                        }
                    _winner = turn;       
                   return 1;
                }
                return 0;
            }

        private bool checkLastShotDiagOne(int posX, int posY, int turn, int noTurn)
        {
           if (((posX + 1 < 19 && posY + 1 < 19 && posX - 2 >= 0 && posY - 2 >= 0) 
                    && _map[posX + 1][posY + 1] == noTurn && _map[posX - 1][posY - 1] == turn && _map[posX - 2][posY - 2] == 0)
                   || ((posX + 2 < 19 && posY + 2 < 19 && posX - 1 >= 0 && posY - 1 >= 0)
                    && _map[posX + 1][posY + 1] == turn && _map[posX - 1][posY - 1] == 0 && _map[posX + 2][posY + 2] == noTurn)
                   || ((posX + 2 < 19 && posY + 1 < 19 && posX - 1 >= 0 && posY - 2 >= 0)
                    && _map[posX + 1][posY + 1] == turn && _map[posX - 1][posY - 1] == noTurn && _map[posX + 2][posY + 2] == 0)
                   || ((posX + 1 < 19 && posY + 1 < 19 && posX - 2 >= 0 && posY - 2 >= 0) 
                    && _map[posX + 1][posY + 1] == 0 && _map[posX - 1][posY - 1] == turn && _map[posX - 2][posY - 2] == noTurn))
                {
                    return true;
                }              
            return false;
        }

        private int checkDiagOne(int posX, int posY, int turn, int noTurn)
            {
                int i = 1, index = 1;
                int count = 1;
                int[] tabX = new int[19];
                int[] tabY = new int[19];      

                tabX[0] = posX;
                tabY[0] = posY;
                while (posX + i < 19 && posY + i < 19 && _map[posX + i][posY + i] == turn)
                {
                    tabX[index] = posX + i;
                    tabY[index] = posY + i;
                    i++;
                    count++;
                    index++;
                }
                i = 1;
                while (posX - i >= 0 && posY - i >= 0 && _map[posX - i][posY - i] == turn)
                {
                    tabX[index] = posX - i;
                    tabY[index] = posY - i;
                    i++;
                    count++;
                    index++;
                }
                tabX[index] = 0;
                if (count >= 5)
                {
                    for (index = 0; index < 5; index++)
                        if (checkLastShotVertical(tabX[index], tabY[index], turn, noTurn) == true || checkLastShotHorizontal(tabX[index], tabY[index], turn, noTurn) == true || checkLastShotDiagTwo(tabX[index], tabY[index], turn, noTurn) == true)
                        {
                            _count = count;
                            tabX.CopyTo(_lastShotTabX, 0);
                            tabY.CopyTo(_lastShotTabY, 0);
                            return 2;
                        }
                    _winner = turn;
                    return 1;
                }
                return 0;
            }

        private bool checkLastShotDiagTwo(int posX, int posY, int turn, int noTurn)
        {
            if (((posX + 1 < 19 && posY + 1 < 19 && posX - 2 >= 0 && posY - 2 >= 0)
                        && _map[posX - 1][posY - 1] == noTurn && _map[posX + 1][posY + 1] == turn && _map[posX + 2][posY + 2] == 0)
                    || ((posX + 1 < 19 && posY + 1 < 19 && posX - 2 >= 0 && posY - 2 >= 0) 
                        && _map[posX - 1][posY - 1] == turn && _map[posX + 1][posY + 1] == 0 && _map[posX - 2][posY - 2] == noTurn)
                    || ((posX + 1 < 19 && posY + 1 < 19 && posX - 2 >= 0 && posY - 2 >= 0) 
                        && _map[posX - 1][posY - 1] == turn && _map[posX + 1][posY + 1] == noTurn && _map[posX - 2][posY - 2] == 0)
                    || ((posX + 2 < 19 && posY + 2 < 19 && posX - 1 >= 0 && posY - 1 >= 0) 
                        && _map[posX - 1][posY - 1] == 0 && _map[posX + 1][posY + 1] == turn && _map[posX + 2][posY + 2] == noTurn))
                {
                       return true;
                }
            return false;
        }

            private int checkDiagTwo(int posX, int posY, int turn, int noTurn)
            {
                int i = 1, index = 1;
                int count = 1;
                int[] tabY = new int[19];
                int[] tabX = new int[19];

                tabX[0] = posX;
                tabY[0] = posY;

                while (posX + i < 19 && posY - i >= 0 && _map[posX + i][posY - i] == turn)
                {
                    tabX[index] = posX + i;
                    tabY[index] = posY - i;
                    i++;
                    count++;
                    index++;
                }
                i = 1;
                while (posX - i >= 0 && posY + i < 19 && _map[posX - i][posY + i] == turn)
                {
                    tabX[index] = posX - i;
                    tabY[index] = posY + i;
                    i++;
                    count++;
                    index++;
                }
                tabX[index] = 0;
                if (count >= 5)
                {
                    for (index = 0; index < 5; index++)
                            if (checkLastShotVertical(tabX[index], posY, turn, noTurn) == true || checkLastShotHorizontal(posX, tabY[index], turn, noTurn) == true || checkLastShotDiagOne(tabX[index], tabY[index], turn, noTurn) == true)
                        {
                            _count = count;
                            tabX.CopyTo(_lastShotTabX, 0);
                            tabY.CopyTo(_lastShotTabY, 0);
                            return 2;
                        }
                    _winner = turn;
                    return 1;
                }
                    return 0;
            }

            private int checkVictory(int posX, int posY)
            {
                int check;
                int turn = (int)_turn + 1;
                int noTurn;

                if (turn == 1)
                    noTurn = 2;
                else
                    noTurn = 1;

                if ((check = checkHorizon(posX, posY, turn, noTurn)) == 0)
                    if ((check = checkVert(posX, posY, turn, noTurn)) == 0)
                        if ((check = checkDiagOne(posX, posY, turn, noTurn)) == 0)
                            if ((check = checkDiagTwo(posX, posY, turn, noTurn)) == 0)
                                return 0;
                if (check == 1)
                    return 1;
                return 2;
            }
    }
}
