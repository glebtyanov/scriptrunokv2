using GlobalHotKeys;
using GlobalHotKeys.Native.Types;
using System.Drawing;
using System.Drawing.Imaging;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Size = System.Drawing.Size;

namespace ScriptrunokV2
{
    public partial class MainWindow
    {
        #region Initialization

        private const string _keyFilePath = "./runtimes/win/lib/net8.0/key.txt";
        private bool _isRunning;
        private readonly HotKeyManager _hotKeyManager;
        private CancellationTokenSource? _buyClickerTokenSource;
        private readonly Config _config;

        public MainWindow()
        {
            InitializeComponent();

            ValidateUser();

            _config = new Config();

            Dispatcher.Invoke(() =>
            {
                RefreshRateTextBox.Text = _config.Sleeps.ChastotaZaprosov.ToString();
                DelayAfterBuyTextBox.Text = _config.Sleeps.PosleKupit.ToString();
            });

            // for hotkeys to work globally
            _hotKeyManager = new HotKeyManager();
            _hotKeyManager.Register(VirtualKeyCode.VK_F6, Modifiers.NoRepeat);
            _hotKeyManager.Register(VirtualKeyCode.VK_F7, Modifiers.NoRepeat);
            _hotKeyManager.HotKeyPressed.Subscribe((Action<HotKey>)StartAction);

            User32.AlwaysOnTop(Title);
        }

        #endregion

        #region Login

        private static void ValidateUser()
        {
            // no file means user didnt login yet
            if (File.Exists(_keyFilePath))
            {
                var hwid = File.ReadAllLines(_keyFilePath)[0];
                if (!hwid.Contains(GetHardwareId()))
                {
                    OpenKeyLoginWindow();
                }

                return;
            }

            OpenKeyLoginWindow();
        }

        private static string GetHardwareId()
        {
            ManagementObjectCollection? mbsList = null;
            ManagementObjectSearcher mos = new ManagementObjectSearcher("Select ProcessorID From Win32_processor");
            mbsList = mos.Get();
            string? processorId = string.Empty;
            foreach (ManagementBaseObject mo in mbsList)
            {
                processorId = mo["ProcessorID"] as string;
            }

            mos = new ManagementObjectSearcher("SELECT UUID FROM Win32_ComputerSystemProduct");
            mbsList = mos.Get();
            string? systemId = string.Empty;
            foreach (ManagementBaseObject mo in mbsList)
            {
                systemId = mo["UUID"] as string;
            }

            var compIdStr = $"{processorId}{systemId}";

            return compIdStr;
        }

        private static void OpenKeyLoginWindow()
        {
            var loginWindow = new KeyLoginWindow();

            loginWindow.ShowDialog();

            if (!loginWindow.IsLoginSuccessful)
            {
                Environment.Exit(0);
            }

            File.WriteAllText(_keyFilePath, GetHardwareId());
        }

        #endregion

        #region StartLogic

        private void StartThreads()
        {
            _buyClickerTokenSource = new CancellationTokenSource();
            Task.WhenAll(
                Task.Run(() => BuyClicker(_buyClickerTokenSource.Token)),
                Task.Run(() => RefreshClicker(_buyClickerTokenSource.Token))
            ).ContinueWith(_ => { StartAction(isRestart: true); },
                TaskContinuationOptions.None);
        }

