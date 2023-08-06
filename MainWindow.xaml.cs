using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xaml;
using System.Xml.Serialization;
using TeleTrader.Database;
using TeleTrader.Models;
using Type = TeleTrader.Database.Type;

namespace TeleTrader
{
    public partial class MainWindow : Window
    {
        private OpenFileDialog _openFileDialog;
        private DB _db;
        private DataDialog _dataDialog;
        private DeleteDialog _deleteDialog;
        private ObservableCollection<DataGridItem> _dataItems;
        private List<Symbol> _allSymbols;
        private List<Exchange> _allExchanges;
        private List<Type> _allTypes;
        private Func<Symbol, bool> _typeFilter;
        private Func<Symbol, bool> _exchangeFilter;
        private long _selectedId;

        public MainWindow()
        {
            InitializeComponent();
            InitLocal();
        }

        #region EVENT HANDLERS

        private async void Button_ConnectDatabase(object sender, RoutedEventArgs e)
        {
            if (_openFileDialog.ShowDialog() == true)
            {
                StatusLabel.Text = "CONNECTING";

                try
                {
                    _db = new(_openFileDialog.FileName);
                    Task typesTask = RetrieveTypes();
                    Task exchangeTask = RetrieveExchanges();
                    Task symbolTask = RetrieveSymbols();

                    await Task.WhenAll(typesTask, exchangeTask, symbolTask);
                }
                catch
                {
                    StatusLabel.Text = "CONNECTION FAILED";
                    MessageBox.Show("Invalid path or file.");
                    LockUI();
                    return;
                }

                StatusLabel.Text = "CONNECTED";
                TypeComboBox.SelectedItem = _allTypes[0];
                ExchangeComboBox.SelectedItem = _allExchanges[0];
                UnlockUI();
            }
        }

        private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;

            long id = ((Type)e.AddedItems[0]).Id;

            _typeFilter = x => id < 0 ? true : x.TypeId == id;

            _dataItems = new ObservableCollection<Models.DataGridItem>(_allSymbols.Where(x => x != null && x.Exchange != null && x.Type != null)
                .Where(_typeFilter)
                .Where(_exchangeFilter)
                .Select(x =>
                new Models.DataGridItem { Id = x.Id, Name = x.Name, Price = Math.Round((double)x.Price, 2), Ticker = x.Ticker, ExchangeName = x.Exchange.Name, TypeName = x.Type.Name }));

            DataGrid.ItemsSource = _dataItems;
        }

        private void ExchangeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;

            long id = ((Exchange)e.AddedItems[0]).Id;

            _exchangeFilter = x => id < 0 ? true : x.ExchangeId == id;

            _dataItems = new ObservableCollection<Models.DataGridItem>(_allSymbols.Where(x => x != null && x.Exchange != null && x.Type != null)
                .Where(_typeFilter)
                .Where(_exchangeFilter)
                .Select(x =>
                new Models.DataGridItem { Id = x.Id, Name = x.Name, Price = Math.Round((double)x.Price, 2), Ticker = x.Ticker, ExchangeName = x.Exchange.Name, TypeName = x.Type.Name }));

