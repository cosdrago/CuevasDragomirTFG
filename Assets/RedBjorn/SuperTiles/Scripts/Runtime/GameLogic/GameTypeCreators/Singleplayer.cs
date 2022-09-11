using RedBjorn.ProtoTiles;
using RedBjorn.SuperTiles.Squad;
using RedBjorn.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RedBjorn.SuperTiles.GameTypeCreators
{
    [Serializable]
    public class Singleplayer : IGameTypeCreator
    {
        public IEnumerator Create(GameEntity game)
        {
            var battleView = GameObject.FindObjectOfType<BattleView>();
            if (!battleView)
            {
                Log.E($"Can't create battle without {nameof(BattleView)} on scene");
                yield break;
            }

            game.Battle = new BattleEntity(); //Create state for current Battle
            game.Battle.Game = game;
            game.Battle.Level = game.Level;
            game.Battle.TurnPlayer = new TurnPlayer(game.Battle);
            var sessionGo = new GameObject("GameSession");
            var session = sessionGo.AddComponent<GameSessionView>();
            session.Init(game);
            var level = game.Level;
            var map = level.Map;
            var mapVisual = GameObject.FindObjectOfType<MapView>();
            //Create state for MapSettings
            game.Battle.Map = map.CreateEntity(mapVisual);
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
            var playersData = level.Players;
            for (int i = 0; i < playersData.Count; i++)
            {
                var id = i + 1;
                var data = playersData[i];
                SquadControllerEntity player = null;
                if (data.ControlledBy == SquadControllerType.AI)
                {
                    player = new AiEntity(id, data.Team, data.Name, game);
                }
                else if (data.ControlledBy == SquadControllerType.Player)
                {
                    player = new PlayerEntity(id, data.Team, data.Name, game);
                    controllerOwners.Add(player);
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

            //Wait until loading screen will disable
            while (SceneLoader.IsLoading)
            {
                yield return null;
            }
            SceneLoader.RemoveLoading();
        }
    }
}