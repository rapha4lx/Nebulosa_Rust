

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
using Oxide.Game.Rust.Libraries;
using Unity.IO.LowLevel.Unsafe;
using System.Runtime;
using Oxide.Core;
using UnityEngine;


namespace Oxide.Plugins
{
    [Info("NebulosaCore", "Caloric", "0.0.1")]
    internal class NebulosaCore : RustPlugin
    {
        private const string ADMIN_PERMISSION = "NebulosaCore.admin";
        private const string USER_REGISTRED_USER_PERMISSION = "NebulosaCore.Registred";
        private const string USER_NOT_REGISTRED_PERMISSION = "NebulosaCore.NotRegistred";

        List<ulong> MenuOpen = new List<ulong>();


        #region Class Field
        //config
        private ConfigData configData;
        private StringBuilder sb = new StringBuilder();
        private MySqlConnection conn;

        #endregion

        #region Mysql
        class mysql
        {
            public string MysqlServerName = "";
            public string server = "";
            public string server_dataBase = "";
            public string server_user = "";
            public string server_pass = "";
            public string server_port = "";

            //public string MysqlServerName = "Nebulosa_Server_#1";
            //public string server = "br938.hostgator.com.br";
            //public string server_dataBase = "radiat52_nebulosa";
            //public string server_user = "radiat52_Monetary7800";
            //public string server_pass = "%3lA*BXQBdZWWRL2zqSCyREp9i&9nofN*N";
            //public string server_port = "3306";
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
        }

        void Unload()
        {
            foreach (BasePlayer player in BasePlayer.activePlayerList.Where(player => MenuOpen.Contains(player.userID)))
                DestroyMenu(player);
        }

        void OnServerInitialized()
        {
            permission.RegisterPermission(ADMIN_PERMISSION, this);
            permission.RegisterPermission(USER_REGISTRED_USER_PERMISSION, this);
            permission.RegisterPermission(USER_NOT_REGISTRED_PERMISSION, this);
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

        [ChatCommand("servermsg")]
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
                Server.Broadcast(message);
            }
            else
            {
                SendReply(player, "Not have permission");
                return;
            }
        }

        #region MenuConfiguration
        void CriarMenu(BasePlayer player)
        {
            var container = new CuiElementContainer();

            // Container principal
            var mainPanel = container.Add(new CuiPanel
            {
                Image = { Color = "0 0 0 0.5" },
                RectTransform = { AnchorMin = "0.1 0.1", AnchorMax = "0.9 0.9" },
                CursorEnabled = true
            }, "Overlay", "MeuMenu");

            // Botão de fechar
            container.Add(new CuiButton
            {
                Button = { Command = "fecharmenu", Color = "1 0 0 1" },
                RectTransform = { AnchorMin = "0.97 0.95", AnchorMax = "1 1" },
                Text = { Text = "X", FontSize = 18, Align = TextAnchor.MiddleCenter }
            }, mainPanel);

            {



            }


            // Adiciona o menu à tela do jogador
            CuiHelper.AddUi(player, container);
        }

        #endregion

        [ChatCommand("menu")]
        private void Menu(BasePlayer player, string cmd, string[] args)
        {
            CriarMenu(player);
            MenuOpen.Add(player.userID);
        }

        [ConsoleCommand("fecharmenu")]
        private void FecharMenu(ConsoleSystem.Arg arg)
        {
            if (arg.Player() == null) return;
            DestroyMenu(arg.Player());
        }

        void DestroyMenu(BasePlayer player)
        {
            // Lógica para fechar o menu
            CuiHelper.DestroyUi(player, "MeuMenu");
        }

        #endregion

        #region playerEvent

        #region Functions
        void CheckUserExist(string SteamID)
        {
            using (var readCommand = conn.CreateCommand())
            {
                readCommand.CommandText = $"SELECT `SteamID` FROM `Users` WHERE `SteamID` = '{SteamID}' ";
                if (readCommand.ExecuteScalar().IsUnityNull())
                {
                    using (var insertCommand = conn.CreateCommand())
                    {
                        insertCommand.CommandText = $"INSERT INTO `Users` (`SteamID`) VALUES ('{SteamID}')  ";
                        insertCommand.ExecuteNonQuery();
                    }
                }

                readCommand.CommandText = $"SELECT `SteamID` FROM `{configData.sql.MysqlServerName}` WHERE `SteamID` = '{SteamID}' ";
                if (readCommand.ExecuteScalar().IsUnityNull())
                {
                    using (var insertCommand = conn.CreateCommand())
                    {
                        insertCommand.CommandText = $"INSERT INTO `{configData.sql.MysqlServerName}` (`SteamID`) VALUES ('{SteamID}')  ";
                        insertCommand.ExecuteNonQuery();
                    }
                }
            }
        }

        bool CheckVipPermission(BasePlayer player, string VIP)
        {
            if (permission.UserHasGroup(player.UserIDString, VIP))
            {
                permission.RemoveUserGroup(player.UserIDString, VIP);
                PrintToChat(player, $"Seu VIP {VIP} expirou");

                using (var command = conn.CreateCommand())
                {
                    command.CommandText = $"DELETE FROM `Subs` WHERE `Days` <= 0 AND `SteamID` = '{player.UserIDString}' AND `VIP` = '{VIP}'";
                    command.ExecuteNonQuery();
                    return false;
                }
            }
            return true;
        }

