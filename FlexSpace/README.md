# FlexSpace - Entrega parcial

Trabajo individual. C# (.NET 8) con SQLite.

## Qué hice
Armé la solución con las capas separadas (Presentacion, Negocio, Datos y Entidades):
- **Entidades**: Cliente, Puesto y Reserva, con sus enums (TipoCliente, TipoPuesto, EstadoReserva).
- **Datos**: crea la base de datos y las tablas la primera vez, carga datos de prueba y tiene las consultas (agregar y cancelar reserva, listar reservas por puesto, listar clientes sancionados).
- **Negocio**: conecta la presentación con los datos.
- **Presentacion**: el menú de consola con las 4 opciones pedidas.

