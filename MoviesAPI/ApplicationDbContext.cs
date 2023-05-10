using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace MoviesAPI
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext([NotNullAttribute] DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MoviesGenres>().HasKey(z => new { z.MovieId, z.GenreId });
            modelBuilder.Entity<MoviesMovieTheaters>().HasKey(z => new { z.MovieId, z.MovieTheaterId });
            modelBuilder.Entity<MoviesActors>().HasKey(z => new { z.MovieId, z.ActorId });

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Genre> Genres { get; set; }
        public DbSet<Actor> Actors { get; set; }
        public DbSet<MovieTheater> MovieTheaters { get; set; }
        public DbSet<Movie> Movies { get; set; }

        public DbSet<MoviesGenres> MoviesGenres { get; set; }
        public DbSet<MoviesMovieTheaters> MoviesMovieTheaters { get; set; }
        public DbSet<MoviesActors> MoviesActors { get; set; }

        public DbSet<Rating> Ratings { get; set; }
    }
}
