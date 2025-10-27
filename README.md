# Adventure-game

A small, modular adventure-game editor + engine. This repository contains a Blazor WebAssembly editor for authoring game packs and a lightweight runtime engine for loading and running them.

## Solution layout

- Solution file: [src/AdventureGame.sln](src/AdventureGame.sln)
- Editor (WASM) project: [src/AdventureGame.Editor](src/AdventureGame.Editor)
  - Entry / hosting: [src/AdventureGame.Editor/Program.cs](src/AdventureGame.Editor/Program.cs)
  - UI pages: [src/AdventureGame.Editor/Pages/GamesPage.razor](src/AdventureGame.Editor/Pages/GamesPage.razor), [src/AdventureGame.Editor/Pages/GameElementsPage.razor](src/AdventureGame.Editor/Pages/GameElementsPage.razor)
  - Persistence: [src/AdventureGame.Editor/Services/IndexedDbGamePackRepository.cs](src/AdventureGame.Editor/Services/IndexedDbGamePackRepository.cs)
  - Current game coordination: [src/AdventureGame.Editor/Services/CurrentGameService.cs](src/AdventureGame.Editor/Services/CurrentGameService.cs)
- Engine (models + runtime): [src/AdventureGame.Engine](src/AdventureGame.Engine)
  - Core model: [`AdventureGame.Engine.Models.GamePack`](src/AdventureGame.Engine/Models/GamePack.cs) ([file](src/AdventureGame.Engine/Models/GamePack.cs))
  - Element hierarchy: [`AdventureGame.Engine.Models.GameElement`](src/AdventureGame.Engine/Models/GameElements.cs) ([file](src/AdventureGame.Engine/Models/GameElements.cs))
  - Runtime session: [`AdventureGame.Engine.Runtime.GameSession`](src/AdventureGame.Engine/Runtime/GameSession.cs) ([file](src/AdventureGame.Engine/Runtime/GameSession.cs))
  - VFS: [`AdventureGame.Engine.Models.GamePackVfs`](src/AdventureGame.Engine/Models/GamePackVfs.cs) ([file](src/AdventureGame.Engine/Models/GamePackVfs.cs))
  - Validation: [`AdventureGame.Engine.Validation.GamePackValidator`](src/AdventureGame.Engine/Validation/GamePackValidator.cs) ([file](src/AdventureGame.Engine/Validation/GamePackValidator.cs))
  - Utilities: navigation, helpers and converters (see `Extensions`, `Helpers`, `Infrastructure` folders)

## Key concepts

- GamePack
  - Authoring-time container for metadata, grid config, elements, verbs, triggers and a virtual file system. See [`AdventureGame.Engine.Models.GamePack`](src/AdventureGame.Engine/Models/GamePack.cs).
- GameElement
  - Base polymorphic element type with concrete subtypes such as [`Scene`](src/AdventureGame.Engine/Models/Scene.cs), [`Item`](src/AdventureGame.Engine/Models/Item.cs), [`Player`](src/AdventureGame.Engine/Models/Player.cs), [`Npc`](src/AdventureGame.Engine/Models/Npc.cs), and [`Exit`](src/AdventureGame.Engine/Models/Exit.cs). See [`AdventureGame.Engine.Models.GameElement`](src/AdventureGame.Engine/Models/GameElements.cs).
- GameSession
  - A live runtime created from a GamePack that manages elements, verbs and triggers. See [`AdventureGame.Engine.Runtime.GameSession`](src/AdventureGame.Engine/Runtime/GameSession.cs).
- VFS
  - Simple embedded file store for media/assets: [`AdventureGame.Engine.Models.GamePackVfs`](src/AdventureGame.Engine/Models/GamePackVfs.cs).

## Editor features

- Create, edit, clone, export and import GamePacks using the Blazor UI ([Games page](src/AdventureGame.Editor/Pages/GamesPage.razor)).
- Stores GamePacks in browser IndexedDB via [IndexedDbGamePackRepository](src/AdventureGame.Editor/Services/IndexedDbGamePackRepository.cs).
- Side editors and dictionary editors allow editing element states, flags, properties and SVG previews (see components under [src/AdventureGame.Editor/Components](src/AdventureGame.Editor/Components)).

## Extensibility points

- Add new element types by creating a new subtype of `GameElement` and registering UI factory entries in [ElementFactory](src/AdventureGame.Editor/Services/ElementFactory.cs).
- Add new runtime effects or condition rules via the `Extensions` helpers (see `GameEffectExtensions` and `ConditionEvaluatorExtensions` in [src/AdventureGame.Engine/Extensions](src/AdventureGame.Engine/Extensions)).
- JSON (de)serialization and custom converters live in `Infrastructure` (e.g., `UlidJsonConverter`).

## Building and running

- Open the solution [src/AdventureGame.sln](src/AdventureGame.sln) in Visual Studio or use dotnet CLI.
- To run the editor (WASM):
  - From the `src/AdventureGame.Editor` folder: `dotnet run`
  - The editor registers services and initializes theme + current pack in [Program.cs](src/AdventureGame.Editor/Program.cs).

## Tests

- Tests live in [src/AdventureGame.Engine.Tests](src/AdventureGame.Engine.Tests). Global usings are in [src/AdventureGame.Engine.Tests/Usings.cs](src/AdventureGame.Engine.Tests/Usings.cs).

## Notes

- The editor persists gamepacks in browser IndexedDB; the engine is designed to be serializable via System.Text.Json with custom converters for strong types (see `ElementId`, `Location`, `Ulid` converters).
- To inspect runtime behavior, construct a `GameSession` from a `GamePack` and exercise triggers/verbs using [`AdventureGame.Engine.Runtime.GameSession`](src/AdventureGame.Engine/Runtime/GameSession.cs).

License: MIT — see LICENSE.

