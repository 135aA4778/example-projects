using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ModernCalculator
{
    public partial class MainWindow : Window
    {
        // Fields to store previous values and states
        private double _lastValue = 0;
        private string _operator = "";
        private bool _isOperatorClicked = false;
        private bool _isFullScreen = false;
        private bool _isNightMode = true; // Default to night mode

        // Fields to store previous window properties for full screen toggle
        private WindowState _previousWindowState;
        private WindowStyle _previousWindowStyle;
        private ResizeMode _previousResizeMode;

        public MainWindow()
        {
            InitializeComponent();
            UpdateTheme();
            UpdateCalcButtonSizes(_isFullScreen);
            UpdateDisplayFontSize(_isFullScreen);
        }

        // Updates the sizes and margins of calculator buttons based on full screen mode.
        private void UpdateCalcButtonSizes(bool fullScreen)
        {
            double newSize = fullScreen ? 100 : 60;
            Thickness newMargin = fullScreen ? new Thickness(1) : new Thickness(4);
            double newFontSize = fullScreen ? 32 : 24;

            foreach (UIElement child in CalcButtonsGrid.Children)
            {
                Button btn = child as Button;
                if (btn != null)
                {
                    btn.Width = newSize;
                    btn.Height = newSize;
                    btn.Margin = newMargin;
                    btn.FontSize = newFontSize;
                }
            }
        }

        // Updates the font size of the display TextBox.
        private void UpdateDisplayFontSize(bool fullScreen)
        {
            txtDisplay.FontSize = fullScreen ? 64 : 48;
        }

        // Handles KeyDown event for the display TextBox by redirecting to the window's PreviewKeyDown.
        private void Display_KeyDown(object sender, KeyEventArgs e)
        {
            Window_PreviewKeyDown(sender, e);
        }

        // Button click event for numeric buttons.
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                // If display is "0" or an operator was just clicked, clear the display.
                if (txtDisplay.Text == "0" || _isOperatorClicked)
                {
                    txtDisplay.Text = "";
                    _isOperatorClicked = false;
                }
                txtDisplay.Text += button.Content.ToString();
            }
        }

        // Button click event for the decimal point button.
        private void DecimalButton_Click(object sender, RoutedEventArgs e)
        {
            if (!txtDisplay.Text.Contains("."))
            {
                // If an operator was just clicked or the display is "0" or empty, start with "0."
                if (_isOperatorClicked || txtDisplay.Text == "0" || string.IsNullOrEmpty(txtDisplay.Text))
                {
                    txtDisplay.Text = "0.";
                    _isOperatorClicked = false;
                }
                else
                {
                    txtDisplay.Text += ".";
                }
            }
        }

        // Button click event for operator buttons (+, -, *, /).
        private void Operation_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                double value;
                // Parse the current display using InvariantCulture to ensure '.' is used as decimal separator.
                double.TryParse(txtDisplay.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
                _lastValue = value;
                _operator = button.Content.ToString();
                _isOperatorClicked = true;
            }
        }

        // Button click event for the percentage button.
        private void Percent_Click(object sender, RoutedEventArgs e)
        {
            double value;
            if (double.TryParse(txtDisplay.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
            {
                if (!string.IsNullOrEmpty(_operator) && _lastValue != 0)
                {
                    // Calculate percentage based on the previous value.
                    txtDisplay.Text = (_lastValue * (value / 100)).ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    // Otherwise, simply divide the current value by 100.
                    txtDisplay.Text = (value / 100).ToString(CultureInfo.InvariantCulture);
                }
                _isOperatorClicked = true;
            }
        }

        // Button click event for the equals button.
        private void Equals_Click(object sender, RoutedEventArgs e)
        {
            double value;
            double.TryParse(txtDisplay.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
            double newValue = value;
            double result = 0;

            switch (_operator)
            {
                case "+":
                    result = _lastValue + newValue;
                    break;
                case "-":
                    result = _lastValue - newValue;
                    break;
                case "*":
                    result = _lastValue * newValue;
                    break;
                case "/":
                    if (newValue != 0)
                        result = _lastValue / newValue;
                    else
                    {
                        MessageBox.Show("Divide by zero is not allowed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        result = 0;
                    }
                    break;
                default:
                    result = newValue;
                    break;
            }
            txtDisplay.Text = result.ToString(CultureInfo.InvariantCulture);
            _lastValue = result;
            _operator = "";
            _isOperatorClicked = false;
        }

        // Button click event for the clear button.
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            txtDisplay.Text = "0";
            _lastValue = 0;
            _operator = "";
            _isOperatorClicked = false;
        }

        // Button click event for the negate button.
        private void Negate_Click(object sender, RoutedEventArgs e)
        {
            double value;
            if (double.TryParse(txtDisplay.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
            {
                value = -value;
                txtDisplay.Text = value.ToString(CultureInfo.InvariantCulture);
            }
        }

        // Handles all key presses at the window level.
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F11)
            {
                ToggleFullScreen();
                e.Handled = true;
            }
            else if ((e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9))
            {
                string digit = "";
                if (e.Key >= Key.D0 && e.Key <= Key.D9)
                    digit = (e.Key - Key.D0).ToString();
                else if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
                    digit = (e.Key - Key.NumPad0).ToString();
                if (txtDisplay.Text == "0" || _isOperatorClicked)
                {
                    txtDisplay.Text = "";
                    _isOperatorClicked = false;
                }
                txtDisplay.Text += digit;
                e.Handled = true;
            }
            else if (e.Key == Key.OemPeriod || e.Key == Key.Decimal || e.Key == Key.OemComma)
            {
                if (!txtDisplay.Text.Contains("."))
                {
                    if (_isOperatorClicked || txtDisplay.Text == "0" || string.IsNullOrEmpty(txtDisplay.Text))
                    {
                        txtDisplay.Text = "0.";
                        _isOperatorClicked = false;
                    }
                    else
                    {
                        txtDisplay.Text += ".";
                    }
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Add || (e.Key == Key.OemPlus && (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift))
            {
                SimulateOperator("+");
                e.Handled = true;
            }
            else if (e.Key == Key.Subtract || e.Key == Key.OemMinus)
            {
                SimulateOperator("-");
                e.Handled = true;
            }
            else if (e.Key == Key.Multiply)
            {
                SimulateOperator("*");
                e.Handled = true;
            }
            else if (e.Key == Key.Divide)
            {
                SimulateOperator("/");
                e.Handled = true;
            }
            else if ((e.Key == Key.D5 && (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift) || e.Key == Key.Oem5)
            {
                Percent_Click(null, null);
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                Equals_Click(null, null);
                e.Handled = true;
            }
            else if (e.Key == Key.C)
            {
                Clear_Click(null, null);
                e.Handled = true;
            }
            else if (e.Key == Key.N)
            {
                Negate_Click(null, null);
                e.Handled = true;
            }
            else if (e.Key == Key.Back)
            {
                if (txtDisplay.Text.Length > 0)
                {
                    txtDisplay.Text = txtDisplay.Text.Substring(0, txtDisplay.Text.Length - 1);
                    if (string.IsNullOrEmpty(txtDisplay.Text))
                        txtDisplay.Text = "0";
                }
                e.Handled = true;
            }
        }

        // Helper method to simulate operator button press via keyboard.
        private void SimulateOperator(string op)
        {
            double value;
            double.TryParse(txtDisplay.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
            _lastValue = value;
            _operator = op;
            _isOperatorClicked = true;
        }

        // Toggles full screen mode.
        private void ToggleFullScreen()
        {
            if (!_isFullScreen)
            {
                _previousWindowState = this.WindowState;
                _previousWindowStyle = this.WindowStyle;
                _previousResizeMode = this.ResizeMode;
                this.WindowStyle = WindowStyle.None;
                this.ResizeMode = ResizeMode.NoResize;
                this.WindowState = WindowState.Maximized;
                MainBorder.Margin = new Thickness(0);
                MainContentBorder.Padding = new Thickness(20);
                _isFullScreen = true;
            }
            else
            {
                this.WindowStyle = _previousWindowStyle;
                this.ResizeMode = _previousResizeMode;
                this.WindowState = _previousWindowState;
                MainBorder.Margin = new Thickness(20);
                MainContentBorder.Padding = new Thickness(20);
                _isFullScreen = false;
            }
            UpdateCalcButtonSizes(_isFullScreen);
            UpdateDisplayFontSize(_isFullScreen);
        }

        // Minimizes the window.
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        // Toggles full screen mode when the full screen button is clicked.
        private void FullScreenButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleFullScreen();
        }

        // Closes the window.
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Enables dragging the window by the title bar.
        private void TopBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        // Toggles between night and day themes.
        private void ThemeToggleButton_Click(object sender, RoutedEventArgs e)
        {
            _isNightMode = !_isNightMode;
            UpdateTheme();
        }

        // Updates the theme colors of the window.
        private void UpdateTheme()
        {
            if (_isNightMode)
            {
                TitleBar.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1F1F1F"));
                TitleText.Foreground = new SolidColorBrush(Colors.White);
                LinearGradientBrush nightBrush = new LinearGradientBrush();
                nightBrush.StartPoint = new System.Windows.Point(0, 0);
                nightBrush.EndPoint = new System.Windows.Point(1, 1);
                nightBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FF2C3E50"), 0));
                nightBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FF34495E"), 1));
                MainContentBorder.Background = nightBrush;
                ThemeToggleButton.Foreground = Brushes.White;
                MinimizeButton.Foreground = Brushes.White;
                FullScreenButton.Foreground = Brushes.White;
                CloseButton.Foreground = Brushes.White;
                ThemeToggleButton.Content = "☀";
            }
            else
            {
                TitleBar.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF0F0F0"));
                TitleText.Foreground = new SolidColorBrush(Colors.Black);
                LinearGradientBrush dayBrush = new LinearGradientBrush();
                dayBrush.StartPoint = new System.Windows.Point(0, 0);
                dayBrush.EndPoint = new System.Windows.Point(1, 1);
                dayBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FFF9F9F9"), 0));
                dayBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FFF0F0F0"), 0.5));
                dayBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FFEDEDED"), 1));
                MainContentBorder.Background = dayBrush;
                ThemeToggleButton.Foreground = Brushes.Black;
                MinimizeButton.Foreground = Brushes.Black;
                FullScreenButton.Foreground = Brushes.Black;
                CloseButton.Foreground = Brushes.Black;
                ThemeToggleButton.Content = "🌙";
            }
        }
    }
}
