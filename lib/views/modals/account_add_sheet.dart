import 'package:flutter/material.dart';

/// Placeholder — será implementado em tarefa futura.
class AccountAddSheet extends StatelessWidget {
  final String? accountId;

  const AccountAddSheet({super.key, this.accountId});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(32),
      decoration: const BoxDecoration(
        color: Color(0xFF0A0F17),
        borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
      ),
      child: const SafeArea(
        child: Center(
          child: Text(
            'Em construção',
            style: TextStyle(color: Color(0xFF94A3B8), fontSize: 16),
          ),
        ),
      ),
    );
  }
}
