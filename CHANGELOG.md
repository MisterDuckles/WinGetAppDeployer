# Changelog

All notable changes to WingetAppDeployer will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Planned
- Dark mode theme
- Check if apps are already installed (status indicators)
- Parallel app installation (install multiple apps at once)
- App icons/logos
- Installation profiles (Gaming, Developer, Office presets)
- Export/Import selected apps
- Multi-language support (Dutch/English)
- Fuzzy search improvements

## [1.0.0] - 2026-03-18

### Added
- Initial release 🎉
- Modern WPF UI with Material Design
- 200+ apps across 8 categories
- Subcategories for better organization
- 3 theme support (Minimal, Fluent, Material)
- Search functionality
- Bootstrap launcher (~5KB)
- Auto-update from GitHub releases
- Scheduled task integration for automatic app updates
- Settings window with theme selection
- Welcome banner on first run
- Installation progress window with live logging
- JSON-based app database (easily extensible)
- Popular apps marking
- Desktop shortcut creation
- Integration with Windows unattended installations

### Categories Included
- 🌐 Browsers (9 apps)
- 💼 Development (6 subcategories, 40+ apps)
  - IDE & Editors
  - Version Control
  - Runtimes & Package Managers
  - Containers & Virtualization
  - Database Tools
  - API & Testing Tools
- 🔐 Security & Privacy (6 apps)
- 📝 Productivity (3 subcategories, 15+ apps)
  - Office Suites
  - Note-Taking & Knowledge
  - PDF Tools
- 💬 Communication (7 apps)
- 🎵 Media & Entertainment (3 subcategories, 15+ apps)
  - Music & Video Players
  - Gaming
  - Streaming & Recording
- 🛠️ Utilities (4 subcategories, 20+ apps)
  - System Tools
  - File Transfer & Sync
  - Remote Access
  - Screenshots & Screen Recording
- 🎨 Creative & Design (3 subcategories, 10+ apps)
  - Graphics & Photo Editing
  - Video Editing
  - 3D & Modeling

### Technical
- Built with .NET 8 and WPF
- Material Design themes via MaterialDesignInXaml
- Winget integration for app installation
- GitHub API integration for updates
- Task Scheduler API for auto-updates
- MVVM architecture
- Settings persistence in AppData

## [0.0.0] - 2026-03-18

### Planning Phase
- Concept design
- Feature planning
- Architecture decisions
- UI mockups created

---

[Unreleased]: https://github.com/YOUR_USERNAME/WingetAppDeployer/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/YOUR_USERNAME/WingetAppDeployer/releases/tag/v1.0.0
