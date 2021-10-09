using System.Collections.Generic;

namespace GenomeTools.ChemistryLibrary.IO.Sam
{
    public class SamAlignmentEntry
    {
        public string Qname { get; }
        public SamAlignmentFlag Flag { get; }
        public string Rname { get; }
        public int Pos { get; }
        public int Mapq { get; }
        public string Cigar { get; }
        public string Rnext { get; }
        public int Pnext { get; }
        public int Tlen { get; }
        public string Seq { get; }
        public string Qual { get; }
        public Dictionary<string,object> OptionalFields { get; }

        public SamAlignmentEntry(
            string qname, SamAlignmentFlag flag, string rname,
            int pos, int mapq, string cigar,
            string rnext, int pnext, int tlen,
            string seq, string qual, 
            Dictionary<string, object> optionalFields)
        {
            Qname = qname;
            Flag = flag;
            Rname = rname;
            Pos = pos;
            Mapq = mapq;
            Cigar = cigar;
            Rnext = rnext;
            Pnext = pnext;
            Tlen = tlen;
            Seq = seq;
            Qual = qual;
            OptionalFields = optionalFields ?? new Dictionary<string, object>();
        }
    }
}