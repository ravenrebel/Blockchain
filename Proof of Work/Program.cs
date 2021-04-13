using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Proof_of_Work
{
    public static class Program
    {
        public static int StartingZeroes => 16;

        public struct Block
        {
            public string BlockHash { get; set; }
            public string Data { get; set; }
            public int Nonce { get; set; }
        }

        public static async Task Main(string[] args)
        {
            var data = Console.ReadLine();
            var blockHash = CalculateBlockHash(data, out var nonce);
            // blockHash = CalculateBlockHashParallel(data, out nonce);

            // blockHash[9] = (byte)(blockHash[9] == 0 ? 1 : 0);

            var json = JsonSerializer.Serialize(new Block
            {
                BlockHash = blockHash,
                Data = data,
                Nonce = nonce
            });

            await WriteAsync(json, "block.txt");
            var blockJson = await ReadAsync("block.txt");

            Console.WriteLine(Validate(JsonSerializer.Deserialize<Block>(blockJson)));
        }

        public static string CalculateBlockHashParallel(string text, out int nonce)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var hashValue = string.Empty;

            var lowestBreakIteration = Parallel.For(0, int.MaxValue, (i, state) =>
            {
                var bits = GetHash(text, i);

                // Console.WriteLine($"Nonce: {i} -- {bits}");

                if (bits.StartsWith(new string('0', StartingZeroes)))
                {
                    state.Break();
                    hashValue = bits;
                }
            }).LowestBreakIteration;

            if (lowestBreakIteration != null)
            {
                nonce = (int)lowestBreakIteration.Value;
            }
            else throw new InvalidOperationException();

            //nonce = Enumerable.Range(0, int.MaxValue).AsParallel()
            //    .First(i =>
            //    {
            //        var bits = GetHash(text, i);

            //        // Console.WriteLine($"Nonce: {nonce} -- {bits}");

            //        return bits.StartsWith(new string('0', StartingZeroes));
            //    });

            stopWatch.Stop();
            Console.WriteLine(stopWatch.ElapsedMilliseconds);

            return hashValue;
        }

        public static string CalculateBlockHash(string text, out int nonce)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            nonce = 0;

            while (true)
            {
                var bits = GetHash(text, nonce);

                // Console.WriteLine($"Nonce: {nonce} -- {bits}");

                if (bits.StartsWith(new string('0', StartingZeroes)))
                {
                    stopWatch.Stop();
                    Console.WriteLine(stopWatch.ElapsedMilliseconds);

                    return bits;
                }

                nonce++;
            }
        }

        public static bool Validate(Block block)
        {
            return GetHash(block.Data, block.Nonce) == block.BlockHash;
        }

        private static async Task<string> ReadAsync(string fileName)
        {
            char[] buffer;

            using (var sr = new StreamReader(Path.Combine(Directory.GetCurrentDirectory(), fileName)))
            {
                buffer = new char[(int)sr.BaseStream.Length];
                await sr.ReadAsync(buffer, 0, (int)sr.BaseStream.Length);
            }

            return new string(buffer);
        }

        private static async Task WriteAsync(string text, string fileName)
        {
            await using var sw = new StreamWriter(Path.Combine(Directory.GetCurrentDirectory(), fileName));
            await sw.WriteAsync(text);
        }

        private static string GetHash(string data, int nonce)
        {
            using var mySha256 = SHA256.Create();
            var hashValue = mySha256.ComputeHash(Encoding.UTF8.GetBytes(data + nonce));

            return new BitArray(hashValue).ToBitString();
        }
    }
}
