import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../providers/register_notifier.dart';
import '../../routes/app_router.dart';

/// Tela de registro.
///
/// Migrada de RegisterPage.xaml + RegisterViewModel.cs — mantém identidade
/// visual (fundo #001524, campos #1E2A3A, botão verde #00E676).
class RegisterPage extends ConsumerStatefulWidget {
  const RegisterPage({super.key});

  @override
  ConsumerState<RegisterPage> createState() => _RegisterPageState();
}

class _RegisterPageState extends ConsumerState<RegisterPage> {
  final _nameController = TextEditingController();
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();
  final _confirmPasswordController = TextEditingController();

  @override
  void dispose() {
    _nameController.dispose();
    _emailController.dispose();
    _passwordController.dispose();
    _confirmPasswordController.dispose();
    super.dispose();
  }

  Future<void> _handleRegister() async {
    final notifier = ref.read(registerNotifierProvider.notifier);
    final success = await notifier.register();

    if (success && mounted) {
      _showDialog(
        'Sucesso',
        'Conta criada com sucesso! Faça login.',
        onDismiss: () => context.go(AppRoutes.login),
      );
    }
  }

  void _showError(String title, String message) {
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text(title),
        content: Text(message),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(ctx).pop(),
            child: const Text('OK'),
          ),
        ],
      ),
    );
  }

  void _showDialog(String title, String message, {VoidCallback? onDismiss}) {
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text(title),
        content: Text(message),
        actions: [
          TextButton(
            onPressed: () {
              Navigator.of(ctx).pop();
              onDismiss?.call();
            },
            child: const Text('OK'),
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final registerState = ref.watch(registerNotifierProvider);
    final notifier = ref.read(registerNotifierProvider.notifier);

    // Mostrar erros via dialog
    ref.listen<RegisterState>(registerNotifierProvider, (prev, next) {
      if (next.error != null && prev?.error != next.error) {
        _showError(next.error!.title, next.error!.message);
        notifier.clearError();
      }
    });

    return Scaffold(
      backgroundColor: const Color(0xFF001524),
      body: SafeArea(
        child: Stack(
          children: [
            // Conteúdo principal
            SingleChildScrollView(
              padding: const EdgeInsets.all(30),
              child: ConstrainedBox(
                constraints: BoxConstraints(
                  minHeight: MediaQuery.of(context).size.height -
                      MediaQuery.of(context).padding.top -
                      MediaQuery.of(context).padding.bottom,
                ),
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    // Logo
                    const Icon(
                      Icons.account_balance_wallet,
                      size: 80,
                      color: Color(0xFF00E676),
                    ),
                    const SizedBox(height: 15),

                    // Títulos
                    const Text(
                      'Crie sua conta',
                      style: TextStyle(
                        color: Colors.white,
                        fontSize: 26,
                        fontWeight: FontWeight.bold,
                      ),
                      textAlign: TextAlign.center,
                    ),
                    const SizedBox(height: 5),
                    const Text(
                      'Junte-se a nós e assuma o controle',
                      style: TextStyle(
                        color: Color(0xFF94A3B8),
                        fontSize: 14,
                      ),
                      textAlign: TextAlign.center,
                    ),
                    const SizedBox(height: 25),

                    // Campo Nome
                    _buildTextField(
                      controller: _nameController,
                      placeholder: 'Nome Completo',
                      onChanged: notifier.setName,
                    ),
                    const SizedBox(height: 15),

                    // Campo E-mail
                    _buildTextField(
                      controller: _emailController,
                      placeholder: 'E-mail',
                      keyboardType: TextInputType.emailAddress,
                      onChanged: notifier.setEmail,
                    ),
                    const SizedBox(height: 15),

                    // Campo Senha
                    _buildTextField(
                      controller: _passwordController,
                      placeholder: 'Senha',
                      obscureText: true,
                      onChanged: notifier.setPassword,
                    ),
                    const SizedBox(height: 15),

                    // Campo Confirmar Senha
                    _buildTextField(
                      controller: _confirmPasswordController,
                      placeholder: 'Confirme sua senha',
                      obscureText: true,
                      onChanged: notifier.setConfirmPassword,
                    ),
                    const SizedBox(height: 25),

                    // Botão REGISTRAR
                    SizedBox(
                      width: double.infinity,
                      height: 55,
                      child: ElevatedButton(
                        onPressed:
                            registerState.isLoading ? null : _handleRegister,
                        style: ElevatedButton.styleFrom(
                          backgroundColor: const Color(0xFF00E676),
                          foregroundColor: const Color(0xFF001524),
                          shape: RoundedRectangleBorder(
                            borderRadius: BorderRadius.circular(12),
                          ),
                          elevation: 0,
                        ),
                        child: registerState.isLoading
                            ? const SizedBox(
                                height: 24,
                                width: 24,
                                child: CircularProgressIndicator(
                                  strokeWidth: 2.5,
                                  color: Color(0xFF001524),
                                ),
                              )
                            : const Text(
                                'REGISTRAR',
                                style: TextStyle(
                                  fontWeight: FontWeight.bold,
                                  fontSize: 16,
                                ),
                              ),
                      ),
                    ),
                    const SizedBox(height: 15),

                    // Botão Voltar
                    SizedBox(
                      width: double.infinity,
                      height: 50,
                      child: TextButton(
                        onPressed: () => context.pop(),
                        child: const Text(
                          'Voltar para Login',
                          style: TextStyle(
                            color: Color(0xFFA1A1AA),
                            fontWeight: FontWeight.bold,
                            fontSize: 14,
                          ),
                        ),
                      ),
                    ),
                  ],
                ),
              ),
            ),

            // Loading overlay
            if (registerState.isLoading)
              Container(
                color: Colors.black.withValues(alpha: 0.5),
                child: const Center(
                  child: CircularProgressIndicator(
                    color: Color(0xFF00E676),
                  ),
                ),
              ),
          ],
        ),
      ),
    );
  }

  Widget _buildTextField({
    required TextEditingController controller,
    required String placeholder,
    TextInputType keyboardType = TextInputType.text,
    bool obscureText = false,
    ValueChanged<String>? onChanged,
  }) {
    return Container(
      height: 55,
      decoration: BoxDecoration(
        color: const Color(0xFF1E2A3A),
        borderRadius: BorderRadius.circular(12),
      ),
      child: TextField(
        controller: controller,
        keyboardType: keyboardType,
        obscureText: obscureText,
        style: const TextStyle(color: Colors.white),
        onChanged: onChanged,
        decoration: InputDecoration(
          hintText: placeholder,
          hintStyle: const TextStyle(color: Color(0xFF64748B)),
          border: InputBorder.none,
          contentPadding:
              const EdgeInsets.symmetric(horizontal: 15, vertical: 16),
        ),
      ),
    );
  }
}
