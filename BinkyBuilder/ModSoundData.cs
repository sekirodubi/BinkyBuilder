using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileHelpers;

namespace BinkyBuilder
{
    [DelimitedRecord("|")]
    internal class ModSoundData
    {
        public string NpcId;

        public string Dialogue;

        public int DialogueSet;

        public string Wav;
    }
}
