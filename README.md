# Porygon Cyber IDS Wallpaper Suite 🦆⚡🛡️

Un **Wallpaper Animado Cyberpunk / Hacker ultraligero (~39 MB RAM)** para Windows 11 escrito en C# nativo (sin motores pesados ni dependencias externas) que integra un **Enjambre Autónomo 3D de las 3 evoluciones de Porygon (`Porygon`, `Porygon2` y `Porygon-Z`)** impulsados por una **Red Neuronal Spiking (inspirada en el conectoma de `snedea/flybrain`)** con aprendizaje Hebbiano y **Detección Estadística de Anomalías de Red (IDS) en tiempo real**.

![Vista Previa del Enjambre Porygon 3D e Inspector de Nodos](assets/preview_swarm.png)

---

## ✨ Características Principales

### 1. 🧠 Enjambre 3D Autónomo con Conectoma Spiking (`FlyBrainConnectome`)
* **Renderizador 3D GLB por Software Integrado:** Carga y renderiza simultáneamente los modelos 3D en formato `.glb` (`pokemon_porygon.glb`, `porygon2__johto.glb` y `porygon-z.glb`) directamente sobre la capa `Progman / WorkerW` detrás de los iconos del escritorio.
* **100% Independientes (`FollowMouse = false` por defecto):** Cada evolución tiene su propia instancia de red neuronal spiking de 64 neuronas divididas en 4 regiones (*Sensory Input*, *Central Complex*, *Mushroom Body / Drives*, *Motor Output*), sus propias rutas de patrullaje y fuerzas de separación espacial.
* **Especialización por Evolución:**
  * **PORYGON (`FLOW_BASELINE_AI`):** Recolecta flujos TCP estándar y entrena la línea base estadística ($\mu, \sigma$).
  * **PORYGON2 (`ENDPOINT_AUDITOR`):** Audita nuevas direcciones IP externas vistas por primera vez y coordina enlaces neuronales P2P.
  * **PORYGON-Z (`ANOMALY_HUNTER_IDS`):** Caza y neutraliza anomalías reales de tráfico mediante *Salto Cuántico* y *Tri-Ataque Elemental*.

### 2. 🛡️ Detección Real de Anomalías de Tráfico (Network IDS)
Los 3 modelos se alimentan cada ciclo con telemetría real del stack de red de Windows (`IPGlobalProperties.GetActiveTcpConnections()`, estadísticas IPv4 de tu interfaz activa y latencia ICMP):
* **Anomalías de Puertos (`NON_STD_PORT`):** Detecta conexiones externas hacia puertos no estándar o sensibles (`21`, `22`, `23`, `445`, `3389`, `4444`, `8080`, etc.).
* **Picos Estadísticos Z-Score de Ancho de Banda (`DL_ZSCORE_SPIKE` / `UL_EXFIL_SPIKE`):** Calcula media móvil exponencial ($\mu$) y desviación estándar ($\sigma$) para detectar ráfagas anómalas ($> +2.0\sigma$).
* **Ráfagas Half-Open (`HALF_OPEN_SYN`):** Detecta sockets salientes atascados en estado `SYN_SENT` / `SYN_RECEIVED`.
* **Anomalías de Jitter / Latencia (`LATENCY_ANOMALY`):** Detecta saltos bruscos de RTT hacia el gateway ($> +2.2\sigma$).
* **Auditoría de Nuevos Hosts (`NEW_EXTERNAL_HOST`):** Genera Orbes Dorados cuando una IP remota nueva aparece en la sesión.

