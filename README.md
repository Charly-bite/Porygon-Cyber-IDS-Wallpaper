# Porygon Cyber IDS Wallpaper Suite 🦆⚡🛡️

Un **Wallpaper Animado Cyberpunk / Hacker ultraligero (~39 MB RAM)** para Windows 11 escrito en C# nativo (sin motores pesados ni dependencias externas) que integra un **Enjambre Autónomo 3D de las 3 evoluciones de Porygon (`Porygon`, `Porygon2` y `Porygon-Z`)** impulsados por una **Red Neuronal Spiking (inspirada en el conectoma de `snedea/flybrain`)** con aprendizaje Hebbiano y **Detección Estadística de Anomalías de Red (IDS) en tiempo real**.

![Vista Previa del Enjambre Porygon 3D e Inspector de Nodos](assets/preview_swarm.png)

![Entornos 3D Procedurales, Conectoma FlyWire 3D y Telemetría GPU](assets/preview_3d_brain_and_scenes.png)

---

## ✨ Características Principales

### 1. 🧠 Enjambre 3D Autónomo (`X, Y, Z`) y Conectoma Anatómico 3D (`FlyWire Drosophila`)
* **Navegación Espacial 3D Completa (`X, Y, Z`):** Tanto los 3 Porygon (`Porygon`, `Porygon2` y `Porygon-Z`) como los nodos/paquetes de red navegan con coordenadas tridimensionales completas ($Z \in [-240, +240]$), escalado de perspectiva real, sombras proyectadas en el suelo 3D y telemetría `XYZ:[x,y,z]`.
* **Inspector Anatómico 3D del Conectoma (`>> FLYWIRE_3D // DROSOPHILA BRAIN CONNECTOME`):**
  * Malla cortical 3D anatómica de alta densidad (~496 triángulos) que modela el cerebro de *Drosophila* (*Central Brain*, *Optic Lobes L/R*, *Mushroom Body Calyx & Pedunculus* en azul eléctrico y *Antennal Lobe Glomeruli* en ámbar).
  * **64 Neuronas Spiking Confinadas Anatómicamente:** Somas neuronales con arborizaciones dendríticas y tractos sinápticos curvos (Bézier 3D) estrictamente contenidos dentro de la corteza 3D.
  * **Rotación 3D Interactiva y Pantalla Completa:** Rota la cámara 3D arrastrando con el mouse o con la rueda del ratón (`Yaw` / `Pitch` / `Zoom`), o ábrelo en **Pantalla Completa (`[FULL]` / `Ctrl + Alt + B`)**.

### 2. 🌲 3 Entornos 3D Procedurales de Fondo (`Vértices + Polígonos + Oscilador`)
Construidos 100% en OpenGL por hardware con geometría poligonal por lotes (`GL_TRIANGLES`, `GL_LINE_STRIP`, `GL_LINES`, `GL_QUADS`) y paleta **Eye-Care Mate** (reducción del 35%–40% de fatiga visual, sin líneas púrpuras saturadas). Alternables en vivo con **`Ctrl + Alt + M`** o el botón **`[BG:1/3]`**:
1. **Modo 1/3 (`[BG:1]` — `SELVA+OSC`):** *Ecosistema Selva Bio-Digital Low-Poly + Río de Datos 3D + Cordillera y Portal en el Horizonte*:
   * **Cordillera Low-Poly y Portal de Convergencia:** 14 picos montañosos facetados que enmarcan el cañón central y un portal/halo geodésico concéntrico en el punto de fuga del horizonte.
   * **Río Cibernético 3D con Flujo de Datos:** Cauce curvado en S que serpentea por el valle y desaparece en el horizonte, con 5 carriles de paquetes de datos luminosos, tokens hexadecimales/binarios arrastrados por la corriente (`VRAM Font Atlas`), **5 rocas semisumergidas con estelas en V** de espuma de datos y **7 velos de neblina translúcida** flotando sobre el agua.
   * **Bosque Low-Poly Multi-Especie y Flora Bioluminiscente:** 14 árboles *low-poly* de 3 especies cohesivas (*Pino Conífero Clásico*, *Abeto Esbelto de 4 niveles* y *Árbol de Dosel Geodésico Hexagonal*), 12 formaciones rocosas con cristales en las riberas, 14 rosetas de helechos arqueados, 26 **luciérnagas/esporas bio-digitales flotantes** y 8 lianas de dosel.
