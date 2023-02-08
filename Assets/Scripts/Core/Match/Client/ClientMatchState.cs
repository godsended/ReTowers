using System;
using System.Collections.Generic;
using System.Linq;
using Core.Cards;
using Core.Castle;
using Core.Client;
using Core.Map;
using Core.Utils;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;

namespace Core.Match.Client
{
    public class ClientMatchState
    {
        public CastleEntity OldMyCastle { get; set; }

        public CastleEntity OldEnemyCastle { get; set; }

        public MatchPlayer MyState { get; set; }

        public MatchPlayer EnemyState { get; set; }

        public bool IsMyTurn { get; set; }

        private List<Guid> draftedCards { get; set; } = new();

        public IReadOnlyList<Guid> DraftedCards => draftedCards.AsReadOnly();

        [CanBeNull] public Guid[] CardsInHandIds { get; set; }

        [CanBeNull] public Fatigue Fatigue { get; set; }

        [CanBeNull] public LevelInfo LevelInfo { get; set; }

        public delegate void ClientMatchStateChangeHandler(ClientMatchState state, CastleEntity newMyCastle,
            CastleEntity newEnemyCastle);

        public event ClientMatchStateChangeHandler OnStateChanged;

        public ClientMatchState()
        {
            Reset();
        }

        public void ApplyChanges()
        {
            OnStateChanged?.Invoke(this, new CastleEntity(MyState.Castle), new CastleEntity(EnemyState.Castle));
        }

        public void Reset()
        {
            MyState = new MatchPlayer()
            {
                Name = "Unknown",
                Castle = new BlankCastleCreator().CreateCastle()
            };
            EnemyState = new MatchPlayer()
            {
                Name = "Unknown",
                Castle = new BlankCastleCreator().CreateCastle()
            };
            OldMyCastle = new CastleEntity(MyState.Castle);
            OldEnemyCastle = new CastleEntity(EnemyState.Castle);

            IsMyTurn = false;
            CardsInHandIds = Array.Empty<Guid>();
            Fatigue = null;
            LevelInfo = null;
            OnStateChanged?.Invoke(this, new CastleEntity(MyState.Castle), new CastleEntity(EnemyState.Castle));
        }

        public void RollbackCastles()
        {
            if (OldEnemyCastle != null)
                EnemyState.Castle = OldEnemyCastle;
            if (OldMyCastle != null)
                MyState.Castle = OldMyCastle;
        }

        public void AddDraftedCard(Guid guid)
        {
            draftedCards.Add(guid);
            CardSpawner.SpawnDraftCard(guid);
        }

        public void RemoveDraftedCard(Guid id)
        {
            draftedCards.Remove(id);
        }
    }
}