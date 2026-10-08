using System;
using System.IO;
using UnityEngine;

public static class SavWav
{
    public static byte[] GetWavBytes(AudioClip clip)
    {
        using (var memoryStream = new MemoryStream())
        {
            var writer = new BinaryWriter(memoryStream);

            int samples = clip.samples;
            int channels = clip.channels;
            int frequency = clip.frequency;

            float[] data = new float[samples * channels];
            clip.GetData(data, 0);

            // Header WAV (RIFF standard a 44 byte)
            writer.Write(new char[4] { 'R', 'I', 'F', 'F' });
            writer.Write(36 + samples * 2);
            writer.Write(new char[4] { 'W', 'A', 'V', 'E' });
            writer.Write(new char[4] { 'f', 'm', 't', ' ' });
            writer.Write(16);
            writer.Write((ushort)1); // Audio formato PCM
            writer.Write((ushort)channels);
            writer.Write(frequency);
            writer.Write(frequency * channels * 2); // Byte rate
            writer.Write((ushort)(channels * 2));   // Block align
            writer.Write((ushort)16);               // Bits per sample
            writer.Write(new char[4] { 'd', 'a', 't', 'a' });
            writer.Write(samples * 2);

            // Conversione float (-1.0f a 1.0f) in short a 16 bit
            short[] intData = new short[data.Length];
            byte[] bytesData = new byte[data.Length * 2];
            int rescaleFactor = 32767;

            for (int i = 0; i < data.Length; i++)
            {
                intData[i] = (short)(data[i] * rescaleFactor);
                byte[] byteArr = BitConverter.GetBytes(intData[i]);
                byteArr.CopyTo(bytesData, i * 2);
            }

            writer.Write(bytesData);
            return memoryStream.ToArray();
        }
    }
}