# Dechange

An interactive 3D solar system explorer built in Unity 6.

Dechange lets you fly through a roughly-to-scale solar system, zoom into any planetary body, and read real data about it: atmosphere composition, surface temperature, and weather.

**Earth is a special focus.** You can see the historical record of CO₂, CH₄, and N₂O concentrations, watch how they track with simulation time, and see a science-based projection of future temperature change extrapolated from the trend of those gases.

## to be added:

- Seamless zoom from the full solar system down to any planet or moon
- Real Keplerian orbits: positions are accurate functions of time
- Rewind, fast-forward, or stop simulation time at any rate
- Toggle between a readable visual scale and true astronomical scale
- Per-body info panel: temperature, atmospheric composition, surface pressure, physical stats
- Earth climate module: CO₂ / CH₄ / N₂O timeline + projected temperature delta
- Data-driven: adding a new stellar system is a drop-in JSON file

## Built with

- Unity 6 (6000.5.0f1) + Universal Render Pipeline
- C# — Keplerian orbit solver, floating-origin rendering, hybrid data service
- Bundled planetary data + optional live refresh from NASA/NOAA

## Status

Early development
