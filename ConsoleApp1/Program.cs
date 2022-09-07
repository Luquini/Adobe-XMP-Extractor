using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace ConsoleApp1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Iniciando...");

            var Parser = new AssetsParser();

            Parser.Load("C:\\Users\\vitor\\OneDrive\\Área de Trabalho\\test\\", "game.spr");

            Console.ReadLine();
        }
    }

    public class AssetsParser
    {
        FileStream fs = null;
        
        byte[] start_pattern = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

        byte[] end_pattern = new byte[] { 0x49, 0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82 };
        
        public bool Load(string path, string fileName)
        {
            if (fs == null)
            {
                fs = File.Open(path + fileName, FileMode.Open, FileAccess.Read,
                FileShare.Read);
            }

            var br = new BinaryReader(fs);

            int file_length = Convert.ToInt32(fs.Length);

            var file_buffer = new byte[file_length];

            br.Read(file_buffer, 0, file_length);

            br.Dispose();

            var start_matchs = IndexOfSequence(file_buffer, start_pattern, 0);
            var end_matchs = IndexOfSequence(file_buffer, end_pattern, 0);

            int count = 0;

            Console.WriteLine($"Número de Assets: {start_matchs.Count()}");

            Thread.Sleep(5000);

            start_matchs.ForEach(matchStart =>
            {
                var index = start_matchs.IndexOf(matchStart);
                var match_length = end_matchs[index] - matchStart;
                var match_buffer = new byte[match_length];

                using (var reader = new FileStream(path + fileName, FileMode.Open, FileAccess.Read))
                {
                    reader.Seek(matchStart, SeekOrigin.Begin);
                    reader.Read(match_buffer, 0, match_length);
                }

                ByteArrayToFile(path + $"asset_{count}.png", match_buffer);
                Console.WriteLine($"Asset Extraído: asset_{count}.png");
                count++;
            });

            Console.WriteLine($"Assets Extraídos: {start_matchs.Count()}");

            fs.Dispose();
            return true;
        }

        public bool ByteArrayToFile(string fileName, byte[] byteArray)
        {
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                fs.Write(byteArray, 0, byteArray.Length);
                return true;
            }      
        }

        public List<int> IndexOfSequence(byte[] buffer, byte[] pattern, int startIndex)
        {
            List<int> positions = new List<int>();
            int i = Array.IndexOf<byte>(buffer, pattern[0], startIndex);
            while (i >= 0 && i <= buffer.Length - pattern.Length)
            {
                byte[] segment = new byte[pattern.Length];
                Buffer.BlockCopy(buffer, i, segment, 0, pattern.Length);
                if (segment.SequenceEqual<byte>(pattern))
                    positions.Add(i);
                i = Array.IndexOf<byte>(buffer, pattern[0], i + 1);
            }
            return positions;
        }
    }
}
