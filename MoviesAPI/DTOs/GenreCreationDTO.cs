﻿using MoviesAPI.Validators;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MoviesAPI.DTOs
{
    public class GenreCreationDTO
    {
        [Required(ErrorMessage = "The field with name {0} is reqired")]
        [StringLength(50)]
        [FirstLetterUppercase]
        public string Name { get; set; }
    }
}
