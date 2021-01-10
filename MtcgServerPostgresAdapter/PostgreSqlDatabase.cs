using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace MtcgServer.Databases.Postgres
{
    public class PostgreSqlDatabase : IDatabase
    {
        private readonly string _connectionString;

        private static readonly Dictionary<PlayerChange, string> PlayerChangeTranslation = new()
        {
            { PlayerChange.Name,        "name"      },
            { PlayerChange.Password,    "pass_hash" },
            { PlayerChange.StatusText,  "statusmsg" },
            { PlayerChange.EmoticonText,"emoticon"  },
            //{ PlayerChange.Stack,       null!       },
            //{ PlayerChange.Deck,        null!       },
            { PlayerChange.Coins,       "coins"     },
            { PlayerChange.ELO,         "elo"       },
            { PlayerChange.Wins,        "wins"      },
            { PlayerChange.Losses,      "losses"    }
        };

        static PostgreSqlDatabase()
        {
            // maps the locally defined enum ElementType to the one defined in the database
            NpgsqlConnection.GlobalTypeMapper.MapEnum<ElementType>("element_type");
            NpgsqlConnection.GlobalTypeMapper.MapEnum<MonsterType>("monster_type");

            // internally used structures
            NpgsqlConnection.GlobalTypeMapper.MapEnum<CardRequirementType>("card_req_types");
            NpgsqlConnection.GlobalTypeMapper.MapComposite<CardRequirement>("card_req");
        }

        public PostgreSqlDatabase(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task CreatePlayer(Player player)
        {
            using var conn = await OpenConnection();
            using var cmd = new NpgsqlCommand("INSERT INTO users VALUES (@id, @name, @statusmsg, @emoticon, @coins, @elo, @wins, @losses, @pass_hash)", conn);
            cmd.Parameters.AddWithValue("@id", player.Id);
            cmd.Parameters.AddWithValue("@name", player.Name);
            cmd.Parameters.AddWithValue("@statusmsg", player.StatusText);
            cmd.Parameters.AddWithValue("@emoticon", player.EmoticonText);
            cmd.Parameters.AddWithValue("@coins", player.Coins);
            cmd.Parameters.AddWithValue("@elo", player.ELO);
            cmd.Parameters.AddWithValue("@wins", player.Wins);
            cmd.Parameters.AddWithValue("@losses", player.Losses);
            cmd.Parameters.AddWithValue("@pass_hash", player.PasswordHash);
            await cmd.PrepareAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<Player?> ReadPlayer(Guid id)
        {
            using var conn = await OpenConnection();
            return await _ReadPlayer(id, conn);
        }

        public async Task<ICollection<Player>> ListPlayers()
        {
            using var conn = await OpenConnection();
            using var cmd = new NpgsqlCommand("SELECT id, name, pass_hash, statusmsg, emoticon, coins, elo, wins, losses FROM users", conn);
            using var reader = await cmd.ExecuteReaderAsync();
            List<Player> players = new();

            while (await reader.ReadAsync())
            {
                var id = reader.GetGuid(0);
                var name = reader.GetString(1);
                var pwHash = await reader.GetFieldValueAsync<byte[]>(2);
                var statusText = reader.GetString(3);
                var emoticon = reader.GetString(4);
                var coins = reader.GetInt32(5);
                var elo = reader.GetInt32(6);
                var wins = reader.GetInt32(7);
                var losses = reader.GetInt32(8);

                players.Add(new Player(id, name, pwHash, statusText, emoticon, coins, Array.Empty<ICard>(), Array.Empty<ICard>(), elo, wins, losses));
            }

            return players;
        }

        private async Task<Player?> _ReadPlayer(Guid id, NpgsqlConnection conn)
        {
            string name, statusText, emoticon;
            byte[] pwHash;
            int coins, elo, wins, losses;
            List<ICard> stackCards = new(), deckCards = new();

            using (var cmd = new NpgsqlCommand("SELECT name, pass_hash, statusmsg, emoticon, coins, elo, wins, losses FROM users WHERE id = @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                await cmd.PrepareAsync();
                using var reader = await cmd.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                    return null;

                name = reader.GetString(0);
                pwHash = await reader.GetFieldValueAsync<byte[]>(1);
                statusText = reader.GetString(2);
                emoticon = reader.GetString(3);
                coins = reader.GetInt32(4);
                elo = reader.GetInt32(5);
                wins = reader.GetInt32(6);
                losses = reader.GetInt32(7);
            }

            using (var cmd = new NpgsqlCommand("SELECT card_id, damage, element_type, monster_type FROM stacks JOIN users ON user_id = users.id JOIN cards ON card_id = cards.id WHERE user_id = @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                await cmd.PrepareAsync();
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var cardId = reader.GetGuid(0);
                    var damage = reader.GetInt32(1);
                    var elementType = await reader.GetFieldValueAsync<ElementType>(2);
                    var monsterType = await reader.GetFieldValueAsync<MonsterType>(3);
                    stackCards.Add(CreateCardFromData(cardId, damage, elementType, monsterType));
                }
            }

            using (var cmd = new NpgsqlCommand("SELECT card_id, damage, element_type, monster_type FROM decks JOIN users ON user_id = users.id JOIN cards ON card_id = cards.id WHERE user_id = @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                await cmd.PrepareAsync();
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var cardId = reader.GetGuid(0);
                    var damage = reader.GetInt32(1);
                    var elementType = await reader.GetFieldValueAsync<ElementType>(2);
                    var monsterType = await reader.GetFieldValueAsync<MonsterType>(3);
                    deckCards.Add(CreateCardFromData(cardId, damage, elementType, monsterType));
                }
            }

            return new Player(id, name, pwHash, statusText, emoticon, coins, stackCards, deckCards, elo, wins, losses);
        }

        public async Task SavePlayer(Player player, PlayerChange changes)
        {
            using var conn = await OpenConnection();
            using var trans = await conn.BeginTransactionAsync();

            // changes made to player object itself
            if ((changes & PlayerChange.UsersMask) != PlayerChange.None)
            {
                using var cmd = new NpgsqlCommand() { Connection = conn, Transaction = trans };
                StringBuilder sb = new("UPDATE users SET");

                foreach (var kvp in PlayerChangeTranslation)
                {
                    if (!changes.HasFlag(kvp.Key))
                        continue;

                    sb.Append(' ').Append(kvp.Value).Append('=').Append('@').Append(kvp.Value);
                    cmd.Parameters.AddWithValue('@' + kvp.Value, kvp.Key switch
                    {
                        PlayerChange.Name => player.Name,
                        PlayerChange.Password => player.PasswordHash,
                        PlayerChange.StatusText => player.StatusText,
                        PlayerChange.EmoticonText => player.EmoticonText,
                        PlayerChange.Coins => player.Coins,
                        PlayerChange.ELO => player.ELO,
                        PlayerChange.Wins => player.Wins,
                        PlayerChange.Losses => player.Losses,
                        _ => throw new InvalidOperationException("Invalid PlayerChange for users: " + kvp.Key)
                    });
                    sb.Append(',');
                }

                --sb.Length; // last ','
                sb.Append(" WHERE id = @id");
                cmd.CommandText = sb.ToString();
                cmd.Parameters.AddWithValue("@id", player.Id);
                await cmd.PrepareAsync();
                await cmd.ExecuteNonQueryAsync();
            }

            // cards must already exists in "cards" table when inserting here

            // changes made to player's stack
            if (changes.HasFlag(PlayerChange.Stack))
            {
                using (var clearCmd = new NpgsqlCommand("DELETE FROM stacks WHERE user_id = @id", conn, trans))
                {
                    clearCmd.Parameters.AddWithValue("@id", player.Id);
                    await clearCmd.PrepareAsync();
                    await clearCmd.ExecuteNonQueryAsync();
                }

                using (var addCmd = new NpgsqlCommand() { Connection = conn, Transaction = trans })
                {
                    StringBuilder sb = new("INSERT INTO stacks VALUES ");

                    foreach (var card in player.Stack)
                        sb.Append("('").Append(player.Id).Append("','").Append(card.Id).Append("'),");

                    --sb.Length; // last ','
                    addCmd.CommandText = sb.ToString();
                    await addCmd.PrepareAsync();
                    System.Diagnostics.Debug.Assert(await addCmd.ExecuteNonQueryAsync() == player.Stack.Count);
                }
            }

            // changes made to player's deck
            if (changes.HasFlag(PlayerChange.Deck))
            {
                using (var clearCmd = new NpgsqlCommand("DELETE FROM decks WHERE user_id = @id", conn, trans))
                {
                    clearCmd.Parameters.AddWithValue("@id", player.Id);
                    await clearCmd.PrepareAsync();
                    await clearCmd.ExecuteNonQueryAsync();
                }

                using (var addCmd = new NpgsqlCommand() { Connection = conn, Transaction = trans })
                {
                    StringBuilder sb = new("INSERT INTO decks VALUES ");

                    foreach (var card in player.Deck)
                        sb.Append("('").Append(player.Id).Append("','").Append(card.Id).Append("'),");

                    --sb.Length; // last ','
                    addCmd.CommandText = sb.ToString();
                    await addCmd.PrepareAsync();
                    System.Diagnostics.Debug.Assert(await addCmd.ExecuteNonQueryAsync() == player.Deck.Count);
                }
            }

            await trans.CommitAsync();
        }

        public async Task<Guid> SearchPlayer(string name)
        {
            using var conn = await OpenConnection();
            using var cmd = new NpgsqlCommand("SELECT id FROM users WHERE name = @name", conn);
            cmd.Parameters.AddWithValue("@name", name);
            await cmd.PrepareAsync();

            var result = await cmd.ExecuteScalarAsync();

            if (result is Guid playerId)
                return playerId;

            return Guid.Empty;
        }

        public async Task<Player> FindOwner(ICard card)
        {
            using var conn = await OpenConnection();
            using var cmd = new NpgsqlCommand("SELECT user_id FROM stacks WHERE card_id = @id", conn);
            cmd.Parameters.AddWithValue("@id", card.Id);
            await cmd.PrepareAsync();
            var result = await cmd.ExecuteScalarAsync();

            if (result is not Guid id)
                throw new DatabaseException("Invalid column type for user_id in stacks: " + (result?.GetType().Name));

            var player = await _ReadPlayer(id, conn);

            if (player is null)
                throw new DatabaseException("Player with card does not exist.");

            return player;
        }

        public async Task<ICard?> ReadCard(Guid id)
        {
            using var conn = await OpenConnection();
            using var cmd = new NpgsqlCommand("SELECT damage, element_type, monster_type FROM cards WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            await cmd.PrepareAsync();

            using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            var damage = reader.GetInt32(0);
            var elementType = await reader.GetFieldValueAsync<ElementType>(1);
            var monsterType = await reader.GetFieldValueAsync<MonsterType>(2);

            return CreateCardFromData(id, damage, elementType, monsterType);
        }

        public async Task CreateCard(ICard card)
        {
            using var conn = await OpenConnection();
            await _CreateCard(card, conn);
        }

        private async Task _CreateCard(ICard card, NpgsqlConnection conn)
        {
            using var cmd = new NpgsqlCommand("INSERT INTO cards VALUES (@id, @damage, @element_type, @monster_type)", conn);
            cmd.Parameters.AddWithValue("@id", card.Id);
            cmd.Parameters.AddWithValue("@damage", card.Damage);
            cmd.Parameters.AddWithValue("@element_type", card.ElementType);
            cmd.Parameters.AddWithValue("@monster_type", card is Cards.SpellCard ? MonsterType.Spell : Enum.Parse(typeof(MonsterType), card.GetType().Name));
            await cmd.PrepareAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<ICollection<CardStoreEntry>> ReadStore()
        {
            using var conn = await OpenConnection();
            using var cmd = new NpgsqlCommand("SELECT e.card_id, c.damage, c.element_type, c.monster_type, e.reqs FROM store_entries e JOIN cards c ON e.card_id = c.id", conn);

            using var reader = await cmd.ExecuteReaderAsync();
            List<CardStoreEntry> entries = new();

            while (await reader.ReadAsync())
            {
                Guid id = reader.GetGuid(0);
                int damage = reader.GetInt32(1);
                var elementType = await reader.GetFieldValueAsync<ElementType>(2);
                var monsterType = await reader.GetFieldValueAsync<MonsterType>(3);
                var card = CreateCardFromData(id, damage, elementType, monsterType);
                var reqs = await reader.GetFieldValueAsync<CardRequirement[]>(4);
                List<ICardRequirement> translatedReqs = new(reqs.Length);

                foreach (var req in reqs)
                {
                    translatedReqs.Add(req.ReqType switch
                    {
                        CardRequirementType.ElementType => new CardRequirements.ElementTypeRequirement { Type = (ElementType)req.ReqValue },
                        CardRequirementType.IsMonsterCard => new CardRequirements.IsMonsterCardRequirement(),
                        CardRequirementType.IsSpellCard => new CardRequirements.IsSpellCardRequirement(),
                        CardRequirementType.MinimumDamage => new CardRequirements.MinimumDamageRequirement { MinimumDamage = req.ReqValue },
                        _ => throw new DatabaseException("Invalid card requirement type: " + req.ReqType)
                    });
                }

                entries.Add(new CardStoreEntry(card, translatedReqs));
            }

            return entries;
        }

        public async Task AddToStore(Player owner, ICard card, ICollection<ICardRequirement> requirements)
        {
            using var conn = await OpenConnection();
            using var cmd = new NpgsqlCommand("INSERT INTO store_entries VALUES (@card_id, @user_id, @req_type, @reqs)", conn);
            cmd.Parameters.AddWithValue("@card_id", card.Id);
            cmd.Parameters.AddWithValue("@user_id", owner.Id);

            List<CardRequirement> reqs = new(requirements.Count);
            foreach (var req in requirements)
            {
                reqs.Add(req switch
                {
                    CardRequirements.ElementTypeRequirement r   => new CardRequirement { ReqType = CardRequirementType.ElementType,   ReqValue = (int)r.Type },
                    CardRequirements.IsMonsterCardRequirement r => new CardRequirement { ReqType = CardRequirementType.IsMonsterCard, ReqValue = default },
                    CardRequirements.IsSpellCardRequirement r   => new CardRequirement { ReqType = CardRequirementType.IsSpellCard,   ReqValue = default },
                    CardRequirements.MinimumDamageRequirement r => new CardRequirement { ReqType = CardRequirementType.MinimumDamage, ReqValue = r.MinimumDamage },
                    _ => throw new DatabaseException("Unknown card requirement: " + req.GetType().Name)
                });
            }

            cmd.Parameters.AddWithValue("@reqs", reqs);
            await cmd.PrepareAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task RemoveFromStore(ICard card)
        {
            using var conn = await OpenConnection();
            using var cmd = new NpgsqlCommand("DELETE FROM store_entries WHERE card_id = @card_id", conn);
            cmd.Parameters.AddWithValue("@card_id", card.Id);
            await cmd.PrepareAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task AddToPackages(CardPackage package)
        {
            using var conn = await OpenConnection();

            foreach (var card in package.Cards)
                await _CreateCard(card, conn);

            using var cmd = new NpgsqlCommand("INSERT INTO packages VALUES (@package_id, @price, @card_ids)", conn);
            cmd.Parameters.AddWithValue("@package_id", package.Id);
            cmd.Parameters.AddWithValue("@price", package.Price);
            cmd.Parameters.AddWithValue("@card_ids", package.Cards.Select(c => c.Id).ToList());
            await cmd.PrepareAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<ICollection<CardPackage>> GetPackages()
        {
            using var conn = await OpenConnection();
            using var cmd = new NpgsqlCommand("SELECT package_id, price, card_ids FROM packages", conn);
            using var reader = await cmd.ExecuteReaderAsync();
            List<CardPackage> packages = new();

            while (await reader.ReadAsync())
            {
                var packageId = reader.GetGuid(0);
                var price = reader.GetInt32(1);
                var cardIds = await reader.GetFieldValueAsync<Guid[]>(2);
                List<ICard> cards = new(cardIds.Length);

                using var cardConn = await OpenConnection();
                using (var cardCmd = new NpgsqlCommand("SELECT id, damage, element_type, monster_type FROM cards WHERE id in ('" + String.Join("','", cardIds) + "')", cardConn))
                {
                    await cardCmd.PrepareAsync();
                    using var cardReader = await cardCmd.ExecuteReaderAsync();

                    while (await cardReader.ReadAsync())
                    {
                        var cardId = cardReader.GetGuid(0);
                        var damage = cardReader.GetInt32(1);
                        var elementType = await cardReader.GetFieldValueAsync<ElementType>(2);
                        var monsterType = await cardReader.GetFieldValueAsync<MonsterType>(3);

                        cards.Add(CreateCardFromData(cardId, damage, elementType, monsterType));
                    }
                }

                packages.Add(new CardPackage(packageId, price, cards));
            }

            return packages;
        }

        private static ICard CreateCardFromData(Guid id, int damage, ElementType elementType, MonsterType monsterType)
            => monsterType switch
        {
            MonsterType.Spell => elementType switch
            {
                ElementType.Normal => new Cards.SpellCards.NormalSpell() { Id = id, Damage = damage },
                ElementType.Water  => new Cards.SpellCards.WaterSpell()  { Id = id, Damage = damage },
                ElementType.Fire   => new Cards.SpellCards.FireSpell()   { Id = id, Damage = damage },
                _ => throw new DatabaseException("Invalid element type for spell card: " + elementType)
            },
            MonsterType.Dragon  => new Cards.MonsterCards.Dragon()  { Id = id, Damage = damage },
            MonsterType.FireElf => new Cards.MonsterCards.FireElf() { Id = id, Damage = damage },
            MonsterType.Goblin  => new Cards.MonsterCards.Goblin()  { Id = id, Damage = damage },
            MonsterType.Knight  => new Cards.MonsterCards.Knight()  { Id = id, Damage = damage },
            MonsterType.Kraken  => new Cards.MonsterCards.Kraken()  { Id = id, Damage = damage },
            MonsterType.Ork     => new Cards.MonsterCards.Ork()     { Id = id, Damage = damage },
            MonsterType.Wizard  => new Cards.MonsterCards.Wizard()  { Id = id, Damage = damage },
            _ => throw new DatabaseException("Invalid value for monster type: " + monsterType)
        };

        private async Task<NpgsqlConnection> OpenConnection()
        {
            var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync().ConfigureAwait(false);
            // manual closing is not required as long as the using keyword is used
            return conn;
        }
    }
}
