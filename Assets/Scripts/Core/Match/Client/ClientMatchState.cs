using System;
using System.Collections.Generic;
using Core.Map;
using Core.Utils;
using JetBrains.Annotations;

namespace Core.Match.Client
{
    public class ClientMatchState
    {
        public MatchPlayer MyState { get; private set; }

        public MatchPlayer EnemyState { get; private set; }
        
        public bool IsMyTurn { get; private set; }

        public List<Guid> CardsToDraftGuids { get; } = new();

        [CanBeNull] public Guid[] CardsInHandIds { get; private set; }
        
        [CanBeNull] public Fatigue Fatigue { get; private set; }
        
        [CanBeNull] public LevelInfo LevelInfo { get; private set; }

        public delegate void ClientMatchStateChangeHandler(ClientMatchState state);

        public event ClientMatchStateChangeHandler OnStateChanged;

        public ClientMatchState()
        {
            Reset();
        }

        public void ApplyChanges(MatchPlayer myState = null, MatchPlayer enemyState = null, bool? isMyTurn = null, [CanBeNull] Guid[]
            cardsInHandIds = null, Fatigue fatigue = null, [CanBeNull] LevelInfo levelInfo = null)
        {
            MyState = myState ?? MyState;
            EnemyState = enemyState ?? EnemyState;
            IsMyTurn = isMyTurn ?? IsMyTurn;
            CardsInHandIds = cardsInHandIds ?? CardsInHandIds;
            Fatigue = fatigue ?? Fatigue;
            LevelInfo = levelInfo != null ? levelInfo : LevelInfo;
            OnStateChanged?.Invoke(this);
        }

        public void Reset()
        {
            MyState = EnemyState = new MatchPlayer()
            {
                Name = "Unknown",
                Castle = new BlankCastleCreator().CreateCastle()
            };

            IsMyTurn = false;
            CardsInHandIds = Array.Empty<Guid>();
            Fatigue = null;
            LevelInfo = null;
            OnStateChanged?.Invoke(this);
        }
    }
}