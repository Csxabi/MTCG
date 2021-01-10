using MtcgLauncher.Models;
using MtcgServer;
using MtcgServer.CardRequirements;
using MtcgServer.Cards.MonsterCards;
using MtcgServer.Cards.SpellCards;
using MtcgServer.Scoreboards;
using Newtonsoft.Json;
using RestWebServer;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading.Tasks;

namespace MtcgLauncher
{
    internal class MtcgWebServer
    {
        private readonly IWebServer _web;
        private readonly MtcgServer.MtcgServer _server;

        public MtcgWebServer(IWebServer webServer, IDatabase database, IBattleHandler battleHandler)
        {
            _web = webServer;
            _server = new MtcgServer.MtcgServer(database, battleHandler);
        }

        /// <summary>
        /// Registers necessary routes for executing MTCG.
        /// This method may only be called once per instance.
        /// </summary>
        public void Setup()
        {
            _server.AddScoreboard(nameof(HighestELO), new HighestELO());
            _server.AddScoreboard(nameof(LeastLosses), new LeastLosses());
            _server.AddScoreboard(nameof(MostWins), new MostWins());
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings() { Converters = new List<JsonConverter> { new Newtonsoft.Json.Converters.StringEnumConverter() } };

            // Register user
            _web.RegisterStaticRoute("POST", "/register", async ctx =>
            {
                if (!TryGetObject<UserCredentialsModel>(ctx, out var creds))
                    return new RestResponse(HttpStatusCode.BadRequest, "User credentials not properly defined.");

                var session = await _server.Register(creds.Username, creds.Password);

                if (session is null)
                    return new RestResponse(HttpStatusCode.BadRequest, "User already exists.");

                return new RestResponse(HttpStatusCode.OK, CreateHeader(("Authorization", session.Token.ToString("N"))), "Successfully registered account.");
            });

            // Login user
            _web.RegisterStaticRoute("POST", "/login", async ctx =>
            {
                if (!TryGetObject<UserCredentialsModel>(ctx, out var creds))
                    return new RestResponse(HttpStatusCode.BadRequest, "User credentials not properly defined.");

                var session = await _server.Login(creds.Username, creds.Password);

                if (session is null)
                    return new RestResponse(HttpStatusCode.BadRequest, "User was not found or wrong password.");

                return new RestResponse(HttpStatusCode.OK, CreateHeader(("Authorization", session.Token.ToString("N"))), "Successfully logged in.");
            });

            // Logout user
            _web.RegisterStaticRoute("POST", "/logout", ctx =>
            {
                if (!ctx.Headers.TryGetValue("Authorization", out var sessionStr))
                    return Task.FromResult(new RestResponse(HttpStatusCode.BadRequest, "No authorization supplied."));

                if (!Guid.TryParse(sessionStr, out var token))
                    return Task.FromResult(new RestResponse(HttpStatusCode.BadRequest, "Invalid authorization supplied."));

                if (!_server.Logout(new Session(token)))
                    return Task.FromResult(new RestResponse(HttpStatusCode.NotFound, "Not logged in."));

                return Task.FromResult(new RestResponse(HttpStatusCode.OK, "Successfully logged out."));
            });

            // Get player "profile"
            _web.RegisterResourceRoute("GET", "/profile/%", async ctx =>
            {
                var username = ctx.Resources[0];

                if (await _server.GetPlayer(username) is not Player player)
                    return new RestResponse(HttpStatusCode.NotFound, "Player was not found.");

                return new RestResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(player));
            });

            // Get own player "profile"
            _web.RegisterStaticRoute("GET", "/profile", async ctx =>
            {
                if (!ctx.Headers.TryGetValue("Authorization", out var sessionStr))
                    return new RestResponse(HttpStatusCode.Unauthorized, "No authorization supplied.");

                if (!Guid.TryParse(sessionStr, out var token))
                    return new RestResponse(HttpStatusCode.BadRequest, "Invalid authorization supplied.");

                if (await _server.GetPlayer(new Session(token)) is not Player player)
                    return new RestResponse(HttpStatusCode.Unauthorized, "Invalid session.");

                return new RestResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(player));
            });

