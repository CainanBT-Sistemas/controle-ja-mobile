import 'package:flutter/material.dart';

/// Constantes de cores da aplicação - Material Design Palette
/// Convertidas de UIConstants.cs
class AppColors {
  AppColors._();

  // Paleta principal (18 cores Material Design)
  static const Color vermelho = Color(0xFFF44336);
  static const Color rosa = Color(0xFFE91E63);
  static const Color roxo = Color(0xFF9C27B0);
  static const Color roxoEscuro = Color(0xFF673AB7);
  static const Color indigo = Color(0xFF3F51B5);
  static const Color azul = Color(0xFF2196F3);
  static const Color azulClaro = Color(0xFF03A9F4);
  static const Color ciano = Color(0xFF00BCD4);
  static const Color verdeAgua = Color(0xFF009688);
  static const Color verde = Color(0xFF4CAF50);
  static const Color verdeClaro = Color(0xFF8BC34A);
  static const Color lima = Color(0xFFCDDC39);
  static const Color ambar = Color(0xFFFFC107);
  static const Color laranja = Color(0xFFFF9800);
  static const Color laranjaEscuro = Color(0xFFFF5722);
  static const Color marrom = Color(0xFF795548);
  static const Color cinza = Color(0xFF9E9E9E);
  static const Color azulCinzento = Color(0xFF607D8B);

  // Cores de status
  static const Color receita = Color(0xFF00E676);
  static const Color despesa = Color(0xFFFF5252);
  static const Color pendente = Color(0xFFFF9800);

  /// Lista das 18 cores disponíveis para seleção
  static const List<Color> availableColors = [
    vermelho,
    rosa,
    roxo,
    roxoEscuro,
    indigo,
    azul,
    azulClaro,
    ciano,
    verdeAgua,
    verde,
    verdeClaro,
    lima,
    ambar,
    laranja,
    laranjaEscuro,
    marrom,
    cinza,
    azulCinzento,
  ];

  /// Lista das cores em formato hexadecimal
  static const List<String> availableColorHex = [
    '#F44336',
    '#E91E63',
    '#9C27B0',
    '#673AB7',
    '#3F51B5',
    '#2196F3',
    '#03A9F4',
    '#00BCD4',
    '#009688',
    '#4CAF50',
    '#8BC34A',
    '#CDDC39',
    '#FFC107',
    '#FF9800',
    '#FF5722',
    '#795548',
    '#9E9E9E',
    '#607D8B',
  ];

  /// Converte uma string hexadecimal (#RRGGBB) para Color
  static Color fromHex(String hex) {
    final buffer = StringBuffer();
    if (hex.length == 6 || hex.length == 7) buffer.write('FF');
    buffer.write(hex.replaceFirst('#', ''));
    return Color(int.parse(buffer.toString(), radix: 16));
  }
}

/// Listas curadas de ícones por contexto
/// Convertidas de UIConstants.cs
class AppIconLists {
  AppIconLists._();

  static const List<String> accountIcons = [
    'account_balance_wallet',
    'account_balance',
    'savings',
    'store',
  ];

  static const List<String> cardIcons = [
    'credit_card',
    'local_dining',
    'work',
    'card_giftcard',
    'flight',
  ];

  static const List<String> categoryIcons = [
    'restaurant',
    'fastfood',
    'local_cafe',
    'directions_car',
    'local_gas_station',
    'flight',
    'home',
    'water_drop',
    'bolt',
    'wifi',
    'local_hospital',
    'fitness_center',
    'shopping_cart',
    'local_mall',
    'sports_esports',
    'pets',
    'movie',
    'school',
    'category',
    'star',
    'favorite',
    'attach_money',
    'receipt_long',
    'checkroom',
  ];
}

/// Constantes de ícones Material (codepoints)
/// Convertidas de MaterialIcons.cs
class AppMaterialIcons {
  AppMaterialIcons._();

  // Menu Inferior
  static const IconData home = Icons.home;
  static const IconData receiptLong = Icons.receipt_long;
  static const IconData creditCard = Icons.credit_card;
  static const IconData directionsCar = Icons.directions_car;
  static const IconData person = Icons.person;

  // Ações
  static const IconData add = Icons.add;
  static const IconData trendingUp = Icons.trending_up;
  static const IconData trendingDown = Icons.trending_down;

  // Categorias
  static const IconData restaurant = Icons.restaurant;
  static const IconData school = Icons.school;
  static const IconData attachMoney = Icons.attach_money;
  static const IconData category = Icons.category;

  // Utilitários
  static const IconData checkCircleOutline = Icons.check_circle_outline;
  static const IconData calendarToday = Icons.calendar_today;
  static const IconData notes = Icons.notes;
  static const IconData label = Icons.label;
  static const IconData accountBalanceWallet = Icons.account_balance_wallet;
  static const IconData repeat = Icons.repeat;
  static const IconData check = Icons.check;

  /// Mapeia um nome de ícone (string) para o IconData correspondente
  static IconData fromName(String name) {
    return _iconMap[name] ?? Icons.help_outline;
  }

  static const Map<String, IconData> _iconMap = {
    'home': Icons.home,
    'receipt_long': Icons.receipt_long,
    'credit_card': Icons.credit_card,
    'directions_car': Icons.directions_car,
    'person': Icons.person,
    'add': Icons.add,
    'trending_up': Icons.trending_up,
    'trending_down': Icons.trending_down,
    'restaurant': Icons.restaurant,
    'fastfood': Icons.fastfood,
    'local_cafe': Icons.local_cafe,
    'local_gas_station': Icons.local_gas_station,
    'flight': Icons.flight,
    'water_drop': Icons.water_drop,
    'bolt': Icons.bolt,
    'wifi': Icons.wifi,
    'local_hospital': Icons.local_hospital,
    'fitness_center': Icons.fitness_center,
    'shopping_cart': Icons.shopping_cart,
    'local_mall': Icons.local_mall,
    'sports_esports': Icons.sports_esports,
    'pets': Icons.pets,
    'movie': Icons.movie,
    'school': Icons.school,
    'category': Icons.category,
    'star': Icons.star,
    'favorite': Icons.favorite,
    'attach_money': Icons.attach_money,
    'checkroom': Icons.checkroom,
    'account_balance_wallet': Icons.account_balance_wallet,
    'account_balance': Icons.account_balance,
    'savings': Icons.savings,
    'store': Icons.store,
    'local_dining': Icons.local_dining,
    'work': Icons.work,
    'card_giftcard': Icons.card_giftcard,
    'check_circle_outline': Icons.check_circle_outline,
    'calendar_today': Icons.calendar_today,
    'notes': Icons.notes,
    'label': Icons.label,
    'repeat': Icons.repeat,
    'check': Icons.check,
    'arrow_downward': Icons.arrow_downward,
    'swap_horiz': Icons.swap_horiz,
  };
}
