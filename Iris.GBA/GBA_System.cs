﻿using System.IO.Compression;
using System.Security.Cryptography;

namespace Iris.GBA
{
    public sealed class GBA_System : Common.System
    {
        internal enum TaskId
        {
            // Video
            StartHBlank,
            StartScanline,

            // Timer
            StartCountingChannel0,
            StartCountingChannel1,
            StartCountingChannel2,
            StartCountingChannel3
        }

        private static readonly int s_taskIdCount = Enum.GetNames(typeof(TaskId)).Length;

        private readonly Common.Scheduler _scheduler = new(s_taskIdCount, s_taskIdCount + 1);

        private readonly CPU.CPU_Core _cpu;
        private readonly Communication _communication = new();
        private readonly Timer _timer;
        private readonly Sound _sound = new();
        private readonly DMA _dma = new();
        private readonly KeyInput _keyInput;
        private readonly SystemControl _systemControl = new();
        private readonly InterruptControl _interruptControl = new();
        private readonly Memory _memory = new();
        private readonly Video _video;

        private readonly BIOS _bios = new("D:\\dev\\Iris\\ROMs\\GBA\\gba_bios.bin");

        private string _romHash;
        private bool _running;
        private bool _disposed;

        private const string StateSaveMagic = "IRIS";
        private const int StateSaveVersion = 1;

        public GBA_System(PollInput_Delegate pollInputCallback, PresentFrame_Delegate presentFrameCallback)
        {
            CPU.CPU_Core.CallbackInterface cpuCallbackInterface = new(_memory.Read8, _memory.Read16, _memory.Read32, _memory.Write8, _memory.Write16, _memory.Write32, _bios.HandleSWI, _bios.HandleIRQ);

            _cpu = new(CPU.CPU_Core.Model.ARM7TDMI, cpuCallbackInterface);
            _timer = new(_scheduler);
            _keyInput = new(pollInputCallback);
            _video = new(_scheduler, presentFrameCallback);

            _communication.Initialize(_interruptControl);
            _timer.Initialize(_interruptControl);
            _dma.Initialize(_interruptControl, _memory);
            _keyInput.Initialize(_interruptControl);
            _interruptControl.Initialize(_cpu);
            _memory.Initialize(_communication, _timer, _sound, _dma, _keyInput, _systemControl, _interruptControl, _video, _bios);
            _video.Initialize(_dma, _interruptControl, _memory);

            _bios.Initialize(_cpu, _communication, _memory);
        }

        public override void Dispose()
        {
            if (_disposed)
                return;

            _memory.Dispose();
            _video.Dispose();

            _bios.Dispose();

            _disposed = true;
        }

        public override void ResetState()
        {
            _scheduler.ResetState();

            _cpu.ResetState();
            _communication.ResetState();
            _timer.ResetState();
            _sound.ResetState();
            _dma.ResetState();
            _keyInput.ResetState();
            _systemControl.ResetState();
            _interruptControl.ResetState();
            _memory.ResetState();
            _video.ResetState();

            _bios.Reset();
        }

        public override void LoadState(string filename)
        {
            using FileStream fileStream = File.Open(filename, FileMode.Open, FileAccess.Read);
            using DeflateStream deflateStream = new(fileStream, CompressionMode.Decompress);
            using BinaryReader reader = new(deflateStream, System.Text.Encoding.UTF8, false);

            if (reader.ReadString() != StateSaveMagic)
                throw new Exception();

            if (reader.ReadInt32() != StateSaveVersion)
                throw new Exception();

            if (reader.ReadString() != _romHash)
                throw new Exception();

            _scheduler.LoadState(reader);

            _cpu.LoadState(reader);
            _communication.LoadState(reader);
            _timer.LoadState(reader);
            _sound.LoadState(reader);
            _dma.LoadState(reader);
            _keyInput.LoadState(reader);
            _systemControl.LoadState(reader);
            _interruptControl.LoadState(reader);
            _memory.LoadState(reader);
            _video.LoadState(reader);
        }

        public override void SaveState(string filename)
        {
            using FileStream fileStream = File.Open(filename, FileMode.Create, FileAccess.Write);
            using DeflateStream deflateStream = new(fileStream, CompressionMode.Compress);
            using BinaryWriter writer = new(deflateStream, System.Text.Encoding.UTF8, false);

            writer.Write(StateSaveMagic);
            writer.Write(StateSaveVersion);
            writer.Write(_romHash);

            _scheduler.SaveState(writer);

            _cpu.SaveState(writer);
            _communication.SaveState(writer);
            _timer.SaveState(writer);
            _sound.SaveState(writer);
            _dma.SaveState(writer);
            _keyInput.SaveState(writer);
            _systemControl.SaveState(writer);
            _interruptControl.SaveState(writer);
            _memory.SaveState(writer);
            _video.SaveState(writer);
        }

        public override void LoadROM(string filename)
        {
            _memory.LoadROM(filename);

            using HashAlgorithm hashAlgorithm = SHA512.Create();
            using FileStream fileStream = File.OpenRead(filename);
            _romHash = BitConverter.ToString(hashAlgorithm.ComputeHash(fileStream));
        }

        public override void SetKeyStatus(Key key, KeyStatus status)
        {
            _keyInput.SetKeyStatus(key, status);
        }

        public override bool IsRunning()
        {
            return _running;
        }

        public override void Run()
        {
            _running = true;

            while (_running)
            {
                while (!_scheduler.HasTaskReady())
                {
                    UInt32 cycleCount = _cpu.Step();
                    _timer.UpdateAllCounters(cycleCount);
                    _scheduler.AdvanceCycleCounter(cycleCount);
                }

                _scheduler.ProcessTasks();
            }
        }

        public override void Pause()
        {
            _running = false;
        }
    }
}
