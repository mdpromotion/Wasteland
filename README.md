# Wasteland ![Unity Tests](https://github.com/mdpromotion/Wasteland/actions/workflows/tests.yml/badge.svg)

[English Version](#-english-version) | [Русская версия](#-русская-версия)

---

# 🇬🇧 English Version

**Wasteland** is a post-apocalyptic survival RPG built in Unity, set in an infinite, procedurally generated world. The project is in active development, with core world-generation and exploration technology already implemented and gameplay systems being built on top of it as the game moves toward release.

## Concept

The player wakes up in a ruined world and has to survive against everything it throws at them — hostile creatures ranging from desperate survivors to mutated horrors, each with distinct combat behavior. There is no fixed map: the world is endless and generated on the fly, dotted with structures worth raiding for loot and **Knowledge** — a core progression mechanic that permanently improves the character (strength, crafting speed, and more). Combat is built around a risk/noise trade-off: quiet, low-durability melee weapons like bats and crowbars versus loud, ammo-hungry firearms that can pull every nearby threat straight to you. The game leans heavily into RPG systems and survival tension.

## 🎥 Gameplay Demos

A collection of technical and gameplay demos showcasing the current state of **Wasteland** — procedural world generation, exploration, water, vegetation, lighting, and other systems.

**[▶ Watch the Wasteland Demo Playlist](https://youtube.com/playlist?list=PLQoi69tn3_QY&si=0sbZkD2x2PrQOACd)**

## 🚀 Core Technologies & Approach

- **Clean Architecture + MVP** — modular separation between gameplay logic, infrastructure, and presentation.
- **Feature-first structure** — every system is isolated and extendable without touching unrelated code.
- **Data-Oriented Design** — performance-critical systems built on Unity Jobs, Burst Compiler, and native memory structures.
- **Dependency Injection (VContainer)** — lightweight composition of gameplay features.

## 🌎 World Generation

- **Infinite Procedural World** — seamless, chunk-streamed landscape generated around the player in real time.
- **Procedural Hydrology** — terrain-aware rivers with natural flow paths, carving, and smooth shoreline transitions.
- **Chunk-Based Water Rendering** — waves, depth-aware color, shoreline fading, reflections, and fog integration.
- **Procedural Vegetation** — automatic tree placement with variants, LODs, and collision across generated terrain.
- **Dynamic Sky & Day/Night Cycle** — sun/moon visuals, procedural stars, and smoothly transitioning lighting and fog.

## ⚙️ Performance

- **Chunk Streaming & Scheduling** — background generation pipeline with prioritization and cancellation of unneeded work.
- **Burst-Accelerated Generation** — multithreaded terrain and world data generation via Unity Jobs + Burst.
- **Low-GC Runtime** — controlled native memory lifetime, minimized managed allocations.
- **Graphics Settings** — an in-game options menu letting players tune visual quality for the performance they want on their hardware.

## 🛠 On the Roadmap Toward Release

- **Combat System** — unique behaviors per enemy type.
- **Enemies & Threats** — a growing bestiary populating the wasteland.
- **Lootable Structures** — hand-placed and procedural points of interest to explore and raid.
- **Knowledge System** — a core progression mechanic that upgrades character stats and abilities through discovery.
- **Weapon Systems** — quiet, low-durability melee weapons vs. loud, ammo-dependent firearms with noise-based enemy attraction.
- **Survival & RPG Layer** — deeper character progression and survival mechanics.

## 🎯 Project Goals

- Build a fully realized post-apocalyptic survival RPG on top of a scalable procedural world.
- Deliver combat, loot, and progression systems that make exploration genuinely rewarding.
- Keep the world infinite, performant, and stable as systems layer on top of it.
- Continue evolving the architecture and technology all the way through to release.

---

# 🇷🇺 Русская версия

**Wasteland** — постапокалиптическая survival-RPG на Unity с бесконечным процедурно-генерируемым миром. Проект находится в активной разработке: технология генерации и исследования мира уже реализована, а поверх неё выстраиваются игровые системы по мере движения к релизу.

## Концепт

Игрок оказывается в разрушенном мире и вынужден выживать в ужасающих условиях. Повсюду его поджидают враждебные существа — от отчаявшихся выживших до мутантов с уникальным боевым поведением. Фиксированной карты нет: мир бесконечен и генерируется на лету, а по пути встречаются структуры, которые стоит обыскать ради лута и **Знаний** — ключевой механики прогрессии, которая постоянно улучшает персонажа (сила, скорость крафта и многое другое). Бой строится на балансе тишины и шума: тихое, но хрупкое ближнее оружие вроде бит и ломов против громкого огнестрела, требующего патроны и способного привлечь всех тварей поблизости. Игра имеет сильный уклон в RPG и выживание.

## 🎥 Демо игрового процесса

Плейлист с техническими и игровыми демо, демонстрирующими текущее состояние **Wasteland** — процедурную генерацию мира, исследование, воду, растительность, освещение и другие системы.

**[▶ Смотреть демо Wasteland](https://youtube.com/playlist?list=PLQoi69tn3_QY&si=0sbZkD2x2PrQOACd)**

## 🚀 Основные технологии и подход

- **Clean Architecture + MVP** — разделение логики, инфраструктуры и представления.
- **Feature-first структура** — каждая система изолирована и развивается независимо.
- **Data-Oriented подход** — производительные системы на Unity Jobs, Burst Compiler и Native-структурах памяти.
- **Dependency Injection (VContainer)** — лёгкая сборка игровых систем.

## 🌎 Генерация мира

- **Бесконечный процедурный мир** — бесшовный, стримящийся по чанкам ландшафт, генерируемый вокруг игрока в реальном времени.
- **Процедурная гидрология** — реки с учётом рельефа, естественными маршрутами течения и плавными переходами берега.
- **Система воды по чанкам** — волны, зависимая от глубины окраска, затухание у берега, отражения и интеграция с туманом.
- **Процедурная растительность** — автоматическое размещение деревьев с вариациями, LOD и коллизиями.
- **Динамическое небо и цикл дня/ночи** — солнце, луна, звёзды и плавные переходы освещения и тумана.

## ⚙️ Производительность

- **Стриминг и планирование чанков** — фоновая генерация с приоритизацией и отменой ненужных задач.
- **Генерация на Burst** — многопоточная генерация мира через Unity Jobs + Burst.
- **Минимизация GC** — контроль времени жизни Native-ресурсов, минимум managed-аллокаций.
- **Настройки графики** — игровое меню опций, позволяющее игрокам подстроить качество картинки под нужную производительность на своём железе.

## 🛠 В планах к релизу

- **Боевая система** — уникальное поведение для разных типов врагов.
- **Враги и угрозы** — растущий бестиарий, населяющий пустошь.
- **Лутаемые структуры** — точки интереса, ручные и процедурные, для исследования и разграбления.
- **Система Знаний** — ключевая механика прогрессии, улучшающая характеристики и способности персонажа через находки.
- **Оружейные системы** — тихое, но хрупкое ближнее оружие против громкого, зависимого от патронов огнестрела, привлекающего врагов.
- **Survival/RPG-слой** — более глубокая прогрессия персонажа и механики выживания.

## 🎯 Цели проекта

- Создать полноценную постапокалиптическую survival-RPG поверх масштабируемого процедурного мира.
- Реализовать боёвку, лут и прогрессию так, чтобы исследование мира было по-настоящему ценным.
- Сохранять бесконечность мира, производительность и стабильность по мере наслоения новых систем.
- Продолжать развивать архитектуру и технологии вплоть до релиза.