        private void StartAction(HotKey? _ = null)
        {
            try
            {
                if (_isRunning)
                {
                    Dispatcher.Invoke(() =>
                    {
                        IsStarted.Text = "[ОСТАНОВЛЕН]";
                        StartButton.Content = "Старт";
                    });
                    _isRunning = false;
                    _buyClickerTokenSource!.Cancel();

                    return;
                }

                _isRunning = true;
                Dispatcher.Invoke(() =>
                {
                    IsStarted.Text = "[ЗАПУЩЕН]";
                    StartButton.Content = "Стоп";
                });

                StartThreads();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void StartAction(bool isRestart = false)
        {
            try
            {
                if (_isRunning && isRestart)
                {
                    _buyClickerTokenSource!.Cancel();
                    StartThreads();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartAction(null);
        }

        #endregion

        #region Clickers

        private async Task RefreshClicker(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Click(_config.Coords.Galochka);
                await Task.Delay(25, cancellationToken);

                Click(_config.Coords.Galochka);
                await Task.Delay(_config.Sleeps.PosleGalochki, cancellationToken);

                if (!CheckCoordsForColor(_config.Coords.Galochka, _config.Colors.Galochka))
                {
                    Click(_config.Coords.Galochka);
                    await Task.Delay(_config.Sleeps.PosleGalochki, cancellationToken);
                }

                await Task.Delay(_config.Sleeps.ChastotaZaprosov, cancellationToken);
            }
        }

        private async Task BuyClicker(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                for (var i = 0; i < _config.Coords.SlotsColvo; i++)
                {
                    if (!CheckCoordsForColor(
                            [
                                _config.Coords.Lot1Nakleika[0],
                                _config.Coords.Lot1Nakleika[1] + _config.Coords.VisotaSlota * i
                            ],
                            _config.Colors.Fon))
                        continue;

                    Click([_config.Coords.Kupit[0], _config.Coords.Kupit[1] + _config.Coords.VisotaSlota * i]);
                    await Task.Delay(_config.Sleeps.PosleKupit, cancellationToken);

                        Click(_config.Coords.Podtverdit);

                    await Task.Delay(_config.Sleeps.PoslePodtverdit, cancellationToken);

                    Click(_config.Coords.Nazad);
                    await Task.Delay(_config.Sleeps.PosleNazad, cancellationToken);

                    Click(_config.Coords.Galochka);
                    await Task.Delay(25, cancellationToken);

                    Click(_config.Coords.Galochka);
                    await Task.Delay(_config.Sleeps.PosleGalochki, cancellationToken);

                    if (CheckCoordsForColor(_config.Coords.Galochka, _config.Colors.Galochka))
                    {
                        Click(_config.Coords.Galochka);
                        await Task.Delay(_config.Sleeps.PosleGalochki, cancellationToken);
                    }
                    

                    // if (CheckCoordsForColor(_config.Coords.Nazad, _config.Colors.Nazad))
                    // {
                    //     Click(_config.Coords.Nazad);
                    // }
                    //
                    // if (CheckCoordsForColor(_config.Coords.Ok, _config.Colors.Ok))
                    // {
                    //     Click(_config.Coords.Ok);
                    //     await Task.Delay(_config.Sleeps.PoslePodtverdit, cancellationToken);
                    // }
                }
            }
        }

        #endregion

        #region Color detect + Click logic

        // Used to get color of a pixel
        private static readonly Bitmap ScreenPixel = new(1, 1, PixelFormat.Format32bppArgb);
        private static readonly Graphics Graphics = Graphics.FromImage(ScreenPixel);

        private static int GetColorAt(int x, int y)
        {
            // Use a lock statement to ensure thread-safety
            lock (ScreenPixel)
            {
                Graphics.CopyFromScreen(x, y, 0, 0, new Size(1, 1), CopyPixelOperation.SourceCopy);
                return ScreenPixel.GetPixel(0, 0).ToArgb() * -1;
            }
        }

        private static bool CheckCoordsForColor(IReadOnlyList<int> xy, IReadOnlyList<int> range)
        {
            var number = GetColorAt(xy[0], xy[1]);
            if (range.Count <= 2) return number >= range[0] && number <= range[1];

            // Second check for sticker cases (they have 2 color ranges)  
            return (number >= range[0] && number <= range[1]) || (number >= range[2] && number <= range[3]);
        }

        private static void Click(IReadOnlyList<int> xy)
        {
            SetCursorPos(xy[0], xy[1]);
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, xy[0], xy[1], 0, 0);
        }

        // Windows functions and consts
        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;

        #endregion

        #region WPF Logic

        private void MainWindow_OnClosed(object? sender, EventArgs e)
        {
            _hotKeyManager.Dispose();
            Graphics.Dispose();
        }

        private void MainWindow_OnDeactivated(object sender, EventArgs e)
        {
            Window window = (Window)sender;
            window.Topmost = true;
        }


        private void DelayAfterBuyTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (int.TryParse(DelayAfterBuyTextBox.Text, out int delay))
                {
                    _config.Sleeps.PosleKupit = delay;
                }
            });
        }

        private void RefreshRateTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (int.TryParse(RefreshRateTextBox.Text, out int delay))
                {
                    _config.Sleeps.ChastotaZaprosov = delay;
                }
            });
        }

        #endregion
    }
}