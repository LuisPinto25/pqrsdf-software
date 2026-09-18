### 7. Módulo de Trazabilidad y Auditoría (Bitácora)

**Historia de Usuario:** Como Usuario Interno, quiero visualizar una bitácora inmutable en el detalle de cada PQRSDF para auditar su ciclo de vida, responsables y tiempos de atención.

**Requerimientos:**

- Toda acción relevante (creación, asignación, cambio de estado, registro de respuesta) debe insertarse en una tabla de auditoría.
- Los registros deben incluir: quién ejecutó (usuario o "sistema"), acción, descripción, estado anterior, estado nuevo y timestamp.
- La captura de la auditoría debe ser centralizada (ej. EF Interceptors, Domain Events) y no depender de invocaciones manuales en los controladores.
- Endpoint específico para consultar la trazabilidad de un id, ordenada cronológicamente.

**Criterios de Aceptación:**

- La bitácora refleja fielmente el historial de inicio a fin.
- Una asignación ejecutada por el Administrador registra explícitamente "quién asignó a quién y cuándo".
- Los datos de la bitácora no pueden ser modificados una vez insertados.