            DataGrid.ItemsSource = _dataItems;
        }

        private void DataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (e.AddedCells.Count > 0)
            {
                _selectedId = (e.AddedCells[0].Item as DataGridItem).Id;
                EditButton.IsEnabled = true;
                DeleteButton.IsEnabled = true;
                return;
            }

            EditButton.IsEnabled = false;
            DeleteButton.IsEnabled = false;
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            _dataDialog = new DataDialog(new Symbol(), _allExchanges.Where(x => x.Id >= 0).ToList(), _allTypes.Where(x => x.Id >= 0).ToList());
            _dataDialog.ShowDialog();

            while (_dataDialog.Status > 0) // Try to save the symbol
            {
                int r = _db.Symbols.Where(x => x.Ticker == _dataDialog.CurrentSymbol.Ticker).Count();

                if (r == 0 && _dataDialog.CurrentSymbol.Isin != null)
                    r = _db.Symbols.Where(x => x.Isin == _dataDialog.CurrentSymbol.Isin).Count() * (-1); //Negative number determines the source

                if (r != 0) // Call again
                {
                    string source = r > 0 ? "Ticker" : "ISIN";
                    MessageBox.Show($"{source} already exists in database.");
                    _dataDialog.CurrentSymbol.DateAdded = null; //Remove invalid date
                    _dataDialog = new DataDialog(_dataDialog.CurrentSymbol, _allExchanges.Where(x => x.Id >= 0).ToList(), _allTypes.Where(x => x.Id >= 0).ToList(), 1);
                    _dataDialog.ShowDialog();
                } else // Save
                {
                    try 
                    {
                        long newId = _db.Symbols.Max(x => x.Id) + 1;
                        _dataDialog.CurrentSymbol.Id = newId;
                        _db.Symbols.Add(_dataDialog.CurrentSymbol);
                        await _db.SaveChangesAsync();
                        RefreshDataGrid();
                        MessageBox.Show("New symbol added to database.");
                    } catch
                    {
                        MessageBox.Show("An error occured");
                    }
                    break;
                }
            }
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            Symbol sym = null;
            try
            {
                sym = _db.Symbols.Single(x => x.Id == _selectedId);
                _db.Symbols.Entry(sym).State = EntityState.Modified;
                _dataDialog = new DataDialog(sym, _allExchanges.Where(x => x.Id >= 0).ToList(), _allTypes.Where(x => x.Id >= 0).ToList(), 1);
                _dataDialog.ShowDialog();
            } catch
            {
                if (sym != null)
                    _db.Symbols.Entry(sym).State = EntityState.Detached;
                return;
            }

            while (_dataDialog.Status > 0) // Try to save the symbol
            {
                int r = _db.Symbols.Where(x => x.Ticker == _dataDialog.CurrentSymbol.Ticker && x.Id != _selectedId).Count();

                if (r == 0 && _dataDialog.CurrentSymbol.Isin != null)
                    r = _db.Symbols.Where(x => x.Isin == _dataDialog.CurrentSymbol.Isin && x.Id != _selectedId).Count() * (-1); //Negative number determines the source

                if (r != 0) // Call again
                {
                    string source = r > 0 ? "Ticker" : "ISIN";
                    MessageBox.Show($"{source} already exists in database.");
                    _dataDialog.CurrentSymbol.DateAdded = null; //Remove invalid date
                    _dataDialog = new DataDialog(_dataDialog.CurrentSymbol, _allExchanges.Where(x => x.Id >= 0).ToList(), _allTypes.Where(x => x.Id >= 0).ToList(), 1);
                    _dataDialog.ShowDialog();
                }
                else // Save
                {
                    try
                    {
                        await _db.SaveChangesAsync();
                        RefreshDataGrid();
                    }
                    catch
                    {
                        _db.Symbols.Entry(sym).State = EntityState.Detached;
                        MessageBox.Show("An error occured");
                    }
                    break;
                }
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            _deleteDialog = new();
            _deleteDialog.ShowDialog();

            if (_deleteDialog.Delete)
            {
                Symbol sym = null;
                try
                {
                    sym = _db.Symbols.Single(x => x.Id == _selectedId);
                    _db.Symbols.Entry(sym).State = EntityState.Deleted;
                    await _db.SaveChangesAsync();
                    RefreshDataGrid();
                } catch
                {
                    if (sym != null)
                        _db.Symbols.Entry(sym).State = EntityState.Detached;
                    MessageBox.Show("Error, couldn't delete entry.");
                }
            }
        }

        #endregion

        #region LOCAL METHODS

        private void InitLocal()
        {
            _openFileDialog = new();
            _dataItems = new();
            DataGrid.ItemsSource = _dataItems;
            DataGrid.Columns[0].Width = 400;
            _typeFilter = x => true;
            _exchangeFilter = x => true;
            StatusLabel.Text = "NOT CONNECTED";
            LockUI();
        }

        private void LockUI()
        {
            AddButton.IsEnabled = false;
            EditButton.IsEnabled = false;
            DeleteButton.IsEnabled = false;
            TypeComboBox.IsEnabled = false;
            ExchangeComboBox.IsEnabled = false;
            DataGrid.IsEnabled = false;
        }

        private void UnlockUI()
        {
            AddButton.IsEnabled = true;
            TypeComboBox.IsEnabled = true;
            ExchangeComboBox.IsEnabled = true;
            DataGrid.IsEnabled = true;
        }

        private async Task RetrieveTypes()
        {
            _allTypes = await _db.Types.ToListAsync();
            _allTypes.Insert(0, new Type { Id = -1, Name = "All"});
            TypeComboBox.ItemsSource = _allTypes;
        }

        private async Task RetrieveExchanges()
        {
            _allExchanges = await _db.Exchanges.ToListAsync();
            _allExchanges.Insert(0, new Exchange { Id = -1, Name = "All"});
            ExchangeComboBox.ItemsSource = _allExchanges;
        }

        private async Task RetrieveSymbols()
        {
            _allSymbols = await _db.Symbols.Include(x => x.Exchange).Include(x => x.Type).ToListAsync();
            _dataItems = new ObservableCollection<Models.DataGridItem>(_allSymbols.Where(x => x != null && x.Exchange != null && x.Type != null)
                .Select(x =>
                new Models.DataGridItem { Id = x.Id, Name = x.Name, Price = Math.Round((double)x.Price, 2), Ticker = x.Ticker, ExchangeName = x.Exchange.Name, TypeName = x.Type.Name }));
            DataGrid.ItemsSource = _dataItems;
        }

        private async void RefreshDataGrid()
        {
            _allSymbols = await _db.Symbols.Include(x => x.Exchange).Include(x => x.Type).ToListAsync();
            _dataItems = new ObservableCollection<Models.DataGridItem>(_allSymbols
                .Where(x => x != null && x.Exchange != null && x.Type != null)
                .Where(_typeFilter)
                .Where(_exchangeFilter)
                .Select(x =>
                new Models.DataGridItem { Id = x.Id, Name = x.Name, Price = Math.Round((double)x.Price, 2), Ticker = x.Ticker, ExchangeName = x.Exchange.Name, TypeName = x.Type.Name }));
            DataGrid.ItemsSource = _dataItems;
        }

        #endregion
    }
}
