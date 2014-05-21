﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading;

namespace SwarchServer
{
    class GameState
    {
        public string roomName;
        public int roomID;
        public bool gameStarted = false;
        public ArrayList playerList;
        public bool isServerRunning = true;
        //public SQLiteDB db = new SQLiteDB();
        Thread gameLoop;

        public GameState(string name, int id)
        {
            roomName = name;
            roomID = id;
            playerList = new ArrayList();

            //gameLoop = new Thread(new ThreadStart(serverLoop));
            //gameLoop.Start();
            //db.dbConnect("users.sqlite");
        }

        void Update()
        {
            Player[] lockedPlayerList;

            lock (playerList)
            {
                lockedPlayerList = new Player[playerList.Count];
                playerList.CopyTo(lockedPlayerList);
            }

            foreach (Player player in lockedPlayerList)
            {
                //grab the readqueue then unlock the readqueue.

                lock (player.readQueue)
                {
                    Queue<Command> prq = player.readQueue;

                    while (prq.Count != 0)
                    {
                        Command cmd = prq.Dequeue();
                        switch (cmd.cType)
                        {
                            case CType.PlayerPosition:
                                break;
                            case CType.SizeUpdate:
                                break;
                            case CType.EatPellet:
                                break;
                            case CType.SpawnPellet:
                                break;
                            case CType.EatPlayer:
                                break;
                            case CType.Death:
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        public void startGame()
        {
            foreach(Player player in playerList)
            {
                player.sendCommand(Command.startGameCommand(0));
            }
        }

        public void sendPlayerInfo(Player player)
        {
            Player[] lockedPlayerList;

            lock (playerList)
            {
                lockedPlayerList = new Player[playerList.Count];
                playerList.CopyTo(lockedPlayerList);
            }

            for (int i = 0; i < lockedPlayerList.Length; i++)
            {
                lockedPlayerList[i].sendCommand(Command.newPlayerCommand(0, player.playerName, player.playerNumber));

                if (player != lockedPlayerList[i])
                {
                    player.sendCommand(Command.newPlayerCommand(0, ((Player)lockedPlayerList[i]).playerName, ((Player)lockedPlayerList[i]).playerNumber));
                }
            }
        }

        public void sendPlayerInfoOnLeave(Player player)
        {
            Player[] lockedPlayerList;

            lock (playerList)
            {
                lockedPlayerList = new Player[playerList.Count];
                playerList.CopyTo(lockedPlayerList);
            }

            for (int i = 0; i < lockedPlayerList.Length; i++)
            {
                lockedPlayerList[i].sendCommand(Command.leftPlayerCommand(0, player.playerName, player.playerNumber));
            }
        }

        public void playerPosition(float x, float y, int dir)
        {

        }

        public void addPlayer(Player p)
        {
            lock (playerList)
            {
                playerList.Add(p);
            }
            sendPlayerInfo(p);
        }

        public void removePlayer(Player p)
        {
            lock (playerList)
            {
                playerList.Remove(p);
            }
            sendPlayerInfoOnLeave(p);
        }

        public int numberOfPlayers()
        {
            return playerList.Count;
        }

        /*public void stopServer()
        {
            isServerRunning = false;
            gameLoop.Abort();
        }

        private void serverLoop()
        {
            while(isServerRunning)
            {
                Update();
            }
        }*/
    }
}
