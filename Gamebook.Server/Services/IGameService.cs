using Gamebook.Server.Models;
using System.Threading.Tasks;

namespace Gamebook.Server.Services
{
    public interface IGameService
    {
        Task<Player> CreateNewGame(string userId);
        Task<GameState> ProcessAction(int playerId, string actionType, object data);
        Task<bool> ValidateMove(int playerId, int connectionId);
        Task<(int newRoomId, int playerHp, string playerStatus)> MovePlayer(Player player, Connection connection);
    }
}