using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace YugiohCardManager
{
    public class RegisterForm : Form
    {
        private TextBox usernameTextBox;
        private TextBox passwordTextBox;
        private TextBox emailTextBox;
        private Button registerButton;
        private Label messageLabel;
        private Database db;

        public RegisterForm()
        {
            db = new Database();

            usernameTextBox = new TextBox { PlaceholderText = "Username", Left = 10, Top = 10, Width = 200 };
            passwordTextBox = new TextBox { PlaceholderText = "Password", Left = 10, Top = 40, Width = 200, UseSystemPasswordChar = true };
            emailTextBox = new TextBox { PlaceholderText = "Email", Left = 10, Top = 70, Width = 200 };
            registerButton = new Button { Text = "Register", Left = 10, Top = 100, Width = 200 };
            messageLabel = new Label { Left = 10, Top = 130, Width = 400 }; // Növeljük a szélességet

            registerButton.Click += RegisterButton_Click;

            Controls.Add(usernameTextBox);
            Controls.Add(passwordTextBox);
            Controls.Add(emailTextBox);
            Controls.Add(registerButton);
            Controls.Add(messageLabel);
        }

        private void RegisterButton_Click(object? sender, EventArgs e)
        {
            string username = usernameTextBox.Text;
            string password = passwordTextBox.Text;
            string email = emailTextBox.Text;
            string hashedPassword = HashPassword(password);

            string query = $"INSERT INTO users (username, password, email) VALUES ('{username}', '{hashedPassword}', '{email}')";
            try
            {
                db.ExecuteNonQuery(query);
                messageLabel.Text = "Registration successful!";
            }
            catch (MySqlException ex)
            {
                // Részletes hibaüzenet megjelenítése
                messageLabel.Text = $"Registration failed: {ex.Message}";
            }
            catch (Exception ex)
            {
                messageLabel.Text = $"An error occurred: {ex.Message}";
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                var builder = new System.Text.StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
