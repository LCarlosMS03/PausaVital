# 🛡️ Pausa Vital

> Bienestar digital y productividad impulsados por Tecnología Calma.

**Pausa Vital** es una aplicación de escritorio diseñada para mitigar la fatiga visual y el sedentarismo mediante la gestión inteligente de descansos. Combina metodologías probadas de productividad con un sistema de gamificación *"Recovery-First"* para ayudar a los usuarios a construir hábitos saludables sin interrumpir agresivamente su flujo de trabajo.

## Características Principales

* **Gestor Dual de Descansos:** Soporte nativo para la regla oftalmológica 20-20-20 (prevención de fatiga visual) y la técnica Pomodoro (25/5 para enfoque profundo).
* **Motor de Gamificación:** Sistema de rachas por descansos completados y una economía de recompensas automatizada que otorga "Escudos protectores" para salvar rachas perdidas.
* **Monitoreo Pasivo:** Ejecución silenciosa desde el *System Tray* (Bandeja del sistema) de Windows con alertas nativas no intrusivas.
* **Alertas de Hidratación:** Recordatorios automatizados para fomentar el consumo regular de agua.
* **Interfaz Bilingüe (L10n):** Soporte dinámico para Inglés y Español en la interfaz gráfica, intercambiable en tiempo real.

## Arquitectura y Tecnologías

El proyecto emplea una arquitectura híbrida que separa la capa de presentación del motor lógico:

* **Frontend:** Desarrollado en **C# con .NET 8 (WPF)**, ofreciendo un consumo de recursos mínimo y una integración visual nativa con Windows.
* **Backend:** Microservicio local construido en **Python (FastAPI)**, incrustado en el ensamblado principal como un *Embedded Resource*.
* **Base de Datos:** El servicio de DB funciona localmente con **Python (SQLite)**, respetando el pensamiento "Local-First".
* **Estandarización:** Para mantener la coherencia técnica y las mejores prácticas de la industria, toda la base de código, el entorno, las variables y las transacciones de base de datos están programadas íntegramente en Inglés.

## Instalación y Despliegue

Pausa Vital está optimizado para funcionar en cualquier equipo sin configuraciones previas:

1. Dirígete a la sección de [Releases](../../releases) a la derecha de este repositorio.
2. Descarga el extraible "PausaVital X.X.X.zip".
3. Extraer el .zip
3. Ejecuta el programa "PausaVital.exe" directamente.
