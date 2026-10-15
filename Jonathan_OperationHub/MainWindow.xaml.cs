
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Win32;

namespace OperationsHub
{
    public class MenuProduct
    {
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public string ImagePath { get; set; } = "";

        public string DisplayName => $"{Name} - {Price:C}";
    }

    public partial class MainWindow : Window
    {
        public ObservableCollection<MenuProduct> MenuItems { get; }
            = new ObservableCollection<MenuProduct>();

        public ObservableCollection<MenuProduct> OrderItems { get; }
            = new ObservableCollection<MenuProduct>();

        private readonly DispatcherTimer countdown = new DispatcherTimer();
        private int secondsRemaining = 15 * 60;

        private const string SpotifyPlaylist =
            "https://open.spotify.com/playlist/6c8Az3i0rhio5ifT0ZnIY7";

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;
            lstItems.ItemsSource = OrderItems;

            // Load menu pictures from the Images folder.
            string imagesFolder = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Images");

            AddProduct("Cookies", 3.00m, "Cookies.jpg", imagesFolder);
            AddProduct("Coffee", 4.00m, "Coffee.jpg", imagesFolder);
            AddProduct("Grilled Chicken", 9.00m, "Grilled Chicken.jpg", imagesFolder);
            AddProduct("Arizona", 2.00m, "Arizona.jpg", imagesFolder);
            AddProduct("Chips", 2.00m, "Chips.jpg", imagesFolder);
            AddProduct("Tulip Bouquet", 12.00m, "Tulip Bouquet.jpg", imagesFolder);
            AddProduct("Water", 1.00m, "Water.jpg", imagesFolder);
            AddProduct("Fries", 3.50m, "Fries.jpg", imagesFolder);

            // Set up the countdown timer.
            countdown.Interval = TimeSpan.FromSeconds(1);
            countdown.Tick += Countdown_Tick;

            UpdateTotal();
            UpdateTimerDisplay();
        }

        // Add a product to the menu.
        private void AddProduct(
            string name,
            decimal price,
            string imageFile,
            string imagesFolder)
        {
            string imagePath = Path.Combine(imagesFolder, imageFile);

            MenuItems.Add(new MenuProduct
            {
                Name = name,
                Price = price,
                ImagePath = File.Exists(imagePath) ? imagePath : ""
            });
        }

        // Add a menu item to the order.
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button &&
                button.Tag is MenuProduct product)
            {
                OrderItems.Add(product);
                UpdateTotal();

                txtStatus.Text = $"{product.Name} added to your order! ♡";
            }
        }

        // Remove the selected item from the order.
        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (lstItems.SelectedItem is MenuProduct product)
            {
                OrderItems.Remove(product);
                UpdateTotal();

                txtStatus.Text = $"{product.Name} removed from your order.";
            }
            else
            {
                txtStatus.Text = "Select an item to remove.";
            }
        }

        // Clear the entire order.
        private void ClearOrder_Click(object sender, RoutedEventArgs e)
        {
            OrderItems.Clear();
            UpdateTotal();

            txtStatus.Text = "Your order has been cleared.";
        }

        // Calculate and display the order total.
        private void UpdateTotal()
        {
            decimal total = OrderItems.Sum(item => item.Price);
            txtTotal.Text = $"Total: {total:C}";
        }

        // Place an order and start the 15-minute countdown.
        private void PlaceOrder_Click(object sender, RoutedEventArgs e)
        {
            if (OrderItems.Count == 0)
            {
                txtStatus.Text = "Please add something to your order first! ♡";
                return;
            }

            secondsRemaining = 15 * 60;
            UpdateTimerDisplay();

            countdown.Start();

            txtStatus.Text = "Order placed! Your food is being prepared. ♡";
        }

        // Start or pause the countdown.
        private void Timer_Click(object sender, RoutedEventArgs e)
        {
            if (countdown.IsEnabled)
            {
                countdown.Stop();
                txtStatus.Text = "Timer paused.";
            }
            else if (secondsRemaining > 0)
            {
                countdown.Start();
                txtStatus.Text = "Your order timer is running! ♡";
            }
            else
            {
                txtStatus.Text = "Time is up! Please place a new order or reset the timer.";
            }
        }

        // Reset the countdown to 15 minutes.
        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            countdown.Stop();

            secondsRemaining = 15 * 60;
            UpdateTimerDisplay();

            txtStatus.Text = "Timer reset to 15 minutes.";
        }

        // Update the timer every second.
        private void Countdown_Tick(object? sender, EventArgs e)
        {
            if (secondsRemaining > 0)
            {
                secondsRemaining--;
                UpdateTimerDisplay();
            }

            if (secondsRemaining <= 0)
            {
                countdown.Stop();
                secondsRemaining = 0;
                UpdateTimerDisplay();

                txtStatus.Text = "Your order should be ready! Enjoy! ♡";
            }
        }

        // Display the countdown as minutes and seconds.
        private void UpdateTimerDisplay()
        {
            int minutes = secondsRemaining / 60;
            int seconds = secondsRemaining % 60;

            txtTimer.Text = $"{minutes:00}:{seconds:00}";
        }

        // Open the restaurant playlist in Spotify or the browser.
        private void Spotify_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = SpotifyPlaylist,
                    UseShellExecute = true
                });
            }
            catch (Exception)
            {
                txtStatus.Text = "Couldn't open Spotify.";
            }
        }

        // Export team notes to a text file.
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Title = "Export Team Notes",
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                FileName = "Lov3rsPlaza_TeamNotes.txt"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(dialog.FileName, txtDocument.Text);
                    txtStatus.Text = "Team notes exported successfully! ♡";
                }
                catch (Exception)
                {
                    txtStatus.Text = "Couldn't export the notes.";
                }
            }
        }

        // Reopen previously saved team notes.
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Reopen Team Notes",
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    txtDocument.Text = File.ReadAllText(dialog.FileName);
                    txtStatus.Text = "Team notes opened successfully! ♡";
                }
                catch (Exception)
                {
                    txtStatus.Text = "Couldn't open the notes.";
                }
            }
        }
    }
}