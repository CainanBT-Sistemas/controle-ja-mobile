using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using controle_ja_mobile.Helpers;
using controle_ja_mobile.Models;
using System.Collections.ObjectModel;

namespace controle_ja_mobile.ViewModels
{
    public partial class ItemSelectionViewModel : BaseViewModel, IQueryAttributable
    {
        [ObservableProperty] private string pageTitle = "Selecione";
        public ObservableCollection<SelectionItem> Items { get; } = new();

        [ObservableProperty] private SelectionItem currentSelection;

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("Title")) PageTitle = query["Title"].ToString();

            if (query.ContainsKey("Items") && query["Items"] is IEnumerable<SelectionItem> items)
            {
                Items.Clear();
                foreach (var item in items) Items.Add(item);
            }
        }

        [RelayCommand]
        public void ToggleExpand(SelectionItem item)
        {
            if (item.HasSubItems) item.IsExpanded = !item.IsExpanded;
        }

        [RelayCommand]
        public void SelectItem(SelectionItem item)
        {
            // Limpa a seleção de todos (pais e filhos)
            foreach (var i in Items)
            {
                i.IsSelected = false;
                foreach (var sub in i.SubItems) sub.IsSelected = false;
            }

            // Marca apenas o clicado
            item.IsSelected = true;
            CurrentSelection = item;
        }

        [RelayCommand]
        public async Task ConfirmSelection()
        {
            if (CurrentSelection == null)
            {
                await App.Current.MainPage.DisplayAlert("Aviso", "Selecione um item para continuar.", "OK");
                return;
            }

            // Devolve o objeto original via mensageria e fecha a tela
            WeakReferenceMessenger.Default.Send(new ItemSelectedMessage(CurrentSelection.OriginalObject));
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        public async Task Close() => await Shell.Current.GoToAsync("..");
    }
}