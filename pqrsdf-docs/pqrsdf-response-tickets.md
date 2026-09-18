### 5. Módulo de Gestión y Respuesta (Funcionario)

**Historia de Usuario:** Como Funcionario, quiero abrir el detalle de una solicitud para cambiar su estado con una justificación, o registrar la respuesta final para dar por cerrado el caso.

**Requerimientos:**

- Vista de detalle accesible desde la bandeja.
- Permitir cambiar el estado de la solicitud, exigiendo obligatoriamente un texto de justificación.
- Permitir registrar la respuesta final al ciudadano.
- Al registrar la respuesta, el estado de la solicitud debe cambiar a cerrado.

**Criterios de Aceptación:**

- Si el funcionario intenta cambiar el estado sin proveer justificación, el sistema arroja error de validación.
- Al enviar la respuesta final, la PQRSDF ya no suma días de gestión y pasa a estado cerrado.
- El ciudadano puede ver inmediatamente el cambio de estado y la respuesta en el módulo público.
