### 6. Módulo de Asignación (Administrador)

**Historia de Usuario:** Como Administrador, quiero visualizar las solicitudes sin asignar para designarlas al funcionario correspondiente y mantener el flujo de trabajo activo.

**Requerimientos:**

- Solo los usuarios con rol Administrador pueden ver las solicitudes no asignadas y ejecutar la acción de asignación.
- Permitir seleccionar un funcionario de una lista y vincularlo al radicado.

**Criterios de Aceptación:**

- Un usuario con rol "Funcionario" no puede acceder al endpoint ni a la UI de asignación.
- Al completarse la asignación, la PQRSDF aparece en la bandeja del funcionario elegido.
