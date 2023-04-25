using System;
using System.Collections.Generic;
using Core;
using Core.Castle;
using Core.Contracts;
using Core.Economics;
using Core.Map;
using Newtonsoft.Json;
using UnityEngine;

namespace Mirror.Extensions
{
    public static class ReaderWriterExtensions
    {
        public static void WriteAuthDto(this NetworkWriter writer, AuthDto authDto)
        {
            writer.WriteString(authDto.Login);
            writer.WriteString(authDto.Name);
            writer.WriteString(authDto.Password);
            writer.WriteString(authDto.LastLoginTime);
            writer.WriteBool(authDto.IsGuest);
        }

        public static AuthDto ReadAuthDto(this NetworkReader reader)
        {
            return new AuthDto
            {
                Login = reader.ReadString(),
                Name = reader.ReadString(),
                Password = reader.ReadString(),
                LastLoginTime = reader.ReadString(),
                IsGuest = reader.ReadBool(),
            };
        }

        public static void WriteAccountDto(this NetworkWriter writer, AccountDto accountDto)
        {
            writer.WriteGuid(accountDto.Id);
            writer.WriteString(accountDto.Login);
            writer.WriteBool(accountDto.IsGuest);
        }

        public static AccountDto ReadAccountDto(this NetworkReader reader)
        {
            return new AccountDto
            {
                Id = reader.ReadGuid(),
                Login = reader.ReadString(),
                IsGuest = reader.ReadBool()
            };
        }

        public static void WriteRequestMatchDto(this NetworkWriter writer, RequestMatchDto requestMatchDto)
        {
            writer.WriteGuid(requestMatchDto.AccountId);
            writer.WriteInt((int)requestMatchDto.RequestType);
            writer.WriteInt(requestMatchDto.LevelId);
        }

        public static RequestMatchDto ReadRequestMatchDto(this NetworkReader reader)
        {
            return new RequestMatchDto
            {
                AccountId = reader.ReadGuid(),
                RequestType = (MatchRequestType)reader.ReadInt(),
                LevelId = reader.ReadInt()
            };
        }

        public static void WriteRequestCardDto(this NetworkWriter writer, RequestCardDto requestCardDto)
        {
            writer.WriteGuid(requestCardDto.AccountId);
            writer.WriteGuid(requestCardDto.CardId);
            writer.WriteInt((int)requestCardDto.ActionType);
        }

        public static RequestCardDto ReadRequestCardDto(this NetworkReader reader)
        {
            return new RequestCardDto
            {
                AccountId = reader.ReadGuid(),
                CardId = reader.ReadGuid(),
                ActionType = (CardActionType)reader.ReadInt()
            };
        }

        public static void WriteRequestBattleInfo(this NetworkWriter writer, RequestBattleInfo requestBattleInfo)
        {
            writer.WriteGuid(requestBattleInfo.AccountId);
            writer.WriteString(requestBattleInfo.EnemyName);
            writer.WriteString(requestBattleInfo.YourName);
            writer.WriteInt(requestBattleInfo.YourTowerHealth);
            writer.WriteInt(requestBattleInfo.EnemyTowerHealth);
            writer.WriteInt(requestBattleInfo.YourWallHealth);
            writer.WriteInt(requestBattleInfo.EnemyWallHealth);
            writer.WriteBool(requestBattleInfo.IsYourTurn);
            writer.WriteInt(requestBattleInfo.Timer);
            writer.WriteInt(requestBattleInfo.TurnFatigue);
            writer.WriteInt(requestBattleInfo.StartDamageFatigue);
            writer.WriteInt(requestBattleInfo.FatigueLimit);
            writer.WriteInt(requestBattleInfo.EnemyWinCount);
            writer.WriteInt(requestBattleInfo.Division);
        }

        public static RequestBattleInfo ReadRequestBattleInfo(this NetworkReader reader)
        {
            return new RequestBattleInfo
            {
                AccountId = reader.ReadGuid(),
                EnemyName = reader.ReadString(),
                YourName = reader.ReadString(),
                YourTowerHealth = reader.ReadInt(),
                EnemyTowerHealth = reader.ReadInt(),
                YourWallHealth = reader.ReadInt(),
                EnemyWallHealth = reader.ReadInt(),
                IsYourTurn = reader.ReadBool(),
                Timer = reader.ReadInt(),
                TurnFatigue = reader.ReadInt(),
                StartDamageFatigue = reader.ReadInt(),
                FatigueLimit = reader.ReadInt(),
                EnemyWinCount = reader.ReadInt(),
                Division = reader.ReadInt(),
            };
        }

