# portafolio-broxchain-backend-dev
Proyecto de desarrollo Broxel, Backend de Fondo de Inversión 

## 📖 Descripción
Este proyecto consiste en una API REST desarrollada en .NET 9.0 bajo una arquitectura de capas (Repository Pattern). Su propósito es gestionar las operaciones de un Fondo de Inversión, incluyendo autenticación segura, manejo de flujos de dinero y auditoría de transacciones.

## 🚀 Tecnologías Utilizadas
* **Lenguaje:** C#
* **Framework:** .NET 9.0
* **Base de Datos:** SQL Server (Compatible con SQL Server 2022 / Azure SQL / Google Cloud SQL)
* **Infraestructura Cloud:** Google Cloud Platform (Cloud Run, Cloud SQL, Cloud Build).
* **Librerías Principales:**
    * **Entity Framework Core:** Mapeo objeto-relacional (ORM).
    * **Google Cloud Storage:** Integración para almacenamiento de archivos en la nube.
    * **JWT Bearer:** Seguridad y autenticación basada en tokens.
    * **CsvHelper:** Procesamiento y lectura de archivos CSV.
    * **Newtonsoft.Json:** Manipulación de datos JSON.
    * **Swagger/OpenAPI:** Documentación automática de la API.
* **Herramientas:** Visual Studio 2022, Docker, Google Cloud SDK, DBeaver.

## 📋 Pre-requisitos
Antes de ejecutar este proyecto, asegúrate de tener instalado:

1.  **Visual Studio Code** (Versión 1.107 o superior) con la extensión [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit).
2.  **.NET SDK 9.0** (Requerido por el proyecto).
3.  **Google Cloud CLI** (Para la ejecución en entorno macOS/Híbrido y despliegues).
4.  **Gestor de BD:** DBeaver (Recomendado) o SSMS.

---

## 🔧 Instalación y Ejecución Local

Existen dos formas de ejecutar el proyecto localmente. Elige la que se adapte a tu sistema operativo.

### Opción A: Vía Docker (Windows / Linux)
Sigue estos pasos si deseas levantar una base de datos local efímera.

1.  **Clonar el Repositorio**
    ```bash
    git clone [https://github.com/EmmanuelEspinoza/portafolio-broxchain-backend-dev.git](https://github.com/EmmanuelEspinoza/portafolio-broxchain-backend-dev.git)
    cd portafolio-broxchain-backend-dev
    ```
2.  **Levantar SQL Server**
    ```bash
    docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=TuPasswordFuerte123!" \
       -p 1433:1433 --name sql_server_local \
       -d [mcr.microsoft.com/mssql/server:2022-latest](https://mcr.microsoft.com/mssql/server:2022-latest)
    ```
3.  **Configurar Conexión**
    Asegúrate de que tu `appsettings.Development.json` apunte a `localhost,1433` con el usuario `sa`.

### Opción B: Entorno macOS / Híbrido (Cloud SQL Proxy)
[cite_start]Solución para ejecutar el proyecto en macOS (chips M1/M2/M3). La base de datos del proyecto es SQL Server sin embargo no existe este software de forma nativa dentro de macOS, existen dos soluciones a este problema. La primera es utilizar docker para correr la base de datos sobre un contenedor pero dadas las limitaciones institucionales esto no es posible. La segunda es utilizar un una instancia de Cloud SQL de GCP que aloje una base de datos SQL Server. Si este es el caso ve a la seccion de despligue e infraestructura e implementa los pasos para crear la base datos. Una vez que hayas creado la base datos regresa a este punto y continua...

#### 1. Preparar el Proxy
1.  [cite_start]Descargar el binario del proxy (v2 para Mac ARM64)[cite: 84]:
    ```bash
    curl -o cloud-sql-proxy [https://storage.googleapis.com/cloud-sql-connectors/cloud-sql-proxy/v2.11.0/cloud-sql-proxy.darwin.arm64](https://storage.googleapis.com/cloud-sql-connectors/cloud-sql-proxy/v2.11.0/cloud-sql-proxy.darwin.arm64)
    ```
2.  Dar permisos de ejecución[cite: 87]:
    ```bash
    chmod +x cloud-sql-proxy
    ```

#### 2. Configurar Autenticación (Service Account)
[cite_start]Debido a restricciones con cuentas institucionales, se debe usar una Cuenta de Servicio[cite: 89, 90]:
1.  [cite_start]En GCP Console, crear una Service Account llamada `sql-proxy-local`[cite: 95].
2.  [cite_start]Asignar el rol: **Cloud SQL Client**[cite: 96].
3.  [cite_start]Generar una clave JSON y descargarla como `key.json` en la raíz del proyecto [cite: 101-103].

#### 3. Ejecutar el Túnel
[cite_start]Ejecuta el proxy apuntando a la instancia de desarrollo (reemplaza con tu `Connection Name`)[cite: 108]:
```bash
./cloud-sql-proxy TU_PROYECTO:us-central1:brox-sqlserver --port 1433 --credentials-file key.json
```

Deberás ver el mensaje: "Ready for new connections" escuchando en 127.0.0.1:1433.

4. Configuración del Proyecto
Modifica tu appsettings.Development.json para que la aplicación crea que la BD es local :

```JSON

"ConnectionStrings": {
  "DefaultConnection": "Server=127.0.0.1,1433;Database=FondoInversionLocal;User Id=sqlserver;Password=TU_PASSWORD_NUBE;TrustServerCertificate=True;"
}
```

