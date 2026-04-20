class LoginRequest {
  final String email;
  final String password;

  const LoginRequest({
    this.email = '',
    this.password = '',
  });

  LoginRequest copyWith({
    String? email,
    String? password,
  }) {
    return LoginRequest(
      email: email ?? this.email,
      password: password ?? this.password,
    );
  }

  factory LoginRequest.fromJson(Map<String, dynamic> json) {
    return LoginRequest(
      email: json['email'] as String? ?? '',
      password: json['password'] as String? ?? '',
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'email': email,
      'password': password,
    };
  }
}

class UserResponse {
  final String id;
  final String username;
  final String email;
  final int createdAt;
  final AuthTokens? tokens;

  const UserResponse({
    this.id = '',
    this.username = '',
    this.email = '',
    this.createdAt = 0,
    this.tokens,
  });

  UserResponse copyWith({
    String? id,
    String? username,
    String? email,
    int? createdAt,
    AuthTokens? tokens,
  }) {
    return UserResponse(
      id: id ?? this.id,
      username: username ?? this.username,
      email: email ?? this.email,
      createdAt: createdAt ?? this.createdAt,
      tokens: tokens ?? this.tokens,
    );
  }

  factory UserResponse.fromJson(Map<String, dynamic> json) {
    return UserResponse(
      id: json['id'] as String? ?? '',
      username: json['username'] as String? ?? '',
      email: json['email'] as String? ?? '',
      createdAt: json['createdAt'] as int? ?? 0,
      tokens: json['tokens'] != null
          ? AuthTokens.fromJson(json['tokens'] as Map<String, dynamic>)
          : null,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'username': username,
      'email': email,
      'createdAt': createdAt,
      'tokens': tokens?.toJson(),
    };
  }
}

class InsertUpdateUserDTO {
  final String username;
  final String password;
  final String email;

  const InsertUpdateUserDTO({
    this.username = '',
    this.password = '',
    this.email = '',
  });

  InsertUpdateUserDTO copyWith({
    String? username,
    String? password,
    String? email,
  }) {
    return InsertUpdateUserDTO(
      username: username ?? this.username,
      password: password ?? this.password,
      email: email ?? this.email,
    );
  }

  factory InsertUpdateUserDTO.fromJson(Map<String, dynamic> json) {
    return InsertUpdateUserDTO(
      username: json['username'] as String? ?? '',
      password: json['password'] as String? ?? '',
      email: json['email'] as String? ?? '',
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'username': username,
      'password': password,
      'email': email,
    };
  }
}

class AuthTokens {
  final String accessToken;
  final String refreshToken;

  const AuthTokens({
    this.accessToken = '',
    this.refreshToken = '',
  });

  AuthTokens copyWith({
    String? accessToken,
    String? refreshToken,
  }) {
    return AuthTokens(
      accessToken: accessToken ?? this.accessToken,
      refreshToken: refreshToken ?? this.refreshToken,
    );
  }

  factory AuthTokens.fromJson(Map<String, dynamic> json) {
    return AuthTokens(
      accessToken: json['accessToken'] as String? ?? '',
      refreshToken: json['refreshToken'] as String? ?? '',
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'accessToken': accessToken,
      'refreshToken': refreshToken,
    };
  }
}
