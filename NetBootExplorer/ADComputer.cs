using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetBootExplorer
{
    class ADComputer
    {
        public string name { get; set; }
        public string netbootGUID { get; set; }
        public string DN { get; set; }

        public ADComputer(string name, string guid, string dn)
        {
            this.name = name;
            this.netbootGUID = guid;
            this.DN = dn;
        }
        
        public string getName()
        {
            return this.name;
        }

        public string getNetBootGUID()
        {
            return this.netbootGUID;
        }

        public string getDN()
        {
            return this.DN;
        }


    }
}
