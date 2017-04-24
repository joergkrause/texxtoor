using System.ComponentModel.DataAnnotations;
using Texxtoor.BaseLibrary.Core;
using Texxtoor.Portal.Core.Extensions.Attributes;

namespace Texxtoor.EasyAuthor.Models
{

    public enum ProjectTemplate
    {
        [Display(Name = "No Template (Empty)")]
        NoTemplate,

        [Display(Name = "Blog Article")]
        Article,

        [Display(Name = "Textbook")]
        Textbook,

        [Display(Name = "Bachelor Thesis")]
        BachelorThesis,

        [Display(Name = "Master Thesis")]
        MasterThesis
    }
    
    public class ProjectDto
    {

        [NgField(CaseNotion.CamelCase)]
        [Display(Name = "Project Title", Description = "The title helps you and others to identify the project. It's not the title when you publish something.", Order = 20)]
        [Required(ErrorMessage = "You must provide a title")]
        [StringLength(100, ErrorMessage = "Title should not have more than 100 characters")]
        [Watermark("Project Title")]
        public string Title { get; set; }

        [NgField(CaseNotion.CamelCase)]
        [Display(Name = "Content Language", Description = "The language of the first text in the project. Subsequent iterations, such as translations, may have different languages.", Order = 30)]
        [UIHint("LanguageDropDown")]
        public string Language { get; set; }

        [NgField(CaseNotion.CamelCase)]
        [Display(Name = "Field or Subject", Description = "Keywords help to automate some later steps, such as overall appearance and catalog selection.", Order = 40)]
        [StringLength(250, ErrorMessage = "Subject should not have more than 250 characters")]
        [Required(ErrorMessage = "Please provide some keywords")]
        [Watermark("Some buzzwords...")]
        public string Keywords { get; set; }

        [NgField(CaseNotion.CamelCase)]
        [Display(Name = "Type of Project", Description = "The type determines the structure of your text once you begin. You can change the structure any time.", Order = 10)]
        public ProjectTemplate TemplateType { get; set; }


    }
}