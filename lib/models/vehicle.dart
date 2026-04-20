import 'enums.dart';

class Vehicle {
  final String id;
  final String name;
  final String brand;
  final String model;
  final int year;
  final String plate;
  final double currentOdometer;
  final double? avgGasoline;
  final double? avgEthanol;

  const Vehicle({
    required this.id,
    this.name = '',
    this.brand = '',
    this.model = '',
    this.year = 0,
    this.plate = '',
    this.currentOdometer = 0,
    this.avgGasoline,
    this.avgEthanol,
  });

  String get fullDescription => '$brand $model - $year';

  Vehicle copyWith({
    String? id,
    String? name,
    String? brand,
    String? model,
    int? year,
    String? plate,
    double? currentOdometer,
    double? avgGasoline,
    double? avgEthanol,
  }) {
    return Vehicle(
      id: id ?? this.id,
      name: name ?? this.name,
      brand: brand ?? this.brand,
      model: model ?? this.model,
      year: year ?? this.year,
      plate: plate ?? this.plate,
      currentOdometer: currentOdometer ?? this.currentOdometer,
      avgGasoline: avgGasoline ?? this.avgGasoline,
      avgEthanol: avgEthanol ?? this.avgEthanol,
    );
  }

  factory Vehicle.fromJson(Map<String, dynamic> json) {
    return Vehicle(
      id: json['id'] as String? ?? '',
      name: json['name'] as String? ?? '',
      brand: json['brand'] as String? ?? '',
      model: json['model'] as String? ?? '',
      year: json['year'] as int? ?? 0,
      plate: json['plate'] as String? ?? '',
      currentOdometer:
          (json['currentOdometer'] as num?)?.toDouble() ?? 0,
      avgGasoline: (json['avgGasoline'] as num?)?.toDouble(),
      avgEthanol: (json['avgEthanol'] as num?)?.toDouble(),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'name': name,
      'brand': brand,
      'model': model,
      'year': year,
      'plate': plate,
      'currentOdometer': currentOdometer,
      'avgGasoline': avgGasoline,
      'avgEthanol': avgEthanol,
    };
  }

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is Vehicle && runtimeType == other.runtimeType && id == other.id;

  @override
  int get hashCode => id.hashCode;

  @override
  String toString() =>
      'Vehicle(id: $id, name: $name, brand: $brand, model: $model, '
      'year: $year, plate: $plate, currentOdometer: $currentOdometer)';
}
