

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
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using static System.Net.Mime.MediaTypeNames;
using System.Numerics;
using System.Net.NetworkInformation;
using System.Reflection.Emit;
using System.Drawing;



namespace Oxide.Plugins
{
    [Info("NebulosaCore", "Caloric", "0.0.1")]
    internal class NebulosaCore : RustPlugin
    {
        private const string ADMIN_PERMISSION = "NebulosaCore.admin";
        private const string USER_REGISTRED_USER_PERMISSION = "NebulosaCore.Registred";
        private const string USER_NOT_REGISTRED_PERMISSION = "NebulosaCore.NotRegistred";

        //dependcs
        [PluginReference]
        private Plugin ImageLibrary;

        #region ImageLibrary Functions
        private void RegisterImage(string name, string url) => ImageLibrary?.Call("AddImage", url, name.Replace(" ", ""), 0UL, null);
        private string? GetImage(string name, ulong skinId = 0UL) => ImageLibrary?.Call<string>("GetImage", name.Replace(" ", ""), skinId, false);
        #endregion

        class MenuPlayerClass
        {
            public int page = 0;
            public int menu = 0;
            public int money = 0;
            public string discord = "";
            public BasePlayer? player;
        };
        List<MenuPlayerClass> MenuOpen = new List<MenuPlayerClass>();

        class Shop
        {
            public string? Name;
            public int Value;
            public int Amount;
            public bool Buyable;
        };
        List<Shop> ShopItens = new List<Shop>();

        class Vehicle
        {
            public string? Name;
            public int Value;
            public int Amount;
            public bool Buyable;
        };
        List<Vehicle> ShopVehicle = new List<Vehicle>();

        private class CuiInputField
        {
            public CuiInputFieldComponent InputField { get; } = new CuiInputFieldComponent();
            public CuiRectTransformComponent RectTransform { get; } = new CuiRectTransformComponent();
            public float FadeOut { get; set; }
        }

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

