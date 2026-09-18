# PQRSDF Frontend

Portal web institucional para la radicación, seguimiento y gestión de Peticiones, Quejas, Reclamos, Sugerencias, Denuncias y Felicitaciones (PQRSDF), desarrollado con **Next.js 15 (App Router)**, **TypeScript estricto** y **Tailwind CSS**.

---

## Requisitos Previos

- **Node.js**: `v20.x` LTS o superior (`node -v`)
- **pnpm**: `v9.x` o superior (`pnpm -v`)

---

## Instalación y Ejecución (< 5 minutos)

```bash
# 1. Posicionarse en el directorio frontend
cd frontend

# 2. Instalar dependencias mediante pnpm
pnpm install

# 3. Configurar variables de entorno
cp .env.example .env.local

# 4. Iniciar el servidor de desarrollo
pnpm dev
```

La aplicación estará disponible en [http://localhost:3000](http://localhost:3000).

---

## Scripts Disponibles

| Comando             | Descripción                                                                        |
| ------------------- | ---------------------------------------------------------------------------------- |
| `pnpm dev`          | Inicia el servidor de desarrollo local con recarga en vivo en el puerto 3000       |
| `pnpm build`        | Compila la aplicación para producción                                              |
| `pnpm start`        | Inicia la versión compilada de producción                                          |
| `pnpm typecheck`    | Ejecuta el compilador de TypeScript en modo estricto (`tsc --noEmit`)              |
| `pnpm lint`         | Ejecuta ESLint validando reglas de estilo y barreras de importación arquitectónica |
| `pnpm format`       | Formatea el código fuente automáticamente con Prettier                             |
| `pnpm format:check` | Comprueba si todo el código fuente cumple con el formato de Prettier               |
| `pnpm test`         | Ejecuta las pruebas unitarias y de componentes con Vitest                          |
| `pnpm test:watch`   | Ejecuta Vitest en modo interactivo/watch                                           |
| `pnpm generate-api` | Regenera bajo demanda los tipos TypeScript (`schema.d.ts`) desde OpenAPI           |

---

## Arquitectura y Principios de Diseño

- **Screaming Architecture & Next.js App Router**: Los dominios de negocio cuelgan directamente de `src/app/` (`src/app/pqrsdf/`, `src/app/auth/`, `src/app/dashboard/`), garantizando correspondencia 1:1 con las rutas visibles del navegador.
- **Barreras de Módulos**: Reglas automáticas de ESLint (`eslint-plugin-boundaries`) evitan importaciones directas entre dominios de negocio.
- **Gobernanza de Errores (Patrón Result)**: Errores operacionales y respuestas HTTP utilizan la mónada `Result<T, E>` en `src/shared/types/result.ts` sin lanzar excepciones no controladas.
- **Localización e Idioma**: 100% de la interfaz gráfica y mensajes al usuario están en español mediante `next-intl` (`messages/es.json`), manteniendo el código fuente estrictamente en inglés.
- **Seguridad**: Cabeceras HTTP de seguridad configuradas (Content Security Policy, X-Frame-Options, nosniff, Referrer-Policy).
