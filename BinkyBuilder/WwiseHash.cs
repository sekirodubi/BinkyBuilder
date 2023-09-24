using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace BinkyBuilder
{
    // from https://github.com/Lolginer/misc/blob/master/WwiseHash.cs
    internal class WwiseHash
    {
        ///<summary>Generates ShortID values for objects such as Actions and Sounds.</summary>
        public static uint HashGUID(string ID)
        {
            Regex alphanum = new Regex("[^0-9A-Za-z]");
            string filtered = alphanum.Replace(ID, "");
            List<byte> guidBytes = new List<byte>();
            int[] byteOrder = { 3, 2, 1, 0, 5, 4, 7, 6, 8, 9, 10, 11, 12, 13, 14, 15 }; //Mixed-endian order
            for (int i = 0; i < byteOrder.Length; i++)
            {
                guidBytes.Add(
                    byte.Parse(
                        filtered.Substring(byteOrder[i] * 2, 2),
                        NumberStyles.HexNumber,
                        CultureInfo.InvariantCulture
                ));
            }
            return FnvHash(guidBytes.ToArray(), false);
        }

        ///<summary>Generates ShortID values for Events, SoundBanks and GameSyncs.
        ///<para>Note: This should NOT be explicitly declared in XML files.</para></summary>
        public static uint HashString(string Name)
        {
            return FnvHash(Encoding.ASCII.GetBytes(Name.ToLowerInvariant()), true);
        }

        static uint FnvHash(byte[] input, bool use32bits)
        {
            uint prime = 16777619;
            uint offset = 2166136261;
            uint mask = 1073741823; //Bitmask for first 30 bits

            uint hash = offset;
            for (int i = 0; i < input.Length; i++)
            {
                hash *= prime;
                hash ^= input[i];
            }

            if (use32bits)
            {
                return hash;
            }
            else
            {
                return (hash >> 30) ^ (hash & mask); //XOR folding
            }
        }
    }
}