### 3. 🎯 Interacción al Pasar el Mouse (Hover Inspector)
Sin bloquear los clics hacia las carpetas del escritorio (`WS_EX_TRANSPARENT`), al pasar el cursor por encima de cualquier elemento:
* **Hover sobre Nodos Flotantes / Anomalías:** Congela el nodo en su posición, dibuja una retícula de bloqueo con línea vectorial hacia el Porygon asignado y abre una **Tarjeta Holográfica de Inspección** con `REMOTE_EP`, `LOCAL_SKT`, barra de `% ANOMALY`, diagnóstico y caudal en `KB/s`.
* **Hover sobre los Blips del Radar Sonar 360°:** Ilumina el punto del radar y despliega su tarjeta de telemetría (`RING1:LAN`, `RING2:WAN_TLS`, `RING3:WAN_CUSTOM`).
* **Hover sobre cualquiera de los 3 Porygon:** Despliega la tarjeta de su núcleo neuronal con sus entradas Z-Score en vivo (`DL_Z`, `UL_Z`, `PING_Z`), nivel, ganancia sináptica y estadísticas IDS.

### 4. ⚡ Sistema de Entrenamiento Hebbiano y 6 Habilidades Desbloqueables
El progreso de entrenamiento de cada Porygon se guarda automáticamente en `porygon_brains.json`:
* **LVL 1 — Rayo de Conversión (`CONVERSION_BEAM`):** Muestreo láser de paquetes de datos.
* **LVL 2 — Escudo Hexagonal Conversion 2 (`CYBER_SHIELD`):** Barrera geométrica hexagonal rotatoria.
* **LVL 3 — Enlace Neuronal P2P (`NEURAL_SYNC_LINK`):** Sincronización de pesos sinápticos entre dos Porygon cercanos (`+18 XP`).
* **LVL 4 — Tri-Ataque Elemental (`TRI_ATTACK_MATRIX`):** Triple haz láser (*Hielo Cian + Plasma Magenta + Electricidad Dorada*) que purga anomalías rojas (`+45 XP`).
* **LVL 5 — Salto Cuántico / Overclock (`QUANTUM_BLINK`):** Teletransporte de alta velocidad hacia anomalías distantes.
* **LVL 6 — Escaneo Sonar Profundo (`SONAR_DEEP_SCAN`):** Onda expansiva que revela nuevos nodos en el ciberespacio.

---

## ⌨️ Atajos de Teclado Globales
| Atajo | Acción |
| :--- | :--- |
| **`Ctrl + Alt + W`** | Abre el **Panel de Control en Vivo** (Paletas de color, velocidad de lluvia, tamaño 3D, modo entrenamiento, etc.). |
| **`Ctrl + Alt + P`** | Lanza una **Simulación / Oleada de Anomalías IDS** para entrenar a los 3 Porygon en vivo. |

---

## 🚀 Compilación e Instalación Rápida (Sin dependencias externas)

El proyecto utiliza el compilador nativo de .NET Framework incluido en Windows (`csc.exe`):

```powershell
# Compilar e instalar automáticamente en %LOCALAPPDATA%\Programs\HackerWallpaper
powershell -ExecutionPolicy Bypass -File .\scripts\install.ps1
```

## 📁 Estructura del Repositorio
* `src/HackerWallpaper.cs` — Motor principal del Wallpaper, Renderizador 3D GLB, Conectoma Spiking `FlyBrainConnectome`, Detector Estadístico IDS y Radar 360°.
* `src/HackerLockScreen.cs` — Protector / Pantalla de bloqueo animada Cyberpunk.
* `src/TrayAutoHide.cs` — Utilidad ligera para auto-ocultar iconos de la bandeja del sistema al pasar el mouse.
* `Models/` — Modelos 3D `.glb` de `Porygon`, `Porygon2` y `Porygon-Z`.
* `scripts/` — Scripts de compilación (`build.ps1`), instalación (`install.ps1`) y generación de iconos Fluent Dark (`generate_fluent_icons.py`).
* `terminal/cyberpunk-neon.omp.json` — Tema Cyberpunk Neón para Windows Terminal + Oh My Posh.
* `docs/plan_optimizacion_rendimiento.md` — Plan y guía de optimización de rendimiento y red para Windows 11.
