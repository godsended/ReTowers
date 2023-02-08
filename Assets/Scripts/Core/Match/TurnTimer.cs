using System.Timers;
using Core.Match.Server;
using Core.Server;
using UnityEngine;

namespace Core.Match
{
    public class TurnTimer
    {
        private static readonly int TurnTime = int.Parse(Configurator.data["BattleConfiguration"]["turnTime"]);

        private readonly Timer timer;

        private int currentTime;

        private MatchServer match;
        
        public TurnTimer(MatchServer match)
        {
            timer = new Timer(1000);
            timer.Elapsed += TimerOnElapsed;
            timer.AutoReset = true;
            this.match = match;
            timer.Start();
        }

        public void Start()
        {
            currentTime = TurnTime;
        }

        public void Stop() => currentTime = 0;

        public void Close() => timer.Close();

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (currentTime >= 0)
                currentTime--;
            Debug.Log(currentTime);
            if (currentTime == 0)
            {
                MatchServerController.instance.ConcurrentActions.Add(OnTime);
            }
        }

        private void OnTime()
        {
            match.PassTheMove(true);
            match.SendOutMatchDetails();
        }
    }
}