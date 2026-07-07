## Plan de implementación del backend

### Objetivo
Construir un backend en C# con ASP.NET Core que obtenga la tasa de cambio del Banco Central de Venezuela mediante scraping, procese el contenido, lo valide, lo cachee y lo exponga por una API.

---

## Fase 1: Preparación del proyecto
1. Crear el proyecto de API en ASP.NET Core.
2. Definir la estructura de carpetas para separar responsabilidades:
   - Servicios
   - Modelos/DTOs
   - Clientes HTTP
   - Utilidades
   - Controladores
3. Configurar dependencias básicas:
   - HttpClient
   - Parser HTML
   - Cache en memoria
   - Logging

### Entregable
Proyecto base funcional con configuración inicial lista para integrar la lógica de scraping.

---

## Fase 2: Implementar el cliente de scraping
1. Crear un servicio encargado de consultar la página del Banco Central.
2. Usar HttpClientFactory para manejar peticiones HTTP de forma limpia.
3. Configurar timeout, headers y manejo de errores.
4. Obtener el HTML de la respuesta.

### Criterio de aceptación
El sistema puede descargar la página sin fallar por problemas de red o respuesta inválida.

---

## Fase 3: Parser de HTML
1. Implementar un parser que extraiga la tasa de cambio del contenido HTML.
2. Definir una estrategia robusta para localizar el valor:
   - por selector,
   - por texto cercano,
   - o por patrón regex si aplica.
3. Convertir el valor a un tipo numérico seguro.

### Criterio de aceptación
Si la página devuelve un contenido esperado, el parser devuelve un valor válido de tasa.

---

## Fase 4: Validación y servicio de negocio
1. Crear un servicio que:
   - reciba el resultado del scraping,
   - valide que el valor sea correcto y no esté vacío,
   - maneje errores cuando el valor no se pueda interpretar.
2. Definir una respuesta estandarizada para la API.
3. Integrar caché para evitar múltiples peticiones innecesarias.

### Criterio de aceptación
Si el valor no es válido, el sistema devuelve un error controlado y no rompe la ejecución.

---

## Fase 5: Exposición por API
1. Crear un endpoint para devolver la tasa actual.
2. Definir el contrato de respuesta:
   - valor
   - fecha de actualización
   - fuente
   - estado
3. Añadir manejo de excepciones y respuestas HTTP claras.

### Endpoint sugerido
- GET /api/rates/current

### Criterio de aceptación
El cliente puede consultar la tasa mediante una llamada HTTP y recibir una respuesta bien formada.

---

## Fase 6: Actualización automática en segundo plano
1. Implementar un proceso en segundo plano con BackgroundService.
2. Programar la actualización cada cierto intervalo.
3. Actualizar la caché o el estado interno con el nuevo valor.
4. Registrar eventos y errores en logs.

### Criterio de aceptación
La tasa se actualiza automáticamente sin intervención manual.

---

## Fase 7: Pruebas y hardening
1. Añadir pruebas unitarias para:
   - parsing del HTML,
   - validación del valor,
   - comportamiento del servicio de caché.
2. Probar escenarios de error:
   - página no disponible,
   - HTML inesperado,
   - valor no numérico.
3. Añadir logs y métricas básicas.

---

## Consideraciones importantes
- El scraping puede romperse si el sitio cambia su estructura HTML.
- Es recomendable aislar la lógica de scraping en un servicio independiente.
- Si existe una API oficial del Banco Central, conviene usarla en lugar de scraping.

---

## Recomendación de arquitectura
- ASP.NET Core Web API
- HttpClientFactory
- IMemoryCache
- BackgroundService
- DTOs y servicios separados
- Logging estructurado
