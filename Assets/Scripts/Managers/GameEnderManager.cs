using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Managers
{
    internal class GameEnderManager : Manager
    {
        //public AudioClip GameWonSFX;
        //public AudioClip GameLostSFX;

        internal const int INQUISITIONDAY = 51;
        private const int ZERO = 0;

        private int _yersterdaysPeasants;

        private TimeManager _timeManager;
        private NPCManager _npcManager; 
       // private SoundManager _soundManager;

        public delegate void GameOverHandler(GameResult resut, string message);
        public event GameOverHandler GameOver;

        public override void Initialize()
        {
            _timeManager = GameManager.Instance.GetManager<TimeManager>();
            _npcManager = GameManager.Instance.GetManager<NPCManager>();
           // _soundManager = GameManager.Instance.GetManager<SoundManager>();

            if (_timeManager != null)
            {
                _timeManager.OnCyclePassage += OnTimePassage;
            }

            _yersterdaysPeasants = _npcManager.GetActivePeasantCount();
        }
        public enum GameResult
        {
            GameWon,
            GameLost
        }

        public void GameWon()
        {
            //_soundManager.PlaySoundEffect(GameWonSFX, this.transform.position);
            SoundManager.Instance.PlaySound("WinScreen", transform.position);
            GameOver?.Invoke(GameResult.GameWon, "You managed to build the portal just in time... The traitor will suffer!");
        }

        private void OnTimePassage(DayCycle currentCycle)
        {
            bool gameOver = false;
            string message = string.Empty;
                                                //Mateusz Note: there is no gameOver condition logic like in below each if/elses should be: gameOver = true; ???
            // Inquisition comes on day 51
            if (INQUISITIONDAY == _timeManager.DayNumber)
                message = "You took too long to have you vengeance... The inquisition has found you!";
            // Peasant count reaches 0
            else if (_npcManager.GetActivePeasantCount() <= ZERO)
                message = "here are no more peasants around to help you build the portal. In time, the inquisition will come and you won't have your vengeance...";
            // Peasants count is less than half since prior day
            else if (_npcManager.GetActivePeasantCount() < _yersterdaysPeasants/2)
                message = "The peasants area leaving, as their families are perishing. In time, the inquisition will come and you won't have your vengeance...";
            // Total dread count is twice as peasant count
            else if (_npcManager.GetNPCsOfType<Peasant>().Sum(peasant => peasant.DreadFactor) > _npcManager.GetActivePeasantCount())
                message = "The peasants are scared of you, they will leave your town. In time, the inquisition will come and won't have your vengeance...";

            if (gameOver)
            {
                //_soundManager.PlaySoundEffect(GameLostSFX, this.transform.position);
                SoundManager.Instance.PlaySound("GameOverScreen", transform.position);
                GameOver?.Invoke(GameResult.GameLost, message);
            }
            
            _yersterdaysPeasants = _npcManager.GetActivePeasantCount();
        }
    }
}