2. **Modo 2/3 (`[BG:2]` — `TOPO+MESH`):** *Terreno de Oscilador Vectorial + Bóveda de Triángulos Delaunay* (9 crestas topográficas de oscilador vectorial moduladas por la red + bóveda superior geodésica 3D de triángulos Delaunay).
3. **Modo 3/3 (`[BG:3]` — `MYCELIUM`):** *Bosque Micelial Poligonal Bioluminiscente* (Troncos poligonales 3D ramificados desde suelo y techo + racimos de cristales octaédricos facetados y pulsos bioluminiscentes).

### 3. 🌧️ Lluvia de Logs 100% Vertical con Profundidad 3D y 7 Categorías Semánticas
* **Texto 100% Vertical Garantizado:** Cada columna renderiza caracteres apilados verticalmente con seguimiento por columna.
* **3 Planos de Profundidad 3D (`DepthLayer 0, 1, 2`):** Efecto paralaje real con diferentes escalas, velocidades y niveles de contraste sobre fondo obsidiana.
* **7 Colores Semánticos Mate:**
  * 🔴 **Carmesí Terracota:** Alertas críticas IDS / Intrusiones (+34% velocidad de caída).
  * 🟡 **Oro Solar Mate:** Inteligencia de amenazas **VirusTotal v3** (+34% velocidad de caída).
  * 🟠 **Ámbar Cobre:** Eventos de Firewall **FortiGate** / IPS.
  * 🌸 **Rosa Coral Mate:** Anomalías estadísticas NIDS / NDR y Z-Score.
  * 🩵 **Cian Salvia Fósforo:** Flujos activos TCP / TLS / HTTPS / DNS.
  * 🟢 **Esmeralda Bosque:** Tráfico verificado limpio / Línea base saludable.
  * 💠 **Verde Azulado Bio-Teal:** Neuro-evolución del enjambre Porygon y poda sináptica.

### 4. ⚡ Alto Rendimiento OpenGL (~42 FPS), Zero-GC y Gobernador Adaptativo de 3 Niveles
* **Telemetría de Hardware en Vivo (`>> SYS_TELEMETRY`):** Muestra carga de `CPU`, **`GPU: XX.X% (X.Xms | XXfps)`** (muestreando los motores 3D de hardware vía `\GPU Engine(*)\Utilization Percentage`), `RAM` (~41 MB) y latencia `PING`.
* **Gobernador Adaptativo Inteligente de 3 Niveles:**
  * **Escritorio Enfocado (`~42 FPS`):** Máxima fluidez interactiva con paso de tiempo normalizado (`dtScale`).
  * **Ventana Flotante No Maximizada (`~38 FPS`):** Mantiene el fondo fluido mientras trabajas en ventanas con el escritorio visible.
  * **Ventana Maximizada / Modo Eco (`~25 FPS [eco]`):** Ahorra GPU automáticamente cuando una aplicación maximizada cubre la pantalla.
* **Batching OpenGL Unificado y Zero-GC:** Toda la vegetación 3D, rocas, río, luciérnagas y terreno se emiten en lotes unificados (`GL_TRIANGLES`, `GL_LINES`, `GL_QUADS`) con buffers preasignados (cero asignaciones por cuadro en el Garbage Collector) y actualización escalonada *Round-Robin* de texturas HUD en VRAM.

### 5. 🛡️ Suite Real de Ciberseguridad v2.5: NIDS+PID, Antivirus/EDR, SIEM y Active IPS (Modo Híbrido)
Cada evolución de la tríada **Porygon** ejecuta un módulo real de defensa sobre Windows 11 a través del motor [`PorygonCyberDefenseEngine`](src/HackerWallpaper.cs):
1. **`PORYGON` — NIDS + Mapeador Real Socket $\leftrightarrow$ Proceso/PID (`iphlpapi.dll` `GetExtendedTcpTable`) + SIEM:**
   * Cruza cada socket TCP activo con la tabla extendida del kernel para identificar el **`PID` exacto**, el **nombre y ruta del `.exe`**, y verifica su **firma digital `Authenticode` (`X509Certificate`)**.
   * Detecta **LOLBins** (`powershell`, `pwsh`, `cmd`, `mshta`, `wscript`, `rundll32`, `regsvr32`, `certutil`) realizando conexiones externas y binarios sin firma ejecutándose desde `%TEMP%` o `Downloads`.
   * **Motor SIEM Local (`MITRE ATT&CK`):** Audita persistencia en el Registro (`HKCU\...\Run` y `HKLM\...\Run` — `T1547.001`), correlaciona el `EventLog` de Windows (instalación de servicios `Event ID 7045` — `T1543.003`) y exporta eventos estructurados en `logs/siem_security_events.jsonl`.
