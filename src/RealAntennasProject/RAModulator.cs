﻿using System;

namespace RealAntennas
{
    public class RAModulator
    {
        public double Frequency { get; set; }       // Frequency in Hz
        public double SymbolRate { get; set; }      // Samples / sec.
        public int ModulationBits { get; set; }     // Bits / symbol (0=OOK, 1=BPSK, 2=QPSK, 3=8-PSK, 4=16-QAM,...
        public int MinModulationBits { get; set; }  // Min modulation supported
        public double NoiseFigure { get => 2 + ((10 - TechLevel) * 0.8); }  // Noise figure of receiver electronics in dB
        public int TechLevel { get; set; }
        public double SpectralEfficiency { get => Math.Max(0.01, 1 - (1 / Math.Pow(2, TechLevel))); }
        public double DataRate { get => SymbolRate * ModulationBits; }              // Data Rate in bits/sec.
        public double Bandwidth { get => SymbolRate / SpectralEfficiency; }         // RF bandwidth required.

        // Given Bandwidth, DataRate and SpectralEfficiency, compute minimum C/I from Shannon-Hartley.
        // C = B log_2 (1 + SNR), where C=Channel Capacity and SNR is linear.  10*Log10(SNR) to convert to dB.
        // We will substitute C = (DateRate / SpectralEfficiency) to account for non-ideal performance
        // Actually, let's just skip to digital land and specify required C/I (Es/No) as a function of modulation.
        // And derive bandwidth as a function of the symbol rate and the spectral efficiency.

        // Use 6, 10, 14, 18, 21, 23.5, 27, 29, 31 for 1e-6 pErr
        //     , BPSK, QPSK, 8PSK, 16QAM, 32QAM, 64QAM, 128QAM, 256QAM
        public readonly double[] QAM_CI = { 6, 10, 14, 18, 21, 23.5, 27, 29, 31 };
        public virtual double RequiredCI() => RequiredCI(ModulationBits);
        public virtual double RequiredCI(int modulationBits)
        {
            if (modulationBits < QAM_CI.Length) return QAM_CI[modulationBits];
            return QAM_CI[QAM_CI.Length - 1];
        }
        public virtual bool Compatible(RAModulator other)
        {
            // Test frequency range and minimum modulation order
            if (Frequency > other.Frequency * 1.1) return false;
            if (other.Frequency > Frequency * 1.1) return false;
            if (MinModulationBits > other.ModulationBits) return false;
            if (other.MinModulationBits > ModulationBits) return false;
            return true;
        }
        public virtual bool SupportModulation(int bits) => bits >= MinModulationBits && bits <= ModulationBits;

        public override string ToString() => $"{BitsToString(ModulationBits)} {DataRate} bps";

        public virtual string BitsToString(int bits)
        {
            switch(bits)
            {
                case 0: return "OOK";
                case 1: return "BPSK";
                case 2: return "QPSK";
                case 3: return "8PSK";
                default: return $"{Math.Pow(2, bits):N0}-QAM";
            }
        }
        public RAModulator() : this(1, 1, 0, 0, 0) { }
        public RAModulator(RAModulator orig) : this(orig.Frequency, orig.SymbolRate, orig.ModulationBits, orig.MinModulationBits, orig.TechLevel) { }
        public RAModulator(double frequency, double symbolRate, int modulationBits, int minModulationBits, int techLevel)
        {
            Frequency = frequency;
            SymbolRate = symbolRate;
            ModulationBits = modulationBits;
            MinModulationBits = minModulationBits;
            TechLevel = techLevel;
        }
        public void Copy(RAModulator orig)
        {
            Frequency = orig.Frequency;
            SymbolRate = orig.SymbolRate;
            ModulationBits = orig.ModulationBits;
            MinModulationBits = orig.MinModulationBits;
            TechLevel = orig.TechLevel;
        }

        public void LoadFromConfigNode(ConfigNode config)
        {
            Frequency = double.Parse(config.GetValue("Frequency"));
            SymbolRate = double.Parse(config.GetValue("SymbolRate"));
            ModulationBits = int.Parse(config.GetValue("ModulationBits"));
            MinModulationBits = int.Parse(config.GetValue("MinModulationBits"));
            TechLevel = int.Parse(config.GetValue("TechLevel"));
        }
    }
}
