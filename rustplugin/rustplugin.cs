

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Oxide.Ext.Discord;
using Oxide.Ext.Discord.Attributes;
using Oxide.Ext.Discord.Builders;
using Oxide.Ext.Discord.Constants;
using Oxide.Ext.Discord.Entities.Gatway;
using Oxide.Ext.Discord.Entities.Gatway.Events;
using Oxide.Ext.Discord.Logging;
using System;
using System.Net;
using System.Text;
using System.Timers;
using System.Collections.Generic;
using Oxide.Game.Rust.Cui;
using UnityEngine;
using MySql.Data.MySqlClient;
using Oxide.Ext.Discord.Helpers.Cdn;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq;
using ConVar;

namespace Oxide.Plugins
{
    [Info("rust/discord", "Caloric", "0.0.1")]
    internal class rustplugin : RustPlugin
    {
        private const string ADMIN_PERMISSION = "rustplugin.admin";
        private const string USER_USE_PERMISSION = "rustplugin.use";

        #region Class Field
        //config
        private ConfigData configData;
        private StringBuilder sb = new StringBuilder();
        private MySqlConnection conn;

        #endregion

        #region Mysql
        class mysql
        {
            public string server = "SERVER_NAME";
            public string server_dataBase = "SERVER_DATA";
            public string server_user = "SERVER_USER";
            public string server_pass = "SERVER_Pass";
            public string server_port = "00000";
        }
        #endregion

        #region config
        class ConfigData
        {
            [JsonProperty(PropertyName = "Mysql Info")]
            public mysql sql = new mysql();

            [JsonProperty(PropertyName = "Reply Message")]
            public string rep = "Fique por dentro das regras do servidor dando /rules \nFique por dentro de todos os comando dando /help \nObrigatorio esta no discord do servidor https://discord.gg/ptkeNWxCFA";

            [JsonProperty(PropertyName = "time")]
            public int time = 600000;

            [JsonProperty(PropertyName = "Rules list:")]
            public string[] serverCommands = new string[]
                {
                    "1. Respeite todos os membros do servidor.",
                    "3. Sem racismo ou coisas do tipo.",
                    "4. Não e permitido espamar no chat.",
                    "5. Cheats, Macros ou qual quer coisa que altere a mira ou a visão do mapa detectado ou pego por um moderador resultara em banimento.",
                    "6. Obrigatorio estar no discord do servidor, caso sejá telado e não tenha o discord sera suspenso até o proximo wipe, reincidente sera banido(clan e amigos tambem).",
                    "7. para saber os comandos do servidor de /help",
                    "8. Entre no discord link em /discord ou https://discord.gg/ptkeNWxCFA"
                };

            [JsonProperty(PropertyName = "Rules Prefix:")]
            public string ChatPrefixRules = "<color=#32CD32>Server Rules: </color>";

        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            //Config.Settings.DefaultValueHandling = DefaultValueHandling.Populate;
            configData = Config.ReadObject<ConfigData>();  //Config.ReadObject<ConfigData>();

            if (configData == null)
                LoadDefaultConfig();

            SaveConfig();
        }

        private void SaveConfig() => Config.WriteObject(configData, true);
        protected override void LoadDefaultConfig()
        {
            Puts("Creating New Config File");
            configData = new ConfigData();
            SaveConfig();
        }

        void Init()
        {
            LoadConfig();

            var builder = new MySqlConnectionStringBuilder
            {
                Server = configData.sql.server,
                Database = configData.sql.server_dataBase,
                UserID = configData.sql.server_user,
                Password = configData.sql.server_pass,
                Port = uint.Parse(configData.sql.server_port)
            };

            conn = new MySqlConnection(builder.ConnectionString);

            //conn.Open();

            //connectMysql();

        }

        void OnServerInitialized()
        {
            permission.RegisterPermission(ADMIN_PERMISSION, this);
            permission.RegisterPermission(USER_USE_PERMISSION, this);


        }

        void Unload()
        {
            foreach (BasePlayer player in BasePlayer.activePlayerList)
            {
                CuiHelper.DestroyUi(player, "Nebulosa_LinkDiscord");
            }
            conn.Close();
        }

        #endregion

        #region ConsoleCommand

        [ConsoleCommand("ConfigTest")]
        void configTest(ConsoleSystem.Arg args)
        {
            Puts(configData.rep);
        }

        #endregion

        #region ChatCommand

