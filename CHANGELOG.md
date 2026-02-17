# Changelog

## [1.0.0] - 2026-02-16

### Added
- Initial package extraction from game-specific UI system
- `ScreenId` and `PopupId` readonly structs with string-backed keys
- `UIManager` singleton facade with config-driven initialization
- `ScreenHandler` with config-driven HUD visibility, history, and transitions
- `PopupHandler` with priority queue and blocker panel
- `UIScreen` and `UIPopup` abstract base classes
- `UIEvents` static event system
- `IUIInputHandler` interface for input abstraction
- `UISystemConfig` ScriptableObject for per-screen behavior configuration
- `ScreenConfig`, `TransitionOverride`, `ScreenTransition` configuration types
- Editor tool: `Tools > UI System > UI Creator` for scaffolding screens/popups
- Code generators for screen classes, popup classes, and enum files
- Custom inspector for `UISystemConfig`
