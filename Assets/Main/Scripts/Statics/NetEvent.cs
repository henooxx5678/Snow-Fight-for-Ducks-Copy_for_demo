using Enum = System.Enum;
using Math = System.Math;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Pun;

using DoubleHeat;

namespace DoubleHeat.SnowFightForDucksGame {

    public enum PunRaiseEventCode {
        DONT_USE,

        InitRound,
        InitVoting,

        // == Round ==
        SetPlayerFacing,
        SetPlayerState,
        SetPlayerCarriedAmout,
        PlayerFiring,
        PlayerGetHit,
        SnowballRebound,
        SnowWallStartBuilding,
        SnowWallBuilt,
        StatueGetHit,
        StatueRepaired,
        SnowWallGetHit,
        UpdateStatuesData,
        UpdateSnowWallsData,

        // == Voting ==
        PlayerVote,
        Elected
    }


    // limit: playerNumber < 8, PlayerState Amount < 16, snowballIdByOwner < 16, SnowWall.FacingDirection Amount < 16, snowWallIdByOwner < 2^32
    public static class NetEvent {


        public enum PunRegion {
            Japan,
            Asia,
            SouthKorea
        }

        public static Dictionary<PunRegion, string> tokenOfRegions = new Dictionary<PunRegion, string>() {
            { PunRegion.Japan, "jp" },
            { PunRegion.Asia, "asia" },
            { PunRegion.SouthKorea, "kr" }
        };

        // public static AppSettings currentAppSettings = new AppSettings();


        // Players custom properties keys
        public static class PlayerCustomPropKeys {
            public const string DUCK_SKIN = "DS";
        }

        // Room custom properties keys
        public static class RoomCustomPropKeys {
            public const string PLAYERS_IN_SEAT_WHEN_PREPARING = "Seats";
        }


        // == Inits ==
        static int[,] GetEmptyPlayerInSeats () {
            int[,] emptySeats = new int[2, Global.PLAYERS_AMOUNT_LIMIT / 2];
            for (int team = 0 ; team < emptySeats.GetLength(0) ; team++) {
                for (int i = 0 ; i < emptySeats.GetLength(1) ; i++) {
                    emptySeats[team, i] = -2;
                }
            }
            return emptySeats;
        }

        public static void InitPlayerCustomProperties () {
            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() {
                { PlayerCustomPropKeys.DUCK_SKIN, (byte) PlayerPrefs.GetInt(Global.PrefKeys.DUCK_SKIN_INDEX) }
            });
        }

        public static Hashtable GetInitRoomCustomProperties () {
            return new Hashtable() {
                { RoomCustomPropKeys.PLAYERS_IN_SEAT_WHEN_PREPARING, PlayersInSeatsToRaw(GetEmptyPlayerInSeats()) }
            };
        }


        // == General ==
        public static void Connect () {

            PhotonNetwork.OfflineMode = false;

            if (PhotonNetwork.IsConnected) {

            }
            else {
                PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.GameVersion = Application.version;
            }
        }

        public static void Disconnect () {
            PhotonNetwork.Disconnect();
        }

        public static void GoOfflineMode () {
            PhotonNetwork.OfflineMode = true;
        }

        public static void CreateRoom () {
            string roomName = "sffd-room-" + System.Guid.NewGuid().ToString() + "-end";
            RoomOptions opts = new RoomOptions() {
                MaxPlayers = Global.PLAYERS_AMOUNT_LIMIT,
                CustomRoomProperties = NetEvent.GetInitRoomCustomProperties()
            };

            PhotonNetwork.CreateRoom(roomName, opts);
        }

        public static void JoinRoom (string roomName) {
            PhotonNetwork.JoinRoom(roomName);
        }

