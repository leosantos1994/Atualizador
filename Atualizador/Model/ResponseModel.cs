using System;
using System.Collections.Generic;

namespace UpdaterService.Model
{
    public class ResponseModel
    {
        public string? Log { get; set; }
        public Guid? ServiceId { get; set; }
        public bool? Complete { get; set; }
    }
}
