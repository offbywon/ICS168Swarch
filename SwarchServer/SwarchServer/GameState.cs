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
        private ArrayList playerList;
        public bool isServerRunning = true;
        public SQLiteDB db = new SQLiteDB();
        Thread gameLoop;

        public GameState()
        {
            playerList = new ArrayList();

            gameLoop = new Thread(new ThreadStart(serverLoop));
            gameLoop.Start();
            db.dbConnect("users.sqlite");
        }

        void Update()
        {
            lock (playerList)
            {
                foreach (Player player in playerList)
                {
                    Queue<Command> prq = player.readQueue;
                    lock (prq)
                    {
                        while (prq.Count != 0)
                        {
                            Command cmd = prq.Dequeue();
                            switch (cmd.cType)
                            {
                                case CType.Login:
                                    loginPlayer(player, cmd.username, cmd.password);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
        }

        public void loginPlayer(Player player, string username, string password)
        {
            LoginResponseType lrt = LoginResponseType.FailedLogin;
            if(username != "" && password != "")
            {
                string realPassword = db.getPassword(username);
                if(realPassword == null)
                {
                    realPassword = password;
                    db.addUser(username, password);
                    lrt = LoginResponseType.NewUser;
                    Console.WriteLine("New user created.");
                }
                if(realPassword == password)
                {
                    //db.dbPrintAll();
                    lrt |= LoginResponseType.SucceededLogin;
                    Console.WriteLine("Player has connected.");
                    for (int i = 0; i < playerList.Count; i++)
                    {
                        ((Player)playerList[i]).sendCommand(Command.newPlayerCommand(0, username, player.playerNumber));
                        
                        if(player != playerList[i])
                        {
                            player.sendCommand(Command.newPlayerCommand(0, ((Player)playerList[i]).playerName, ((Player)playerList[i]).playerNumber));
                        }
                    }
                }
            }

            player.sendCommand(Command.loginCommand(0, lrt));
        }

        public void startGame()
        {
            foreach(Player player in playerList)
            {
                player.sendCommand(Command.startGameCommand(0));
            }
        }

        public void addPlayer(Player p)
        {
            lock (playerList)
            {
                playerList.Add(p);
            }
        }

        public void removePlayer(Player p)
        {
            lock (playerList)
            {
                playerList.Remove(p);
            }
        }

        public int numberOfPlayers()
        {
            return playerList.Count;
        }

        public void stopServer()
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
        }
    }
}
