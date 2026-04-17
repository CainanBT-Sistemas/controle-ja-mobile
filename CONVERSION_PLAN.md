# Plano de Conversão: .NET MAUI → Android Nativo

## Objetivo
Converter o projeto **Controle Já Mobile** de .NET MAUI para Android Nativo (Android Studio / Kotlin).

## Estrutura Atual do Projeto MAUI

### Arquitetura
- Padrão **MVVM** com CommunityToolkit.Mvvm
- Navegação via **Shell** (AppShell.xaml)
- Injeção de dependências em MauiProgram.cs

### Módulos Funcionais
| Módulo | Serviço | ViewModel | Views |
|--------|---------|-----------|-------|
| Autenticação | AuthService, BiometricAuthService | LoginViewModel, RegisterViewModel | LoginPage, RegisterPage, WelcomePage |
| Dashboard | DashboardService | DashboardViewModel | HomeView (Tab) |
| Transações | TransactionService | TransactionsViewModel, TransactionAddViewModel | TransactionsListView, TransactionAddPage |
| Cartões de Crédito | CreditCardService, InvoiceService | CreditCardsViewModel, CreditCardAddViewModel, InvoiceDetailsViewModel | CreditCardsView, CreditCardAddPopup, InvoiceDetailsPage |
| Contas | AccountService | AccountsViewModel, AccountAddViewModel | ManageAccountsPopup, AccountAddPopup |
| Categorias | CategoryService | CategoriesViewModel, CategoryAddViewModel | ManageCategoriesPopup, CategoryAddPopup |
| Veículos | VehicleService | VehiclesViewModel, VehicleAddViewModel | VehicleListView, ManageVehiclesPopup, VehicleAddPopup |
| Perfil | — | ProfileViewModel, SettingsViewModel, ChangePasswordViewModel | ProfilePage, SettingsView, ChangePasswordPage |

### Modelos de Dados
- Account, Category, Transaction, TransactionGroup
- CreditCard, DashboardData, Vehicle
- UserModels, InvoiceModels, SelectionItem, Enums

### Behaviors
- CurrencyMaskBehavior
- NumericMaskBehavior
- KeyboardAvoidanceBehavior

### Dependências MAUI
- CommunityToolkit.Maui v9.1.1
- CommunityToolkit.Mvvm v8.4.0
- Microcharts.Maui v1.0.1
- Plugin.Fingerprint v3.0.0-beta.1

---

## Mapeamento de Tecnologias: MAUI → Android Nativo

| MAUI / .NET | Android Nativo (Kotlin) |
|---|---|
| XAML Layouts | XML Layouts / Jetpack Compose |
| Shell Navigation | Navigation Component (Jetpack) |
| CommunityToolkit.Mvvm | Android ViewModel + LiveData/StateFlow |
| HttpClient (ApiService) | Retrofit + OkHttp |
| SecureStorage | EncryptedSharedPreferences |
| Plugin.Fingerprint | AndroidX Biometric |
| Microcharts | MPAndroidChart |
| CommunityToolkit.Maui (Popups) | Material Dialogs / BottomSheetDialogFragment |
| DI (MauiProgram.cs) | Hilt / Koin |
| Behaviors (CurrencyMask) | TextWatcher / InputFilter |
| MAUI Preferences | SharedPreferences |

---

## Etapas da Conversão

### Fase 1 — Configuração do Projeto
- [ ] Criar projeto Android Studio com Kotlin
- [ ] Configurar Gradle com dependências equivalentes
- [ ] Configurar o tema e estilos (Material Design 3)
- [ ] Configurar ícone do app e splash screen

### Fase 2 — Camada de Dados (Models)
- [ ] Converter todos os modelos (data classes Kotlin)
- [ ] Converter Enums

### Fase 3 — Camada de Serviços (Network/API)
- [ ] Configurar Retrofit + OkHttp
- [ ] Implementar ApiService base (interceptors, auth token)
- [ ] Converter todos os serviços (AuthService, DashboardService, etc.)
- [ ] Implementar armazenamento seguro (EncryptedSharedPreferences)
- [ ] Implementar autenticação biométrica (AndroidX Biometric)

### Fase 4 — Camada de Apresentação (UI)
- [ ] Implementar Navigation Graph
- [ ] Converter telas públicas (Welcome, Login, Register)
- [ ] Converter Dashboard (HomeView)
- [ ] Converter lista de transações e formulário de adição
- [ ] Converter telas de cartões de crédito e faturas
- [ ] Converter popups/dialogs (Contas, Categorias, Veículos, Cartões)
- [ ] Converter telas de perfil e configurações
- [ ] Implementar BottomNavigation (substituir BottomMenu)
- [ ] Implementar FloatingActionButton

### Fase 5 — ViewModels
- [ ] Converter todos os ViewModels para Android ViewModel
- [ ] Implementar BaseViewModel com estados de loading/error
- [ ] Conectar ViewModels com Retrofit/Repository

### Fase 6 — Comportamentos e Utilitários
- [ ] Implementar CurrencyMask como TextWatcher
- [ ] Implementar NumericMask como InputFilter
- [ ] Converter Converters para BindingAdapters

### Fase 7 — Testes e Finalização
- [ ] Testes de integração com API
- [ ] Testes de UI
- [ ] Ajustes de performance
- [ ] Build de release
