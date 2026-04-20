import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../services/account_service.dart';
import '../services/category_service.dart';
import '../services/credit_card_service.dart';
import '../services/transaction_service.dart';
import '../services/vehicle_service.dart';
import 'auth_providers.dart';

/// Provider do AccountService.
final accountServiceProvider = Provider<AccountService>((ref) {
  final apiClient = ref.watch(apiClientProvider);
  return AccountService(apiClient: apiClient);
});

/// Provider do CategoryService.
final categoryServiceProvider = Provider<CategoryService>((ref) {
  final apiClient = ref.watch(apiClientProvider);
  return CategoryService(apiClient: apiClient);
});

/// Provider do CreditCardService.
final creditCardServiceProvider = Provider<CreditCardService>((ref) {
  final apiClient = ref.watch(apiClientProvider);
  return CreditCardService(apiClient: apiClient);
});

/// Provider do VehicleService.
final vehicleServiceProvider = Provider<VehicleService>((ref) {
  final apiClient = ref.watch(apiClientProvider);
  return VehicleService(apiClient: apiClient);
});

/// Provider do TransactionService.
final transactionServiceProvider = Provider<TransactionService>((ref) {
  final apiClient = ref.watch(apiClientProvider);
  return TransactionService(apiClient: apiClient);
});
