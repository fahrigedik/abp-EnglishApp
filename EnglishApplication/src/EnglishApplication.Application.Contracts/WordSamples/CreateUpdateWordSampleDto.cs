using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.UI.Bootstrap.TagHelpers.Form;

namespace EnglishApplication.WordSamples;

public class CreateUpdateWordSampleDto
{

    [Required]
    [HiddenInput]
    public Guid WordId { get; set; }


    [Display(Name = "Sample")]
    public string Sample { get; set; }
}