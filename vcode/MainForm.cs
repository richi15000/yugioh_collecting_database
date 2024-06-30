using System;
using System.Data;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YugiohCardManager
{
    public class MainForm : Form
    {
        private string username;
        private DataGridView cardsDataGridView;
        private PictureBox cardPictureBox;
        private Button loadCardsButton;
        private Button newCardButton;
        private Button deleteButton;
        private Button logoutButton;
        private Button refreshButton;
        private Button deleteAllButton;
        private CheckBox selectAllCheckBox;
        private Database db;
        private int currentPage = 1;
        private const int pageSize = 50;

        public MainForm(string username)
        {
            this.username = username;
            db = new Database();

            Text = $"Welcome, {username}";
            WindowState = FormWindowState.Maximized; // Program indítása teljes méretben

            cardsDataGridView = new DataGridView { Left = 10, Top = 10, Width = 960, Height = 400 };
            cardsDataGridView.AllowUserToAddRows = false;
            cardsDataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            cardsDataGridView.MultiSelect = false;
            cardPictureBox = new PictureBox
            {
                Left = 980,
                Top = 10,
                Width = 300,
                Height = 400,
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            cardPictureBox.Click += CardPictureBox_Click; // Kattintási eseménykezelő hozzáadása

            loadCardsButton = new Button { Text = "Load Cards", Left = 230, Top = 420, Width = 100 };
            newCardButton = new Button { Text = "New Card", Left = 340, Top = 420, Width = 100 };
            deleteButton = new Button { Text = "Delete", Left = 450, Top = 420, Width = 100 };
            logoutButton = new Button { Text = "Logout", Left = 560, Top = 420, Width = 100 };
            refreshButton = new Button { Text = "Refresh", Left = 670, Top = 420, Width = 100 };
            deleteAllButton = new Button { Text = "Delete All", Left = 780, Top = 420, Width = 100 };
            selectAllCheckBox = new CheckBox { Text = "Select All", Left = 890, Top = 420, Width = 100 };

            loadCardsButton.Click += async (sender, e) => await LoadCardsAsync();
            newCardButton.Click += NewCardButton_Click;
            deleteButton.Click += async (sender, e) => await DeleteButton_Click(sender, e);
            logoutButton.Click += LogoutButton_Click;
            refreshButton.Click += async (sender, e) => await LoadUserCardsAsync();
            deleteAllButton.Click += async (sender, e) => await DeleteAllButton_Click(sender, e);
            selectAllCheckBox.CheckedChanged += SelectAllCheckBox_CheckedChanged;

            Controls.Add(cardsDataGridView);
            Controls.Add(cardPictureBox);
            Controls.Add(loadCardsButton);
            Controls.Add(newCardButton);
            Controls.Add(deleteButton);
            Controls.Add(logoutButton);
            Controls.Add(refreshButton);
            Controls.Add(deleteAllButton);
            Controls.Add(selectAllCheckBox);

            cardsDataGridView.SelectionChanged += async (sender, e) => await CardsDataGridView_SelectionChanged(sender, e);

            this.Shown += async (sender, e) => await LoadUserCardsAsync();
            this.FormClosing += MainForm_FormClosing;
        }

        private async Task LoadCardsOnStartup()
        {
            this.Hide();
            ProgressForm progressForm = new ProgressForm();
            progressForm.Show();

            await Task.Run(() => CardLoader.LoadCardsToDatabase(db, progressForm));

            progressForm.Close();
            MessageBox.Show("Cards loaded successfully!");

            await LoadUserCardsAsync();
            this.Show(); // Mutatjuk a fő ablakot a betöltés után
        }

        private async Task LoadCardsAsync()
        {
            ProgressForm progressForm = new ProgressForm();
            progressForm.Show();

            await Task.Run(() => CardLoader.LoadCardsToDatabase(db, progressForm));

            progressForm.Close();
            MessageBox.Show("Cards loaded successfully!");
            await LoadUserCardsAsync(); // Frissíti a felhasználói kártyákat a betöltés után
        }

        private async Task LoadUserCardsAsync()
        {
            string query = $"SELECT passcode, quantity, name, type, description, atk, def, level, race, attribute, archetype, card_images FROM user_cards WHERE user_id = (SELECT id FROM users WHERE username = '{username}') LIMIT {(currentPage - 1) * pageSize}, {pageSize}";
            DataTable cards = db.ExecuteQuery(query);
            cards.Columns.Add("Select", typeof(bool));
            cardsDataGridView.DataSource = cards;
            cardsDataGridView.Columns["Select"].DisplayIndex = 0;

            foreach (DataGridViewColumn column in cardsDataGridView.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            if (cards.Rows.Count < pageSize)
            {
                // Disable the Next button if less than 50 rows are loaded
            }
        }

        private async Task CardsDataGridView_SelectionChanged(object? sender, EventArgs e)
        {
            if (cardsDataGridView.SelectedRows.Count > 0)
            {
                var selectedRow = cardsDataGridView.SelectedRows[0];
                string? imageUrl = selectedRow.Cells["card_images"]?.Value?.ToString(); // Megfelelő oszlopnév használata
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    cardPictureBox.Image = await LoadImageAsync(imageUrl);
                }
                else
                {
                    cardPictureBox.Image = null;
                }
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

        private void NewCardButton_Click(object? sender, EventArgs e)
        {
            NewCardForm newCardForm = new NewCardForm(db, username);
            newCardForm.ShowDialog();
            LoadUserCardsAsync();
        }

        private async Task DeleteButton_Click(object? sender, EventArgs e)
        {
            foreach (DataGridViewRow row in cardsDataGridView.Rows)
            {
                if (Convert.ToBoolean(row.Cells["Select"].Value))
                {
                    string? passcode = row.Cells["passcode"]?.Value?.ToString();
                    if (!string.IsNullOrEmpty(passcode))
                    {
                        string query = $"DELETE FROM user_cards WHERE user_id = (SELECT id FROM users WHERE username='{username}') AND passcode = '{passcode}'";
                        try
                        {
                            db.ExecuteNonQuery(query);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error deleting card {row.Cells["name"].Value}: {ex.Message}");
                        }
                    }
                }
            }

            MessageBox.Show("Selected cards deleted successfully!");
            await LoadUserCardsAsync();
        }

        private async Task DeleteAllButton_Click(object? sender, EventArgs e)
        {
            var confirmForm = new Form();
            var usernameTextBox = new TextBox { PlaceholderText = "Username", Left = 10, Top = 10, Width = 200 };
            var passwordTextBox = new TextBox { PlaceholderText = "Password", Left = 10, Top = 40, Width = 200, UseSystemPasswordChar = true };
            var confirmButton = new Button { Text = "Confirm", Left = 10, Top = 70, Width = 200 };

            confirmButton.Click += async (s, ea) =>
            {
                if (usernameTextBox.Text == username && passwordTextBox.Text == "user_password") // replace with actual password verification
                {
                    string query = $"DELETE FROM user_cards WHERE user_id = (SELECT id FROM users WHERE username='{username}')";
                    try
                    {
                        db.ExecuteNonQuery(query);
                        MessageBox.Show("All cards deleted successfully!");
                        confirmForm.Close();
                        await LoadUserCardsAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting all cards: {ex.Message}");
                    }
                }
                else
                {
                    MessageBox.Show("Invalid username or password.");
                }
            };

            confirmForm.Controls.Add(usernameTextBox);
            confirmForm.Controls.Add(passwordTextBox);
            confirmForm.Controls.Add(confirmButton);
            confirmForm.ShowDialog();
        }

        private void SelectAllCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            foreach (DataGridViewRow row in cardsDataGridView.Rows)
            {
                row.Cells["Select"].Value = selectAllCheckBox.Checked;
            }
        }

        private void LogoutButton_Click(object? sender, EventArgs e)
        {
            this.Hide();
            LoginForm loginForm = new LoginForm();
            loginForm.Show();
        }

        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            Application.Exit(); // Biztosítja, hogy a folyamat leálljon
        }

        private void CardPictureBox_Click(object? sender, EventArgs e)
        {
            if (cardPictureBox.Image != null)
            {
                ImageForm imageForm = new ImageForm(cardPictureBox.Image);
                imageForm.ShowDialog();
            }
        }
    }
}
