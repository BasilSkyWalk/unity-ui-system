# UI System Core

A reusable, config-driven UI management system for Unity. Provides screen lifecycle management, popup handling with priority queues, transition animations, and an editor tool for scaffolding new screens/popups.

## Features

- **Screen Management** — Show/hide screens with transition animations, history stack, and back navigation
- **Popup System** — Priority-based popup queue with blocker panel
- **Config-Driven** — Per-screen behavior (HUD visibility, input mode, history exclusion) configured via `UISystemConfig` ScriptableObject
- **Transition Overrides** — Define custom transitions between specific screen pairs
- **String-Backed IDs** — `ScreenId`/`PopupId` structs wrap strings; game project generates enums via editor tool
- **Input Abstraction** — `IUIInputHandler` interface decouples from InputSystem
- **Editor Tool** — `Tools > UI System > UI Creator` scaffolds screen/popup classes and auto-updates enum files

## Setup

1. Create a `UISystemConfig` asset: `Create > UI > UI System Config`
2. Create transition assets: `Create > UI > Screen Transition`
3. Add `UIManager` MonoBehaviour to a scene object and assign the config
4. Implement `IUIInputHandler` in your game project
5. Call `UIManager.Instance.Initialize(yourInputHandler)` at startup

## Game Project Integration

Implement `IUIInputHandler` for your input system and create a setup script that:
- Initializes the input handler
- Calls `UIManager.Instance.Initialize(inputHandler)`
- Sets `UIManager.Instance.OnCancelPressed` for game-specific cancel routing