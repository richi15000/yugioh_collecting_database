using System;
using System.Data;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YugiohCardManager
{
    public class NewCardForm : Form
    {
        private TextBox passcodeTextBox;
        private TextBox cardSetsTextBox;
        private TextBox nameTextBox;
        private TextBox quantityTextBox;
        private Button addButton;
        private PictureBox cardPictureBox;
        private Database db;
        private string username;

        public NewCardForm(Database db, string username)
        {
            this.db = db;
            this.username = username;

            Text = "Add New Card";
            this.MinimumSize = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Resize += NewCardForm_Resize!;
            this.BackColor = Color.White;

            passcodeTextBox = new TextBox { PlaceholderText = "Passcode", Width = 200 };
            cardSetsTextBox = new TextBox { PlaceholderText = "Card Sets Code", Width = 200 };
            nameTextBox = new TextBox { PlaceholderText = "Name", Width = 200 };
            quantityTextBox = new TextBox { PlaceholderText = "Quantity", Width = 200 };
            addButton = new Button { Text = "Add", Width = 200 };
            cardPictureBox = new PictureBox
            {
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right
            };

            addButton.Click += AddButton_Click;

            Controls.Add(passcodeTextBox);
            Controls.Add(cardSetsTextBox);
            Controls.Add(nameTextBox);
            Controls.Add(quantityTextBox);
            Controls.Add(addButton);
            Controls.Add(cardPictureBox);

            ArrangeControls();
        }

        private void NewCardForm_Resize(object? sender, EventArgs e)
        {
            ArrangeControls();
        }

        private void ArrangeControls()
        {
            int verticalSpacing = 10;
            int controlWidth = 200;
            int controlHeight = 30;
            int leftMargin = 10;

            passcodeTextBox.Width = controlWidth;
            passcodeTextBox.Height = controlHeight;
            passcodeTextBox.Left = leftMargin;
            passcodeTextBox.Top = verticalSpacing;

            cardSetsTextBox.Width = controlWidth;
            cardSetsTextBox.Height = controlHeight;
            cardSetsTextBox.Left = leftMargin;
            cardSetsTextBox.Top = passcodeTextBox.Bottom + verticalSpacing;

            nameTextBox.Width = controlWidth;
            nameTextBox.Height = controlHeight;
            nameTextBox.Left = leftMargin;
            nameTextBox.Top = cardSetsTextBox.Bottom + verticalSpacing;

            quantityTextBox.Width = controlWidth;
            quantityTextBox.Height = controlHeight;
            quantityTextBox.Left = leftMargin;
            quantityTextBox.Top = nameTextBox.Bottom + verticalSpacing;

            addButton.Width = controlWidth;
            addButton.Height = controlHeight;
            addButton.Left = leftMargin;
            addButton.Top = quantityTextBox.Bottom + verticalSpacing;

            cardPictureBox.Width = this.ClientSize.Width / 3;
            cardPictureBox.Height = this.ClientSize.Height - 2 * verticalSpacing;
            cardPictureBox.Left = this.ClientSize.Width - cardPictureBox.Width - verticalSpacing;
            cardPictureBox.Top = (this.ClientSize.Height - cardPictureBox.Height) / 2;
        }

        private async void AddButton_Click(object? sender, EventArgs e)
        {
            string passcode = passcodeTextBox.Text;
            string cardSets = cardSetsTextBox.Text;
            string name = nameTextBox.Text;
            int quantity = int.TryParse(quantityTextBox.Text, out var qty) ? qty : 0;

            if ((string.IsNullOrEmpty(passcode) && string.IsNullOrEmpty(cardSets) && string.IsNullOrEmpty(name)) || quantity <= 0)
            {
                MessageBox.Show("Please enter a valid passcode, card sets code, or name and quantity.");
                return;
            }

            string query = "";
            DataTable cardData;

            if (!string.IsNullOrEmpty(passcode))
            {
                query = $"SELECT * FROM cards WHERE passcode = '{passcode}'";
            }
            else if (!string.IsNullOrEmpty(cardSets))
            {
                query = $"SELECT * FROM cards WHERE set_code = '{cardSets}'";
            }
            else if (!string.IsNullOrEmpty(name))
            {
                query = $"SELECT * FROM cards WHERE name = '{name}'";
            }

            cardData = db.ExecuteQuery(query);

            if (cardData.Rows.Count == 0)
            {
                MessageBox.Show("Card not found in the database.");
                return;
            }

            DataRow card = cardData.Rows[0];
            string cardName = card["name"]?.ToString() ?? "";
            string type = card["type"]?.ToString() ?? "";
            string description = card["description"]?.ToString() ?? "";
            int atk = int.TryParse(card["atk"]?.ToString(), out var atkValue) ? atkValue : 0;
            int def = int.TryParse(card["def"]?.ToString(), out var defValue) ? defValue : 0;
            int level = int.TryParse(card["level"]?.ToString(), out var levelValue) ? levelValue : 0;
            string race = card["race"]?.ToString() ?? "";
            string attribute = card["attribute"]?.ToString() ?? "";
            string archetype = card["archetype"]?.ToString() ?? "";
            string imageUrl = card["image_url"]?.ToString() ?? "";

            // Kép betöltése a PictureBox-ba
            cardPictureBox.Image = await LoadImageAsync(imageUrl);

            query = $"INSERT INTO user_cards (user_id, passcode, card_sets, quantity, name, type, description, atk, def, level, race, attribute, archetype, card_images) " +
                    $"VALUES ((SELECT id FROM users WHERE username = '{username}'), '{passcode}', '{cardSets}', {quantity}, '{cardName}', '{type}', '{description}', {atk}, {def}, {level}, '{race}', '{attribute}', '{archetype}', '{imageUrl}')";

            try
            {
                db.ExecuteNonQuery(query);
                MessageBox.Show("Card added successfully!");
                // Form bezárása elhagyva
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private async Task<Image?> LoadImageAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var stream = await response.Content.ReadAsStreamAsync();
                    return Image.FromStream(stream);
                }
                return null;
            }
        }
    }
}