        void CheckVip(BasePlayer player)
        {
            using (var readCommand = conn.CreateCommand())
            {
                readCommand.CommandText = $"SELECT `VIP`, `Days` FROM `Subs` WHERE `SteamID` = '{player.UserIDString}'";
                using (var reader = readCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if ((int)reader["Days"] <= 0)
                        {
                            if (!CheckVipPermission(player, (string)reader["VIP"]))
                            {
                                continue;
                            }
                        }

                        if (!permission.UserHasGroup(player.UserIDString, (string)reader["VIP"]))
                        {
                            permission.AddUserGroup(player.UserIDString, (string)reader["VIP"]);
                            PrintToChat(player, $"Voce foi adicionando no grupo {(string)reader["VIP"]}");
                        }
                    }
                }
            }
        }
        #endregion

        #region CanUserLogin
        [HookMethod("CanUserLogin")]
        private object CanUserLogin(string name, string id, string ipAddress)
        {
            if (name.ToLower().Contains("admin"))
            {
                Puts($"{name} ({id}) at {ipAddress} tried to connect with 'admin' in name");
                return "Sorry, your name cannot have 'admin' in it";
            }

            if (conn.State == System.Data.ConnectionState.Closed)
                conn.Open();
            try
            {
                CheckUserExist(id);
                Puts("Check Player Connect");
            }
            catch (MySqlException ex)
            {
                Puts($"Mysql error: {ex}");
            }
            finally
            {
                //if (conn.State == System.Data.ConnectionState.Open)
                //conn.Close();
            }
            return true;
        }
        #endregion

        #region OnPlayerConnected
        [HookMethod("OnPlayerConnected")]
        private void OnPlayerConnected(BasePlayer player)
        {
            if (conn.State == System.Data.ConnectionState.Closed)
                conn.Open();

            try
            {
                CheckVip(player);
                Puts($"user try connect {player.UserIDString}");
            }
            catch (MySqlException ex)
            {
                Puts($"Mysql error: {ex}");
            }
            finally
            {
                //if (conn.State == System.Data.ConnectionState.Open)
                //conn.Close();
            }
        }
        #endregion

        #region PlayerDeath
        void addPlayerDeath(string steamID)
        {
            using (var readCommand = conn.CreateCommand())
            {
                readCommand.CommandText = $"SELECT SteamID FROM `{configData.sql.MysqlServerName}` WHERE SteamID = '{steamID}'";

                if (readCommand.ExecuteScalar().IsUnityNull())
                {
                    using (var insertComman = conn.CreateCommand())
                    {
                        insertComman.CommandText = $"INSERT INTO `{configData.sql.MysqlServerName}`(`SteamID`, `Kills`, `Deaths`) VALUES ('{steamID}','0','0')";
                        insertComman.ExecuteNonQuery();
                    }
                }
                else
                {
                    using (var updateCommand = conn.CreateCommand())
                    {
                        updateCommand.CommandText = $"UPDATE `{configData.sql.MysqlServerName}` SET `Deaths` = (Deaths + 1) WHERE `SteamID` = '{steamID}' ";
                        updateCommand.ExecuteNonQuery();
                    }
                }
            }
        }

        void addPlayerKill(string steamID)
        {
            using (var readCommand = conn.CreateCommand())
            {
                readCommand.CommandText = $"SELECT SteamID FROM `{configData.sql.MysqlServerName}` WHERE SteamID = '{steamID}'";

                if (readCommand.ExecuteScalar().IsUnityNull())
                {
                    using (var insertComman = conn.CreateCommand())
                    {
                        insertComman.CommandText = $"INSERT INTO `{configData.sql.MysqlServerName}`(`SteamID`, `Kills`, `Deaths`) VALUES ('{steamID}','1','0')";
                        insertComman.ExecuteNonQuery();
                    }
                }
                else
                {
                    using (var updateCommand = conn.CreateCommand())
                    {
                        updateCommand.CommandText = $"UPDATE `{configData.sql.MysqlServerName}` SET `Kills` = (Kills + 1) WHERE `SteamID` = '{steamID}' ";
                        updateCommand.ExecuteNonQuery();
                    }
                }
            }
        }

        [HookMethod("OnPlayerDeath")]
        private void OnPlayerDeath(BaseCombatEntity player, HitInfo info)
        {
            if (player is BasePlayer && info.Initiator is BasePlayer)
            {
                BasePlayer morto = player.ToPlayer();
                BasePlayer assassino = info.Initiator.ToPlayer();

                if (morto.IsBot || assassino.IsBot)
                {
                    return;
                }

                if (morto != null && assassino != null)
                {
                    try
                    {
                        if (conn.State == System.Data.ConnectionState.Closed)
                            conn.Open();

                        addPlayerDeath(morto.UserIDString);
                        addPlayerKill(assassino.UserIDString);
                    }
                    catch (MySqlException ex)
                    {
                        Puts($"Mysql error: {ex}");
                    }
                    finally
                    {
                        //if (conn.State == System.Data.ConnectionState.Open)
                        //conn.Close();
                    }
                }

                if (MenuOpen.Contains(morto.userID))
                {
                    DestroyMenu(morto);
                }
            }
        }

        #endregion

        #region OnPlayerDisconnected
        [HookMethod("OnPlayerDisconnected")]
        private void OnPlayerDisconnected(BasePlayer player)
        {
            if (MenuOpen.Contains(player.userID))
                DestroyMenu(player);
        }
        #endregion


        #endregion

        private static System.Random random = new System.Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(System.Linq.Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}