        //private bool bInitializeMenu = false;
        void InitializeMenuItens()
        {
            if (conn.State == System.Data.ConnectionState.Closed)
            {
                conn.Open();
            }

            try
            {
                using (var readerCommand = conn.CreateCommand())
                {
                    readerCommand.CommandText = $"SELECT `Name`, `Value`, `Amount`, `Buyable`, `Class` FROM `Shop`";
                    using (var reader = readerCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int vv = (int)reader["Class"];
                            switch (vv)
                            {
                                case 0:
                                    {
                                        Shop item = new Shop();
                                        item.Name = reader.GetString(0);
                                        item.Value = reader.GetInt32(1);
                                        item.Amount = reader.GetInt32(2);
                                        item.Buyable = reader.GetBoolean(3);
                                        ShopItens.Add(item);
                                        break;
                                    }
                                case 1:
                                    {
                                        Vehicle vehicle = new Vehicle();
                                        vehicle.Name = reader.GetString(0);
                                        vehicle.Value = reader.GetInt32(1);
                                        vehicle.Amount = reader.GetInt32(2);
                                        vehicle.Buyable = reader.GetBoolean(3);
                                        ShopVehicle.Add(vehicle);
                                        break;
                                    }
                                case 2:
                                    {

                                        break;
                                    }
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Puts($"try actived: {ex.Message}");
            }
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

            InitializeMenuItens();
        }

        void Unload()
        {
            foreach (BasePlayer player in BasePlayer.activePlayerList.Where(player => MenuOpen.Any(menu => menu.player.userID == player.userID)))
                DestroyMenu(player);

            conn.Close();
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

        string GetGroup(BasePlayer player)
        {
            if (player.IsAdmin)
            {
                return "Admin";
            }

            //check vip code here

            if (permission.UserHasGroup(player.UserIDString, "Registred"))
            {
                return "Registred";
            }

            if (permission.UserHasGroup(player.UserIDString, "NotRegistred"))
            {
                return "NotRegistred";
            }

            return "NotRegistred";
        }

        string GetMoney(BasePlayer player)
        {
            if (conn.State == System.Data.ConnectionState.Closed)
            {
                conn.Open();
            }

            using (var readCommand = conn.CreateCommand())
            {
                readCommand.CommandText = $"SELECT `Money` FROM `Users` WHERE `SteamID` = '{player.UserIDString}' ";
                using (var reader = readCommand.ExecuteReader())
                {
                    reader.Read();
                    return reader.GetString(0);
                }
            }
        }

        bool DiscordLinked(string steamID)
        {
            if (conn.State == System.Data.ConnectionState.Closed)
            {
                conn.Open();
            }

            using (var readCommand = conn.CreateCommand())
            {
                readCommand.CommandText = $"SELECT `Discord` FROM `Users` WHERE `SteamID` = '{steamID}' ";
                using (var reader = readCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if ((string)reader["Discord"] != "")
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        [ConsoleCommand("LinkDiscord")]
        void LinkDiscord(ConsoleSystem.Arg arg)
        {

            if (conn.State == System.Data.ConnectionState.Closed)
            {
                conn.Open();
            }

            BasePlayer player = arg.Player();

            using (var UpdateCommand = conn.CreateCommand())
            {
                UpdateCommand.CommandText = $"UPDATE `Users` SET `Discord` = '{arg.Args[0]}' WHERE `SteamID` = '{player.UserIDString}'";
                UpdateCommand.ExecuteNonQuery();
            }
            Puts($"the argument in LinkDiscord: {arg.Args[0]}");
        }

        static string GetVariableName<T>(System.Linq.Expressions.Expression<Func<T>> expr)
        {
            var memberExpr = expr.Body as System.Linq.Expressions.MemberExpression;

            if (memberExpr == null)
            {
                throw new ArgumentException("Expression must be a member expression");
            }

            return memberExpr.Member.Name;
        }

        private void ShopMenu(ref CuiElementContainer container, string? midPanel, int page)
        {
            int end = page * 10;

            double spaceOffsetsX = 0.008;
            double spaceOffsetsY = 0.1;

            double backgroundSizeX = 0.19;
            double backgroundSizeY = 0.40;

            double minX = spaceOffsetsX;
            double minY = 0.47;

            double maxX = minX + backgroundSizeX;
            double maxY = minY + backgroundSizeY;

            int x = end - 10;
            int rowLimit = x + 5;

            Puts($"ShopItens Count: {ShopItens.Count}");

            for (int y = 0; y < 2; y++)
            {
                for (; x < rowLimit; x++)
                {
                    if (ShopItens.Count <= x)
                    {
                        Puts("finished shop function");
                        return;
                    }

                    container.Add(new CuiPanel
                    {
                        Image = { Color = "0 0 0 1" },
                        RectTransform = { AnchorMin = $"{minX} {minY}", AnchorMax = $"{maxX} {maxY}" },
                        CursorEnabled = true,
                        KeyboardEnabled = true
                    }, midPanel, ShopItens[x].Name);


                    container.Add(new CuiButton
                    {
                        Button = { Command = "", Color = "1 0 0 1" },
                        RectTransform = { AnchorMin = $"0 0", AnchorMax = $"1 0.1" },
                        Text = { Text = $"{x}/{ShopItens[x].Name}/{ShopItens[x].Value}", FontSize = 12, Align = TextAnchor.MiddleCenter }
                    }, ShopItens[x].Name);

                    minX = minX + 0.009 + backgroundSizeX;
                    maxX = minX + backgroundSizeX;
                }
                minX = spaceOffsetsX;
                maxX = minX + backgroundSizeX;

                minY = 0.06;
                maxY = minY + backgroundSizeY;
                rowLimit += 5;
            }
        }

        [ConsoleCommand("RefreshMenu")]
        private void RefreshMenu(BasePlayer player)
        {
            DestroyMenu(player);
            CriarMenu(player);
        }

        private void LeftPanel(ref CuiElementContainer container, BasePlayer player)
        {
            foreach (MenuPlayerClass MenuPlayer in MenuOpen.Where(playerd => playerd?.player?.userID == player.userID))
            {
                var leftPanel = container.Add(new CuiPanel
                {
                    Image = { Color = "0 0 0 0.8" },
                    RectTransform = { AnchorMin = "0 0", AnchorMax = "0.3 1" },
                    CursorEnabled = true,
                    KeyboardEnabled = true
                }, "NebulosaMenu");

                //picture
                {
                    var picture = container.Add(new CuiPanel
                    {
                        Image = { Color = "1 1 1 1" },
                        RectTransform =
                            {
                                AnchorMin = "0 0.75", AnchorMax = "1 1"
                            }
                    }, leftPanel);

                    container.Add(new CuiLabel
                    {
                        Text = { Text = "Discord Picture Here",
                                Align = TextAnchor.MiddleCenter,
                                Color = "0 0 1 1", FontSize = 19},
                        RectTransform =
                            {
                                AnchorMin = "0.2 0.25"
                                //AnchorMax = "0.3 0.3"
                            }
                    }, picture);

                    //container.Add(new CuiElement
                    //{
                    //    Name = CuiHelper.GetGuid(),
                    //    Parent = picture,
                    //    Components =
                    //{
                    //    new CuiRawImageComponent {Png = "https://cdn.discordapp.com/attachments/938896762482069514/1199453126911078420/image.png?ex=65c298c6&is=65b023c6&hm=1c0d7194abf784514acd3ac856ddc4fe382c379ccd402f92eac8907dc960f0cf&=&quality=lossless" },
                    //    new CuiRectTransformComponent { AnchorMin = "0 0.75", AnchorMax = "1 1" }
                    //}
                    //});
                }

                container.Add(new CuiLabel
                {
                    Text = { Text = $"Group: {GetGroup(MenuPlayer.player)}", FontSize = 20, Align = TextAnchor.MiddleLeft },
                    RectTransform = { AnchorMin = "0 0.47"/*, AnchorMax = "0 0.55"*/ }
                }, leftPanel);

                container.Add(new CuiLabel
                {
                    Text = { Text = $"Money: {GetMoney(MenuPlayer.player)}", FontSize = 20, Align = TextAnchor.MiddleLeft },
                    RectTransform = { AnchorMin = "0 0.40" }
                }, leftPanel);

                if (!DiscordLinked(MenuPlayer.player.UserIDString))
                {
                    var background = container.Add(new CuiPanel
                    {
                        Image = { Color = "0.1 0.1 0.1 0.8" },
                        RectTransform = { AnchorMin = "0 0.60", AnchorMax = "1 0.65" }
                    }, leftPanel);

                    container.Add(new CuiLabel
                    {
                        RectTransform = { AnchorMin = "0 0", AnchorMax = "0.25 1" },
                        Text = { Text = "Discord ID HERE", Align = TextAnchor.MiddleLeft, Color = "1 1 1 1", FontSize = 18 },
                    }, background);

                    container.Add(new CuiElement
                    {
                        Name = CuiHelper.GetGuid(),
                        Parent = leftPanel,
                        Components =
                            {
                                new CuiInputFieldComponent
                                {
                                    Align = TextAnchor.MiddleCenter,
                                    CharsLimit = 300,
                                    Command = $"RefreshMenu",
                                    FontSize = 18,
                                    IsPassword = false,
                                    Text = MenuPlayer.discord,
                                    NeedsKeyboard = true
                                },
                                new CuiRectTransformComponent {
                                    AnchorMin = "0.253 0.60",
                                    AnchorMax = "1 0.65"
                                }
                            }
                    });

                    container.Add(new CuiButton
                    {
                        Button = { Command = $"LinkDiscord {MenuPlayer.discord}", Color = "0 0 1 1" },
                        RectTransform = { AnchorMin = "0 0.53", AnchorMax = "1 0.59" },
                        Text = { Text = "Link Discord", FontSize = 18, Align = TextAnchor.MiddleCenter }
                    }, leftPanel);

                }

                //discord
                container.Add(new CuiButton
                {
                    Button = { Command = "discord", Color = "0 0 1 1" },
                    RectTransform = { AnchorMin = "0 0", AnchorMax = "1 0.04" },
                    Text = { Text = "Discord", FontSize = 18, Align = TextAnchor.MiddleCenter }
                }, leftPanel);

                //website
                container.Add(new CuiButton
                {
                    Button = { Command = "website", Color = "1 0 0 1" },
                    RectTransform = { AnchorMin = "0 0.04", AnchorMax = "1 0.08" },
                    Text = { Text = "WebSite", FontSize = 18, Align = TextAnchor.MiddleCenter }
                }, leftPanel);
            }
        }

        private void CriarMenu(BasePlayer player, int page = 1, int menu = 0)
        {
            var container = new CuiElementContainer();

            // Container principal
            var mainPanel = container.Add(new CuiPanel
            {
                Image = { Color = "0 0 0 0.5" },
                RectTransform = { AnchorMin = "0.1 0.1", AnchorMax = "0.9 0.9" },
                CursorEnabled = true,
                KeyboardEnabled = true
            }, "Overlay", "NebulosaMenu");

            // Botão de fechar
            container.Add(new CuiButton
            {
                Button = { Command = "fecharmenu", Color = "1 0 0 1" },
                RectTransform = { AnchorMin = "0.95 0.95", AnchorMax = "1 1" },
                Text = { Text = "X", FontSize = 16, Align = TextAnchor.MiddleCenter }
            }, mainPanel);

            //left Panel
            {
                LeftPanel(ref container, player);
            }

            //mid Panel
            {
                var MidPanel = container.Add(new CuiPanel
                {
                    Image = { Color = "0 0 0 0.9" },
                    RectTransform = { AnchorMin = "0.301 0", AnchorMax = "1 0.94" },
                    CursorEnabled = true,
                    KeyboardEnabled = true
                }, mainPanel);

                //topMidPanelButtons
                {
                    var TopMidPanelButtons = container.Add(new CuiPanel
                    {
                        Image = { Color = "0 0 0 0.9" },
                        RectTransform = { AnchorMin = "0 0.9", AnchorMax = "1 1" },
                        CursorEnabled = true,
                        KeyboardEnabled = true
                    }, MidPanel);

                    container.Add(new CuiButton
                    {
                        Button = { Command = "", Color = "0 0 0 1" },
                        Text = { Text = "Shop", FontSize = 18, Align = TextAnchor.MiddleCenter },
                        RectTransform = { AnchorMin = "0 0", AnchorMax = "0.2 1" }
                    }, TopMidPanelButtons);

                    container.Add(new CuiButton
                    {
                        Button = { Command = "", Color = "0 0 0 1" },
                        Text = { Text = "Vehicle", FontSize = 18, Align = TextAnchor.MiddleCenter },
                        RectTransform = { AnchorMin = "0.3 0", AnchorMax = "0.5 1" }
                    }, TopMidPanelButtons);

                    container.Add(new CuiButton
                    {
                        Button = { Command = "", Color = "0 0 0 1" },
                        Text = { Text = "Events", FontSize = 18, Align = TextAnchor.MiddleCenter },
                        RectTransform = { AnchorMin = "0.6 0", AnchorMax = "0.8 1" }
                    }, TopMidPanelButtons);
                }

                //Menus
                {
                    switch (menu)
                    {
                        case 0:
                            {
                                ShopMenu(ref container, MidPanel, page);
                                break;
                            }

                        case 1:
                            {

                                break;
                            }

                        case 2:
                            {

                                break;
                            }
                    }
                }



                //Bottom Menu
                {
                    var BottomMidPanel = container.Add(new CuiPanel
                    {
                        Image = { Color = "0 0 0 1" },
                        RectTransform = { AnchorMin = "0 0", AnchorMax = "1 0.05" },
                        CursorEnabled = true,
                        KeyboardEnabled = true
                    }, MidPanel);

                    container.Add(new CuiButton
                    {
                        Button = { Command = "", Color = "1 0 0 1" },
                        Text = { Text = "Next Page", FontSize = 13, Align = TextAnchor.MiddleCenter },
                        RectTransform = { AnchorMin = "0.80 0.20", AnchorMax = "0.95 0.80" }
                    }, BottomMidPanel);

                    container.Add(new CuiButton
                    {
                        Button = { Command = "", Color = "1 0 0 1" },
                        Text = { Text = "Back Page", FontSize = 13, Align = TextAnchor.MiddleCenter },
                        RectTransform = { AnchorMin = "0.20 0.20", AnchorMax = "0.35 0.80" }
                    }, BottomMidPanel);
                }
            }
            // Adiciona o menu à tela do jogador
            CuiHelper.AddUi(player, container);
        }

        //private string ObterURLDaImagemDoPerfil(BasePlayer player)
        //{
        //    // Obtenha a identificação Steam do jogador
        //    var steamId = player.UserIDString;

        //    // Construa a URL da API da Steam para obter informações do perfil
        //    var apiUrl = $"http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key=SUA_API_KEY&steamids={steamId}";

        //    try
        //    {
        //        // Faça uma requisição à API da Steam
        //        var apiResponse = webrequest.EnqueueGet(apiUrl, (code, response) =>
        //        {
        //            if (code != 200 || response == null)
        //            {
        //                Puts($"Erro na requisição à API da Steam. Código: {code}");
        //                return;
        //            }

        //            // Analise a resposta JSON
        //            var jsonData = JObject.Parse(response);
        //            var players = jsonData["response"]["players"] as JArray;
        //            if (players != null && players.Count > 0)
        //            {
        //                var playerData = players[0];
        //                var avatarUrl = playerData["avatarfull"]?.ToString();
        //                if (!string.IsNullOrEmpty(avatarUrl))
        //                {
        //                    // Devolva a URL da imagem do perfil
        //                    return avatarUrl;
        //                }
        //            }

        //            Puts("Erro ao analisar a resposta da API da Steam.");
        //        }, this);

        //        // Aguarde a resposta da API antes de continuar
        //        apiResponse.Timeout = 5f;
        //        apiResponse.WaitForResponse();

        //        return apiResponse.Response?.ToString();
        //    }
        //    catch (Exception ex)
        //    {
        //        Puts($"Erro ao obter a URL da imagem do perfil: {ex.Message}");
        //        return null;
        //    }
        //}


        #endregion
        [ChatCommand("menu")]
        private void Menu(BasePlayer player, string cmd, string[] args)
        {
            if (conn.State == System.Data.ConnectionState.Closed)
            {
                conn.Open();
            }
            MenuPlayerClass menu = new MenuPlayerClass();
            menu.player.userID = player.userID;

            MenuOpen.Add(menu);

            try
            {
                CriarMenu(player);
            }
            catch (Exception ex)
            {
                DestroyMenu(player);
            }
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
            CuiHelper.DestroyUi(player, "NebulosaMenu");
            //CuiHelper.DestroyUi(player, "LeftPanel");
            //CuiHelper.DestroyUi(player, "Picture");
        }

        [ConsoleCommand("discord")]
        private void discord(ConsoleSystem.Arg arg)
        {
            if (arg.Player() == null) { return; }
            SendReply(arg.Player(), "Abra seu navegador e cole o seguinte link: https://discord.gg/GMVqTAWBmx");
        }

        [ConsoleCommand("website")]
        private void website(ConsoleSystem.Arg arg)
        {
            if (arg.Player() == null) { return; }
            SendReply(arg.Player(), "Abra seu navegador e cole o seguinte link: ");
        }

        #endregion

        #region playerEvent

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
            if (conn.State == System.Data.ConnectionState.Closed)
                conn.Open();

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
            if (conn.State == System.Data.ConnectionState.Closed)
                conn.Open();

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

                    addPlayerDeath(morto.UserIDString);
                    addPlayerKill(assassino.UserIDString);

                }

                if (MenuOpen.Any(menu => menu.player.userID == morto.userID))
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
            if (MenuOpen.Any(menu => menu.player.userID == player.userID))
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