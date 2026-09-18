### 1. Módulo de Radicación de PQRSDF (Acceso Público)

**Historia de Usuario:** Como Ciudadano, quiero radicar una PQRSDF mediante un formulario público para comunicar mi solicitud, queja o sugerencia a la entidad sin necesidad de crear una cuenta.

**Requerimientos:**

- El formulario debe ser accesible sin autenticación.
- Debe permitir seleccionar el tipo de solicitud (Petición, Queja, Reclamo, Sugerencia, Denuncia, Felicitación) y el área destino, cargada dinámicamente desde el backend.
- Debe solicitar datos del solicitante, asunto y descripción.
- El backend debe calcular la fecha de vencimiento en días hábiles (15 días por regla general, 30 días exclusivamente para denuncias).
- El cálculo debe excluir sábados, domingos y días festivos colombianos.
- Se debe generar un número de radicado único y concurrente con el formato `AAAA-NNNNNNNN`.

**Criterios de Aceptación:**

- Al enviar el formulario con datos válidos, el sistema persiste la información y devuelve una pantalla de confirmación.
- La pantalla de confirmación muestra claramente el número de radicado generado y la fecha máxima de respuesta.
- El sistema no genera duplicados en el formato de radicado, incluso bajo condiciones de concurrencia.
