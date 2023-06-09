﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MoviesAPI.Entities
{
    public class Movie
    {
        public int Id { get; set; }
        [Required]
        [StringLength(maximumLength: 100)]
        public string Title { get; set; }
        public string Summary { get; set; }
        public bool inTheaters { get; set; }
        public string Trailer { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string Poster { get; set; }
        public List<MoviesGenres> MoviesGenres { get; set; }
        public List<MoviesMovieTheaters> MoviesMovieTheaters { get; set; }
        public List<MoviesActors> MoviesActors { get; set; }
    }
}
