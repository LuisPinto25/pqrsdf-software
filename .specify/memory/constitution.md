<!--
Sync Impact Report:
- Version change: 1.2.0 -> 1.3.0
- List of modified principles / sections:
  - IV. Frontend Screaming Architecture y Next.js App Router Feature-Driven: Se alinea la Screaming Architecture con el file-system routing del App Router de Next.js, estableciendo que las carpetas de los dominios del sistema (pqrsdf, auth, dashboard, etc.) cuelgan directamente dentro de `app/` para aprovechar las bondades nativas del framework (layouts anidados, error boundaries, loading states co-ubicados).
  - Directrices Técnicas y Restricciones de Implementación: Especificación del layout `src/app/[feature]/` para módulos de negocio y `src/shared/` para utilidades y componentes transversales.
  - Flujo de Desarrollo, Revisión y Criterios de Calidad: Verificación de co-ubicación en `app/` por dominio en revisiones arquitectónicas.
- Added sections:
  - Ninguna (enmienda y refinamiento de principio existente).
- Removed sections:
  - Ninguna.
- Templates requiring updates:
  - .specify/templates/plan-template.md (✅ actualizado para reflejar la estructura `src/app/[feature]/`)
  - .specify/templates/spec-template.md (✅ verificado y compatible)
  - .specify/templates/tasks-template.md (✅ verificado y compatible)
- Follow-up TODOs:
  - Ninguno.
-->

# PQRSDF Software Constitution

Esta constitución es la fuente de verdad inmutable para la generación, diseño, revisión y refactorización de código en el proyecto MVP de gestión de PQRSDF (Peticiones, Quejas, Reclamos, Sugerencias, Denuncias y Felicitaciones). Cualquier contribución, respuesta, sugerencia o código producido en este repositorio DEBE cumplir de manera estricta y sin excepción los principios aquí establecidos.

## Core Principles

### I. Stack Tecnológico y Arquitectura Global Cliente-Servidor
- **Arquitectura**: El sistema opera bajo un modelo Cliente-Servidor estrictamente desacoplado, comunicándose mediante contratos API RESTful con formato JSON.
- **Backend**: Implementado obligatoriamente en **ASP.NET Core versión 10**.
- **Frontend**: Implementado obligatoriamente en **Next.js** (App Router con TypeScript en modo estricto).
- **Independencia Operativa**: Frontend y Backend deben poder compilar, probarse y desplegarse de manera independiente respetando contratos de datos explícitos.

### II. Backend Clean Architecture e Inversión de Dependencias
- **Capas Estrictas**: El backend se estructura taxativamente en 4 capas concéntricas:
  1. `Domain`: Núcleo puro del negocio. Contiene entidades, value objects, eventos de dominio y contratos de interfaces de repositorio/servicios de dominio.
  2. `Application`: Casos de uso, comandos, consultas, DTOs, validadores y orquestación del flujo de negocio mediante CQRS organizativo.
  3. `Infrastructure`: Implementaciones técnicas concretas (persistencia, base de datos única, clientes HTTP externos, servicios del sistema, adaptadores).
  4. `API`: Controladores/Minimal APIs, middleware, configuración de dependency injection, filtros y serialización.
- **CQRS Estrictamente Organizativo en Application**:
  - Es **obligatorio** organizar los casos de uso separando formalmente **Commands** (operaciones de mutación de estado) y **Queries** (operaciones de solo lectura/consulta) con sus respectivos Handlers.
  - **Estructura Co-ubicada por Caso de Uso**: Cada caso de uso representa una unidad autocontenida que agrupa su definición (Command o Query), su Handler de ejecución y sus DTOs exclusivos.
  - **Límite Estricto de Alcance (Sin Sobreingeniería)**: CQRS se aplica **única y exclusivamente** como patrón organizativo interno del código de aplicación. Queda **terminantemente prohibido** incorporar arquitecturas con bases de datos separadas (nada de BD de lectura vs BD de escritura) o buses de eventos / brokers de mensajería asíncronos para replicación de datos. Todo el sistema persiste y consulta sobre una **única base de datos unificada**.
- **Regla de Dependencia Unidireccional**:
  - `Domain` NO depende de ninguna otra capa ni de frameworks externos (POCO puro).
  - `Application` depende ÚNICAMENTE de `Domain`.
  - `Infrastructure` depende de `Application` y `Domain`.
  - `API` depende de `Application` y resuelve la inyección de `Infrastructure`.
- **Inversión de Dependencias (DIP)**: Ninguna capa de alto nivel dependerá de implementaciones concretas. Todo acoplamiento se resuelve estrictamente a través de interfaces registradas en el contenedor IoC oficial de ASP.NET Core.

