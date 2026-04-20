import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:go_router/go_router.dart';
import 'package:shared_preferences/shared_preferences.dart';

import '../../providers/auth_providers.dart';
import '../modals/manage_accounts_sheet.dart';
import '../modals/manage_categories_sheet.dart';
import '../modals/manage_credit_cards_sheet.dart';
import '../modals/manage_vehicles_sheet.dart';

/// Aba "Mais" / Configurações.
///
/// Migrado de SettingsViewModel.cs — perfil, gestão de entidades e ações.
class SettingsTab extends ConsumerStatefulWidget {
  const SettingsTab({super.key});

  @override
  ConsumerState<SettingsTab> createState() => _SettingsTabState();
}

class _SettingsTabState extends ConsumerState<SettingsTab> {
  static const _bgColor = Color(0xFF001524);
  static const _cardColor = Color(0xFF1E2A3A);
  static const _textColor = Color(0xFF94A3B8);
  static const _green = Color(0xFF00E676);
  static const _red = Color(0xFFFF5252);

  String _userName = '';
  String _userEmail = '';

  @override
  void initState() {
    super.initState();
    _loadUserInfo();
  }

  Future<void> _loadUserInfo() async {
    final prefs = await SharedPreferences.getInstance();
    setState(() {
      _userName = prefs.getString('UserName') ?? 'Usuário';
      _userEmail = prefs.getString('UserEmail') ?? '';
    });
  }

  @override
  Widget build(BuildContext context) {
    return SafeArea(
      child: ListView(
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 20),
        children: [
          _buildUserCard(),
          const SizedBox(height: 24),
          _sectionLabel('Gerenciamento'),
          _menuItem(Icons.account_balance_wallet, 'Contas', () {
            showManageAccountsSheet(context);
          }),
          _menuItem(Icons.category, 'Categorias', () {
            showManageCategoriesSheet(context);
          }),
          _menuItem(Icons.credit_card, 'Cartões de Crédito', () {
            showManageCreditCardsSheet(context);
          }),
          _menuItem(Icons.directions_car, 'Veículos', () {
            showManageVehiclesSheet(context);
          }),
          const SizedBox(height: 16),
          _sectionLabel('Conta'),
          _menuItem(Icons.person, 'Meu Perfil', () {
            // TODO: perfil detail
          }),
          const SizedBox(height: 16),
          _sectionLabel('Zona de perigo'),
          _menuItem(Icons.restart_alt, 'Resetar Dados', _onResetData,
              iconColor: _red),
          _menuItem(Icons.logout, 'Sair', _onLogout, iconColor: _red),
          const SizedBox(height: 16),
          _sectionLabel('Informações'),
          _menuItem(Icons.info_outline, 'Sobre', _onAbout),
        ],
      ),
    );
  }

  // ---- User card ----

  Widget _buildUserCard() {
    final initial =
        _userName.isNotEmpty ? _userName[0].toUpperCase() : '?';

    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: _cardColor,
        borderRadius: BorderRadius.circular(14),
      ),
      child: Row(
        children: [
          CircleAvatar(
            radius: 28,
            backgroundColor: _green.withAlpha(50),
            child: Text(
              initial,
              style: const TextStyle(
                color: _green,
                fontSize: 24,
                fontWeight: FontWeight.bold,
              ),
            ),
          ),
          const SizedBox(width: 16),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  _userName,
                  style: const TextStyle(
                    color: Colors.white,
                    fontSize: 18,
                    fontWeight: FontWeight.bold,
                  ),
                ),
                if (_userEmail.isNotEmpty)
                  Text(
                    _userEmail,
                    style: const TextStyle(color: _textColor, fontSize: 13),
                  ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  // ---- Helpers ----

  Widget _sectionLabel(String text) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 8),
      child: Text(
        text,
        style: const TextStyle(
          color: _textColor,
          fontSize: 12,
          fontWeight: FontWeight.w600,
          letterSpacing: 1,
        ),
      ),
    );
  }

  Widget _menuItem(
    IconData icon,
    String label,
    VoidCallback onTap, {
    Color iconColor = _green,
  }) {
    return Container(
      margin: const EdgeInsets.only(bottom: 6),
      decoration: BoxDecoration(
        color: _cardColor,
        borderRadius: BorderRadius.circular(10),
      ),
      child: ListTile(
        leading: Icon(icon, color: iconColor, size: 22),
        title: Text(label, style: const TextStyle(color: Colors.white, fontSize: 15)),
        trailing: const Icon(Icons.chevron_right, color: _textColor, size: 20),
        onTap: onTap,
        dense: true,
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
      ),
    );
  }

  // ---- Actions ----

  Future<void> _onLogout() async {
    final confirmed = await _showConfirm(
      'Sair',
      'Tem certeza que deseja sair da sua conta?',
    );
    if (confirmed != true) return;
    await _clearStorageAndNavigate();
  }

  Future<void> _onResetData() async {
    final confirmed = await _showConfirm(
      'Resetar Dados',
      'Todos os seus dados serão apagados permanentemente. Deseja continuar?',
    );
    if (confirmed != true) return;

    final authService = ref.read(authServiceProvider);
    await authService.resetDataUser();
    await _clearStorageAndNavigate();
  }

  Future<void> _clearStorageAndNavigate() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.clear();
    const storage = FlutterSecureStorage();
    await storage.deleteAll();
    if (mounted) {
      context.go('/welcome');
    }
  }

  void _onAbout() {
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        backgroundColor: _cardColor,
        title: const Text('Sobre', style: TextStyle(color: Colors.white)),
        content: const Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text('Controle Já', style: TextStyle(color: Colors.white, fontSize: 16)),
            SizedBox(height: 4),
            Text('Versão 1.0.0', style: TextStyle(color: _textColor)),
            SizedBox(height: 8),
            Text(
              'Gerencie suas finanças de forma simples e eficiente.',
              style: TextStyle(color: _textColor, fontSize: 13),
            ),
          ],
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx),
            child: const Text('Fechar'),
          ),
        ],
      ),
    );
  }

  Future<bool?> _showConfirm(String title, String message) {
    return showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        backgroundColor: _cardColor,
        title: Text(title, style: const TextStyle(color: Colors.white)),
        content: Text(message, style: const TextStyle(color: _textColor)),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx, false),
            child: const Text('Cancelar'),
          ),
          TextButton(
            onPressed: () => Navigator.pop(ctx, true),
            child: const Text('Confirmar', style: TextStyle(color: _red)),
          ),
        ],
      ),
    );
  }
}
