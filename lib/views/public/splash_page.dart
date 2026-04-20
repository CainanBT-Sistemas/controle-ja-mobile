import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:go_router/go_router.dart';

import '../../providers/auth_providers.dart';
import '../../routes/app_router.dart';

/// Tela de splash / auto-login.
///
/// Verifica se existe refresh_token salvo e tenta auto-login.
/// Caso contrário, redireciona para a WelcomePage.
class SplashPage extends ConsumerStatefulWidget {
  const SplashPage({super.key});

  @override
  ConsumerState<SplashPage> createState() => _SplashPageState();
}

class _SplashPageState extends ConsumerState<SplashPage> {
  @override
  void initState() {
    super.initState();
    _checkAutoLogin();
  }

  Future<void> _checkAutoLogin() async {
    final authService = ref.read(authServiceProvider);
    final secureStorage = ref.read(secureStorageProvider);

    try {
      final refreshToken = await secureStorage.read(key: 'refresh_token');

      if (refreshToken != null && refreshToken.isNotEmpty) {
        final success = await authService.loginWithToken(refreshToken);
        if (success && mounted) {
          context.go(AppRoutes.home);
          return;
        }
      }
    } catch (e) {
      debugPrint('SplashPage auto-login error: $e');
    }

    if (mounted) {
      context.go(AppRoutes.welcome);
    }
  }

  @override
  Widget build(BuildContext context) {
    return const Scaffold(
      backgroundColor: Color(0xFF001524),
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            // Logo placeholder
            Icon(
              Icons.account_balance_wallet,
              size: 80,
              color: Color(0xFF00E676),
            ),
            SizedBox(height: 24),
            Text(
              'Controle Já',
              style: TextStyle(
                color: Colors.white,
                fontSize: 28,
                fontWeight: FontWeight.bold,
              ),
            ),
            SizedBox(height: 32),
            CircularProgressIndicator(color: Color(0xFF00E676)),
          ],
        ),
      ),
    );
  }
}
