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
    class Ia
    {
        /*Info jeu*/
        int[][] _map = new int[19][];               // Map de jeu
        int[][] _resultMap = new int[19][];     // Map de resultat
        bool _doubleThree = false;                  // Regle du double 3
        bool _breakingFive = false;                 // Regle du 5 cassable
        int _timeout;                               // Timeout
        int _count = 0;

        /*Info Bot*/
        int _capture = 0;                           // Nombre de capture du bot

        /*Info Ennemi*/
        Vector2 _lastPlay = new Vector2(-1, -1);    // Derniere pierre pose par l'ennemi, (-1, -1) si l'ennemi a jamais joue
        int _captureEnnemi = 0;                     // Nombre de capture faites par l'ennemi

        #region Define
        int SIZE = 19;
        #endregion

        public Ia()
        {
            for (int i = 0; i < SIZE; ++i)
            {
                _map[i] = new int[SIZE];
                for (int y = 0; y < SIZE; ++y)
                    _map[i][y] = 0;
            }
            for (int i = 0; i < SIZE; ++i)
            {
                _resultMap[i] = new int[SIZE];
                for (int y = 0; y < SIZE; ++y)
                    _resultMap[i][y] = 0;
            }
        }

        #region Setter_et_Getter

        /*Setter*/
        public void updateCase(int x, int y, int value) { _map[y][x] = value; }
        public void setLastPlay(Vector2 lastPlay) { _lastPlay = lastPlay; }
        public void setCaptureEnnemi(int captureEnnemi) { _captureEnnemi = captureEnnemi; }
        public void setCapture(int capture) { _capture = capture; }
        public void setDoubleThree(bool doubleThree) { _doubleThree = doubleThree; }
        public void setBreakingFive(bool breakingFive) { _breakingFive = breakingFive; }
        public void setTimeout(int timeout) { _timeout = timeout; }

        /*Getter*/
        public int getCaptureEnnemi() { return _captureEnnemi; }
        public int getCapture() { return _capture; }

        #endregion

        /*Deroulement de l'IA*/
        public Vector2 findPlay()
        {
            Vector2 play = new Vector2(-1, -1);

            Vector2 Capture1 = new Vector2(-1, -1);
            Vector2 Capture2 = new Vector2(-1, -1);

            if (CheckForWinnerAbsolute(ref play, 2, 1, ref Capture1, ref Capture2, _map))
                return play;
            if (CheckForWinnerAbsolute(ref play, 1, 2, ref Capture1, ref Capture2, _map))
                return play;
            if ((checkLose(ref play) == true) && (play.X != -1 && play.Y != -1))
                return play;
            if (checkWin(ref play))
                return play;
            if (CheckCatch(ref play, ref Capture1, ref Capture2, 1, 2, _map))
            {
                _captureEnnemi += 2;
                return play;
            }
            if (CheckCatch(ref play, ref Capture1, ref Capture2, 2, 1, _map))
            {
                _capture += 2;
                return play;
            }
            monteCarlos(ref play);
            return play;
        }



        /* 
         * Fonction qui verifie si l'IA peut gagner.
         * Si elle peut gagner, retourne true et met les coords de victoire dans play.
         * Si elle ne peut pas, retourne false et met les coords de play a (-1, -1).
         */

        //Cherche Une Position permettant de gagner (CheckWinSmart)
        private bool checkWin(ref Vector2 play)
        {
            // Penser a verifier par rapport aux regles en vigueur
            for (int y = 0; y < SIZE; y++)
            {
                for (int x = 0; x < SIZE; x++)
                {
                    if (_map[y][x] == 2)
                    {
                        if (BeginningToMakeAWinner(ref play, x, y, 2))
                            return true;
                    }
                }
            }
            return false;
        }

        /* 
         * Fonction qui verifie si l'ennemi peut gagner et cherche une case pour le contrer.
         * Si l'ennemi peut gagner, la fonction retourne true, sinon elle retourne false.
         * Si l'IA peut contrer elle set play aux coords de la case du contre, sinon elle set play a (-1, -1)
         */
        //Cherche Une Position empechant l'adversaire de gagner (CheckLoseSmart)
        private bool checkLose(ref Vector2 play)
        {
            if (BeginningToMakeAWinner(ref play, (int)_lastPlay.X, (int)_lastPlay.Y, _map[(int)_lastPlay.Y][(int)_lastPlay.X]))
                return true;
            return false;
        }

        public void reset()
        {
            for (int i = 0; i < SIZE; ++i)
            {
                _map[i] = new int[SIZE];
                for (int y = 0; y < SIZE; ++y)
                    _map[i][y] = 0;
            }
            for (int i = 0; i < SIZE; ++i)
            {
                _resultMap[i] = new int[SIZE];
                for (int y = 0; y < SIZE; ++y)
                    _resultMap[i][y] = 0;
            }

            _lastPlay = new Vector2(-1, -1);
            _capture = 0;
            _captureEnnemi = 0;
            _count = 0;
        }

        private void putRandomToken(int[][] _virtualMap, ref Vector2 play, int count)
        {
            Random random = new Random();

            int randomNbr = random.Next(count);
            int X = 0;
            int Y = 0;
            int countToRandom = 0;

            while (Y < 19)
            {
                X = 0;
                while (X < 19)
                {
                    if (_virtualMap[Y][X] == 3)
                    {
                        if (countToRandom == randomNbr)
                        {
                            play.X = X;
                            play.Y = Y;
                            return;
                        }
                        else
                            countToRandom++;
                    }
                    X++;
                }
                Y++;
            }
        }

        private void findWhereToPlay()
        {
            int X = 0;
            int Y = 0;

            while (Y < 19)
            {
                X = 0;
                while (X < 19)
                {
                    if (_map[Y][X] == 1 || _map[Y][X] == 2)
                    {
                        if (Y - 1 >= 0)
                            if (_map[Y - 1][X] == 0)
                                _map[Y - 1][X] = 3;
                        if (Y + 1 < 19)
                            if (_map[Y + 1][X] == 0)
                                _map[Y + 1][X] = 3;
                        if (X - 1 >= 0)
                            if (_map[Y][X - 1] == 0)
                                _map[Y][X - 1] = 3;
                        if (X + 1 < 19)
                            if (_map[Y][X + 1] == 0)
                                _map[Y][X + 1] = 3;
                        if (Y - 1 >= 0 && X - 1 >= 0)
                            if (_map[Y - 1][X - 1] == 0)
                                _map[Y - 1][X - 1] = 3;
                        if (Y + 1 < 19 && X + 1 < 19)
                            if (_map[Y + 1][X + 1] == 0)
                                _map[Y + 1][X + 1] = 3;
                        if (X - 1 >= 0 && Y + 1 < 19)
                            if (_map[Y + 1][X - 1] == 0)
                                _map[Y + 1][X - 1] = 3;
                        if (X + 1 < 19 && Y - 1 >= 0)
                            if (_map[Y - 1][X + 1] == 0)
                                _map[Y - 1][X + 1] = 3;
                    }
                    if (_map[Y][X] == 3)
                        _count++;
                    X++;
                }
                Y++;
            }
        }

        private void virtualFindWhereToPlay(ref int[][] _virtualMap, ref int count)
        {
            int X = 0;
            int Y = 0;

            while (Y < 19)
            {
                X = 0;
                while (X < 19)
                {
                    if (_virtualMap[Y][X] == 1 || _virtualMap[Y][X] == 2)
                    {
                        if (Y - 1 >= 0)
                            if (_virtualMap[Y - 1][X] == 0)
                                _virtualMap[Y - 1][X] = 3;
                        if (Y + 1 < 19)
                            if (_virtualMap[Y + 1][X] == 0)
                                _virtualMap[Y + 1][X] = 3;
                        if (X - 1 >= 0)
                            if (_virtualMap[Y][X - 1] == 0)
                                _virtualMap[Y][X - 1] = 3;
                        if (X + 1 < 19)
                            if (_virtualMap[Y][X + 1] == 0)
                                _virtualMap[Y][X + 1] = 3;
                        if (Y - 1 >= 0 && X - 1 >= 0)
                            if (_virtualMap[Y - 1][X - 1] == 0)
                                _virtualMap[Y - 1][X - 1] = 3;
                        if (Y + 1 < 19 && X + 1 < 19)
                            if (_virtualMap[Y + 1][X + 1] == 0)
                                _virtualMap[Y + 1][X + 1] = 3;
                        if (X - 1 >= 0 && Y + 1 < 19)
                            if (_virtualMap[Y + 1][X - 1] == 0)
                                _virtualMap[Y + 1][X - 1] = 3;
                        if (X + 1 < 19 && Y - 1 >= 0)
                            if (_virtualMap[Y - 1][X + 1] == 0)
                                _virtualMap[Y - 1][X + 1] = 3;
                    }
                    if (_virtualMap[Y][X] == 3)
                        count++;
                    X++;
                }
                Y++;
            }
        }

        private bool execVirtualGames(ref int[][] _virtualMap)
        {
            /* Fonction qui va realiser une partie aleatoire et renvoyer true si il gagne*/
            int turn = 1;
            int noTurn = 2;
            int playingLimit = 0;
            int count = 0;
            int nbCapturePlayer = 0;
            int nbCaptureBot = 0;
            Vector2 Capture1 = new Vector2(-1, -1);
            Vector2 Capture2 = new Vector2(-1, -1);
            Vector2 play = new Vector2(-1, -1);

            while (playingLimit != 50)
            {
                count = 0;
                virtualFindWhereToPlay(ref _virtualMap, ref count);
                putRandomToken(_virtualMap, ref play, count);
                if (CheckCatch(ref play, ref Capture1, ref Capture2, turn, noTurn, _virtualMap) == true && Capture1.Y != -1 && Capture1.X != -1 && Capture2.Y != -1 && Capture2.X != -1)
                {
                    if (turn == 1)
                        nbCapturePlayer += 2;
                    else
                        nbCaptureBot += 2;
                    _virtualMap[(int)Capture1.Y][(int)Capture1.X] = 3;
                    _virtualMap[(int)Capture2.Y][(int)Capture2.X] = 3;
                }
                if (nbCaptureBot >= 10)
                    return true;
                if (nbCapturePlayer >= 10)
                    return false;

                if (play.X > -1)
                {
                    _virtualMap[(int)play.Y][(int)play.X] = turn;
                    play.X = -1;
                }
                if (turn == 1)
                {
                    turn = 2;
                    noTurn = 1;
                }
                else
                {
                    turn = 1;
                    noTurn = 2;
                }
                playingLimit++;
            }
            if (nbCaptureBot > nbCapturePlayer)
                return true;
            return false;
        }

        private void fillVirtualMap(ref int[][] virtualMap)
        {
            int x = 0;
            int y = 0;

            for (int i = 0; i < SIZE; ++i)
            {
                virtualMap[i] = new int[SIZE];
                for (int j = 0; j < SIZE; ++j)
                    virtualMap[i][j] = 0;
            }
            while (y < 19)
            {
                x = 0;
                while (x < 19)
                {
                    virtualMap[y][x] = _map[y][x];
                    x++;
                }
                y++;
            }
        }

        private void launchMultipleGames(int Y, int X)
        {
            /* Fonction qui va appeler plusieurs fois execVirtualGames() puis qui va faire la moyenne du nombre de parties gagnees et stocker cette valeur */
            int nbGameToPlay = 10;
            int i = 0;
            int nbVictory = 0;
            int[][] _virtualMap = new int[19][];               // Map de jeu


            while (i < nbGameToPlay)
            {
                fillVirtualMap(ref _virtualMap);
                _virtualMap[Y][X] = 2;
                if (execVirtualGames(ref _virtualMap) == true)
                    nbVictory++;
                i++;
            }
            _resultMap[Y][X] = nbVictory;
        }

        private void goThroughMap()
        {
            /* Fonction qui va parcourir le tableau et qui va appeler launchMultipleGames() chaque fois qu'il trouvera la valeur '3' */
            int X = 0;
            int Y = 0;

            while (Y < 19)
            {
                X = 0;
                while (X < 19)
                {
                    if (_map[Y][X] == 3)
                        launchMultipleGames(Y, X);
                    X++;
                }
                Y++;
            }
        }

        private void putToken(int bestResult, int rdCount, ref Vector2 play)
        {
            Random random = new Random();

            int randomNbr = random.Next(rdCount);
            int X = 0;
            int Y = 0;
            int countToRandom = 0;

            while (Y < 19)
            {
                X = 0;
                while (X < 19)
                {
                    if (_resultMap[Y][X] == bestResult)
                    {
                        if (countToRandom == randomNbr)
                        {
                            play.X = X;
                            play.Y = Y;
                            return;
                        }
                        else
                            countToRandom++;
                    }
                    X++;
                }
                Y++;
            }
        }

        private void compareResultMap(ref Vector2 play)
        {
            /* Fonction qui va chercher le meilleur resultat dans la resultMap et qui renverra les coordonnees de la meilleure valeur */
            int bestResult = 0;
            int X = 0;
            int Y = 0;
            int rdCount = 0;

            while (Y < 19)
            {
                X = 0;
                while (X < 19)
                {
                    if (_resultMap[Y][X] > bestResult)
                        bestResult = _resultMap[Y][X];
                    if (_resultMap[Y][X] == bestResult && bestResult != 0)
                        rdCount++;
                    X++;
                }
                Y++;
            }

            if (bestResult == 0)
                putRandomToken(_map, ref play, _count);
            else
               putToken(bestResult, rdCount, ref play);
            _count = 0;
        }

        /* 
         * Fonction qui trouve la "meilleure" case possible pour l'IA.
         * Algo type Monte Carlos.
         * Set play avec les coords du "meilleur" coup. 
         */

        private void resetResultMap()
        {
            for (int i = 0; i < SIZE; ++i)
            {
                _resultMap[i] = new int[SIZE];
                for (int y = 0; y < SIZE; ++y)
                    _resultMap[i][y] = 0;
            }
        }

        private void monteCarlos(ref Vector2 play)
        {
            findWhereToPlay();
            goThroughMap();
            compareResultMap(ref play);
            resetResultMap();
        }



        //[CheckLose]Fonction qui va boucler sur les directions (Horizontal vertical diagonal1 diagonal2)
        private bool BeginningToMakeAWinner(ref Vector2 play, int x, int y, int PlayerType)
        {
            Vector2 TmpPos;
            int NbrMaxTurn = 0;

            for (int i = 0; i < 4; ++i)
            {
                if (i == 0 || i == 2)
                    TmpPos.X = x - 4;
                else if (i == 3)
                    TmpPos.X = x + 4;
                else
                    TmpPos.X = x;
                if (i > 0)
                    TmpPos.Y = y - 4;
                else
                    TmpPos.Y = y;
                NbrMaxTurn = CountNbrMaxTurn(ref TmpPos, i);
                if (CloseToWinPerType(ref play, ref TmpPos, i, PlayerType, 9 - NbrMaxTurn))
                {
                    if (CloseToWinPerRules(ref play, TmpPos, PlayerType, i) == true
                        && play.X < SIZE && play.Y < SIZE && play.X > -1 && play.Y > -1
                        && (_map[(int)play.Y][(int)play.X] == 0 || _map[(int)play.Y][(int)play.X] == 3))
                        return true;
                }
            }
            play.X = -1;
            play.Y = -1;
            return false;
        }

        //[CheckLose] Tourne jusqu'a trouver une position sur le plateau
        private int CountNbrMaxTurn(ref Vector2 TmpPos, int CurrentCheckedType)
        {
            int i = 0;
            while ((TmpPos.X < 0 || TmpPos.Y < 0) && i < SIZE)
            {
                TmpPos = SortOfGetterTypePos(TmpPos, CurrentCheckedType, 1);
                i++;
            }
            return i;
        }

        //[CheckLose] apres avoir remonter de 4 pos, verifier les 9 suivantes 0000X0000
        private bool CloseToWinPerType(ref Vector2 play, ref Vector2 TmpPos, int CurrentCheckedType, int PlayerType, int MaxTurnNbr)
        {
            for (int i = 0; i < MaxTurnNbr; i++)
            {
                if (TmpPos.X < SIZE && TmpPos.Y < SIZE && TmpPos.X > -1 && TmpPos.Y > -1 && _map[(int)TmpPos.Y][(int)TmpPos.X] == PlayerType)
                {
                    play.X = -1;
                    play.Y = -1;
                    if (CloseToWinCheckingFourNext(ref play, TmpPos, CurrentCheckedType, PlayerType))
                        return true;

                }
                TmpPos = SortOfGetterTypePos(TmpPos, CurrentCheckedType, 1);
                i++;
            }
            return false;
        }

        //[CheckLose] une fois positionne sur un pion a nous, verifier si a partir de cette position il y a un risque
        private bool CloseToWinCheckingFourNext(ref Vector2 play, Vector2 TmpPos, int CurrentCheckedType, int PlayerType)
        {
            Vector2 TmpSpecificCase;
            int ItsAGoodPos = 1;
            TmpSpecificCase = SortOfGetterTypePos(TmpPos, CurrentCheckedType, -1);
            TmpPos = SortOfGetterTypePos(TmpPos, CurrentCheckedType, 1);
            for (int i = 1; i < 4; i++)
            {
                if (SaveFreePosition(ref play, TmpPos) == false && TmpPos.X < SIZE && TmpPos.Y < SIZE && TmpPos.X > -1 && TmpPos.Y > -1
                    && _map[(int)TmpPos.Y][(int)TmpPos.X] != PlayerType)
                    return false;
                if (TmpPos.X < SIZE && TmpPos.Y < SIZE && TmpPos.X > -1 && TmpPos.Y > -1 && _map[(int)TmpPos.Y][(int)TmpPos.X] == PlayerType)
                    ItsAGoodPos++;
                if (ItsAGoodPos > 2 && i > 2)
                {
                    if (ItsAGoodPos == 3 && TmpSpecificCase.X < SIZE && TmpSpecificCase.Y < SIZE && TmpSpecificCase.X > -1 && TmpSpecificCase.Y > -1
                        && _map[(int)TmpSpecificCase.Y][(int)TmpSpecificCase.X] != 0
                        && _map[(int)TmpSpecificCase.Y][(int)TmpSpecificCase.X] != 3
                        && _map[(int)TmpSpecificCase.Y][(int)TmpSpecificCase.X] != PlayerType)
                        return false;
                    CloseToFindAGoodPlace(ref play, TmpPos, PlayerType, CurrentCheckedType);
                    return true;
                }
                TmpPos = SortOfGetterTypePos(TmpPos, CurrentCheckedType, 1);
                if (TmpPos.X > SIZE - 1 && TmpPos.Y > SIZE - 1 && TmpPos.X < 0 && TmpPos.Y < 0)
                    return false;
            }
            return false;
        }

        //[CheckLose] Sauvegarde des position libre (qui ne sont pas occupees)
        private bool SaveFreePosition(ref Vector2 play, Vector2 TmpPos)
        {
            if (TmpPos.X < SIZE && TmpPos.Y < SIZE && TmpPos.X > -1 && TmpPos.Y > -1 &&
                    (_map[(int)TmpPos.Y][(int)TmpPos.X] == 0 || _map[(int)TmpPos.Y][(int)TmpPos.X] == 3))
            {
                play.X = TmpPos.X;
                play.Y = TmpPos.Y;
                return true;
            }
            return false;
        }

        //[CheckLose] c'est une forme a jouer je cherche une place libre parce que je n'en est pas encore
        private void CloseToFindAGoodPlace(ref Vector2 play, Vector2 TmpPos, int PlayerType, int CurrentCheckedType)
        {
            if (play.X > SIZE - 1 || play.Y > SIZE - 1 || play.X < 0 || play.Y < 0 ||
                    (_map[(int)play.Y][(int)play.X] != 0 && _map[(int)play.Y][(int)play.X] != 3))
            {
                if (HereIsMyLastChance(ref play, TmpPos, PlayerType, CurrentCheckedType, 1) == false)
                    if (HereIsMyLastChance(ref play, TmpPos, PlayerType, CurrentCheckedType, -3) == false)
                        HereIsMyLastChance(ref play, TmpPos, PlayerType, CurrentCheckedType, -4);
            }
        }

        //[CheckLose] Sauvegarde la position a jouer faute de mieux.
        private bool HereIsMyLastChance(ref Vector2 play, Vector2 TmpPos, int PlayerType, int CurrentCheckedType, int ValueChecked)
        {
            TmpPos = SortOfGetterTypePos(TmpPos, CurrentCheckedType, ValueChecked);
            if (TmpPos.X < SIZE && TmpPos.Y < SIZE && TmpPos.X > -1 && TmpPos.Y > -1 &&
                (_map[(int)TmpPos.Y][(int)TmpPos.X] == 0 || _map[(int)TmpPos.Y][(int)TmpPos.X] == 3))
            {
                play.X = TmpPos.X;
                play.Y = TmpPos.Y;
                return true;
            }
            play.X = -1;
            play.Y = -1;
            return false;
        }

        //incrementation de la position en fonction de la recherche dans la map (Hori/Verti/Diago)
        private Vector2 SortOfGetterTypePos(Vector2 TmpPos, int CurrentCheckedType, int NbrPos)
        {
            int i = 0;
            while (NbrPos > 0 && i < NbrPos)
            {
                if (CurrentCheckedType == 0 || CurrentCheckedType == 2)
                    TmpPos.X++;
                else if (CurrentCheckedType == 3)
                    TmpPos.X--;
                if (CurrentCheckedType > 0)
                    TmpPos.Y++;
                i++;
            }
            while (i > NbrPos)
            {
                if (CurrentCheckedType == 0 || CurrentCheckedType == 2)
                    TmpPos.X--;
                else if (CurrentCheckedType == 3)
                    TmpPos.X++;
                if (CurrentCheckedType > 0)
                    TmpPos.Y--;
                i--;
            }
            return TmpPos;
        }

        //[CloseToWinPerRules] Definir une position a joue si celle trouvee ne respect pas les regles
        private bool CloseToWinPerRules(ref Vector2 play, Vector2 TmpPos, int PlayerType, int CurrentCheckedType)
        {
            if (CheckDoubleThreeFree(ref play, TmpPos, CurrentCheckedType))
            {
                return false;
            }
            return true;
        }

        //[DEBUT] CHECKWIN/CHECKLOSE ABSOLU ne verifie que si il est possible de gagner immediatement (pas intelligent)
        private bool CheckForWinnerAbsolute(ref Vector2 play, int PlayerType, int UnPlayerType, ref Vector2 Capture1, ref Vector2 Capture2, int [][] map)
        {
            if (PlayerType == 1 && _captureEnnemi > 7)
                if (CheckCatch(ref play, ref Capture1, ref Capture2, PlayerType, UnPlayerType, map))
                    return true;
                else if (PlayerType == 2 && _captureEnnemi > 7)
                    if (CheckCatch(ref play, ref Capture1, ref Capture2, PlayerType, UnPlayerType, map))
                        return true;
            for (int y = 0; y < SIZE; y++)
            {
                for (int x = 0; x < SIZE; x++)
                {
                    if (_map[y][x] == PlayerType)
                    {
                        if (CheckForWinnerAbsoluteDirection(ref play, PlayerType, UnPlayerType, y, x))
                            return true;
                    }
                }
            }
            return false;
        }

        //[CheckWin/CheckLose Absolu] Boucle sur les directions
        private bool CheckForWinnerAbsoluteDirection(ref Vector2 play, int PT, int UnPT, int PosY, int PosX)
        {
            Vector2 TmpPos;
            TmpPos.X = PosX;
            TmpPos.Y = PosY;
            for (int i = 0; i < 4; i++)
            {
                if (CheckForWinnerAbsoluteChecking(ref play, TmpPos, i, PT, UnPT))
                    if (CloseToWinPerRules(ref play, TmpPos, PT, i))
                        return true;
            }
            return false;
        }

        //[CheckWin/CheckLose Absolu] Boucle sur les positions relatives a une directions (2 et -2 -> ..?..)
        private bool CheckForWinnerAbsoluteChecking(ref Vector2 play, Vector2 TmpPos, int CurrentCheckedType, int PT, int UnPT)
        {
            int check = 0;
            Vector2 InitTmpPos = TmpPos;
            for (int NbrDirection = 0; NbrDirection < 2; NbrDirection++)
            {
                TmpPos = InitTmpPos;
                TmpPos = CheckDoubleDirection(TmpPos, NbrDirection, CurrentCheckedType);
                for (int i = 0; i < 2; i++)
                {
                    if (TmpPos.X > SIZE - 1 || TmpPos.Y > SIZE - 1 || TmpPos.X < 0 || TmpPos.Y < 0 ||
                        _map[(int)TmpPos.Y][(int)TmpPos.X] == UnPT)
                        return false;
                    else if (_map[(int)TmpPos.Y][(int)TmpPos.X] == PT)
                        check++;
                    else
                    {
                        play.X = TmpPos.X;
                        play.Y = TmpPos.Y;
                    }
                    TmpPos = CheckDoubleDirection(TmpPos, NbrDirection, CurrentCheckedType);
                }
            }
            if (check > 2 && play.X != -1)
                return true;
            play.X = -1;
            play.Y = -1;
            return false;
        }
        //[FIN] CHECKWIN/CHECKLOSE ABSOLU

        //[DEBUT] VERIFICATION DE LA REGLE DE CAPTURE
        //On boucle sur toutes les positions et verifie celles de l'IA (2)
        private bool CheckCatch(ref Vector2 play, ref Vector2 Capture1, ref Vector2 Capture2, int PlayerType, int UnPlayerType, int [][]map)
        {
            for (int y = 0; y < SIZE; y++)
            {
                for (int x = 0; x < SIZE; x++)
                {
                    if (map[y][x] == PlayerType && (FindSomethingToCatch(ref play, PlayerType, UnPlayerType, y, x, ref Capture1, ref Capture2, map) == true))
                        return true;
                }
            }
            return false;
        }

        //On parcour boucle sur les 4 directions (horizontal / vertical / diag1 / diag2)
        private bool FindSomethingToCatch(ref Vector2 play, int PlayerType, int UnPlayerType, int PosY, int PosX, ref Vector2 Capture1, ref Vector2 Capture2, int [][]map)
        {
            Vector2 TmpPos;
            TmpPos.X = PosX;
            TmpPos.Y = PosY;
            for (int i = 0; i < 4; i++)
            {
                if (CheckCatchOnMindTwoOther(ref play, TmpPos, i, PlayerType, UnPlayerType, ref Capture1, ref Capture2, map))
                    if (CloseToWinPerRules(ref play, TmpPos, PlayerType, i))
                        return true;
            }
            return false;
        }


        //si pos=PT, que les deux suivantes = UnPT (for i<2) et que la position suivante est libre alors TRUE
        private bool CheckCatchOnMindTwoOther(ref Vector2 play, Vector2 TmpPos, int CurrentCheckedType, int PT, int UnPT, ref Vector2 Capture1, ref Vector2 Capture2, int [][] map)
        {
            Vector2 InitTmpPos = TmpPos;
            int checkedValue;
            for (int TmpDirection = 0; TmpDirection < 2; TmpDirection++)
            {
                checkedValue = 1;
                TmpPos = InitTmpPos;
                for (int i = 0; i < 2; i++)
                {
                    TmpPos = CheckDoubleDirection(TmpPos, TmpDirection, CurrentCheckedType);
                    if (TmpPos.X > SIZE - 1 || TmpPos.Y > SIZE - 1 || TmpPos.X < 0 || TmpPos.Y < 0 ||
                        map[(int)TmpPos.Y][(int)TmpPos.X] != UnPT)
                        checkedValue = 0;
                    if (i == 0)
                        Capture1 = TmpPos;
                    else
                        Capture2 = TmpPos;
                }
                TmpPos = CheckDoubleDirection(TmpPos, TmpDirection, CurrentCheckedType);
                if (TmpPos.X < SIZE && TmpPos.Y < SIZE && TmpPos.X > -1 && TmpPos.Y > -1 && checkedValue == 1 &&
                    (map[(int)TmpPos.Y][(int)TmpPos.X] == 0 || map[(int)TmpPos.Y][(int)TmpPos.X] == 3))
                {
                    play.X = TmpPos.X;
                    play.Y = TmpPos.Y;
                    return true;
                }
            }
            Capture1.X = -1;
            Capture1.Y = -1;
            Capture2.X = -1;
            Capture2.Y = -1;
            return false;
        }

        //[FIN] VERIFICATION DE LA REGLE DE CAPTURE

        //[DEBUT] VERIFICATION DE LA REGLE DU DOUBLE TROIS LIBRE
        //[CloseToWinPerRules] verification de la regle des double 3 libres dans toutes les directions
        private bool CheckDoubleThreeFree(ref Vector2 play, Vector2 TmpPos, int CurrentCheckedType)
        {
            int TmpCount = 0;
            TmpPos = play;
            for (int i = 0; i < 4; i++)
            {
                if (CheckDoubleThreeFreeEachWay(ref play, TmpPos, i, 1, 2))
                    TmpCount++;
            }
            if (TmpCount > 1)
            {
                _doubleThree = true;
                return true;
            }
            return false;
        }

        //[CloseToWinPerRules] Verificaction de la regle dans une direction definie (3 et -3 -> ...?...)
        private bool CheckDoubleThreeFreeEachWay(ref Vector2 play, Vector2 TmpPos, int CurrentCheckedType, int P1, int P2)
        {
            Vector2 TmpSpecificCase = TmpPos;
            int check = 0;
            for (int NbrDirection = 0; NbrDirection < 2; NbrDirection++)
            {
                TmpPos = play;
                TmpSpecificCase = CheckDoubleDirection(TmpPos, NbrDirection, CurrentCheckedType);
                for (int i = 0; i <= 3; i++)
                {
                    if (TmpPos.X > SIZE - 1 || TmpPos.Y > SIZE - 1 || TmpPos.X < 0 || TmpPos.Y < 0 ||
                        TmpSpecificCase.X > SIZE - 1 || TmpSpecificCase.Y > SIZE - 1 || TmpSpecificCase.X < 0 || TmpSpecificCase.Y < 0)
                        break;
                    else if (_map[(int)TmpPos.Y][(int)TmpPos.X] == P1 || _map[(int)TmpSpecificCase.Y][(int)TmpSpecificCase.X] == P1)
                        return false;
                    else if (_map[(int)TmpPos.Y][(int)TmpPos.X] == P2)
                        check++;
                    TmpPos = CheckDoubleDirection(TmpPos, NbrDirection, CurrentCheckedType);
                    TmpSpecificCase = CheckDoubleDirection(TmpPos, NbrDirection, CurrentCheckedType);
                }
            }
            if (check >= 2)
                return true;
            return false;
        }

        //[CloseToWinPerRules] Direction d'incrementation (vers la droite de ma position)
        private Vector2 CheckDoubleDirection(Vector2 TmpPos, int NbrDirection, int CurrentCheckedType)
        {
            if (NbrDirection == 0)
                TmpPos = SortOfGetterTypePos(TmpPos, CurrentCheckedType, 1);
            else
                TmpPos = SortOfGetterTypePos(TmpPos, CurrentCheckedType, -1);
            return TmpPos;
        }

        //[FIN] VERIFICATION DE LA REGLE DU DOUBLE TROIS 
    }
}