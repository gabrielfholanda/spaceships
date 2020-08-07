using System;
using System.Collections.Generic;
using System.Text;

namespace StartshipsStops.model
{
    public class Spaceship
    {
        public int count { get; set; }
        public string next { get; set; }
        public object previous { get; set; }
        public List<Result> results { get; set; }
    }
}
