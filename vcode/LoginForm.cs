using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace YugiohCardManager
{
    public class LoginForm : Form
    {
        private TextBox usernameTextBox;
        private TextBox passwordTextBox;
        private Button loginButton;
        private Button registerButton;
        private Label messageLabel;
        private Database db;

        public LoginForm()
        {
            db = new Database();

            this.Text = "Yugioh Card Manager - Login";
            this.MinimumSize = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormClosing += LoginForm_FormClosing!;
            this.Resize += LoginForm_Resize;

            usernameTextBox = new TextBox { PlaceholderText = "Username", Width = 200 };
            passwordTextBox = new TextBox { PlaceholderText = "Password", Width = 200, UseSystemPasswordChar = true };
            loginButton = new Button { Text = "Login", Width = 200 };
            registerButton = new Button { Text = "Register", Width = 200 };
            messageLabel = new Label { Width = 400 };

            loginButton.Click += LoginButton_Click!;
            registerButton.Click += RegisterButton_Click!;

            Controls.Add(usernameTextBox);
            Controls.Add(passwordTextBox);
            Controls.Add(loginButton);
            Controls.Add(registerButton);
            Controls.Add(messageLabel);

            CenterControls();
        }

        private void CenterControls()
        {
            int centerX = this.ClientSize.Width / 2;
            int totalHeight = usernameTextBox.Height + passwordTextBox.Height + loginButton.Height + registerButton.Height + messageLabel.Height + 40;
            int startY = (this.ClientSize.Height - totalHeight) / 2;

            usernameTextBox.Left = centerX - usernameTextBox.Width / 2;
            usernameTextBox.Top = startY;

            passwordTextBox.Left = centerX - passwordTextBox.Width / 2;
            passwordTextBox.Top = usernameTextBox.Top + usernameTextBox.Height + 10;

            loginButton.Left = centerX - loginButton.Width / 2;
            loginButton.Top = passwordTextBox.Top + passwordTextBox.Height + 10;

            registerButton.Left = centerX - registerButton.Width / 2;
            registerButton.Top = loginButton.Top + loginButton.Height + 10;

            messageLabel.Left = centerX - messageLabel.Width / 2;
            messageLabel.Top = registerButton.Top + registerButton.Height + 10;
        }

        private void LoginForm_Resize(object? sender, EventArgs e)
        {
            CenterControls();
        }

        private void LoginForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit(); // Biztosítja, hogy a folyamat leálljon
        }

        private void LoginButton_Click(object? sender, EventArgs e)
        {
            string username = usernameTextBox.Text;
            string password = passwordTextBox.Text;
            string hashedPassword = HashPassword(password);

            string query = $"SELECT * FROM users WHERE username = '{username}' AND password = '{hashedPassword}'";
            DataTable user = db.ExecuteQuery(query);

            if (user.Rows.Count > 0)
            {
                messageLabel.Text = "Login successful!";
                // További lépések, például átnavigálás a fő alkalmazásra
                var mainForm = new MainForm(username);
                mainForm.Show();
                this.Hide();
            }
            else
            {
                messageLabel.Text = "Invalid username or password.";
            }
        }

        private void RegisterButton_Click(object? sender, EventArgs e)
        {
            var registerForm = new RegisterForm();
            registerForm.ShowDialog();
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
