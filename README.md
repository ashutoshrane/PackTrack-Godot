# PackTrack — Digital Packing Logs for Safer Skies

A parachute packing management app for skydiving drop zones, built with **Godot 4.6 Mono (C#)**.

## What is PackTrack?

PackTrack replaces manual whiteboard tracking at skydiving drop zones — giving parachute packers a digital log of every pack job, rig history, and billing record.

## Features

- **Packer Queue** — Live queue of rigs awaiting packing with status indicators
- **One-Tap Pack Logging** — Log a pack in seconds with automatic timestamping and billing
- **Rig Detail** — Full rig info with repack cycle tracking (FAA 180-day compliance)
- **Billing Dashboard** — Per-skydiver billing with Paid/Unpaid tracking
- **Pack History** — Filterable history of all completed packs
- **Operator Dashboard** — DZ-wide stats, revenue, and safety alerts
- **Rig Management** — Full rig inventory with warning/overdue status
- **Alert Center** — Proactive repack deadline notifications
- **Skydiver View** — Rig history and billing tab for skydivers
- **Profile** — Packer stats, certifications, and activity summary

## Tech Stack

- **Engine:** Godot 4.6.2 Mono
- **Language:** C# (.NET 8.0)
- **Architecture:** Programmatic UI (no .tscn dependencies for screens)
- **Pattern:** Autoload singletons (GameData, NavManager, ThemeManager)

## Three User Roles

1. **Packer** — Log packs, track earnings, settle billing
2. **DZ Operator** — Dashboard, rig management, alerts, billing reconciliation
3. **Skydiver** — View rig history, check tab, pay

## Getting Started

1. Install [Godot 4.6+ Mono](https://godotengine.org/download)
2. Clone this repo
3. Open `project.godot` in Godot
4. Build the C# project: `dotnet build`
5. Run the project (F5)

## Design

This app was designed as part of the **PackTrack UX Case Study** — a full end-to-end UX project covering research, personas, wireframes, hi-fi designs, and usability testing.

## Author

**Ashutosh Rane** — UX/UI Designer & Skydiver

---

*Built with Godot Engine*
