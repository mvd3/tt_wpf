using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TeleTrader.Database;
using Type = TeleTrader.Database.Type;

namespace TeleTrader
{
    /// <summary>
    /// Interaction logic for DataDialog.xaml
    /// </summary>
    public partial class DataDialog : Window
    {
        public Symbol CurrentSymbol;
        public int Status; // Tells MainWindows what to do after this dialog is closed

        private const string _codeRegex = "^[A-Z]{3}$";
        private const string _priceRegex = "^\\d+(\\.\\d{2})?$";
        private const string _isinRegex = "^[A-Z]{2}[A-Z0-9]{10}$";

        public DataDialog()
        {
            InitializeComponent();
        }

        public DataDialog(Symbol symbol, List<Exchange> exchanges, List<Type> types, int mode = 0)
        {
            InitializeComponent();
            InitData(symbol, exchanges, types, mode);
        }

        #region EVENT HANDLERS

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (SymbolNameTextBox.Text.Length == 0)
            {
                MessageBox.Show("Symbol name field can't be empty");
                return;
            }
            if (TickerTextBox.Text.Length == 0)
            {
                MessageBox.Show("Ticker field can't be empty.");
                return;
            }
            if (!Regex.IsMatch(CurrencyTextBox.Text, _codeRegex))
            {
                MessageBox.Show("Invalid currency code.");
                return;
            }
            if (!Regex.IsMatch(PriceTextBox.Text, _priceRegex))
            {
                MessageBox.Show("Invalid price value.");
                return;
            }
            if (!PriceDatePicker.SelectedDate.HasValue || DateTime.Compare(PriceDatePicker.SelectedDate.Value, DateTime.Now) > 0)
            {
                MessageBox.Show("Invalid price date.");
                return;
            }
            if (!Regex.IsMatch(IsinTextBox.Text, _isinRegex))
            {
                MessageBox.Show("Invalid ISIN code.");
                return;
            }

            CurrentSymbol.Name = SymbolNameTextBox.Text;
            CurrentSymbol.Ticker = TickerTextBox.Text;
            CurrentSymbol.ExchangeId = ((Exchange)ExchangeComboBox.SelectedItem).Id;
            CurrentSymbol.TypeId = ((Type)TypeComboBox.SelectedItem).Id;
            CurrentSymbol.CurrencyCode = CurrencyTextBox.Text.Length > 0 ? CurrencyTextBox.Text : null;
            CurrentSymbol.Price = PriceTextBox.Text.Length > 0 ? Double.Parse(PriceTextBox.Text) : null;
            CurrentSymbol.PriceDate = PriceDatePicker.SelectedDate.Value;
            CurrentSymbol.Isin = IsinTextBox.Text.Length > 0 ? IsinTextBox.Text : null;
            if (CurrentSymbol.DateAdded == null)
            {
                CurrentSymbol.DateAdded = DateTime.Now;
            }


            Status = 1; // Save modifications
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion

        private void InitData(Symbol symbol, List<Exchange> exchanges, List<Type> types, int mode = 0)
        {
            Status = -1; // Don't modify anything
            CurrentSymbol = symbol;
            ExchangeComboBox.ItemsSource = exchanges;
            ExchangeComboBox.SelectedIndex = 0;
            TypeComboBox.ItemsSource = types;
            TypeComboBox.SelectedIndex = 0;

            if (mode > 0) // mode is used when preexisting data needs to be fetched to input field
            {
                SymbolNameTextBox.Text = symbol.Name;
                TickerTextBox.Text = symbol.Ticker;
                ExchangeComboBox.SelectedIndex = exchanges.FindIndex(x => x.Id == symbol.ExchangeId);
                TypeComboBox.SelectedIndex = types.FindIndex(x => x.Id == symbol.TypeId);
                CurrencyTextBox.Text = symbol.CurrencyCode ?? "";
                PriceTextBox.Text = symbol.Price.HasValue ? symbol.Price.ToString() : "";
                IsinTextBox.Text = symbol.Isin ?? "";
                if (symbol.PriceDate.HasValue)
                    PriceDatePicker.SelectedDate = symbol.PriceDate;
            }
        }
    }
}
