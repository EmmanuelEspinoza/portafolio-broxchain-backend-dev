# portafolio-broxchain-backend-dev
Proyecto de desarrollo Broxel, Backend de Fondo de Inversión 

## 📖 Descripción
Aquí explica con más detalle el propósito del proyecto.
* ¿Qué problema resuelve?
* ¿Es una aplicación de consola, una API, Web Forms o MVC?
* ¿Es un proyecto final para una materia específica?

## 🚀 Tecnologías Utilizadas
* **Lenguaje:** C#
* **Framework:** .NET 9.0
* **Base de Datos:** SQL Server (Compatible con SQL Server 2022)
* **Librerías Principales:**
    * **Entity Framework Core:** Mapeo objeto-relacional (ORM).
    * **Google Cloud Storage:** Integración para almacenamiento de archivos en la nube.
    * **JWT Bearer:** Seguridad y autenticación basada en tokens.
    * **CsvHelper:** Procesamiento y lectura de archivos CSV.
    * **Newtonsoft.Json:** Manipulación de datos JSON.
    * **Swagger/OpenAPI:** Documentación automática de la API.
* **Herramientas:** Visual Studio 2022, Docker, Google Cloud SDK.


## 📋 Pre-requisitos
Antes de ejecutar este proyecto, asegúrate de tener instalado:

1.  **Visual Studio Code** (Versión 1.107 o superior).
    * Extensión recomendada: [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit).
2.  **.NET SDK 9.0** (Requerido por el proyecto).
3.  **Docker Desktop** (Obligatorio en macOS para ejecutar SQL Server).


## 🔧 Instalación y Configuración

Sigue estos pasos secuenciales para levantar el proyecto en tu entorno local (macOS).

### 1. Clonar el Repositorio
Abre tu terminal y descarga el código fuente:

```bash
git clone [https://github.com/EmmanuelEspinoza/portafolio-broxchain-backend-dev.git](https://github.com/EmmanuelEspinoza/portafolio-broxchain-backend-dev.git)
cd portafolio-broxchain-backend-dev
```

### 2. Restaurar Paquetes
dotnet restore

### 3. Levantar Base de Datos (Docker)
Ejecuta este comando para crear y encender el contenedor de SQL Server:

docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=TuPasswordFuerte123!" \
   -p 1433:1433 --name sql_server_local \
   -d [mcr.microsoft.com/mssql/server:2022-latest](https://mcr.microsoft.com/mssql/server:2022-latest)

### 3. Configurar Cadena de Conexión
Abre el archivo appsettings.Development.json y asegúrate de que la conexión apunte a tu Docker local:

### 4. Inicializar la Base de Datos (Migraciones)

Crea las tablas ejecutando:

```bash
    dotnet ef database update
```

### 🚀 Cómo Ejecutar
Una vez configurado el entorno, sigue estos pasos para iniciar el servicio:

### 1. Iniciar la aplicación:

```bash
    dotnet run
```

### 2. Verificar el funcionamiento: 
La terminal indicará que el servicio escucha en el puerto 5206.

### 3. Probar Endpoints: 
Abre tu navegador en la siguiente URL para ver la documentación interactiva:

👉 http://localhost:5206/swagger

### 4. Configurar entorno local
    El archivo appsettings.Development.json por defecto no incluye la conexión local.

    4.1 Abre el archivo: /FondoInversion/appsettings.Development.json.

    4.2 Reemplaza todo su contenido con el siguiente JSON (configurado para Docker y JWT local):

    {
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=FondoInversionLocal;User Id=sa;Password=TuPasswordFuerte123!;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Secret": "ClaveSecretaSuperSeguraParaDesarrolloLocal123!",
    "RefreshSecret": "ClaveRefreshParaLocal123!",
    "Issuer": "broxchain",
    "Audience": "broxchain"
  },
  "AllowedHosts": "*"
    }


## 🔐 Seguridad y Comunicación (CORS)

Para permitir la comunicación segura entre el cliente (Frontend) y el servidor, se configuró una política de **CORS (Cross-Origin Resource Sharing)** restrictiva pero funcional.

Esto resuelve los problemas de bloqueo de peticiones HTTP (`Network Error` o `CORS Policy blocked`) permitiendo explícitamente solo los orígenes de confianza.

### Política: `_myAllowSpecificOrigins`
Se definieron los siguientes orígenes permitidos ("Whitelist"):

1.  **Entorno de Desarrollo Local:**
    * `http://localhost:4200` (Cliente Angular/React local)
    * `http://localhost:5206` (Pruebas de API local)
2.  **Entorno de Producción (GCP):**
    * `https://fondo-inversion-front-387791937810.us-central1.run.app` (Frontend desplegado en Cloud Run)

### Configuración en `Program.cs`
La política habilita el intercambio completo de recursos bajo estas condiciones:
* ✅ **.AllowAnyHeader()**: Permite cualquier encabezado HTTP.
* ✅ **.AllowAnyMethod()**: Permite todos los verbos (GET, POST, PUT, DELETE, etc.).
* ✅ **.AllowCredentials()**: Permite el envío de cookies o credenciales de autenticación.

```csharp
// Ejemplo de la implementación actual en Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "_myAllowSpecificOrigins",
        policy =>
        {
            policy.WithOrigins([
                "http://localhost:4200", 
                "http://localhost:5206", 
                "[https://fondo-inversion-front-387791937810.us-central1.run.app](https://fondo-inversion-front-387791937810.us-central1.run.app)"
            ])
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        });
});


### 🏗️ Estructura del Proyecto
La solución sigue una arquitectura modular:

Plaintext

/FondoInversion
├── Controllers/       # API Endpoints (Presentación)
├── Services/          # Lógica de Negocio (Business Logic)
├── Repositories/      # Acceso a Datos (Repository Pattern)
├── Models/            # Entidades de Dominio (EF Core)
├── Data/              # DbContext y Configuración de BD
├── DTO/               # Data Transfer Objects
└── Helpers/           # Utilidades (Encryption, JWT)