### III. Domain-Driven Design (DDD) y Tipos Fuertes
- **Prohibición de Modelos Anémicos**: Queda estrictamente prohibido el uso de entidades anémicas (clases con puros getters/setters públicos sin lógica). Las entidades encapsulan estado, validaciones de invariantes y comportamiento de negocio mediante métodos expresivos.
- **Obsesión por Primitivos Prohibida**: El dominio debe modelar conceptos a través de **Value Objects** inmutables (ej. `RadicadoNumber`, `Email`, `PqrsdfType`, `CitizenIdentification`, `PqrsdfDescription`). Los tipos primitivos (`string`, `int`, `Guid`) no deben utilizarse directamente para representar lógica de dominio sin envoltorio validado.
- **Invariantes Garantizados**: Ninguna entidad o Value Object podrá instanciarse en un estado inválido.

### IV. Frontend Screaming Architecture y Next.js App Router Feature-Driven
- **Screaming Architecture Integrada con File-System Routing**: La estructura del proyecto frontend debe "gritar" el dominio del negocio de PQRSDF y alinearse de forma natural con el App Router de Next.js. Las carpetas de dominio del sistema cuelgan directamente dentro de `app/` (ej. `app/pqrsdf/`, `app/auth/`, `app/dashboard/` o route groups asociados), haciendo que la estructura de directorios defina simultáneamente las rutas de la aplicación y exprese con total claridad el dominio del negocio.
- **Prohibición de Carpetas Técnicas Genéricas en la Raíz**: Queda terminantemente prohibido organizar el código agrupándolo exclusivamente en carpetas técnicas globales (`/components`, `/hooks`, `/pages` sueltas o genéricas). Toda vista, componente específico, hook particular o lógica de presentación de una feature debe co-ubicarse dentro de la carpeta correspondiente en `app/[dominio]/...`.
- **Módulos Autónomos y Aprovechamiento de Next.js**: Cada carpeta de feature bajo `app/` opera como un módulo de negocio autónomo que aprovecha las capacidades nativas de Next.js App Router (layouts anidados `layout.tsx`, pantallas de carga `loading.tsx`, fronteras de error `error.tsx`, Server Actions y componentes clientes/servidor co-ubicados).
- **Recursos Compartidos**: Todo componente verdaderamente transversal (diseño base, utilidades globales, contratos de tipos o cliente API global) debe residir en una carpeta `shared/` (ej. `src/shared/`), manteniéndose estrictamente diferenciada de las carpetas de feature de negocio.

### V. Gobernanza Unificada de Errores y Excepciones
- **Patrón Result Obligatorio**:
  - Tanto en Backend como en Frontend se prohíbe el uso de excepciones para el control de flujo normal o errores esperados del negocio (validaciones, recursos no encontrados, reglas insatisfechas).
  - Todos los casos de uso (Commands y Queries), servicios de aplicación y clientes API deben retornar un tipo de resultado explícito (ej. `Result<T, Error>` o `Result<T>`).
- **Centralización Global de Excepciones**:
  - Las excepciones quedan reservadas exclusivamente para fallos catastróficos no previstos o pánicos de infraestructura/sistema.
  - Backend: Se gestionan obligatoriamente en un único **Global Exception Handler** / ProblemDetails middleware en ASP.NET Core 10.
  - Frontend: Se gestionan obligatoriamente en **Global Error Boundaries** en Next.js con pantallas de contingencia claras para el usuario.

### VI. Prevención de Code Smells y Cumplimiento Estricto de SOLID
- **Cumplimiento SOLID**:
  - **S**ingle Responsibility: Cada clase, comando, consulta, función y componente tendrá una única razón de cambio.
  - **O**pen/Closed: Extensión mediante polimorfismo e interfaces, sin modificar código testeado.
  - **L**iskov Substitution: Los subtipos o implementaciones deben ser sustituibles sin alterar el comportamiento.
  - **I**nterface Segregation: Interfaces pequeñas, específicas y orientadas al consumidor.
  - **D**ependency Inversion: Dependencia exclusiva sobre abstracciones.
- **Tolerancia Cero a Anti-Patrones**:
  - **God Objects**: Prohibidas las clases o componentes que acumulen múltiples responsabilidades operativas.
  - **Shotgun Surgery**: Toda regla de negocio debe estar encapsulada en un único punto; un cambio de regla jamás debe obligar a modificar múltiples módulos dispersos.

### VII. Convención de Idioma y Localización
- **Código Fuente Estrictamente en Inglés**: Todo el código fuente del proyecto DEBE estar escrito en **inglés**. Esto abarca sin excepción:
  - Nombres de clases, métodos, variables, interfaces, enums y tipos.
  - Nombres de tablas, columnas y scripts de migración o persistencia.
  - Comentarios de código, documentación técnica interna, nombres de ramas y mensajes de commit.