            // Edit player "profile"
            _web.RegisterStaticRoute("POST", "/profile", async ctx =>
            {
                if (!ctx.Headers.TryGetValue("Authorization", out var sessionStr))
                    return new RestResponse(HttpStatusCode.Unauthorized, "No authorization supplied.");

                if (!Guid.TryParse(sessionStr, out var token))
                    return new RestResponse(HttpStatusCode.BadRequest, "Invalid authorization supplied.");

                if (!TryGetObject<EditPlayerModel>(ctx, out var editModel))
                    return new RestResponse(HttpStatusCode.BadRequest, "Invalid edit.");

                if (!await _server.EditPlayer(new Session(token), editModel.Username, editModel.StatusText, editModel.EmoticonText, editModel.Password))
                    return new RestResponse(HttpStatusCode.Unauthorized, "Invalid session or invalid field length.");

                return new RestResponse(HttpStatusCode.OK, "Changes have been saved.");
            });

            // Get any player stack
            _web.RegisterResourceRoute("GET", "/profile/%/stack", async ctx =>
            {
                var username = ctx.Resources[0];

                if (await _server.GetPlayer(username) is Player player)
                    return new RestResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(player.Stack));

                return new RestResponse(HttpStatusCode.NotFound, "Player was not found.");
            });

            // Get any player deck
            _web.RegisterResourceRoute("GET", "/profile/%/deck", async ctx =>
            {
                var username = ctx.Resources[0];

                if (await _server.GetPlayer(username) is Player player)
                    return new RestResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(player.Deck));

                return new RestResponse(HttpStatusCode.NotFound, "Player was not found.");
            });

            // Get own stack
            _web.RegisterStaticRoute("GET", "/stack", async ctx =>
            {
                if (!ctx.Headers.TryGetValue("Authorization", out var sessionStr))
                    return new RestResponse(HttpStatusCode.Unauthorized, "No authorization supplied.");

                if (!Guid.TryParse(sessionStr, out var token))
                    return new RestResponse(HttpStatusCode.BadRequest, "Invalid authorization supplied.");

                if (await _server.GetPlayer(new Session(token)) is Player player)
                    return new RestResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(player.Stack));

                return new RestResponse(HttpStatusCode.Unauthorized, "Player not logged in.");
            });

            // Get own deck
            _web.RegisterStaticRoute("GET", "/deck", async ctx =>
            {
                if (!ctx.Headers.TryGetValue("Authorization", out var sessionStr))
                    return new RestResponse(HttpStatusCode.Unauthorized, "No authorization supplied.");

                if (!Guid.TryParse(sessionStr, out var token))
                    return new RestResponse(HttpStatusCode.BadRequest, "Invalid authorization supplied.");

                if (await _server.GetPlayer(new Session(token)) is Player player)
                    return new RestResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(player.Deck));

                return new RestResponse(HttpStatusCode.Unauthorized, "Player not logged in.");
            });

            // Edit deck
            _web.RegisterStaticRoute("POST", "/deck", async ctx =>
            {
                if (!ctx.Headers.TryGetValue("Authorization", out var sessionStr))
                    return new RestResponse(HttpStatusCode.Unauthorized, "No authorization supplied.");

                if (!Guid.TryParse(sessionStr, out var token))
                    return new RestResponse(HttpStatusCode.BadRequest, "Invalid authorization supplied.");

                if (!TryGetObject<Guid[]>(ctx, out var cardIds))
                    return new RestResponse(HttpStatusCode.BadRequest, "Invalid deck specification.");

                if (await _server.SetDeck(new Session(token), cardIds))
                    return new RestResponse(HttpStatusCode.OK, "Successfully updated deck.");

                return new RestResponse(HttpStatusCode.BadRequest, "Invalid session, not exactly four cards or invalid card ID.");
            });

            // Battling
            _web.RegisterStaticRoute("POST", "/battle", async ctx =>
            {
                if (!ctx.Headers.TryGetValue("Authorization", out var sessionStr))
                    return new RestResponse(HttpStatusCode.Unauthorized, "No authorization supplied.");

                if (!Guid.TryParse(sessionStr, out var token))
                    return new RestResponse(HttpStatusCode.BadRequest, "Invalid authorization supplied.");

                if (await _server.InvokeBattle(new Session(token)) is not BattleResult result)
                    return new RestResponse(HttpStatusCode.BadRequest, "Invalid session or invalid deck configuration.");

                return new RestResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(result));
            });

            // View card store ("trading")
            _web.RegisterStaticRoute("GET", "/store/cards", async _ =>
                new RestResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(await _server.GetAvailableStoreCards())));

            // View card store ("trading") eligible entries
            _web.RegisterStaticRoute("GET", "/store/cards/eligible", async ctx =>
            {
                if (!ctx.Headers.TryGetValue("Authorization", out var sessionStr))
                    return new RestResponse(HttpStatusCode.Unauthorized, "No authorization supplied.");

                if (!Guid.TryParse(sessionStr, out var token))
                    return new RestResponse(HttpStatusCode.BadRequest, "Invalid authorization supplied.");

                if (!TryGetObject<Guid>(ctx, out var cardId))
                    return new RestResponse(HttpStatusCode.BadRequest, "Incorrectly formatted card ID.");

                var result = await _server.GetEligibleStoreCards(new Session(token), cardId);

                if (result is null)
                    return new RestResponse(HttpStatusCode.BadRequest, "Invalid session, invalid card or card not in stack");

                return new RestResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(result));
            });

            // Push to card store ("trading push")
            _web.RegisterStaticRoute("POST", "/store/cards/new", async ctx =>
            {
                if (!ctx.Headers.TryGetValue("Authorization", out var sessionStr))
                    return new RestResponse(HttpStatusCode.Unauthorized, "No authorization supplied.");

                if (!Guid.TryParse(sessionStr, out var token))
                    return new RestResponse(HttpStatusCode.BadRequest, "Invalid authorization supplied.");

                if (!TryGetObject<PushCardStoreModel>(ctx, out var pushModel))
                    return new RestResponse(HttpStatusCode.BadRequest, "Invalid specification.");

                List<ICardRequirement> translatedReqs = new(pushModel.Requirements.Length);

                foreach (var req in pushModel.Requirements)
                {
                    switch (req.RequirementType)
                    {
                        case RequirementType.ElementType when req.RequiredElement.HasValue:
                            translatedReqs.Add(new ElementTypeRequirement() { Type = req.RequiredElement.Value });
                            break;

                        case RequirementType.IsMonsterCard:
                            translatedReqs.Add(new IsMonsterCardRequirement());
                            break;

                        case RequirementType.IsSpellCard:
                            translatedReqs.Add(new IsSpellCardRequirement());
                            break;

                        case RequirementType.MinimumDamage when req.MinimumDamage.HasValue:
                            translatedReqs.Add(new MinimumDamageRequirement() { MinimumDamage = req.MinimumDamage.Value });
                            break;

                        default:
                            return new RestResponse(HttpStatusCode.BadRequest, "Unknown requirement type or missing value.");
                    }
                }

                if (!await _server.PushCardToStore(new Session(token), pushModel.CardId, translatedReqs))
                    return new RestResponse(HttpStatusCode.BadRequest, "Invalid session or card ID.");

                return new RestResponse(HttpStatusCode.Created, "Successfully pushed card to store: " + pushModel.CardId);
            });

            // Trade with card store ("trading")
            _web.RegisterStaticRoute("POST", "/store/cards/trade", async ctx =>
            {
                if (!ctx.Headers.TryGetValue("Authorization", out var sessionStr))
                    return new RestResponse(HttpStatusCode.Unauthorized, "No authorization supplied.");

                if (!Guid.TryParse(sessionStr, out var token))
                    return new RestResponse(HttpStatusCode.BadRequest, "Invalid authorization supplied.");

                if (!TryGetObject<CardTradeModel>(ctx, out var tradeModel))
                    return new RestResponse(HttpStatusCode.BadRequest, "Incorrectly formatted trade model.");

                if (!await _server.TradeCards(new Session(token), tradeModel.OwnCard, tradeModel.OtherCard))
                    return new RestResponse(HttpStatusCode.BadRequest, "Invalid session or card specifiction.");

                return new RestResponse(HttpStatusCode.OK, "Successfully traded cards.");
            });

            // Get packages
            _web.RegisterStaticRoute("GET", "/store/packages", async _ =>
                new RestResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(await _server.GetAllPackages())));

            // Get affordable packages (max price in route)
            _web.RegisterResourceRoute("GET", "/store/packages/max/%", async ctx =>
            {
                if (!int.TryParse(ctx.Resources[0], out var maxPrice))
                    return new RestResponse(HttpStatusCode.BadRequest, "Invalid maximum price.");

                return new RestResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(await _server.GetAffordablePackages(maxPrice)));
            });

            // Get affordable packages for player
            _web.RegisterStaticRoute("GET", "/store/packages/affordable", async ctx =>
            {
                if (!ctx.Headers.TryGetValue("Authorization", out var sessionStr))
                    return new RestResponse(HttpStatusCode.Unauthorized, "No authorization supplied.");

                if (!Guid.TryParse(sessionStr, out var token))
                    return new RestResponse(HttpStatusCode.BadRequest, "Invalid authorization supplied.");

                var result = await _server.GetAffordablePackages(new Session(token));

                if (result is null)
                    return new RestResponse(HttpStatusCode.BadRequest, "Invalid session.");

                return new RestResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(result));
            });

            // Push to package store
            _web.RegisterStaticRoute("POST", "/store/packages", async ctx =>
            {
                if (!ctx.Headers.TryGetValue("Authorization", out var sessionStr))
                    return new RestResponse(HttpStatusCode.Unauthorized, "No authorization supplied.");

                if (!Guid.TryParse(sessionStr, out var token))
                    return new RestResponse(HttpStatusCode.BadRequest, "Invalid authorization supplied.");

                if (!TryGetObject<PushCardPackageModel>(ctx, out var pushModel))
                    return new RestResponse(HttpStatusCode.BadRequest, "Invalid package.");

                List<ICard> translatedCards = new(pushModel.Cards.Length);
                var packageId = Guid.NewGuid();

                foreach (var card in pushModel.Cards)
                {
                    try
                    {
                        translatedCards.Add(card.CardType switch
                        {
                            CardType.Dragon      => new Dragon      { Id = Guid.NewGuid(), Damage = card.Damage },
                            CardType.FireElf     => new FireElf     { Id = Guid.NewGuid(), Damage = card.Damage },
                            CardType.Goblin      => new Goblin      { Id = Guid.NewGuid(), Damage = card.Damage },
                            CardType.Knight      => new Knight      { Id = Guid.NewGuid(), Damage = card.Damage },
                            CardType.Kraken      => new Kraken      { Id = Guid.NewGuid(), Damage = card.Damage },
                            CardType.Ork         => new Ork         { Id = Guid.NewGuid(), Damage = card.Damage },
                            CardType.Wizard      => new Wizard      { Id = Guid.NewGuid(), Damage = card.Damage },
                            CardType.NormalSpell => new NormalSpell { Id = Guid.NewGuid(), Damage = card.Damage },
                            CardType.WaterSpell  => new WaterSpell  { Id = Guid.NewGuid(), Damage = card.Damage },
                            CardType.FireSpell   => new FireSpell   { Id = Guid.NewGuid(), Damage = card.Damage },
                            _ => throw new ArgumentOutOfRangeException()
                        });
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        return new RestResponse(HttpStatusCode.BadRequest, "Unknown card type: " + card.CardType);
                    }
                }

                if (!await _server.RegisterPackage(new Session(token), new CardPackage(packageId, pushModel.Price, translatedCards)))
                    return new RestResponse(HttpStatusCode.Unauthorized, "Invalid session.");

                return new RestResponse(HttpStatusCode.Created, "Successfully created package with ID: " + packageId.ToString("N"));
            });

            // Buy random cards
            _web.RegisterStaticRoute("POST", "/store/packages/buy/random/cards", async ctx =>
            {
                if (!ctx.Headers.TryGetValue("Authorization", out var sessionStr))
                    return new RestResponse(HttpStatusCode.Unauthorized, "No authorization supplied.");

                if (!Guid.TryParse(sessionStr, out var token))
                    return new RestResponse(HttpStatusCode.BadRequest, "Invalid authorization supplied.");

                if (!await _server.BuyRandomCards(new Session(token)))
                    return new RestResponse(HttpStatusCode.BadRequest, "Invalid session or too little money.");

                return new RestResponse(HttpStatusCode.OK, "Bought randomly chosen cards.");
            });

            // Buy random package
            _web.RegisterStaticRoute("POST", "/store/packages/buy/random/package", async ctx =>
            {
                if (!ctx.Headers.TryGetValue("Authorization", out var sessionStr))
                    return new RestResponse(HttpStatusCode.Unauthorized, "No authorization supplied.");

                if (!Guid.TryParse(sessionStr, out var token))
                    return new RestResponse(HttpStatusCode.BadRequest, "Invalid authorization supplied.");

                if (!await _server.BuyRandomPackage(new Session(token)))
                    return new RestResponse(HttpStatusCode.BadRequest, "Invalid session or too little money.");

                return new RestResponse(HttpStatusCode.OK, "Bought randomly chosen affordable package.");
            });

            // Buy specific package
            _web.RegisterResourceRoute("POST", "/store/packages/buy/%", async ctx =>
            {
                if (!ctx.Headers.TryGetValue("Authorization", out var sessionStr))
                    return new RestResponse(HttpStatusCode.Unauthorized, "No authorization supplied.");

                if (!Guid.TryParse(sessionStr, out var token))
                    return new RestResponse(HttpStatusCode.BadRequest, "Invalid authorization supplied.");

                if (!Guid.TryParse(ctx.Resources[0], out var packageId))
                    return new RestResponse(HttpStatusCode.BadRequest, "Invalid package id.");

                if (!await _server.BuySpecificPackage(new Session(token), packageId))
                    return new RestResponse(HttpStatusCode.BadRequest, "Invalid session, invalid package or too little money.");

                return new RestResponse(HttpStatusCode.OK, "Successfully bought package.");
            });

            // Player scoreboards
            _web.RegisterResourceRoute("GET", "/scoreboards/%", async ctx =>
            {
                var scoreboard = ctx.Resources[0];

                var results = await _server.GetScoreboard(scoreboard, 50);

                if (results is null)
                    return new RestResponse(HttpStatusCode.NotFound, "Unknown scoreboard.");

                return new RestResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(results));
            });
        }

        /// <summary>
        /// Starts listening for incoming requests.
        /// </summary>
        public void Start()
            => _web.Start();

        /// <summary>
        /// Stops listening for incoming requests.
        /// </summary>
        public void Stop()
            => _web.Stop();

        private static bool TryGetObject<T>(RequestContext context, [MaybeNullWhen(false)] out T result)
        {
            result = default;

            if (String.IsNullOrEmpty(context.Payload))
                return false;

            try
            {
                result =  JsonConvert.DeserializeObject<T>(context.Payload);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        private static T GetObject<T>(RequestContext context)
            => JsonConvert.DeserializeObject<T>(context.Payload);

        private static Dictionary<string, string> CreateHeader(params (string key, string value)[] entries)
        {
            Dictionary<string, string> dict = new();

            foreach (var entry in entries)
                dict[entry.key] = entry.value;

            return dict;
        }
    }
}