### 🗄️ Inicialización de Base de Datos
Independientemente de la opción elegida (A o B), ejecuta las migraciones para crear las tablas:

```Bash

dotnet ef database update
```

### ▶️ Ejecución de la API

## 1. Iniciar la aplicación:

# Limpiar el repositorio
Borra la carpeta /bin y /obj. Esto garantiza que no haya "fantasmas" o archivos compilados viejos que causen errores raros (muy común si cambiaste de rama en Git o modificaste archivos de configuración profundos)

```bash
dotnet clean
```
# Compilacion correcta de codigo.
Verifica que el código compila correctamente antes de intentar ejecutarlo. Si hay un error de sintaxis, fallará aquí claramente en lugar de explotar durante el arranque.
dotnet clean

# Utilizar perfil de desarrollo
 Forzar que la variable dea Development, asegurando que se cargue la configuracion local.
```Bash
dotnet run --launch-profile http
```

# 2. Probar Endpoints: Navega a: http://localhost:5206/swagger

#### ☁️ Infraestructura y Despliegue (Google Cloud Platform)

Este proyecto utiliza una arquitectura **Serverless** con alta seguridad en la red. A continuación, se detallan los comandos de `gcloud` utilizados para aprovisionar el entorno, basados en la documentación del proyecto.

### 1. Configuración de Red (VPC)
[cite_start]Se creó una red privada virtual y una subred personalizada para aislar los recursos y evitar accesos públicos no autorizados[cite: 7, 8].

```bash
# Crear la VPC en modo personalizado
gcloud compute networks create brox-vpc --subnet-mode=custom
```

# Crear la Subred en la región us-central1
```bash
gcloud compute networks subnets create brox-subnet-1 \
    --network brox-vpc \
    --region us-central1 \
    --range 10.80.1.0/28
```

### 2. Acceso Privado a Servicios (Private Service Access)

Configuración necesaria para que Cloud Run y SQL Server se comuniquen internamente mediante VPC Peering.

# 2.1. Reservar rango de IP para Peering
gcloud compute addresses create brox-psa-range \
    --global \
    --purpose VPC_PEERING \
    --prefix-length=16 \
    --network brox-vpc

# 2.2 Conectar la red privada con los servicios de Google
gcloud services vpc-peerings connect \
    --service servicenetworking.googleapis.com \
    --network brox-vpc \
    --ranges=brox-psa-range


### 3. Base de Datos (Cloud SQL)
Provisión de la instancia SQL Server 2022. Nota importante: Se utiliza el flag --no-assign-ip para no asignar IP pública por seguridad.

```bash
gcloud sql instances create brox-sqlserver \
    --database-version=SQLSERVER_2022_STANDARD \
    --cpu=2 \
    --memory=8GB \
    --region=us-central1 \
    --root-password=PASSWORD_SECRETO \
    --network=projects/pruebasbroxchain/global/networks/brox-vpc \
    --no-assign-ip
```


### 4. Conector VPC (Serverless VPC Access)
Se crea un conector para servir de puente y permitir que las instancias Serverless (Cloud Run) accedan a la red privada.
```bash
gcloud compute networks vpc-access connectors create brox-connector-1 \
    --region us-central1 \
    --subnet brox-subnet-1 \
    --min-instances=2 \
    --max-instances=3
```

### 5. Despliegue del Servicio (Cloud Run)
Finalmente, se despliega el contenedor Docker conectándolo al conector VPC para que alcance la base de datos.

```bash
gcloud run deploy fondo-inversion-service \
    --image us-central1-docker.pkg.dev/broxel1/broxchain/cloud-run-source-deploy/fondo-inversion-1 \
    --region us-central1 \
    --execution-environment gen2 \
    --vpc-connector brox-connector-1 \
    --vpc-egress all-traffic
```


#### Pipeline de CI/CD (Cloud Build)
El despliegue está automatizado mediante Google Cloud Build siguiendo estos pasos definidos en cloud-build.yaml :

### 1. Build: Empaquetado del código en una imagen Docker.

```Bash
gcloud builds submit --config cloud-build.yaml
```

### 2. Deploy: Despliegue de la imagen en Cloud Run conectada al conector VPC .

```Bash

gcloud run deploy fondo-inversion-service \
  --image us-central1-docker.pkg.dev/.../fondo-inversion-1 \
  --vpc-connector brox-connector-1 \
  --vpc-egress all-traffic
```

### 🔐 Seguridad y Comunicación (CORS)
Se configuró una política de CORS restrictiva (_myAllowSpecificOrigins) en Program.cs que permite explícitamente solo los orígenes de confianza:

- http://localhost:4200 (Frontend Local)

- http://localhost:5206 (Swagger Local)

- https://fondo-inversion-front-[ID].us-central1.run.app (Producción GCP)


### 🏗️ Estructura del Proyecto

/FondoInversion
├── Controllers/       # API Endpoints (Presentación)
├── Services/          # Lógica de Negocio (Business Logic)
├── Repositories/      # Acceso a Datos (Repository Pattern)
├── Models/            # Entidades de Dominio (EF Core)
├── Data/              # DbContext y Configuración de BD
├── DTO/               # Data Transfer Objects
└── Helpers/           # Utilidades (Encryption, JWT)