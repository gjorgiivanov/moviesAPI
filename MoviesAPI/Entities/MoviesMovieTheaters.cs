using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoviesAPI.Entities
{
    public class MoviesMovieTheaters
    {
        public int MovieTheaterId { get; set; }
        public int MovieId { get; set; }
        public MovieTheater movieTheater { get; set; }
        public Movie Movie { get; set; }
    }
}
