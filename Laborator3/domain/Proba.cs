using System.Collections.Generic;

namespace Laborator3.domain
{
    public class Proba : Entity<long>
    {
        public string Distance { get; set; }
        public string Style { get; set; }
        
       
       

        public Proba(long id, string distance, string style)
        {
            Id = id;
            Distance = distance;
            Style = style;
           
        }
    }
}