        public static void WritePinger(this NetworkWriter writer, Pinger pinger)
        {
            writer.WriteGuid(pinger.AccountId);
        }

        public static Pinger ReadPinger(this NetworkReader reader)
        {
            return new Pinger
            {
                AccountId = reader.ReadGuid()
            };
        }

        public static void WriteMapProgressDto(this NetworkWriter writer, MapProgressDto mapProgressDto)
        {
            writer.WriteString(mapProgressDto.PlayFabId);
            writer.WriteString(JsonConvert.SerializeObject(mapProgressDto.Progress));
            writer.WriteBool(mapProgressDto.IsError);
        }

        public static MapProgressDto ReadMapProgressDto(this NetworkReader reader)
        {
            return new MapProgressDto()
            {
                PlayFabId = reader.ReadString(),
                Progress = JsonConvert.DeserializeObject<MapProgress>(reader.ReadString()),
                IsError = reader.ReadBool()
            };
        }
        
        public static void WriteMatchDetailsDto(this NetworkWriter writer, MatchDetailsDto details)
        {
            writer.WriteString(details.PlayerId);
            writer.WriteArray(details.Players);
            writer.WriteString(JsonConvert.SerializeObject(details.Fatigue));
            writer.WriteArray(details.CardsInHandIds);
            writer.WriteString(JsonConvert.SerializeObject(details.LevelInfo));
            writer.WriteBool(details.IsYourTurn);
        }

        public static MatchDetailsDto ReadMatchDetailsDto(this NetworkReader reader)
        {
            return new()
            {
                PlayerId = reader.ReadString(),
                Players = reader.ReadArray<MatchPlayerDto>(),
                Fatigue = JsonConvert.DeserializeObject<Fatigue>(reader.ReadString()),
                CardsInHandIds = reader.ReadArray<string>(),
                LevelInfo = JsonConvert.DeserializeObject<LevelInfo>(reader.ReadString()),
                IsYourTurn = reader.ReadBool()
            };
        }
        
        
        public static void WriteMatchPlayerDto(this NetworkWriter writer, MatchPlayerDto player)
        {
            writer.WriteString(player.PlayerId);
            writer.WriteString(player.Name);
            writer.WriteString(JsonConvert.SerializeObject(player.Castle));
        }

        public static MatchPlayerDto ReadMatchPlayerDto(this NetworkReader reader)
        {
            return new()
            {
                PlayerId = reader.ReadString(),
                Name = reader.ReadString(),
                Castle = JsonConvert.DeserializeObject<CastleEntity>(reader.ReadString())
            };
        }

        public static void WriteFatigueDto(this NetworkWriter writer, FatigueDto dto)
        {
            writer.WriteGuid(dto.PlayerId);
            writer.WriteInt(dto.Damage);
        }

        public static FatigueDto ReadFatigueDto(this NetworkReader reader)
        {
            return new()
            {
                PlayerId = reader.ReadGuid(),
                Damage = reader.ReadInt()
            };
        }

        public static void WriteLoadBattleSceneDto(this NetworkWriter writer, LoadBattleSceneDto dto)
        {
            writer.WriteString(dto.MatchId);
            writer.WriteString(dto.RequestId);
        }

        public static LoadBattleSceneDto ReadLoadBattleSceneDto(this NetworkReader reader)
        {
            return new()
            {
                MatchId = reader.ReadString(),
                RequestId = reader.ReadString(),
            };
        }

        public static void WriteWalletDto(this NetworkWriter writer, WalletDto dto)
        {
            writer.WriteString(dto.PlayFabId);
            writer.WriteString(JsonConvert.SerializeObject(dto.Balance));
            writer.WriteInt((int)dto.RequestType);
        }

        public static WalletDto ReadWalletDto(this NetworkReader reaader)
        {
            return new WalletDto()
            {
                PlayFabId = reaader.ReadString(),
                Balance = JsonConvert.DeserializeObject<Dictionary<string, Currency>>(reaader.ReadString()),
                RequestType = (WalletRequestType)reaader.ReadInt()
            };
        }
    }
}