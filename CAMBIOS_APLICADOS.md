# Cambios aplicados en PausaVital

## Correcciones principales

1. Se corrigió el error de nombre `trayManager` / `TrayManager`.
   - Antes `App.xaml.cs` definía `trayManager` en minúscula.
   - `MainWindow.xaml.cs` usaba `App.TrayManager`, lo que podía provocar error de compilación.

2. Se agregó `BackendProcessManager`.
   - Intenta iniciar FastAPI automáticamente usando:
     - `py -m uvicorn main:app --host 127.0.0.1 --port 8000`
     - `python -m uvicorn main:app --host 127.0.0.1 --port 8000`
     - `python3 -m uvicorn main:app --host 127.0.0.1 --port 8000`
   - Si el backend ya está funcionando, no inicia otro proceso.
   - Si Python/Uvicorn no están instalados, la app no se cae; solamente marca el backend como desconectado.

3. Se mejoró `ActivityMonitor`.
   - Usa `Environment.TickCount64` y resta segura con wraparound de 32 bits, compatible con `GetLastInputInfo`.

4. Se mejoró `BreakManager`.
   - Cuenta solo tiempo activo.
   - Si el usuario se ausenta por 5 minutos o más, se considera una pausa natural y se reinicia el ciclo de trabajo.
   - Expone `WorkTime` para futuras estadísticas.

5. Se mejoró `ApiService`.
   - Usa un `HttpClient` estático reutilizable.
   - Agrega timeout de 3 segundos para evitar esperas largas.

6. Se mejoró SQLite.
   - La base de datos ahora se crea junto al archivo `database.py`, no dependiendo del directorio desde donde se ejecute el proceso.
   - Se agregaron índices para `event_logs(habit_id)` y `event_logs(completed_at)`.

7. Se agregó `Backend/requirements.txt`.
   - Incluye `fastapi` y `uvicorn[standard]`.

8. Se corrigió `Backend/.gitignore`.
   - Antes decía `__pycahe__/`.
   - Ahora dice `__pycache__/`.

9. Se actualizó `PausaVital.csproj`.
   - Copia los archivos del backend al directorio de salida/publish, excluyendo `.sqlite` y `__pycache__`.

## Nota

Este ZIP fue limpiado para no incluir carpetas generadas como `.vs`, `bin`, `obj`, `.git`, `__pycache__` ni bases SQLite locales.
