# FlexSpace

Trabajo individual. C# (.NET 8) con SQLite.

## Qué hice
Armé la solución con las capas separadas, igual que hice en la agenda:
- **Entidades**: Cliente, Puesto y Reserva, con sus enums. Son solo clases con datos, no tienen reglas adentro.
- **Datos**: crea la base de datos y las tablas la primera vez, carga datos de prueba y tiene todas las consultas.
- **Negocio**: acá puse las reglas (disponibilidad, cálculo de tarifa, bloqueo de clientes sancionados y sanción por cancelar tarde) y las excepciones.
- **Presentacion**: el menú de consola. Solo pide datos y muestra resultados.

Las 4 opciones del menú funcionan:
1. Registrar reserva: muestra el resumen del precio antes de confirmar.
2. Cancelar reserva: si falta menos de 2 horas para el inicio, suma 1 sanción al cliente.
3. Consultar reservas activas por puesto.
4. Listar clientes sancionados.

## Reglas de negocio (están en Negocio/Negocio.cs)
- No se puede reservar un puesto si ya tiene una reserva confirmada que se solape.
- Costo: horas x tarifa base, +15% si toca sábado o domingo, -10% si son 5 horas o más, -5% si es VIP.
- Si el cliente tiene sanciones pierde los descuentos y paga +20% sobre la tarifa base (mantiene el recargo de fin de semana).
- Con 3 o más sanciones lanza `ClienteSancionadoException`.

## Cómo correrlo
```
dotnet run --project Presentacion
```
Clientes de prueba: 1 Ana, 2 Beto (VIP), 3 Carla (1 sanción), 4 Diego (3 sanciones, bloqueado).
Puestos de prueba: 1 ESC-01, 2 SALA-01, 3 CAB-01.
