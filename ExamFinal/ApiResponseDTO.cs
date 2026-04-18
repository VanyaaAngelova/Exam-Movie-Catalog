using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ExamFinal
{
    internal class ApiResponseDTO
    {
        [JsonPropertyName("msg")]

        public string Msg { get; set; }

        [JsonPropertyName("movie")]
        public MovieDTO movie { get; set; } = new MovieDTO();
    }
}
