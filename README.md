# Plataforma de Gestión de PQRSDF

Sistema integral para la radicación, seguimiento, asignación y resolución de Peticiones, Quejas, Reclamos, Sugerencias, Denuncias y Felicitaciones (PQRSDF), diseñado bajo estándares de arquitectura web moderna, alta disponibilidad, Clean Architecture y Screaming Architecture.

---

## 🏛️ Arquitectura del Repositorio (Monorepo)

- **Backend**: Implementado en **ASP.NET Core 10** siguiendo Clean Architecture (Domain, Application con CQRS organizativo, Infrastructure, Api) y SQL Server 2022.
- **Frontend**: Implementado en **Next.js 15 (App Router)** con TypeScript estricto, Tailwind CSS, `next-intl` (localización al español), y barreras de importación arquitectónica mediante `eslint-plugin-boundaries`.
- **Base de Datos**: Microsoft SQL Server 2022 Developer Edition.

---

## 🚀 Despliegue Rápido con Docker Compose

Puedes levantar todo el ecosistema (Base de Datos + Inicializador + Backend API + Frontend Portal) con un solo comando ejecutado desde la **raíz del proyecto**:

```bash
# Construir imágenes y levantar todos los servicios en segundo plano
docker compose up -d --build
```

### Servicios y Puertos Expuestos

| Servicio | Contenedor | URL / Puerto | Descripción |
|---|---|---|---|
| **Frontend Portal** | `pqrsdf-frontend` | [http://localhost:3000](http://localhost:3000) | Aplicación Next.js 15 App Router |
| **Backend API** | `pqrsdf-backend` | [http://localhost:5023](http://localhost:5023) | API RESTful en ASP.NET Core 10 |
| **Swagger UI** | `pqrsdf-backend` | [http://localhost:5023/swagger](http://localhost:5023/swagger) | Interfaz interactiva de documentación OpenAPI |
| **Health Checks** | `pqrsdf-backend` | [http://localhost:5023/health](http://localhost:5023/health) | Estado operativo y conectividad a SQL Server |
| **SQL Server** | `pqrsdf-sqlserver` | `localhost:1433` | Motor de base de datos relacional (sa / `YourStrong@Passw0rd!`) |

---

## 🛠️ Comandos de Gestión con Docker Compose

```bash
# Ver estado de los contenedores
docker compose ps

# Ver logs en tiempo real de todos los servicios
docker compose logs -f

# Ver logs de un servicio específico
docker compose logs -f backend
docker compose logs -f frontend

# Detener todos los servicios
docker compose down

# Detener y eliminar volúmenes de datos (reinicio limpio de base de datos)
docker compose down -v
```