- **Mensajes de Cara al Usuario en Español**: Todos los textos mostrados al cliente / usuario final desde el frontend DEBEN estar redactados en **español**. Esto comprende:
  - Textos de interfaces gráficas, formularios, botones, encabezados y etiquetas.
  - Mensajes de error amigables para el usuario final, alertas y retroalimentación de validaciones.
  - Notificaciones, correos o comprobantes generados para el ciudadano/usuario.

## Directrices Técnicas y Restricciones de Implementación

- **Backend Framework**: ASP.NET Core 10 con C# nullable reference types activados (`<Nullable>enable</Nullable>`) y advertencias como errores en CI (`<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`).
- **Estructura Mandatoria de Casos de Uso en Application**:
  - Todos los casos de uso deben estructurarse obligatoriamente bajo:
    `Application/Features/[FeatureName]/UseCases/[UseCaseName]/`
  - Dentro de la carpeta de cada caso de uso en particular (ej. `MakePqrsdf`, `GetUser`, `AssignTicket`):
    - Se ubica la definición del Command o Query (ej. `MakePqrsdfCommand.cs`, `GetUserQuery.cs`).
    - Se ubica el Handler correspondiente (ej. `MakePqrsdfCommandHandler.cs`, `GetUserQueryHandler.cs`).
    - Se ubican los DTOs de entrada/salida que sean **únicos y exclusivos** de ese caso de uso (ej. `MakePqrsdfResponse.cs`, `MakePqrsdfDto.cs`).
- **Ubicación de DTOs Compartidos**:
  - Todo DTO que sea reutilizable o compartido entre múltiples casos de uso o features debe residir obligatoriamente en `Application/Shared/Dtos/`.
- **Persistencia**: Única base de datos relacional para lecturas y escrituras, gestionada mediante Entity Framework Core u orquestador de datos equivalente detrás de abstracciones de repositorio.
- **Frontend Framework y Organización**: Next.js 15+ (App Router con TypeScript estricto `strict: true`, ESLint, Tailwind CSS). La estructura de carpetas de negocio se implementa bajo `src/app/` (ej. `src/app/pqrsdf/`, `src/app/auth/`, `src/app/dashboard/`), co-ubicando componentes, hooks y sub-rutas por dominio, con recursos transversales en `src/shared/`.
- **Comunicación e Integración**: Contratos basados en especificación OpenAPI / Swagger generada automáticamente desde ASP.NET Core y consumida fielmente en Next.js.
- **Seguridad**: Sanitización obligatoria en fronteras de entrada (API y UI) para prevención de XSS, CSRF e inyecciones.

## Flujo de Desarrollo, Revisión y Criterios de Calidad

1. **Revisión de Arquitectura en Cada Modificación**:
   - Todo caso de uso backend implementado o refactorizado debe respetar la ruta `Application/Features/[FeatureName]/UseCases/[UseCaseName]/` con sus artefactos co-ubicados, verificando que los DTOs compartidos estén debidamente centralizados en `Application/Shared/Dtos/`.
   - En el frontend, se validará que las funcionalidades se organicen bajo sus carpetas de dominio en `app/` aprovechando el file routing de Next.js, sin dispersar componentes de feature en carpetas técnicas globales.
   - Se validará la ausencia total de bases duales o brokers de mensajería para CQRS.
2. **Defensa contra Regresiones**:
   - Cada Command Handler y Query Handler debe contar con pruebas unitarias aisladas verificando tanto caminos felices como flujos con `Result<T, Error>`.
3. **Control de Calidad y Refactorización**:
   - Ningún código con violaciones a SOLID, mezcla indebida de idiomas (código en español o UI al usuario en inglés), dependencias circulares o dispersión de DTOs será aprobado para merge.

## Governance

- **Inmutabilidad y Jerarquía**: Esta constitución es la norma suprema del repositorio. Prevalece sobre cualquier convenio informal, preferencia de estilo personal o atajo de desarrollo.
- **Procedimiento de Enmienda**: Cualquier cambio a esta constitución requiere una propuesta formal, justificación arquitectónica documentada y actualización de la versión semántica.
- **Política de Versionado**:
  - **MAJOR (X.0.0)**: Cambios incompatibles en principios rectores, capas o gobernanza de errores.
  - **MINOR (1.X.0)**: Nuevos estándares, módulos arquitectónicos adicionales o directrices extendidas.
  - **PATCH (1.0.X)**: Correcciones tipográficas, refinamientos de redacción o aclaraciones menores.
- **Auditoría Automatizada y Asistida**: El asistente de desarrollo y los revisores humanos DEBEN comprobar y garantizar el cumplimiento estricto de estos mandatos en cada plan, especificación, tarea e implementación generada.

**Version**: 1.3.0 | **Ratified**: 2026-09-17 | **Last Amended**: 2026-09-18
