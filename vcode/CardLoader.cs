using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace YugiohCardManager
{
    public class Card
    {
        public string? id { get; set; }
        public string? name { get; set; }
        public string? type { get; set; }
        public string? desc { get; set; }
        public int? atk { get; set; }
        public int? def { get; set; }
        public int? level { get; set; }
        public string? race { get; set; }
        public string? attribute { get; set; }
        public string? archetype { get; set; }
        public CardSet[]? card_sets { get; set; }
        public CardImage[]? card_images { get; set; }
        public CardPrice[]? card_prices { get; set; }
    }

    public class CardSet
    {
        public string? set_name { get; set; }
        public string? set_code { get; set; }
        public string? set_rarity { get; set; }
        public string? set_price { get; set; }
    }

    public class CardImage
    {
        public string? id { get; set; }
        public string? image_url { get; set; }
        public string? image_url_small { get; set; }
    }

    public class CardPrice
    {
        public string? cardmarket_price { get; set; }
        public string? tcgplayer_price { get; set; }
        public string? ebay_price { get; set; }
        public string? amazon_price { get; set; }
        public string? coolstuffinc_price { get; set; }
    }

    public class CardLoader
    {
        private static readonly HttpClient client = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };

        public static async Task<List<Card>> GetAllCardsAsync()
        {
            Console.WriteLine("Starting to download card data...");
            List<Card> cards = new List<Card>();

            try
            {
                var response = await client.GetAsync("https://db.ygoprodeck.com/api/v7/cardinfo.php");
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Card data downloaded.");

                var cardData = JsonConvert.DeserializeObject<Dictionary<string, List<Card>>>(responseBody);
                cards = cardData?["data"] ?? new List<Card>();

                Console.WriteLine($"Total cards downloaded: {cards.Count}");
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"HTTP Request error: {httpEx.Message}");
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"JSON Parsing error: {jsonEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }

            return cards;
        }

        public static async Task LoadCardsToDatabase(Database db, ProgressForm progressForm)
        {
            var cards = await GetAllCardsAsync();
            if (cards == null || cards.Count == 0)
            {
                Console.WriteLine("No cards data retrieved.");
                return;
            }

            int totalCards = cards.Count;
            int count = 0;

            foreach (var card in cards)
            {
                if (string.IsNullOrEmpty(card.id) || string.IsNullOrEmpty(card.name) || string.IsNullOrEmpty(card.type))
                {
                    Console.WriteLine("Skipping card with missing required fields.");
                    continue;
                }

                Console.WriteLine($"Processing card: {card.name} (ID: {card.id})");

                string desc = card.desc?.Replace("'", "''") ?? "";
                int atk = card.atk ?? 0;
                int def = card.def ?? 0;
                int level = card.level ?? 0;
                string race = card.race?.Replace("'", "''") ?? "";
                string attribute = card.attribute?.Replace("'", "''") ?? "";
                string archetype = card.archetype?.Replace("'", "''") ?? "";

                string set_name = "";
                string set_code = "";
                string set_rarity = "";
                string set_price = "";
                if (card.card_sets != null && card.card_sets.Length > 0)
                {
                    var firstSet = card.card_sets[0];
                    set_name = firstSet.set_name?.Replace("'", "''") ?? "";
                    set_code = firstSet.set_code?.Replace("'", "''") ?? "";
                    set_rarity = firstSet.set_rarity?.Replace("'", "''") ?? "";
                    set_price = firstSet.set_price?.Replace("'", "''") ?? "";
                }

                string image_url = "";
                string image_url_small = "";
                if (card.card_images != null && card.card_images.Length > 0)
                {
                    var firstImage = card.card_images[0];
                    image_url = firstImage.image_url ?? "";
                    image_url_small = firstImage.image_url_small ?? "";
                }

                string cardmarket_price = "";
                string tcgplayer_price = "";
                string ebay_price = "";
                string amazon_price = "";
                string coolstuffinc_price = "";
                if (card.card_prices != null && card.card_prices.Length > 0)
                {
                    var firstPrice = card.card_prices[0];
                    cardmarket_price = firstPrice.cardmarket_price ?? "";
                    tcgplayer_price = firstPrice.tcgplayer_price ?? "";
                    ebay_price = firstPrice.ebay_price ?? "";
                    amazon_price = firstPrice.amazon_price ?? "";
                    coolstuffinc_price = firstPrice.coolstuffinc_price ?? "";
                }

                string query = $"INSERT INTO cards (passcode, name, type, description, atk, def, level, race, attribute, archetype, set_name, set_code, set_rarity, set_price, image_url, image_url_small, cardmarket_price, tcgplayer_price, ebay_price, amazon_price, coolstuffinc_price) VALUES ('{card.id}', '{card.name?.Replace("'", "''")}', '{card.type?.Replace("'", "''")}', '{desc}', {atk}, {def}, {level}, '{race}', '{attribute}', '{archetype}', '{set_name}', '{set_code}', '{set_rarity}', '{set_price}', '{image_url}', '{image_url_small}', '{cardmarket_price}', '{tcgplayer_price}', '{ebay_price}', '{amazon_price}', '{coolstuffinc_price}')";

                try
                {
                    db.ExecuteNonQuery(query);
                    Console.WriteLine($"Inserted card: {card.name} (ID: {card.id}) into the database.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error inserting card {card.name} (ID: {card.id}): {ex.Message}");
                }

                count++;
                int progress = (int)((double)count / totalCards * 100);
                progressForm.Invoke(new Action(() => progressForm.UpdateProgress(progress, $"Loading {count}/{totalCards} cards...")));

                Console.Write($"\rLoading {count}/{totalCards} cards... {progress}% completed.");
            }

            Console.WriteLine("\nAll cards have been loaded to the database.");
        }
    }
}
