﻿namespace Iris.GBA
{
    internal sealed class KeyInput(Common.System.PollInput_Delegate pollInputCallback)
    {
        internal enum Register
        {
            KEYINPUT,
            KEYCNT
        }

        private UInt16 _KEYINPUT;
        private UInt16 _KEYCNT;

        private readonly Common.System.PollInput_Delegate _pollInputCallback = pollInputCallback;

        private InterruptControl _interruptControl;

        private const int StateSaveVersion = 1;

        internal void Initialize(InterruptControl interruptControl)
        {
            _interruptControl = interruptControl;
        }

        internal void ResetState()
        {
            _KEYINPUT = 0x03ff;
            _KEYCNT = 0;
        }

        internal void LoadState(BinaryReader reader)
        {
            if (reader.ReadInt32() != StateSaveVersion)
                throw new Exception();

            _KEYINPUT = reader.ReadUInt16();
            _KEYCNT = reader.ReadUInt16();
        }

        internal void SaveState(BinaryWriter writer)
        {
            writer.Write(StateSaveVersion);

            writer.Write(_KEYINPUT);
            writer.Write(_KEYCNT);
        }

        internal UInt16 ReadRegister(Register register)
        {
            switch (register)
            {
                case Register.KEYINPUT:
                    _pollInputCallback();
                    CheckInterrupts();
                    return _KEYINPUT;

                case Register.KEYCNT:
                    return _KEYCNT;

                // should never happen
                default:
                    throw new Exception("Iris.GBA.KeyInput: Register read error");
            }
        }

        internal void WriteRegister(Register register, UInt16 value, Memory.RegisterWriteMode mode)
        {
            switch (register)
            {
                case Register.KEYCNT:
                    Memory.WriteRegisterHelper(ref _KEYCNT, value, mode);
                    CheckInterrupts();
                    break;

                // should never happen
                default:
                    throw new Exception("Iris.GBA.KeyInput: Register write error");
            }
        }

        internal void SetKeyStatus(Common.System.Key key, Common.System.KeyStatus status)
        {
            int pos;

            switch (key)
            {
                case Common.System.Key.A:
                    pos = 0;
                    break;
                case Common.System.Key.B:
                    pos = 1;
                    break;
                case Common.System.Key.Select:
                    pos = 2;
                    break;
                case Common.System.Key.Start:
                    pos = 3;
                    break;
                case Common.System.Key.Right:
                    pos = 4;
                    break;
                case Common.System.Key.Left:
                    pos = 5;
                    break;
                case Common.System.Key.Up:
                    pos = 6;
                    break;
                case Common.System.Key.Down:
                    pos = 7;
                    break;
                case Common.System.Key.R:
                    pos = 8;
                    break;
                case Common.System.Key.L:
                    pos = 9;
                    break;
                default:
                    return;
            }

            _KEYINPUT = (UInt16)((_KEYINPUT & ~(1 << pos)) | ((int)status << pos));
        }

        private void CheckInterrupts()
        {
            if ((_KEYCNT & 0x4000) == 0x4000)
            {
                if ((_KEYCNT & 0x8000) == 0)
                {
                    if ((~_KEYINPUT & _KEYCNT & 0x03ff) != 0)
                        _interruptControl.RequestInterrupt(InterruptControl.Interrupt.Key);
                }
                else
                {
                    if ((~_KEYINPUT & _KEYCNT & 0x03ff) == (_KEYCNT & 0x03ff))
                        _interruptControl.RequestInterrupt(InterruptControl.Interrupt.Key);
                }
            }
        }
    }
}
