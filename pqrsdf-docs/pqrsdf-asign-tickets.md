### 6. Módulo de Asignación (Administrador)

**Historia de Usuario:** Como Administrador, quiero visualizar las solicitudes sin asignar para designarlas al funcionario correspondiente y mantener el flujo de trabajo activo[cite: 2].

**Requerimientos:**

- Solo los usuarios con rol Administrador pueden ver las solicitudes no asignadas y ejecutar la acción de asignación[cite: 2].
- Permitir seleccionar un funcionario de una lista y vincularlo al radicado[cite: 2].

**Criterios de Aceptación:**

- Un usuario con rol "Funcionario" no puede acceder al endpoint ni a la UI de asignación[cite: 2].
- Al completarse la asignación, la PQRSDF aparece en la bandeja del funcionario elegido[cite: 2].