        public static void StartGame() {
            if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient) {

                PhotonNetwork.CurrentRoom.IsOpen = false;
                PhotonNetwork.CurrentRoom.IsVisible = false;

                PhotonNetwork.LoadLevel(Global.SceneNames.GAME);
            }
        }

        public static void LeaveRoom () {
            PhotonNetwork.LeaveRoom();
        }

        public static void KickPlayer (int playerNumber) {
            if (PhotonNetwork.IsMasterClient) {
                PhotonNetwork.CloseConnection(PhotonNetwork.CurrentRoom.Players[playerNumber]);
            }
        }


        // == Room ==
        // teams
        static int[,] ParseToPlayersInSeats (int[] raw) {
            int[,] result = new int[2, raw.Length / 2];

            for (int i = 0 ; i < result.GetLength(0) ; i++) {
                for (int j = 0 ; j < result.GetLength(1) ; j++) {
                    result[i, j] = raw[i * result.GetLength(1) + j];
                }
            }
            return result;
        }

        static int[] PlayersInSeatsToRaw (int[,] playerInSeats) {
            int[] raw = new int[playerInSeats.GetLength(0) * playerInSeats.GetLength(1)];

            for (int i = 0 ; i < playerInSeats.GetLength(0) ; i++) {
                for (int j = 0 ; j < playerInSeats.GetLength(1) ; j++) {
                    raw[i * playerInSeats.GetLength(1) + j] = playerInSeats[i, j];
                }
            }
            return raw;
        }

        public static int[,] GetCurrentPlayerInSeats () {
            object prop = PhotonNetwork.CurrentRoom.CustomProperties[RoomCustomPropKeys.PLAYERS_IN_SEAT_WHEN_PREPARING];
            if (prop != null) {
                return ParseToPlayersInSeats((int[]) prop);
            }
            return GetEmptyPlayerInSeats();
        }

        public static void RemoveLeftPlayersFromSeat () {
            Debug.Log("remove left players from seat");

            int[,] currentPlayerInSeats = GetCurrentPlayerInSeats();

            int[] playersNumber = PhotonNetwork.CurrentRoom.Players.Keys.ToArray();

            for (byte team = 0 ; team < currentPlayerInSeats.GetLength(0) ; team++) {
                for (int i = 0 ; i < currentPlayerInSeats.GetLength(1) ; i++) {

                    if (!playersNumber.Contains(currentPlayerInSeats[team, i]))
                        currentPlayerInSeats[team, i] = -2;
                }
            }

            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() {
                { RoomCustomPropKeys.PLAYERS_IN_SEAT_WHEN_PREPARING, PlayersInSeatsToRaw(currentPlayerInSeats) }
            });
        }

        public static int[] GetPlayersCountOfTeams (int[,] currentPlayerInSeats) {

            int[] playersCountOfTeams = new int[currentPlayerInSeats.GetLength(0)];

            for (byte team = 0 ; team < playersCountOfTeams.GetLength(0) ; team++) {
                playersCountOfTeams[team] = 0;

                for (int i = 0 ; i < currentPlayerInSeats.GetLength(1) ; i++) {
                    if (currentPlayerInSeats[team, i] != -2) {
                        playersCountOfTeams[team]++;
                    }
                }
            }
            return playersCountOfTeams;
        }

        public static byte GetPlayerTeam (int[,] currentPlayerInSeats, int playerNumber) {
            for (byte team = 0 ; team < currentPlayerInSeats.GetLength(0) ; team++) {
                for (int i = 0 ; i < currentPlayerInSeats.GetLength(1) ; i++) {
                    if (currentPlayerInSeats[team, i] == playerNumber) {
                        return team;
                    }
                }
            }
            return 255;
        }

        public static byte[] GetPlayerSeat (int[,] currentPlayerInSeats, int playerNumber) {
            for (byte team = 0 ; team < currentPlayerInSeats.GetLength(0) ; team++) {
                for (int i = 0 ; i < currentPlayerInSeats.GetLength(1) ; i++) {
                    if (currentPlayerInSeats[team, i] == playerNumber) {
                        return new byte[] {team, (byte) i};
                    }
                }
            }
            return new byte[0];
        }

        public static void ChangePlayerTeam (int[,] currentPlayerInSeats, int playerNumber, byte team) {

            // remove player from original seat
            for (int teamIndex = 0 ; teamIndex < currentPlayerInSeats.GetLength(0) ; teamIndex++) {
                for (int i = 0 ; i < currentPlayerInSeats.GetLength(1) ; i++) {
                    if (currentPlayerInSeats[teamIndex, i] == playerNumber) {
                        currentPlayerInSeats[teamIndex, i] = -2;
                    }
                }
            }

            // find and put player into new seat
            for (int i = 0 ; i < currentPlayerInSeats.GetLength(1) ; i++) {
                if (currentPlayerInSeats[team, i] == -2) {
                    currentPlayerInSeats[team, i] = playerNumber;
                    break;
                }
            }

            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() {
                { RoomCustomPropKeys.PLAYERS_IN_SEAT_WHEN_PREPARING, PlayersInSeatsToRaw(currentPlayerInSeats) }
            });
        }

        // local player
        public static void SetInitTeamForJoinedLocalPlayer () {
            Debug.Log("Set Initital Team For Joined Local Player");

            int[,] currentPlayerInSeats = GetCurrentPlayerInSeats();

            int[] playersCountOfTeams = GetPlayersCountOfTeams(currentPlayerInSeats);
            byte setTeam = 255;

            if (playersCountOfTeams[0] < playersCountOfTeams[1])
                setTeam = 0;
            else if (playersCountOfTeams[1] > playersCountOfTeams[0])
                setTeam = 1;
            else
                if (playersCountOfTeams[0] < currentPlayerInSeats.GetLength(1) && playersCountOfTeams[1] < currentPlayerInSeats.GetLength(1))
                    setTeam = (byte) Random.Range(0, 2);

            ChangePlayerTeam(currentPlayerInSeats, PhotonNetwork.LocalPlayer.ActorNumber, setTeam);
        }

        public static void LocalPlayerTryToSwitchTeam () {
            byte currentTeam = GetPlayerTeam(GetCurrentPlayerInSeats(), PhotonNetwork.LocalPlayer.ActorNumber);
            if (currentTeam == 255)
                return ;
            byte otherTeam = currentTeam == 0 ? (byte) 1 : (byte) 0;
            LocalPlayerTryToChangeToTeam(otherTeam);
        }

        public static void LocalPlayerTryToChangeToTeam (byte team) {
            int[,] currentPlayerInSeats = GetCurrentPlayerInSeats();

            int[] playersCountOfTeams = GetPlayersCountOfTeams(currentPlayerInSeats);

            if (playersCountOfTeams[team] < currentPlayerInSeats.GetLength(1)) {
                ChangePlayerTeam(currentPlayerInSeats, PhotonNetwork.LocalPlayer.ActorNumber, team);
            }
        }

        public static byte GetLocalPlayerTeam () {
            return GetPlayerTeam(GetCurrentPlayerInSeats(), PhotonNetwork.LocalPlayer.ActorNumber);
        }

        public static int GetLocalPlayerTeamCountOfMembers () {
            int[,] currentPlayerInSeats = GetCurrentPlayerInSeats();

            byte localPlayerTeam = GetPlayerTeam(currentPlayerInSeats, PhotonNetwork.LocalPlayer.ActorNumber);
            return GetPlayersCountOfTeams(currentPlayerInSeats)[localPlayerTeam];
        }


        // duck skins
        public static void SetLocalPlayerDuckSkin (DuckSkin skin) {
            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable() {
                { PlayerCustomPropKeys.DUCK_SKIN, (byte) skin }
            });
        }

        public static DuckSkin GetPlayerDuckSkin (int playerNumber) {
            Player player;
            if (PhotonNetwork.CurrentRoom.Players.TryGetValue(playerNumber, out player)) {
                if (player.CustomProperties[PlayerCustomPropKeys.DUCK_SKIN] != null) {
                    return (DuckSkin) (byte) PhotonNetwork.CurrentRoom.Players[playerNumber].CustomProperties[PlayerCustomPropKeys.DUCK_SKIN];
                }
            }
            return DuckSkin.Yellow;
        }

        // == Round ==
        // === RaiseEvents ===
        public static void EmitInitRoundEvent (OptionalRule[] activeRules, int roundInitTimestamp) {

            object[] content = new object[] {
                OptionalRulesToByteArray(activeRules),
                roundInitTimestamp
            };

            EmitEventToAll(PunRaiseEventCode.InitRound, content);
        }

        public static void EmitInitVotingEvent (OptionalRule[] candidates, int timestamp) {

            object[] content = new object[] {
                OptionalRulesToByteArray(candidates),
                timestamp
            };

            EmitEventToAll(PunRaiseEventCode.InitVoting, content);
        }


        public static void EmitSetPlayerStateEvent (int playerNumber, PlayerState state, bool isFacingRight) {

            byte data = (byte) (playerNumber + (isFacingRight ? 1 : 0) * 8 + (byte) state * 16);

            object[] content = new object[] {
                playerNumber,
                (byte) state,
                isFacingRight
            };

            EmitEventToOthers(PunRaiseEventCode.SetPlayerState, content);
        }

        public static void EmitSetPlayerCarriedAmountEvent (int playerNumber, int carriedAmount) {

            object[] content = new object[] {
                playerNumber,
                carriedAmount
            };

            EmitEventToOthers(PunRaiseEventCode.SetPlayerCarriedAmout, content);
        }

        public static void EmitPlayerFiringEvent (int playerNumber, Vector2 firePos, Vector2 fireDir, float chargingRate = 1f, bool isCurveBallDirRight = true) {

            PhotonNetwork.FetchServerTimestamp();

            object[] content = new object[] {
                PhotonNetwork.ServerTimestamp,
                playerNumber,
                firePos,
                DataCompression.Direction2DToAngleDegree(fireDir),
                chargingRate,
                isCurveBallDirRight
            };

            EmitEventToAll(PunRaiseEventCode.PlayerFiring, content);
        }

        public static void EmitPlayerGetHitEvent (int playerNumber, Vector2 playerPosition, int snowballOwnerNumber, int snowballIdByOwner) {

            PhotonNetwork.FetchServerTimestamp();

            object[] content = new object[] {
                PhotonNetwork.ServerTimestamp,
                playerNumber,
                playerPosition,
                snowballOwnerNumber,
                snowballIdByOwner
            };

            EmitEventToOthers(PunRaiseEventCode.PlayerGetHit, content);
        }

        public static void EmitSnowballReboundEvent (int ownerNumber, int idByOwner, Vector2 pos, Vector2 newDir) {

            PhotonNetwork.FetchServerTimestamp();

            object[] content = new object[] {
                PhotonNetwork.ServerTimestamp,
                ownerNumber,
                idByOwner,
                pos,
                DataCompression.Direction2DToAngleDegree(newDir)
            };

            EmitEventToOthers(PunRaiseEventCode.SnowballRebound, content);
        }

        public static void EmitSnowWallStartBuildingEvent (int playerNumber, SnowWall.FacingDirection facing) {

            object[] content = new object[] {
                playerNumber,
                (byte) facing
            };

            EmitEventToAll(PunRaiseEventCode.SnowWallStartBuilding, content);
        }

        public static void EmitSnowWallBuilt (int playerNumber, Vector2 position, SnowWall.FacingDirection facing) {

            object[] content = new object[] {
                playerNumber,
                position,
                (byte) facing
            };

            EmitEventToAll(PunRaiseEventCode.SnowWallBuilt, content);
        }

        public static void EmitStatueGetHitEvent (int statueNumber, int snowballOwnerNumber, int snowballIdByOwner) {

            PhotonNetwork.FetchServerTimestamp();

            byte compoundData = (byte) (snowballOwnerNumber + snowballIdByOwner * 16);

            object[] content = new object[] {
                PhotonNetwork.ServerTimestamp,
                statueNumber,
                snowballOwnerNumber,
                snowballIdByOwner
            };

            EmitEventToAll(PunRaiseEventCode.StatueGetHit, content);
        }

        public static void EmitStatueRepairedEvent (int playerNumber, int statueNumber) {

            PhotonNetwork.FetchServerTimestamp();

            object[] content = new object[] {
                PhotonNetwork.ServerTimestamp,
                playerNumber,
                statueNumber
            };

            EmitEventToOthers(PunRaiseEventCode.StatueRepaired, content);
        }

        public static void EmitSnowWallGetHitEvent (int snowWallOwnerNumber, int snowWallIdByOwner, int snowballOwnerNumber, int snowballIdByOwner) {

            PhotonNetwork.FetchServerTimestamp();

            object[] content = new object[] {
                PhotonNetwork.ServerTimestamp,
                snowWallOwnerNumber,
                snowWallIdByOwner,
                snowballOwnerNumber,
                snowballIdByOwner
            };

            EmitEventToAll(PunRaiseEventCode.SnowWallGetHit, content);
        }

        public static void EmitUpdateStatuesHP (int[] statuesHP) {

            object[] content = new object[] {
                statuesHP
            };

            EmitEventToOthers(PunRaiseEventCode.UpdateStatuesData, content);
        }

        public static void EmitUpdateSnowWallsData (SnowWall.NetProperties[] propss) {

            int[]     ownersNumber;
            int[]     idsByOwner;
            Vector2[] positions;
            int[]     hps;

            if (SnowWall.NetProperties.UnpackArray(propss, out ownersNumber, out idsByOwner, out positions, out hps)) {

                object[] content = new object[] {
                    ownersNumber,
                    idsByOwner,
                    positions,
                    hps
                };

                EmitEventToOthers(PunRaiseEventCode.UpdateSnowWallsData, content);
            }
        }



        // --- Vote ---
        public static void EmitPlayerVoteEvent (int playerNumber, int candidateIndex) {

            object[] content = new object[] {
                playerNumber,
                candidateIndex
            };

            EmitEventToAll(PunRaiseEventCode.PlayerVote, content);
        }

        public static void EmitElectedEvent (int electedIndex) {

            object[] content = new object[] {
                electedIndex
            };

            EmitEventToOthers(PunRaiseEventCode.Elected, content);
        }





        public static byte[] OptionalRulesToByteArray (OptionalRule[] rules) {
            byte[] byteFormatRules = new byte[rules.Length];
            for (int i = 0 ; i < byteFormatRules.Length ; i++) {
                byteFormatRules[i] = (byte) rules[i];
            }
            return byteFormatRules;
        }

        public static OptionalRule[] ByteArrayToOptionalRules (byte[] byteFormatRules) {
            OptionalRule[] rules = new OptionalRule[byteFormatRules.Length];
            for (int i = 0 ; i < rules.Length ; i++) {
                rules[i] = (OptionalRule) byteFormatRules[i];
            }
            return rules;
        }

        static void EmitEventToOthers (PunRaiseEventCode eventCode, object[] content) {
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
            PhotonNetwork.RaiseEvent((byte) eventCode, content, raiseEventOptions, SendOptions.SendReliable);
        }

        static void EmitEventToAll (PunRaiseEventCode eventCode, object[] content) {
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent((byte) eventCode, content, raiseEventOptions, SendOptions.SendReliable);
        }

    }
}
