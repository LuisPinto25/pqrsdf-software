### 4. Módulo de Bandeja de Entrada (Funcionario)

**Historia de Usuario:** Como Funcionario, quiero ver una bandeja paginada con las solicitudes, aplicando filtros y ordenamientos para priorizar mi trabajo según los tiempos de vencimiento.

**Requerimientos:**

- Lista paginada obtenida desde el servidor (server-side pagination).
- Las columnas mostradas deben ser: radicado, tipo, asunto corto, estado, fecha de vencimiento y días restantes.
- Indicador visual (código de color) para los días restantes: verde (> 5 días), ámbar (1 a 5 días) y rojo (vencida).
- Filtros dinámicos requeridos: estado, tipo y rango de fechas.
- Búsqueda de texto requerida: por número de radicado o por asunto.
- Ordenamiento requerido: por radicado o por fecha de vencimiento.

**Criterios de Aceptación:**

- El funcionario visualiza sus solicitudes respetando la paginación y la metadata enviada por el servidor (`items`, `totalCount`, `page`, etc.).
- El cambio en los filtros ejecuta consultas eficientes en la base de datos (mediante patrones como _Specification_ o LINQ dinámico) sin procesar en memoria del cliente.
- Los colores de semaforización se renderizan correctamente según la fecha actual contra la fecha de vencimiento calculada.
