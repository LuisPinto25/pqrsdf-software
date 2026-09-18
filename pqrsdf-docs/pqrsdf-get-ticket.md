### 2. Módulo de Consulta Pública

**Historia de Usuario:** Como Ciudadano, quiero consultar el estado de mi solicitud usando mi número de radicado para hacerle seguimiento sin comprometer mi privacidad.

**Requerimientos:**

- Pantalla pública con un campo de entrada para el número de radicado.
- El sistema debe mostrar: estado actual, fecha de radicación, fecha de vencimiento y días restantes.
- Si la solicitud ya fue cerrada, debe mostrar el contenido de la respuesta.
- No se debe exponer ningún dato sensible o personal del solicitante.

**Criterios de Aceptación:**

- Si el usuario ingresa un radicado existente, el sistema devuelve exclusivamente la línea de tiempo y estado.
- Si la solicitud tiene respuesta final, el texto es visible en esta pantalla.
- La UI no expone nombre, correo, ni documento del creador de la solicitud bajo ninguna circunstancia.
