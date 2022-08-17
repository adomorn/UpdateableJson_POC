using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApplication5.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IOptionsMonitor<Test> test;
        private readonly IConfigurationRoot test1;
        [BindProperty(SupportsGet = true)]
        public string JsonFile { get; set; }
        [BindProperty(SupportsGet = true)] public SortedDictionary<string, string> Props { get; set; }
        [BindProperty] public int Props1 { get; set; }
        public List<SelectListItem> Options { get; set; }
        public IndexModel(ILogger<IndexModel> logger, IOptionsMonitor<Test> test, IConfigurationRoot test1)
        {
            _logger = logger;
            this.test = test;
            this.test1 = test1;
        }

        public void OnGet()
        {
          
            var g = ConfigManipulator.GetJsonFiles(test1);
            if (string.IsNullOrEmpty(JsonFile))
                JsonFile = g.FirstOrDefault();
            Options = g.Select(a =>
                new SelectListItem
                {
                    Value = a,
                    Text = a
                }).ToList();

            Props = ConfigManipulator.GetValues(test1, JsonFile);

            var b = test.CurrentValue;

        }

        public void OnPost()
        {
         
            var g = ConfigManipulator.GetJsonFiles(test1);
            Options = g.Select(a =>
                new SelectListItem
                {
                    Value = a,
                    Text = a
                }).ToList();
            ConfigManipulator.UpdateValues(new Dictionary<string, string>(Props), JsonFile);

           

           
        }

        public void Deneme()
        {
            var a = test.CurrentValue.Deneme;
        }
    }
}