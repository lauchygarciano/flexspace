# FlexSpace - Entrega parcial

## Qué hice
Armé la solución con las 3 capas que pedía la consigna:
- **FlexSpace.UI**: el menú de consola. Solo pide datos y muestra resultados.
- **FlexSpace.BLL**: la lógica (validaciones, disponibilidad y cálculo del precio).
- **FlexSpace.DAL**: el acceso a la base de datos con ADO.NET.

También creé la base de datos con las tablas Cliente, Puesto y Reserva, con algunos datos de prueba.

De las opciones del menú hice:
- **1. Registrar nueva reserva**: pide los datos, verifica que el puesto no esté ocupado en ese horario, calcula el precio con las reglas (fin de semana, descuento por 5 horas o más, VIP y sanciones), muestra el resumen y después de confirmar la guarda. Si el cliente tiene 3 o más sanciones tira `ClienteSancionadoException`.
- **4. Listar clientes sancionados**.

Al abrirlo por primera vez se crea solo el archivo de la base de datos.
Clientes de prueba: 1 Ana (normal), 2 Beto (VIP), 3 Carla (1 sanción), 4 Diego (3 sanciones, bloqueado).
Puestos de prueba: 1 ESC-01, 2 SALA-01, 3 CAB-01.


