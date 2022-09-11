using RedBjorn.ProtoTiles;
using RedBjorn.SuperTiles.Multiplayer;
using RedBjorn.SuperTiles.Squad;
using RedBjorn.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RedBjorn.SuperTiles.GameTypeCreators
{
    /// <summary>
    /// Creator which represents the way multiplayer game session will be created
    /// </summary>
    [Serializable]
    public class Multiplayer : IGameTypeCreator
    {
        List<string> AiNicknames = new List<string>();

        public IEnumerator Create(GameEntity game)
        {
            ///Find battle scene controller
            var battleView = UnityEngine.Object.FindObjectOfType<BattleView>();
            if (!battleView)
            {
                Log.E($"Can't create battle without {nameof(BattleView)} on scene");
                yield break;
            }

            game.Battle = new BattleEntity(); //Create state for current Battle
            game.Battle.Game = game;
            var sessionGo = new GameObject("GameSession");
            var session = sessionGo.AddComponent<GameSessionView>();
            session.Init(game);
            var turnPlayer = new NetworkTurnPlayer(game.Battle); //Create network turn player
            var networkSessionGo = new GameObject("NetworkGameSession"); //Create gameobject to receive Unity callbacks
            var networkSession = networkSessionGo.AddComponent<NetworkGameSessionView>();
            networkSession.Init(battleView);
            game.Battle.TurnPlayer = turnPlayer;
            var level = game.Level;
            game.Battle.Level = level;
            var mapVisual = UnityEngine.Object.FindObjectOfType<MapView>();
            var map = level.Map;
            game.Battle.Map = map.CreateEntity(mapVisual); //Create state for MapSettings
            if (mapVisual != null)
            {
                mapVisual.Init(game.Battle.Map);
            }
            else
            {
                Log.W($"Can't find {nameof(MapView)}. Some map functions will be disabled");
            }

            //Find all unit spawnpoints and set order from leftmost bottom point to rightmost top point to have certain order
            var spawns = UnityEngine.Object.FindObjectsOfType<UnitSpawnPoint>().OrderBy(s => s.transform.position.x * 10000 + s.transform.position.y);

            //Create Ai and Player controllers
            var controllerOwners = new List<SquadControllerEntity>();
            game.Battle.Players = new List<SquadControllerEntity>();
            var roomData = NetworkController.GetCurrentRoom();
            var playersData = roomData.Slots;
            var idIndex = 1;
            for (int i = 0; i < playersData.Count; i++)
            {
                SquadControllerEntity player = null;

                if (playersData[i].Type == SquadControllerType.Player)
                {
                    //Find network player for current slot
                    var networkPlayer = NetworkController.RoomSlotOwner(i);
                    if (networkPlayer != null)
                    {
                        var id = networkPlayer.Id;
                        player = new PlayerEntity(id, playersData[i].Team, networkPlayer.Nickname, game);
                        if (id == NetworkController.Id)
                        {
                            controllerOwners.Add(player);
                        }
                    }
                }

                if (playersData[i].Type == SquadControllerType.AI)
                {
                    var id = idIndex;
                    idIndex++;
                    player = new AiEntity(id, playersData[i].Team, GetAiNickname(id), game);
                    turnPlayer.LocalPlayerAdd(player);
                }

                //Fallback squad controller entity creation
                if (player == null && playersData[i].Type != SquadControllerType.None)
                {
                    var id = idIndex;
                    idIndex++;

                    Log.W("Wasn't create approporiate player. Will be replaced by Ai");
                    player = new AiEntity(id, playersData[i].Team, GetAiNickname(id), game);
                    turnPlayer.LocalPlayerAdd(player);
                }

                if (player != null)
                {
                    game.Battle.Players.Add(player);
                }
            }

            //Create units from already found spawnpoints
            foreach (var sp in spawns)
            {
                var controlEntity = game.Battle.Players.FirstOrDefault(p => p.Team == sp.Team);
                if (controlEntity != null)
                {
                    var unit = new UnitEntity(game.Battle.UnitId, sp.transform.position, sp.transform.rotation, sp.Data, game);
                    game.Battle.RegisterUnit(unit);
                    game.Battle.Map.RegisterUnit(unit);

                    controlEntity.AddUnit(unit);

                    //If unit belongs to team which is controlled by Ai then we create Ai logic for unit
                    if (controlEntity is AiEntity controlAi)
                    {
                        var ai = new UnitAiEntity() { Unit = unit, Data = sp.Ai };
                        controlAi.Bots.Add(ai);
                        game.Battle.RegisterUnitAi(ai);
                    }
                }
                else
                {
                    Log.W($"Can't find Player for spawn point {sp}");
                }
            }

            //Check Player squad valid condition
            for (int i = game.Battle.Players.Count - 1; i >= 0; i--)
            {
                var player = game.Battle.Players[i];
                if (player.Squad.Count == 0)
                {
                    Log.W($"Player {player} was created with empty squad");
                }
            }

            //Send root state object to view of BattleEntity
            battleView.Init(game, controllerOwners);

            //Create predefined main camera or change values of existed camera
            Camera camera = null;
            if (level.Camera.Prefab)
            {
                if (Camera.main)
                {
                    Spawner.Despawn(Camera.main.gameObject);
                }
                var cameraGo = Spawner.Spawn(level.Camera.Prefab);
                camera = cameraGo.GetComponent<Camera>();
                camera.tag = "MainCamera";
            }
            else
            {
                camera = Camera.main;
            }
            camera.transform.position = level.Camera.StartPosition;

            //Wait until loading proccess will finish
            while (SceneLoader.IsLoading)
            {
                yield return null;
            }

            //This client is ready
            NetworkController.SendBattleIsLoaded();

            //Wait untill all players will be ready
            var allLoaded = false;
            while (!allLoaded)
            {
                allLoaded = NetworkController.GetCurrentRoom().IsLoaded();
                yield return null;
            }

            //Remove loading screen
            SceneLoader.RemoveLoading();
        }

        string GetAiNickname(int id)
        {
            var nickname = $"AI-{id}";
            if (AiNicknames.Count == 0)
            {
                AiNicknames = new List<string>(S.Network.Ai.Nicknames);
            }
            if (AiNicknames.Count > 0)
            {
                nickname = $"{AiNicknames[0]} (AI)";
                AiNicknames.RemoveAt(0);
            }
            return nickname;
        }
    }
}
