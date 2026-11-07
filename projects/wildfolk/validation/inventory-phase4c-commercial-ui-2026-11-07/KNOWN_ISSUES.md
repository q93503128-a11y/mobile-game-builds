# Remaining Issues and Limits

## P2 — 844×390 detail-copy size

The longest English title and description fit, but some selected-item detail text remains small at 844×390. The automated height/containment checks pass and there is no observed clipping. This needs user review and a physical Android test before deciding whether detail copy should be shortened or re-presented. Do not shrink the type to hide the issue.

## UNVERIFIED — physical touch/accessibility

Unity Game View sizes and RectTransform units are not Android dp or Apple pt. The scene test checks the four equipment cell reference hit areas at 250×180 units and confirms that the four hit targets do not overlap; it does not establish their physical size or one-handed thumb reach on a device. Android 48dp, Apple 44pt and MobileHCI target research are references, not a passed device result.

## UNVERIFIED — safe areas and runtime hardware

No Android or iOS device was connected. Notch/cutout and gesture-system regions, actual display density, landscape adaptation, accessibility scaling and real touch precision remain unverified. Physical device tests must follow after design approval.

## Diagnostic — intermediate scene-transition NullReference entries

One early visual-QA fixture load produced several transient NullReference exceptions in gameplay components while the test directly switched into the gameplay scene, plus two `OnDestroy` exceptions when that test session ended. They did not reproduce in the final refreshed session’s console read after focused tests/capture, but their cause was not established here. They were outside this inventory UI change and were not hidden by modifying unrelated runtime code. If they recur during normal new-game/Continue play, investigate initialization ordering separately.

## Approval and scope

This is an implemented Unity review candidate, not a final art approval. No HUD, Main Menu, or other UI screen was changed. Wait for the user’s review of the actual final PNGs before expanding the visual redesign.
