using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QData.QueryContainerProvider.Test
{
    public class WasserwerkSearchIndex
    {
        public int Id { get; set; }

        /// <summary>
        /// Bezeichnung des Wasserwerks.
        /// </summary>
        public string Bezeichnung { get; set; }

        /// <summary>
        /// Bezeichnung des Wasserwerks für ElasticSearch-Suche.
        /// </summary>
       
        public string BezeichnungRaw { get; set; }

        /// <summary>
        /// Adresse des Wasserwerks
        /// </summary>
        public string Standort { get; set; }

        /// <summary>
        /// Adresse des Wasserwerks für ElasticSearch-Suche.
        /// </summary>
      
        public string StandortRaw { get; set; }

        /// <summary>
        /// Wasserqualität im Versorgungsgebiet
        /// </summary>
        /// <remarks>
        /// aktuell nur Härtegrad ggf. später erweitern durch weitere Werte
        /// </remarks>
        public string Wasserqualitaet { get; set; }

        /// <summary>
        /// Wasserqualität im Versorgungsgebiet
        /// </summary>
       
        public string WasserqualitaetRaw { get; set; }
    }
}
