using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using GameOfMemory.Web.Models;

namespace GameOfMemory.Web
{
    public class GameHub : Hub
    {
        public bool Join(string userName)
        {
            var player = GameState.Instance.GetPlayer(userName);

            if (player != null)
            {
                Clients.Caller.PlayerExists();
                return true;
            }

            player = GameState.Instance.CreatePlayer(userName);
            player.ConnectionId = Context.ConnectionId;
            Clients.Caller.name = player.Name;
            Clients.Caller.hash = player.Hash;
            Clients.Caller.id = player.Id;

            Clients.Caller.playerJoined(player);

            return StartGame(player);
        }

        private bool StartGame(Player player)
        {
            if (player != null)
            {
                Player player2;
                var game = GameState.Instance.FindGame(player, out player2);

                if (game != null)
                {
                    Clients.Group(player.Group).buildBoard(game);
                    return true;
                }

                player2 = GameState.Instance.GetNewOpponent(player);
                if (player2 == null)
                {
                    Clients.Caller.waitingList();
                    return true;
                }

                game = GameState.Instance.CreateGame(player, player2);
                game.WhosTurn = player.Id;

                Clients.Group(player.Group).buildBoard(game);
                return true;
            }
            return false;
        }

        public bool Flip(string cardName)
        {
            var userName = Clients.Caller.name;
            Player player = GameState.Instance.GetPlayer(userName);
            if (player != null)
            {
                Player playerOppenent;
                var game = GameState.Instance.FindGame(player, out playerOppenent);

                if (game != null)
                {
                    if (!string.IsNullOrEmpty(game.WhosTurn) && game.WhosTurn != player.Id)
                    {
                      Clients.Group(player.Group).notYourTurn(game);
                      return true;
                    }

                    var card = FindCard(game, cardName);
                    Clients.Group(player.Group).flipCard(card);
                    return true;
                }
            }
            return false;
        }

        private Card FindCard(Game game, string cardName)
        {
            return game.Board.Pieces.FirstOrDefault(c => c.Name == cardName);
        }

        public bool CheckCard(string cardName)
        {
            var userName = Clients.Caller.name;
            Player player = GameState.Instance.GetPlayer(userName);
            if (player != null)
            {
                Player playerOpponent;
                Game game = GameState.Instance.FindGame(player, out playerOpponent);
                if (game != null)
                {
                    if (!string.IsNullOrEmpty(game.WhosTurn) && game.WhosTurn != player.Id)
                        return true;

                    Card card = FindCard(game, cardName);
                    if (game.LastCard == null)
                    {
                        game.WhosTurn = player.Id;
                        game.LastCard = card;
                        return true;
                    }

                    //second flip
                    bool isMatch = IsMatch(game, card);
                    if (isMatch)
                    {
                        StoreMatch(player, card);
                        game.LastCard = null;
                        Clients.Group(player.Group).showMatch(card, userName);

                        if (player.Matches.Count >= 16)
                        {
                            Clients.Group(player.Group).winner(card, userName);
                            GameState.Instance.ResetGame(game);
                            return true;
                        }

                        return true;
                    }

                    Player opponent = GameState.Instance.GetOpponent(player, game);
                    //shift to other player
                    game.WhosTurn = opponent.Id;

                    Clients.Group(player.Group).resetFlip(game.LastCard, card);
                    game.LastCard = null;
                    return true;
                }
            }

            return false;
        }

        private void StoreMatch(Player player, Card card)
        {
            player.Matches.Add(card.Id);
            player.Matches.Add(card.Pair);
        }

        private bool IsMatch(Game game, Card card)
        {
            if (card == null)
                return false;


            if (game.LastCard != null)
            {
                if (game.LastCard.Pair == card.Id)
                {
                    return true;
                }

                return false;
            }
            return false;
        }

    }
}