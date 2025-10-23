using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Managers
{
    public enum GameResult
    {
        GameWon,
        GameLost
    }
    internal class GameEnderManager : Manager
    {
        internal const int INQUISITIONDAY = 51;
        private const int ZERO = 0;

        private int _yersterdaysPeasants;

        private TimeManager _timeManager;
        private NPCManager _npcManager; 
        
        public delegate void GameOverHandler(GameResult resut, string message);
        public event GameOverHandler GameOver;

        public override void Initialize()
        {
            _timeManager = GameManager.Instance.GetManager<TimeManager>();
            _npcManager = GameManager.Instance.GetManager<NPCManager>();

            if (_timeManager != null)
            {
                _timeManager.OnCyclePassage += OnTimePassage;
            }

            _yersterdaysPeasants = _npcManager.GetActivePeasantCount();
        }

        public void GameWon()
        {
            GameOver?.Invoke(GameResult.GameWon, "You managed to build the portal just in time... The traitor will suffer!");
        }

        public void TestGameLost(int lostCondition = 0)
        {
            switch (lostCondition)
            {
                case 0:
                    GameOver?.Invoke(GameResult.GameLost, "You took too long to have you vengeance... The inquisition has found you!");
                    break;
                case 1:
                    GameOver?.Invoke(GameResult.GameLost, "There are no more peasants around to help you build the portal. In time, the inquisition will come and you won't have your vengeance...");
                    break;
                case 2:
                    GameOver?.Invoke(GameResult.GameLost, "The peasants area leaving, as their families are perishing. In time, the inquisition will come and you won't have your vengeance...");
                    break;
                case 3:
                    GameOver?.Invoke(GameResult.GameLost, "The peasants are scared of you, they will leave your town. In time, the inquisition will come and won't have your vengeance...");
                    break;
            }
        }

        private void OnTimePassage(DayCycle currentCycle)
        {
            // Inquisition comes on day 51
            if (INQUISITIONDAY == _timeManager.DayNumber)
            {
                GameOver?.Invoke(GameResult.GameLost, "You took too long to have you vengeance... The inquisition has found you!");
            }
            // Peasant count reaches 0
            else if (_npcManager.GetActivePeasantCount() <= ZERO)
            {
                GameOver?.Invoke(GameResult.GameLost, "There are no more peasants around to help you build the portal. In time, the inquisition will come and you won't have your vengeance...");
            }
            // Peasants count is less than half since prior day
            else if (_npcManager.GetActivePeasantCount() < _yersterdaysPeasants/2)
            {
                GameOver?.Invoke(GameResult.GameLost, "The peasants area leaving, as their families are perishing. In time, the inquisition will come and you won't have your vengeance...");
            }
            // Total dread count is twice as peasant count
            else if (_npcManager.GetNPCsOfType<Peasant>().Sum(peasant => peasant.DreadFactor) > _npcManager.GetActivePeasantCount())
            {
                GameOver?.Invoke(GameResult.GameLost, "The peasants are scared of you, they will leave your town. In time, the inquisition will come and won't have your vengeance...");
            }
            _yersterdaysPeasants = _npcManager.GetActivePeasantCount();
        }
    }
}
