using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.DTOs;
using MoviesAPI.Entities;
using MoviesAPI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoviesAPI.Controllers
{
    [ApiController]
    [Route("/api/movies")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "IsAdmin")]
    public class MoviesController: ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IFileStorageService fileStorageService;
        private readonly UserManager<IdentityUser> userManager;
        private readonly string containerName = "movies";

        public MoviesController(ApplicationDbContext context, IMapper mapper, IFileStorageService fileStorageService, UserManager<IdentityUser> userManager)
        {
            this.context = context;
            this.mapper = mapper;
            this.fileStorageService = fileStorageService;
            this.userManager = userManager;
        }
        
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<HomeDTO>> Get()
        {
            const int top = 6;
            var today = DateTime.Today;

            var inTheaters = await context.Movies.Where(z => z.inTheaters).OrderBy(z => z.ReleaseDate).Take(top).ToListAsync();
            var upcomingReleases = await context.Movies.Where(z => z.ReleaseDate > today).OrderBy(z => z.ReleaseDate).Take(top).ToListAsync();

            var homeDTO = new HomeDTO();
            homeDTO.InTheaters = mapper.Map<List<MovieDTO>>(inTheaters);
            homeDTO.UpcomingReleases = mapper.Map<List<MovieDTO>>(upcomingReleases);

            return homeDTO;
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<MovieDTO>> Get(int id)
        {
            var movie = await context.Movies
                .Include(z => z.MoviesGenres).ThenInclude(z => z.Genre)
                .Include(z => z.MoviesMovieTheaters).ThenInclude(z => z.movieTheater)
                .Include(z => z.MoviesActors).ThenInclude(z => z.Actor)
                .FirstOrDefaultAsync(z => z.Id == id);

            if (movie == null)
            {
                return NotFound();
            }

            var averageVote = 0.0;
            var userVote = 0;

            if(await context.Ratings.AnyAsync(z => z.MovieId == id))
            {
                averageVote = await context.Ratings.Where(z => z.MovieId == id).AverageAsync(z => z.Rate);

                if (HttpContext.User.Identity.IsAuthenticated)
                {
                    var email = HttpContext.User.Claims.FirstOrDefault(z => z.Type == "email").Value;
                    var user = await userManager.FindByEmailAsync(email);
                    var userId = user.Id;

                    var rateingDb = await context.Ratings.FirstOrDefaultAsync(z => z.MovieId == id && z.UserId == userId);
                    if (rateingDb != null)
                    {
                        userVote = rateingDb.Rate;
                    }
                }
            }

            var movieDTO = mapper.Map<MovieDTO>(movie);

            movieDTO.AverageVote = averageVote;
            movieDTO.UserVote = userVote;
            movieDTO.Actors = movieDTO.Actors.OrderBy(z => z.Order).ToList();

            return movieDTO;
        }

        [HttpGet("filter")]
        [AllowAnonymous]
        public async Task<ActionResult<List<MovieDTO>>> Filter([FromQuery] FilterMoviesDTO filterMoviesDTO)
        {
            var moviesQueryable = context.Movies.AsQueryable();

            if (!string.IsNullOrEmpty(filterMoviesDTO.Title))
            {
                moviesQueryable = moviesQueryable.Where(z => z.Title.Contains(filterMoviesDTO.Title));
            }
            if (filterMoviesDTO.InTheaters)
            {
                moviesQueryable = moviesQueryable.Where(z => z.inTheaters);
            }
            if (filterMoviesDTO.UpcomingReleases)
            {
                var today = DateTime.Today;
                moviesQueryable = moviesQueryable.Where(z => z.ReleaseDate > today);
            }
            if (filterMoviesDTO.GenreId > 0)
            {
                moviesQueryable = moviesQueryable.Where(
                    z => z.MoviesGenres.Select(x => x.GenreId).Contains(filterMoviesDTO.GenreId)
                    );
            }

            await HttpContext.InsertParametersPaginationInHeaders(moviesQueryable);
            var movies = await moviesQueryable.OrderBy(z => z.Title).Paginate(filterMoviesDTO.PaginationDTO).ToListAsync();

            return mapper.Map<List<MovieDTO>>(movies);
        }

        [HttpGet("PostGet")]
        public async Task<ActionResult<MoviePostGetDTO>> PostGet()
        {
            var movieTheaters = await context.MovieTheaters.OrderBy(z => z.Name).ToListAsync();
            var genres = await context.Genres.OrderBy(z => z.Name).ToListAsync();

            var movieTheatersDTO = mapper.Map<List<MovieTheaterDTO>>(movieTheaters);
            var genresDTO = mapper.Map<List<GenreDTO>>(genres);

            return new MoviePostGetDTO() { MovieTheaters = movieTheatersDTO, Genres = genresDTO };
        }

        [HttpPost]
        public async Task<ActionResult<int>> Post([FromForm] MovieCreationDTO movieCreationDTO)
        {
            var movie = mapper.Map<Movie>(movieCreationDTO);

            if(movieCreationDTO.Poster != null)
            {
                movie.Poster = await fileStorageService.SaveFile(containerName, movieCreationDTO.Poster);
            }

            AnnotateActorsOrder(movie);
            context.Add(movie);
            await context.SaveChangesAsync();

            return movie.Id;
        }

        [HttpGet("putget/{id:int}")]
        public async Task<ActionResult<MoviePutGetDTO>> PutGet(int id)
        {
            var movieActionResult = await Get(id);

            if (movieActionResult.Result is NotFoundResult)
            {
                return NotFound();
            }

            var movie = movieActionResult.Value;

            var selectedGenresIds = movie.Genres.Select(z => z.Id).ToList();
            var nonSelectedGenres = await context.Genres.Where(z => !selectedGenresIds.Contains(z.Id)).ToListAsync();

            var selectedMovieTheatesIds = movie.MovieTheaters.Select(z => z.Id).ToList();
            var nonSelectedMovieTheaters = await context.MovieTheaters.Where(z => !selectedMovieTheatesIds.Contains(z.Id)).ToListAsync();

            var nonSelectedGenresDTOs = mapper.Map<List<GenreDTO>>(nonSelectedGenres);
            var nonSelectedMovieTheatesDTOs = mapper.Map<List<MovieTheaterDTO>>(nonSelectedMovieTheaters);

            var response = new MoviePutGetDTO();
            response.Movie = movie;
            response.SelectedGenres = movie.Genres;
            response.NonSelectedGenres = nonSelectedGenresDTOs;
            response.SelectedMovieTheaters = movie.MovieTheaters;
            response.NonSelectedMovieTheaters = nonSelectedMovieTheatesDTOs;
            response.Actors = movie.Actors;

            return response;
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, [FromForm] MovieCreationDTO movieCreationDTO)
        {
            var movie = await context.Movies
                .Include(z => z.MoviesGenres)
                .Include(z => z.MoviesMovieTheaters)
                .Include(z => z.MoviesActors)
                .FirstOrDefaultAsync(z => z.Id == id);

            if (movie == null)
            {
                return NotFound();
            }

            movie = mapper.Map(movieCreationDTO, movie);

            if (movieCreationDTO.Poster != null)
            {
                movie.Poster = await fileStorageService.EditFile(containerName, movieCreationDTO.Poster, movie.Poster);
            }

            AnnotateActorsOrder(movie);
            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var movie = await context.Movies.FirstOrDefaultAsync(z => z.Id == id);

            if (movie == null)
            {
                return NotFound();
            }

            context.Remove(movie);
            await context.SaveChangesAsync();
            await fileStorageService.DeleteFile(movie.Poster, containerName);

            return NoContent();
        }

        private void AnnotateActorsOrder(Movie movie)
        {
            if (movie.MoviesActors != null)
            {
                for (int i = 0; i < movie.MoviesActors.Count; i++)
                {
                    movie.MoviesActors[i].Order = i;
                }
            }
        }
    }
}
