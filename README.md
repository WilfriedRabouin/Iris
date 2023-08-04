# Iris

Iris is a WIP GameBoy Advance emulator (and maybe DS in the future). I wanted to emulate the GameBoy Advance and needed a project as a playground to learn the C# language so I started this.

## TODOLIST

### GBA

- Reorganize architecture
- Waitstates
- BIOS function timings
- Sprites
- Add Settings dialog
- Allow to specify and load a BIOS (settings)
- Pokemon Mystery Dungeon in playable state
- Load/save states
- Use OpenGL/Vulkan to make rendering faster

### NDS

- Missing ARMv5TE instructions
- Pass ARMWrestler test ROM
- ARM946E-S instruction timings
- Pokemon Mystery Dungeon in playable state
- Rudimentary audio

## Compatible games

### GBA

None atm

### NDS

None atm

## Screenshots

<p align="center">
  <img src="Screenshots/Capture.PNG"/>
  <img src="Screenshots/Capture-2.PNG"/>
</p>

## Resources

- The Official Gameboy Advance Programming Manual
- ARM Architecture Reference Manual
- ARM7TDMI Technical Reference Manual
- [GBATEK](https://problemkaputt.de/gbatek.htm)
- [ARMWrestler test ROM](https://github.com/destoer/armwrestler-gba-fixed)
- [gba-tests test ROMs](https://github.com/jsmolka/gba-tests) (arm.gba and thumb.gba)
- [FuzzARM test ROMs](https://github.com/DenSinH/FuzzARM)
- [TONC demos](https://www.coranac.com/tonc/text/toc.htm) (key_demo.gba)
