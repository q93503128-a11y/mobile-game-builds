# Inventory Character & Equipment Visual Revision

This is an unapproved, static mobile UI study. It preserves the Phase 3D brown inventory composition and replaces only its character/equipment treatment.

## Rebuild

From the repository root:

```powershell
./projects/wildfolk/art-source/inventory-character-equipment-2026-11-07/render_previews.ps1
```

The generator imports the Phase 3D source resolver for existing Lowfire item icons, localization, recipes, background, and the unmodified 9 Hotbar / 27 Storage / 4 Equipment fixture. A shows the real gameplay capture in a downsampled world-context patch, without the previous circular face crop or an invented body. B removes the character and uses a connected equipment rail with four labelled slots and current item status. The output comparison includes the earlier Phase 3D equipped-item screen.

The current player has no conventional full-body sprite: it is a top-down rig composed of a circular skin sprite (160×160), separate eyes (32×32) and hands (64×64), and wearable overlays (150×150). A therefore shows the rendered in-game top-down actor in its natural world context, not a newly invented front-facing or anatomical body. This limitation is intentional and is reported in the validation review.

All source PNGs, Unity files, player data, saves, and network code are read-only inputs. Generated SVGs and PNGs live only under this new art-source and validation folder.
