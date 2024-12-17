﻿using Gamebook.Server.Data;
using Gamebook.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace Gamebook.Server.Services
{
    public interface IGameService
    {
        Task<Player> CreateNewGame(string userId);
        Task<GameState> ProcessAction(int playerId, string actionType, object data);
        Task<bool> ValidateMove(int playerId, int connectionId);
    }

    public class GameService : IGameService
    {
        private readonly GamebookDbContext _dbContext;
        private readonly ILogger<GameService> _gameLogger;

        public GameService(GamebookDbContext dbContext, ILogger<GameService> logger)
        {
            _dbContext = dbContext;
            _gameLogger = logger;
        }

        public async Task<Player> CreateNewGame(string userId)
        {
            var existingPlayer = await _dbContext.Players
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (existingPlayer != null)
            {
                _gameLogger.LogInformation($"Resetting existing player for user {userId}");
                existingPlayer.HP = 100;
                existingPlayer.CurrentRoomID = 1;
                existingPlayer.Status = "Active";
            }
            else
            {
                _gameLogger.LogInformation($"Creating new player for user {userId}");
                existingPlayer = new Player
                {
                    UserId = userId,
                    HP = 100,
                    CurrentRoomID = 1,
                    Status = "Active"
                };
                _dbContext.Players.Add(existingPlayer);
            }

            var gameState = new GameState
            {
                PlayerID = existingPlayer.ID,
                Timestamp = DateTime.UtcNow,
                Data = System.Text.Json.JsonSerializer.Serialize(new
                {
                    Action = "NewGame",
                    HP = existingPlayer.HP,
                    Room = existingPlayer.CurrentRoomID
                })
            };

            _dbContext.GameStates.Add(gameState);
            await _dbContext.SaveChangesAsync();

            return existingPlayer;
        }

        public async Task<GameState> ProcessAction(int playerId, string actionType, object data)
        {
            var player = await _dbContext.Players
                .Include(p => p.CurrentRoom)
                .FirstOrDefaultAsync(p => p.ID == playerId);

            if (player == null)
            {
                _gameLogger.LogWarning($"Player {playerId} not found");
                throw new InvalidOperationException("Player not found");
            }

            GameState gameState;

            switch (actionType)
            {
                case "challenge":
                    _gameLogger.LogInformation($"Processing challenge for player {playerId}");
                    var challengeData = (int)data;
                    gameState = await ProcessChallenge(player, challengeData);
                    break;

                case "move":
                    _gameLogger.LogInformation($"Processing move for player {playerId}");
                    var connectionId = (int)data;
                    gameState = await ProcessMove(player, connectionId);
                    break;

                default:
                    _gameLogger.LogWarning($"Unknown action type: {actionType}");
                    throw new InvalidOperationException($"Unknown action type: {actionType}");
            }

            await _dbContext.SaveChangesAsync();
            return gameState;
        }

        public async Task<bool> ValidateMove(int playerId, int connectionId)
        {
            var player = await _dbContext.Players.FindAsync(playerId);
            if (player == null)
            {
                _gameLogger.LogWarning($"Player {playerId} not found during move validation");
                return false;
            }

            var connection = await _dbContext.Connections
                .FirstOrDefaultAsync(c =>
                    c.ID == connectionId &&
                    c.RoomID1 == player.CurrentRoomID);

            if (connection == null)
            {
                _gameLogger.LogWarning($"Invalid connection {connectionId} for player {playerId}");
                return false;
            }

            if (player.HP <= 0)
            {
                _gameLogger.LogInformation($"Player {playerId} cannot move - HP is 0");
                return false;
            }

            return true;
        }

        private async Task<GameState> ProcessChallenge(Player player, int challengeId)
        {
            var challenge = await _dbContext.Challenges.FindAsync(challengeId);
            if (challenge == null)
            {
                _gameLogger.LogWarning($"Challenge {challengeId} not found");
                throw new InvalidOperationException("Challenge not found");
            }

            var random = new Random();
            var success = random.Next(100) > 50;
            var hpChange = success ? 0 : -20;

            player.HP += hpChange;

            if (player.HP <= 0)
            {
                _gameLogger.LogInformation($"Player {player.ID} died during challenge");
                player.Status = "Dead";
                player.HP = 0;
            }

            var gameState = new GameState
            {
                PlayerID = player.ID,
                Timestamp = DateTime.UtcNow,
                Data = System.Text.Json.JsonSerializer.Serialize(new
                {
                    Action = "Challenge",
                    ChallengeId = challengeId,
                    Success = success,
                    HPChange = hpChange,
                    CurrentHP = player.HP,
                    Outcome = success ? challenge.SuccessOutcome : challenge.FailureOutcome
                })
            };

            _dbContext.GameStates.Add(gameState);
            return gameState;
        }

        private async Task<GameState> ProcessMove(Player player, int connectionId)
        {
            var connection = await _dbContext.Connections
                .FirstOrDefaultAsync(c =>
                    c.ID == connectionId &&
                    c.RoomID1 == player.CurrentRoomID);

            if (connection == null)
            {
                _gameLogger.LogWarning($"Invalid movement: connection {connectionId} for player {player.ID}");
                throw new InvalidOperationException("Invalid movement");
            }

            _gameLogger.LogInformation($"Moving player {player.ID} from room {player.CurrentRoomID} to {connection.RoomID2}");
            player.CurrentRoomID = connection.RoomID2;

            var gameState = new GameState
            {
                PlayerID = player.ID,
                Timestamp = DateTime.UtcNow,
                Data = System.Text.Json.JsonSerializer.Serialize(new
                {
                    Action = "Move",
                    FromRoom = connection.RoomID1,
                    ToRoom = connection.RoomID2,
                    CurrentHP = player.HP
                })
            };

            _dbContext.GameStates.Add(gameState);
            return gameState;
        }
    }
}