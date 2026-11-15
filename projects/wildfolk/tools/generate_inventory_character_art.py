"""Original front-facing inventory portrait art for Lowfire; uses the shared Pillow primitives."""
from pathlib import Path
import argparse
import generate_visual_assets as art


def build(output: Path):
    art.OUT = output
    output.mkdir(parents=True, exist_ok=True)
    poly, ellipse, line, save = art.poly, art.ellipse, art.line, art.save
    ink = art.INK

    im = art.canvas(180, 300)

    # A quiet ground shadow keeps the standing figure anchored on the inventory panel.
    ellipse(im, (35, 277, 145, 294), '#262f28')

    # Boots and trousers are separate silhouettes so the legs read clearly at phone scale.
    poly(im, [(54, 251), (83, 250), (87, 275), (81, 285), (44, 285), (42, 276)], '#735739', ink, 4)
    poly(im, [(94, 251), (123, 249), (138, 276), (134, 285), (96, 285), (90, 277)], '#735739', ink, 4)
    poly(im, [(49, 277), (82, 277), (81, 285), (44, 285), (42, 280)], '#4a3c2e')
    poly(im, [(96, 277), (136, 277), (134, 285), (96, 285), (91, 280)], '#4a3c2e')
    line(im, [(52, 271), (74, 270)], '#ad8a58', 3)
    line(im, [(105, 270), (126, 271)], '#ad8a58', 3)

    poly(im, [(60, 177), (86, 181), (86, 255), (78, 266), (52, 262), (49, 232)], '#68735b', ink, 4)
    poly(im, [(86, 181), (116, 177), (129, 231), (119, 263), (91, 266), (85, 250)], '#58664f', ink, 4)
    poly(im, [(60, 188), (78, 190), (72, 251), (57, 251)], '#849070')
    poly(im, [(97, 190), (112, 188), (121, 232), (111, 250), (101, 248)], '#748167')
    line(im, [(86, 194), (84, 244)], '#465744', 3)

    # Arms sit behind the tunic, with broad matte highlights and dark Lowfire outlines.
    poly(im, [(49, 99), (64, 103), (60, 159), (57, 190), (48, 201), (34, 195), (36, 150)], '#bf906d', ink, 4)
    poly(im, [(114, 103), (131, 98), (145, 151), (144, 191), (131, 201), (120, 190), (118, 158)], '#bf906d', ink, 4)
    poly(im, [(39, 149), (59, 153), (57, 174), (39, 170)], '#78876b', ink, 3)
    poly(im, [(119, 153), (140, 148), (143, 170), (122, 176)], '#687c64', ink, 3)
    poly(im, [(37, 177), (55, 180), (53, 196), (45, 201), (34, 196)], '#d4a681', ink, 3)
    poly(im, [(125, 180), (143, 175), (145, 194), (135, 201), (127, 197)], '#d4a681', ink, 3)

    # A practical moss-green tunic matches the game's forest palette without imitating another game.
    poly(im, [(57, 97), (72, 86), (106, 86), (123, 99), (132, 139), (122, 194), (59, 194), (48, 139)], '#5c735f', ink, 4)
    poly(im, [(65, 101), (77, 94), (83, 179), (63, 183), (55, 141)], '#87927a')
    poly(im, [(103, 96), (117, 103), (125, 141), (116, 183), (99, 180)], '#4a6251')
    poly(im, [(73, 92), (88, 103), (104, 92), (99, 119), (88, 125), (77, 118)], '#cba77b', ink, 3)
    line(im, [(87, 122), (87, 170)], '#394e42', 3)
    poly(im, [(73, 143), (84, 141), (84, 156), (73, 157)], '#a48a5b', ink, 2)
    poly(im, [(93, 141), (104, 143), (104, 157), (93, 156)], '#a48a5b', ink, 2)

    # Belt, buckle, and neck establish a readable break between the tunic and legs.
    poly(im, [(58, 177), (122, 177), (120, 191), (60, 191)], '#58432f', ink, 3)
    poly(im, [(81, 176), (97, 176), (98, 191), (80, 191)], '#c2a05d', ink, 2)
    poly(im, [(85, 180), (94, 180), (94, 187), (85, 187)], '#59634e')
    poly(im, [(76, 77), (102, 77), (106, 103), (72, 103)], '#c79470', ink, 3)

    # The face is authored front-on rather than enlarging the circular top-down player sprite.
    ellipse(im, (48, 22, 66, 50), '#c79470', ink, 3)
    ellipse(im, (114, 22, 132, 50), '#c79470', ink, 3)
    poly(im, [(62, 17), (73, 9), (107, 9), (120, 19), (124, 57), (116, 75), (103, 86), (76, 85), (63, 72), (55, 51)], '#d3a27e', ink, 4)
    poly(im, [(64, 20), (73, 10), (106, 10), (120, 21), (116, 34), (102, 31), (91, 36), (76, 32), (63, 40)], '#493a2f', ink, 4)
    poly(im, [(64, 26), (71, 30), (71, 64), (65, 58)], '#72563e')
    poly(im, [(108, 27), (119, 31), (117, 58), (110, 65)], '#604936')
    poly(im, [(74, 43), (81, 42), (81, 49), (74, 49)], '#293b32')
    poly(im, [(98, 43), (105, 42), (105, 49), (98, 49)], '#293b32')
    poly(im, [(84, 55), (91, 54), (89, 61), (84, 61)], '#b27f60')
    line(im, [(81, 68), (89, 70), (97, 68)], '#805b47', 2)
    poly(im, [(70, 29), (78, 34), (73, 38)], '#786044')
    poly(im, [(101, 13), (112, 17), (108, 21), (96, 19)], '#826648')

    bounds = im.getchannel('A').getbbox()
    if bounds:
        im = im.crop(bounds)
    save(im, 'player_inventory_fullbody.png')


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--output', type=Path, default=Path(__file__).parents[1] / 'client' / 'Assets' / 'Wildfolk' / 'Art')
    build(parser.parse_args().output)
