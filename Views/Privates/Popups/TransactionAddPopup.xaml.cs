using CommunityToolkit.Maui.Views;
using controle_ja_mobile.ViewModels;

namespace controle_ja_mobile.Views.Popups;

public partial class TransactionAddPopup : Popup
{
    public TransactionAddPopup(TransactionAddViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        // A MÁGICA: Avisa o ViewModel quem é a tela dele, para o botão "Salvar" conseguir sumir com esse popup
        viewModel.PopupInstance = this;
    }
}