        [ConsoleCommand("servermsg")]
        private void ServerMsgCommand(BasePlayer player, string command, string[] args)
        {
            if (!permission.UserHasPermission(player.UserIDString, ADMIN_PERMISSION))
            {
                if (args.Length == 0)
                {
                    SendReply(player, "Uso correto: /servermsg <mensagem>");
                    return;
                }

                string message = string.Join(" ", args);
                //SendServerMessage(message);
                Server.Broadcast(message);
            }
            else
            {
                SendReply(player, "Not have permission");
                return;
            }
        }

        #endregion

        #region playerEvent

        private void OnPlayerInit(BasePlayer player)
        {
            //PrintToChat($"Primeira vez do jogador {player.displayName} jogando no servidor.");
           // Puts($"Primeira vez do jogador {player.displayName} jogando no servidor.");

        }

        private void OnPlayerConnected(BasePlayer player)
        {
            if(conn.State == System.Data.ConnectionState.Closed)
                conn.Open();

            InsertUser(player);

            if (!checkUserAsync(player))
            {
                Puts("menu sucess created");
                menu(player);
            }

            if (conn.State == System.Data.ConnectionState.Open)
                conn.Close();
        }




        private void OnPlayerDisconnected(BasePlayer player, string reason)
        {
            // PrintToChat($"O jogador {player.displayName} saiu do servidor. Motivo: {reason}");
            // Puts($"O jogador {player.displayName} saiu do servidor. Motivo: {reason}");
            menu_delete(player);
        }

        private void OnPlayerDeath(BasePlayer player, HitInfo info)
        {
          
        }


        #endregion

        #region Functions







        #endregion


        string menu_message(BasePlayer player)
        {
            if (conn.State == System.Data.ConnectionState.Closed)
                conn.Open();

            // conn.Open();
            using (var command = conn.CreateCommand())
            {
                //connectMysql();
                command.CommandText = $"SELECT Token FROM `rust_discord` WHERE SteamId = '{player.userID}'";

                using (var reader = command.ExecuteReader())
                {
                    reader.Read();
                    return $"Entre no nosso discord https://discord.gg/vq9D4zG5uY e use esse token: {reader.GetString(0)} para linkar sua steam.";
                }
            }
        }

        [ChatCommand("menu")]
        void menu(BasePlayer player)
        {
            var container = new CuiElementContainer();

            var panel = container.Add(new CuiPanel
            {
                Image =
                {
                    Color = "0.1 0.1 0.1 0"
                },

                RectTransform = {
                    AnchorMin = "0.25 0.861",
                    AnchorMax = "0.771 1"
                },

                CursorEnabled = false

            }, "Overlay", "nebulosa");


            var text = container.Add(new CuiLabel
            {
                Text = 
                {
                    Color = "red",
                    Text = menu_message(player),
                    Align = TextAnchor.MiddleCenter,
                    FontSize = 15  
                },
                RectTransform =
                {
                    AnchorMin = "0.00 0.00",
                    AnchorMax = "1.00 1.00"
                }
            }, panel);

            CuiHelper.DestroyUi(player, "nebulosa");
            CuiHelper.AddUi(player, container);
        }

        [ChatCommand("delete")]
        void menu_delete(BasePlayer player)
        {
            CuiHelper.DestroyUi(player, "nebulosa");
        }

        [ChatCommand("linked")]
        void linked(BasePlayer player)
        {
            if (conn.State == System.Data.ConnectionState.Closed)
                conn.Open();

            if (checkUserAsync(player))
                CuiHelper.DestroyUi(player, "nebulosa");

            if (conn.State == System.Data.ConnectionState.Open)
                conn.Close();
        }

        private static System.Random random = new System.Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(System.Linq.Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        bool checkUserAsync(BasePlayer player)
        {
            using (var command = conn.CreateCommand())
            {
                //connectMysql();
                command.CommandText = $"SELECT EXISTS(SELECT bVerifield FROM `rust_discord` WHERE SteamId = '{player.userID}' AND bVerifield = 1)";

                using (var reader = command.ExecuteReader())
                {
                    reader.Read();
                    Puts($"resultado da busca: {reader.GetBoolean(0)}");
                    return reader.GetBoolean(0);
                }
            }
        }

        void InsertUser(BasePlayer player)
        {
            if (!checkUserAsync(player))
            {
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = $"INSERT INTO `rust_discord` ( `SteamId`, `DiscordID`, `Token` ) VALUES ( '{player.userID}', '{player.userID}', '{RandomString(6)}' )";
                    command.ExecuteNonQuery();
                }
            }
        }




    }
}