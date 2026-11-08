# Demon's Souls format roundtrip

| Format | Bucket | Files |
|---|---|---|
| BND3 (anibnd.dcx) | has files | 101 |
| BND3 (anibnd.dcx) | reads as empty | 4 |
| BND3 (anibnd) | bytes differ, content equal | 4 |
| BND3 (anibnd) | exact | 108 |
| BND3 (chrbnd) | exact | 110 |
| BND3 (parambnd) | exact | 12 |
| DCX (chrbnd.dcx) | bytes differ, content equal | 40 |
| FMG | bytes differ, content equal | 29 |
| FMG | exact | 37 |
| MSBD | bytes differ, content equal | 2 |
| MSBD | exact | 40 |
| MTD | bytes differ, content equal | 266 |
| MTD | exact | 40 |
| PARAM | bytes differ, content equal | 185 |
| TAE | bytes differ, content equal | 26 |
| TAE | exact | 102 |
| TPF | bytes differ, content equal | 122 |
| TPF | exact | 9 |

## BND3 (anibnd.dcx): has files
- c0000.anibnd.dcx: 35 files, 378256 bytes
- c0200.anibnd.dcx: 40 files, 310704 bytes
- c1000.anibnd.dcx: 28 files, 437120 bytes
- c1001.anibnd.dcx: 38 files, 519392 bytes
- c1020.anibnd.dcx: 24 files, 2439072 bytes
- c1030.anibnd.dcx: 26 files, 892768 bytes
- c1090.anibnd.dcx: 2 files, 7600 bytes
- c1200.anibnd.dcx: 29 files, 178816 bytes

## BND3 (anibnd.dcx): reads as empty
- c0300.anibnd.dcx: 0 files, 43296 bytes
- c1040.anibnd.dcx: 0 files, 533760 bytes
- c9950.anibnd.dcx: 0 files, 263120 bytes
- c9951.anibnd.dcx: 0 files, 263120 bytes

## BND3 (anibnd): bytes differ, content equal
- c0300.anibnd (orig 180592, new 32, first diff 0x17)
- c1040.anibnd (orig 855472, new 32, first diff 0x17)
- c9950.anibnd (orig 539840, new 32, first diff 0x17)
- c9951.anibnd (orig 539840, new 32, first diff 0x17)

## DCX (chrbnd.dcx): bytes differ, content equal
- c0000.chrbnd.dcx (orig 31584, new 32528, first diff 0x22)
- c0200.chrbnd.dcx (orig 1884160, new 1902720, first diff 0x21)
- c0300.chrbnd.dcx (orig 1371568, new 1386864, first diff 0x21)
- c1000.chrbnd.dcx (orig 2394576, new 2423696, first diff 0x22)
- c1001.chrbnd.dcx (orig 2175040, new 2203296, first diff 0x22)
- c1020.chrbnd.dcx (orig 7918400, new 8006416, first diff 0x21)
- c1030.chrbnd.dcx (orig 1998240, new 2015312, first diff 0x22)
- c1040.chrbnd.dcx (orig 1280912, new 1295200, first diff 0x22)

## FMG: bytes differ, content equal
- msg\item.msgbnd/アイテム名.fmg (orig 3240, new 3238, first diff 0x7)
- msg\item.msgbnd/特徴説明.fmg (orig 1272, new 1270, first diff 0x7)
- msg\item.msgbnd/特徴うんちく.fmg (orig 1272, new 1270, first diff 0x7)
- msg\item.msgbnd/地名.fmg (orig 1740, new 1738, first diff 0x7)
- msg\item.msgbnd/武器説明.fmg (orig 41244, new 41242, first diff 0x7)
- msg\item.msgbnd/防具説明.fmg (orig 2344, new 2342, first diff 0x7)
- msg\item.msgbnd/アクセサリ説明.fmg (orig 1056, new 1054, first diff 0x7)
- msg\item.msgbnd/アイテムうんちく.fmg (orig 19368, new 19366, first diff 0x7)

## MSBD: bytes differ, content equal
- m02_03_00_00.msb (orig 150848, new 150848, first diff 0x543B)
- m03_01_00_00.msb (orig 219216, new 219216, first diff 0x996B)

## MTD: bytes differ, content equal
- C[DB].mtd (orig 1852, new 1852, first diff 0x4D)
- C[DB]_Edge_Skin.mtd (orig 1872, new 1872, first diff 0x4D)
- C[DB]_Skin.mtd (orig 1864, new 1864, first diff 0x4D)
- C[Dn]_Skin.mtd (orig 1820, new 1820, first diff 0x111)
- C[D].mtd (orig 1796, new 1796, first diff 0x4D)
- C[D]_Alp.mtd (orig 1804, new 1804, first diff 0x4D)
- C[D]_Alp_Skin.mtd (orig 1808, new 1808, first diff 0x4D)
- C[D]_Edge.mtd (orig 1804, new 1804, first diff 0x4D)

## PARAM: bytes differ, content equal
- gameparam.parambnd/EquipParamWeapon.param (orig 383318, new 382626, first diff 0x3)
- gameparam.parambnd/EquipParamProtector.param (orig 20885, new 20858, first diff 0x3)
- gameparam.parambnd/EquipParamAccessory.param (orig 2583, new 2587, first diff 0x3B)
- gameparam.parambnd/EquipParamGoods.param (orig 12278, new 12282, first diff 0x3B)
- gameparam.parambnd/EquipMtrlSetParam.param (orig 42184, new 42176, first diff 0x3)
- gameparam.parambnd/default_EnemyBehaviorBank.param (orig 3518, new 3514, first diff 0x3)
- gameparam.parambnd/default_AIStandardInfoBank.param (orig 6502, new 6506, first diff 0x3B)
- gameparam.parambnd/ThrowParam.param (orig 4066, new 4041, first diff 0x3)

## TAE: bytes differ, content equal
- c0100/c0100.tae (orig 21056, new 19024, first diff 0xE)
- c0200/ｃ0200.tae (orig 5920, new 5760, first diff 0xE)
- c1000/c1000.tae (orig 5456, new 4848, first diff 0xE)
- c1001/c1001.tae (orig 19440, new 18112, first diff 0xE)
- c1200/ｃ1200.tae (orig 4928, new 4864, first diff 0xF)
- c2040/c2040.tae (orig 20752, new 20752, first diff 0x48)
- c2050/2050.tae (orig 9968, new 9968, first diff 0x48)
- c2060/c2060.tae (orig 8144, new 8144, first diff 0x48)

## TPF: bytes differ, content equal
- c0000.tpf (orig 384, new 512, first diff 0x6)
- c0100.tpf (orig 1803136, new 1803264, first diff 0x6)
- c0200.tpf (orig 2655360, new 2655360, first diff 0x6)
- c0300.tpf (orig 2622336, new 2622464, first diff 0x6)
- c1000.tpf (orig 3147008, new 3147008, first diff 0x6)
- c1001.tpf (orig 2797184, new 2797312, first diff 0x6)
- c1020.tpf (orig 9263616, new 9263616, first diff 0x6)
- c1030.tpf (orig 2622336, new 2622464, first diff 0x6)

