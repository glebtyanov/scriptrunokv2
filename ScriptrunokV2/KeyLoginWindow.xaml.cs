using System.Net;
using System.Net.Http;
using System.Windows;

namespace ScriptrunokV2
{
    public partial class KeyLoginWindow : Window
    {
        private const string
            ServerApiUrl =
                "https://scriptrunok-key-manager.up.railway.app/api/Key"; // Replace with your actual server API URL

        public bool IsLoginSuccessful { get; private set; } = false;

        public KeyLoginWindow()
        {
            InitializeComponent();
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            var key = KeyTextBox.Text.Trim();

            if (!string.IsNullOrEmpty(key))
            {
                var isKeyValid = await ValidateKeyWithServerAsync(key);

                if (!isKeyValid)
                {
                    return;
                }

                IsLoginSuccessful = true;
                Close();
            }
            else
            {
                MessageBox.Show("Введите ключ");
            }
        }

        private static async Task<bool> ValidateKeyWithServerAsync(string key)
        {
            using var client = new HttpClient();

            try
            {
                var response = await client.PostAsync($"{ServerApiUrl}/activate?key={key}",
                    new StringContent(string.Empty));

                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                    {
                        return true;
                    }

                    case HttpStatusCode.NotFound:
                    {
                        MessageBox.Show("Ключ не найден");
                        return false;
                    }

                    case HttpStatusCode.Conflict:
                    {
                        MessageBox.Show("Ключ уже активирован");
                        return false;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            catch
            {
                MessageBox.Show("Проблемы с сетью. Проверьте свое соединение.");
                return false;
            }

            
        }
    }
}