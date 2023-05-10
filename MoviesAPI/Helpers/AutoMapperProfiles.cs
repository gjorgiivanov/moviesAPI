using AutoMapper;
using Microsoft.AspNetCore.Identity;
using MoviesAPI.DTOs;
using MoviesAPI.Entities;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoviesAPI.Helpers
{
    public class AutoMapperProfiles: Profile
    {
        public AutoMapperProfiles(GeometryFactory geometryFactory)
        {
            CreateMap<GenreDTO, Genre>().ReverseMap();
            CreateMap<GenreCreationDTO, Genre>();

            CreateMap<ActorDTO, Actor>().ReverseMap();
            CreateMap<ActorCreationDTO, Actor>().ForMember(z => z.Picture, options => options.Ignore());

            CreateMap<MovieTheater, MovieTheaterDTO>()
                .ForMember(z => z.Latitude, dto => dto.MapFrom(prop => prop.Point.Y))
                .ForMember(z => z.Longitude, dto => dto.MapFrom(prop => prop.Point.X));

            CreateMap<MovieTheaterCreationDTO, MovieTheater>()
                .ForMember(z => z.Point, z => z.MapFrom(
                    dto => geometryFactory.CreatePoint(new Coordinate(dto.Longitude, dto.Latitude))
                    )
                );

            CreateMap<MovieCreationDTO, Movie>()
                .ForMember(z => z.Poster, options => options.Ignore())
                .ForMember(z => z.MoviesGenres, options => options.MapFrom(MapMoviesGenres))
                .ForMember(z => z.MoviesMovieTheaters, options => options.MapFrom(MapMoviesMovieTheaters))
                .ForMember(z => z.MoviesActors, options => options.MapFrom(MapMoviesActors));

            CreateMap<Movie, MovieDTO>()
                .ForMember(z => z.Genres, options => options.MapFrom(MapMoviesGenres))
                .ForMember(z => z.MovieTheaters, options => options.MapFrom(MapMoviesMovieTheaters))
                .ForMember(z => z.Actors, options => options.MapFrom(MapMoviesActors));

            CreateMap<IdentityUser, UserDTO>();
        }

        private List<MoviesGenres> MapMoviesGenres(MovieCreationDTO movieCreationDTO, Movie movie)
        {
            var result = new List<MoviesGenres>();

            if(movieCreationDTO.GenresIds == null)
            {
                return result;
            }

            foreach(var id in movieCreationDTO.GenresIds)
            {
                result.Add(new MoviesGenres() { GenreId = id });
            }

            return result;
        }

        private List<MoviesMovieTheaters> MapMoviesMovieTheaters(MovieCreationDTO movieCreationDTO, Movie movie)
        {
            var result = new List<MoviesMovieTheaters>();

            if (movieCreationDTO.MovieTheatersIds == null)
            {
                return result;
            }

            foreach (var id in movieCreationDTO.MovieTheatersIds)
            {
                result.Add(new MoviesMovieTheaters() { MovieTheaterId = id });
            }

            return result;
        }

        private List<MoviesActors> MapMoviesActors(MovieCreationDTO movieCreationDTO, Movie movie)
        {
            var result = new List<MoviesActors>();

            if (movieCreationDTO.Actors == null)
            {
                return result;
            }

            foreach (var actor in movieCreationDTO.Actors)
            {
                result.Add(new MoviesActors() { ActorId = actor.Id, Character = actor.Character });
            }

            return result;
        }

        private List<GenreDTO> MapMoviesGenres(Movie movie, MovieDTO movieDTO)
        {
            var result = new List<GenreDTO>();

            if (movie.MoviesGenres != null)
            {
                foreach (var genre in movie.MoviesGenres)
                {
                    result.Add(new GenreDTO() { Id = genre.GenreId, Name = genre.Genre.Name });
                }
            }

            return result;
        }

        private List<MovieTheaterDTO> MapMoviesMovieTheaters(Movie movie, MovieDTO movieDTO)
        {
            var result = new List<MovieTheaterDTO>();

            if (movie.MoviesGenres != null)
            {
                foreach (var movieTheater in movie.MoviesMovieTheaters)
                {
                    result.Add(new MovieTheaterDTO() {
                        Id = movieTheater.MovieTheaterId,
                        Name = movieTheater.movieTheater.Name,
                        Latitude = movieTheater.movieTheater.Point.Y,
                        Longitude = movieTheater.movieTheater.Point.X,
                    });
                }
            }

            return result;
        }

        private List<ActorsMoviesDTO> MapMoviesActors(Movie movie, MovieDTO movieDTO)
        {
            var result = new List<ActorsMoviesDTO>();

            if (movie.MoviesActors != null)
            {
                foreach (var actor in movie.MoviesActors)
                {
                    result.Add(new ActorsMoviesDTO() {
                        Id = actor.ActorId,
                        Name = actor.Actor.Name,
                        Character = actor.Character,
                        Order = actor.Order,
                        Picture = actor.Actor.Picture,
                    });
                }
            }

            return result;
        }
    }
}
