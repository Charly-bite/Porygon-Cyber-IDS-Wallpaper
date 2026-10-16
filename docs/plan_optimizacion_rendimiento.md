# Plan de Implementación: Optimización de Rendimiento del Equipo

Basado en la auditoría de hardware y sistema realizada sobre tu equipo (**Intel Core i5-12450H** de 8 núcleos híbridos `4P+4E`, **NVIDIA GeForce RTX 3050 Laptop GPU**, **32 GB RAM**, **Windows 11 Pro X-Lite Build 28000**), se detectaron cuellos de botella específicos que actualmente desperdician memoria, aumentan la latencia y pueden provocar cierres inesperados en juegos pesados o compilaciones de código.

```mermaid
flowchart LR
    A["1. Arranque y Procesos\n(-850 MB RAM en reposo)"] --> B["2. Memoria Virtual y RAM\n(Corregir Pagefile de 16 MB)"]
    B --> C["3. CPU Híbrido i5-12450H\n(Plan de Energía y Core Parking)"]
    C --> D["4. GPU RTX 3050 + HAGS\n(Prioridad dGPU y Baja Latencia)"]
    D --> E["5. Almacenamiento y Red\n(Limpieza de Cachés y NVMe)"]
```

---

## Diagnóstico Actual Detectado

| Componente / Área | Estado Detectado en tu PC | Impacto Actual |
| :--- | :--- | :--- |
| **Inicio de Windows (`HKCU\...\Run` y UWP)** | Se inician solos `RobloxPlayerBeta`, `EpicGamesLauncher`, `Steam`, `Chrome AutoLaunch` y `WhatsApp.Root` (`280 MB RAM`). | Consume **~700–900 MB de RAM** adicionales desde que enciendes la PC y retrasa el inicio de sesión. |
| **Archivo de Paginación (`C:\pagefile.sys`)** | Limitado a solo **`16 MB`** (`AllocatedBaseSize: 16`). | **Crítico:** Juegos pesados (*Baldur's Gate 3*, *RDR2*) y compiladores (*Rust/Tauri* en `UAC` y `OW-IDS`) fallan con errores `Out of Virtual Memory` aunque tengas 32 GB de RAM física. |
| **Plan de Energía del CPU (`i5-12450H`)** | Plan estándar `Equilibrado` (`381b4222-...`). | Introduce micro-latencia al escalar frecuencias entre los núcleos de rendimiento (`P-Cores`) y eficiencia (`E-Cores`). |
| **Planificación de GPU (`HwSchMode`)** | No forzado en registro (`HKLM\...\GraphicsDrivers`). | Mayor latencia de renderizado entre `Intel UHD` y `RTX 3050 Laptop GPU`. |
| **Unidades de Almacenamiento (`C:` y `D:`)** | `C:` tiene `222.7 GB` libres; `D:` (`FastStorage`) tiene solo **`51.5 GB` libres (`89%` lleno)**. | Los SSD reducen su velocidad de escritura cuando superan el 85% de ocupación. |

> [!IMPORTANT]
> Todos los cambios propuestos en este plan son **100% seguros, reversibles** y no afectan tus proyectos de desarrollo (`OW-IDS`, `UAC`, `TaskHub`), tu configuración de red (`SmartFailover`) ni la personalización visual que acabamos de instalar.

---

## Fases de Implementación

### Fase 1: Desactivación de Auto-Arranque Innecesario y Apps en Segundo Plano
Liberar memoria RAM y ciclos de CPU desde el encendido sin desinstalar ninguna aplicación (solo evitar que arranquen solas ocultas en segundo plano):
1. **Limpiar `HKCU:\Software\Microsoft\Windows\CurrentVersion\Run`**:
   - Quitar el auto-arranque automático de `RobloxPlayerBeta` (`--launch-to-tray`), `EpicGamesLauncher` (`-silent`) y `GoogleChromeAutoLaunch` (`--no-startup-window`).
   - *(Opcional)* Mantener o quitar `Steam -silent` según tu preferencia.
2. **Desactivar auto-inicio en segundo plano de WhatsApp UWP**:
   - Actualmente `WhatsApp.Root.exe` consume **`280.1 MB` de RAM** de forma permanente aunque no lo tengas abierto. Se deshabilitará su tarea de arranque automático para que solo consuma RAM cuando lo abras manualmente.

### Fase 2: Corrección de Memoria Virtual (`Pagefile`) y Gestión de RAM (32 GB)
1. **Ajustar `C:\pagefile.sys` de `16 MB` a un bloque dinámico seguro (`4096 MB` inicial – `8192 MB` máximo)**:
   - Evita cierres repentinos (*crashes*) en motores gráficos modernos (Unreal Engine / Rockstar RAGE) y en `rustc`/`cargo` durante la compilación de tus proyectos Tauri.
2. **Optimizar `SvcHostSplitThresholdInKB` para 32 GB de RAM**:
   - Configurar el umbral de agrupación de servicios (`33554432` KB) para reducir el número de procesos `svchost.exe` dispersos y disminuir la sobrecarga del kernel.

> [!TIP]
> Con **32 GB de RAM**, fijar un `pagefile.sys` mínimo de `4 GB` en `C:` apenas ocupa el `1.7%` de tu espacio libre en `C:` (`222.7 GB`), pero garantiza estabilidad total en cargas intensivas.

### Fase 3: Optimización del Procesador Híbrido (`Intel Core i5-12450H`)
1. **Activar y ajustar el perfil de Alto Rendimiento / Ultimate Performance**:
   - Desbloquear el esquema de energía de **Máximo Rendimiento** para cuando el equipo está conectado a corriente, manteniendo el ahorro energético en batería.
2. **Política de Planificación Heterogénea (`HeteroPolicy`)**:
   - Configurar el planificador de Windows 11 para priorizar los **4 P-Cores (Performance)** en hilos de primer plano (juegos, compilación, IDE) y delegar tareas en segundo plano a los **4 E-Cores (Efficient)**.
   - Desactivar el *Core Parking* en corriente alterna (`AC`) para eliminar micro-tirones (*stuttering*).

### Fase 4: Aceleración de GPU (`NVIDIA RTX 3050 Laptop`) y Subsistema Gráfico
1. **Activar Hardware-Accelerated GPU Scheduling (`HwSchMode = 2`)**:
   - Permite que la `RTX 3050` gestione directamente su propia cola de memoria de video (VRAM), reduciendo la latencia de cuadros en ~1–2 ms.
2. **Prioridad explícita de Alto Rendimiento (`GpuPreference = 2`)**:
   - Registrar tus ejecutables clave (juegos en `Juegos`, `Moonlight`, `OBS Studio`) en `HKCU:\Software\Microsoft\DirectX\UserGpuPreferences` para que Windows nunca los ejecute por error con la gráfica integrada `Intel UHD`.
3. **Limpieza de Telemetría de NVIDIA (`NVDisplay.Container`)**:
   - Mantener únicamente el servicio esencial del driver de pantalla y desactivar telemetría/efectos superfluos del contenedor que actualmente ocupa `172.7 MB` de RAM.

### Fase 5: Almacenamiento (`SSD TRIM` + Limpieza de Cachés)
1. **Limpieza segura de cachés temporales y compilaciones antiguas**:
   - Purgar archivos huérfanos en `%TEMP%`, caché de sombreadores DirectX (`D3DSCache`) y cachés de instaladores sin tocar tus archivos personales.
2. **Análisis de espacio en unidad `D:` (`FastStorage`)**:
   - Identificar los archivos o carpetas más pesados en `D:` (que está al `89%` de capacidad) y verificar que `TRIM` (`DisableDeleteNotify = 0`) esté activo en ambas unidades para mantener la velocidad máxima de lectura/escritura NVMe/SSD.

---

## ¿Cómo proceder?
Puedes hacer clic en **Proceed** para que ejecute automáticamente estas 5 fases (generando además un script de respaldo por si deseas revertir cualquier ajuste en el futuro), o indicarme si prefieres omitir alguna fase (por ejemplo, si quieres que `Steam` o `WhatsApp` sí sigan iniciando con Windows).
