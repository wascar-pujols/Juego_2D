# Kage no Michi — El Camino de la Sombra

Juego de plataformas 2D estilo Mario hecho en Unity, para la materia Programación de Videojuegos.
Autor: Wascar Pujols (1-22-0075)

## Cómo abrirlo
1. Abre **Unity Hub**, ve a **Add → Add project from disk** y selecciona esta carpeta (`KageNoMichi`).
2. Ábrelo con Unity 6 (también funciona con 2022.3). La primera vez tarda unos minutos importando.
3. Al terminar, el proyecto crea solo las escenas `Menu`, `Nivel1`, `Nivel2` y `Nivel3` en `Assets/Scenes`,
   las agrega a *Build Settings* y abre `Menu`. Pulsa **Play**.
   - Si no se crearon, usa el menú **Kage no Michi → Regenerar escenas**.

## Controles
| Tecla | Acción |
|---|---|
| A / D o ← → | Moverse |
| Shift | Correr |
| Espacio / W / ↑ / Z | Saltar (mantén para saltar más alto) |
| J / X / K | Golpear (rompe cajas y desvía shuriken) |
| Esc / P | Pausa (M en pausa = volver al menú) |

## Reglas
- 3 niveles: **Aldea del Bambú → Paso de la Montaña Kaze → Castillo del Shogun (jefe)**.
- 3 vidas (corazones). Cada golpe quita una; caer al vacío quita una y te devuelve al último checkpoint.
- Si te quedas sin vidas o se acaba el tiempo aparece **"¡KENJI HA MUERTO!"** y vuelves al **Nivel 1**.
- 100 monedas = 1 vida. Los bloques **?** dan monedas o corazones; las cajas se rompen.

## Enemigos y obstáculos
1. **Ronin** (samurái): patrulla y ataca con la katana.
2. **Shinobi**: persigue, salta obstáculos, aguanta 2 golpes.
3. **Yokai farol**: espíritu volador que flota en ondas.
4. **Torreta de piedra**: dispara shuriken.
5. **Barra de fuego onibi**: llamas que giran.
6. **Pinchos de bambú**.
7. **Puente frágil**: se cae al pisarlo.
8. **Precipicios** y plataformas móviles.
- **Jefe: Shogun Oscuro**: embestida, salto con ondas de fuego y lluvia de shuriken. Tiene 2 fases y 12 golpes de vida.

## Sonido
Toda la música (menú, 3 niveles, jefe, victoria) y los efectos se generan por código (`Scripts/Core/Synth.cs`).
Al entrar a la arena del jefe cambia la música.

## Estructura
- `Assets/Scripts/Core`: GameManager (flujo, vidas, tiempo), audio, input, arte.
- `Assets/Scripts/Level`: LevelData (mapas de los niveles en texto), LevelBuilder, cámara, parallax.
- `Assets/Scripts/Player`, `Enemies`, `Hazards`, `Items`, `UI`.
- `Assets/Resources/Sprites`: personajes (pack) + tiles, objetos y fondos originales.
- `Herramientas/`: scripts de Python que generaron el arte original y los mapas.

## Créditos
- Personajes: *Free Shinobi Sprites Pixel Art* de CraftPix.net (licencia: https://craftpix.net/file-licenses/).
- Tiles, objetos, fondos, música y efectos: creación original para este proyecto.

## Publicar en Itch.io (WebGL)
File → Build Profiles (o Build Settings) → **Web/WebGL** → Build. Sube el contenido de la carpeta como .zip
a Itch.io marcando "This file will be played in the browser". (La compresión ya está desactivada para Itch.io.)
