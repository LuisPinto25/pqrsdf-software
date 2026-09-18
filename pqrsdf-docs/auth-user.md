### 3. Módulo de Autenticación

**Historia de Usuario:** Como Funcionario o Administrador, quiero iniciar sesión en el sistema con mis credenciales para acceder a la bandeja de gestión según mis permisos.

**Requerimientos:**

- Endpoint de login (`POST /api/auth/login`) que valide credenciales usando BCrypt.
- El sistema debe generar y devolver un token JWT.
- El token JWT debe incluir el rol del usuario (Funcionario o Administrador) en sus claims.

**Criterios de Aceptación:**

- Un usuario con credenciales correctas recibe un JWT válido y es redirigido a su bandeja.
- Las rutas de API y del Frontend (excepto radicación y consulta pública) están protegidas y deniegan el acceso si no hay un JWT válido.