2. **`PORYGON2` — Antivirus / EDR en Tiempo Real (`FileSystemWatcher` + `SHA-256` + Entropía de Shannon):**
   * Vigila en segundo plano `Downloads`, `Desktop`, `%TEMP%` y `Startup` al crearse o modificarse ejecutables y scripts (`.exe`, `.dll`, `.ps1`, `.vbs`, `.bat`, `.cmd`, `.hta`, `.scr`, `.msi`).
   * Calcula el hash **`SHA-256` real**, mide la **Entropía de Shannon ($0\text{–}8\text{ bits/byte}$)** para detectar binarios empaquetados/cifrados (`H >= 7.30b`, `MITRE T1027`), verifica la firma `Authenticode` e inspecciona indicadores estáticos de evasión (`FromBase64String`, `-enc`, `AmsiUtils`, `DownloadString`, `VirtualAlloc`).
3. **`PORYGON-Z` — IPS Activo en Windows Firewall + Bóveda de Cuarentena AV (Modo Híbrido):**
   * **Política Híbrida Inteligente:** Bloquea automáticamente en Windows Firewall (`netsh advfirewall firewall add rule name="PorygonIPS_Block_<IP>"`) o aísla en la Bóveda de Cuarentena (`.qz`) únicamente amenazas críticas confirmadas (`Score >= 0.92` / `0.94`), protegiendo siempre IPs LAN/DNS (`RFC1918`, `127.0.0.1`, `1.1.1.1`, `8.8.8.8`) y binarios del sistema.
   * **Botones 1-Click en el HUD y Centro de Comando SOC (`Ctrl + Alt + S`):** Incluye botones interactivos **`[SIEM/IPS]`**, **`[SCAN]`**, **`[BLOCK IP]`** y **`[QUAR]`** directamente en la tarjeta HUD del escritorio, además de la ventana dedicada **`ShowSiemCommandCenterWindow`** para auditar eventos SIEM, escanear archivos bajo demanda, desbloquear IPs del Firewall o restaurar archivos en cuarentena con un clic.

---

## ⌨️ Atajos de Teclado Globales
| Atajo | Acción |
| :--- | :--- |
| **`Ctrl + Alt + S`** | Abre el **Centro de Comando SOC / SIEM / IPS / Antivirus / Bóveda de Cuarentena**. |
| **`Ctrl + Alt + W`** | Abre el **Panel de Configuración y Evolución Genómica en Vivo**. |
| **`Ctrl + Alt + G`** | Activa / desactiva el **Modo Gamer (0.0% GPU // Hibernación Total)**. |
| **`Ctrl + Alt + M`** | Alterna en vivo entre los **3 Entornos 3D Procedurales de Fondo** (`SELVA+OSC` $\rightarrow$ `TOPO+MESH` $\rightarrow$ `MYCELIUM`). |
| **`Ctrl + Alt + B`** | Abre / cierra el **Inspector del Conectoma 3D FlyWire en Pantalla Completa**. |
| **`Ctrl + Alt + P`** | Lanza una **Simulación / Oleada de Anomalías IDS** para entrenar a los 3 Porygon en vivo. |
| **`Ctrl + Alt + Up / Down`** | Aumenta o disminuye globalmente la **opacidad de Windows Terminal** (±10%, también disponible con botones `[OP-]`/`[OP+]` en el HUD). |

---

## 🚀 Compilación e Instalación Rápida (Sin dependencias externas)

El proyecto utiliza el compilador nativo de .NET Framework incluido en Windows (`csc.exe`):

```powershell
# Compilar e instalar automáticamente en %LOCALAPPDATA%\Programs\HackerWallpaper
powershell -ExecutionPolicy Bypass -File .\scripts\install.ps1
```

## 📁 Estructura del Repositorio
* `src/HackerWallpaper.cs` — Motor principal OpenGL + GDI+ del Wallpaper, 3 Entornos 3D Poligonales, Renderizador 3D GLB, Conectoma Anatómico 3D `FlyBrainConnectome`, Detector Estadístico IDS, Integración VirusTotal v3 y Radar 360°.
* `src/HackerLockScreen.cs` — Protector / Pantalla de bloqueo animada Cyberpunk.
* `src/TrayAutoHide.cs` — Utilidad ligera para auto-ocultar iconos secundarios de la bandeja del sistema sin alterar la composición DWM (`Shell_TrayWnd`).
* `Models/` — Modelos 3D `.glb` de `Porygon`, `Porygon2` y `Porygon-Z`.
* `scripts/` — Scripts de compilación (`build.ps1`), instalación (`install.ps1`) y generación de iconos Fluent Dark (`generate_fluent_icons.py`).
* `terminal/cyberpunk-neon.omp.json` — Tema Cyberpunk Neón para Windows Terminal + Oh My Posh.
* `docs/plan_optimizacion_rendimiento.md` — Plan y guía de optimización de rendimiento y red para Windows 11.
