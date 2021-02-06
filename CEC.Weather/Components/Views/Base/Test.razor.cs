using CEC.Blazor.SPA.Components;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;
using CEC.Blazor.Core;
using Microsoft.AspNetCore.Components.Forms;
using System.Linq;
using Microsoft.AspNetCore.Components.Web;

namespace CEC.Weather.Components.Views
{
    public partial class Test : ComponentBase, IView
    {
        [CascadingParameter]
        public ViewManager ViewManager { get; set; }

        public class EditData
        {
            public int d1 { get; set; }
        }

        public EditData MyData { get; set; } = new EditData() { d1 = 4 };

        public EditContext editContext { get; set; }

        private List<string> identifiers = new List<string> {
            "Aaa",
            "Aab",
            "Aba",
            "Aca",
            "Baa",
            "Bba"
        };

        private List<string> identifiersUI = new List<string>();

        private Dictionary<string, ElementReference> refs
        {
            get
            {
                return _refs;
            }
            set
            {
                _refs = value;
            }
        }

        private Dictionary<string, ElementReference> _refs = new Dictionary<string, ElementReference>();
        protected override async Task OnInitializedAsync()
        {
            this.editContext = new EditContext(MyData);
            identifiersUI = identifiers;
            await base.OnInitializedAsync();
        }

        private void FilterIdentifiers(ChangeEventArgs e)
        {
            string input = e.Value.ToString();
            //refs.Clear();
            identifiersUI = identifiers.Where(s => s.ToUpper().StartsWith(input.ToUpper())).ToList();

        }

        private void HandleKBEvents(KeyboardEventArgs e)
        {
            if (e.Code == "ArrowDown")
            {
                refs[identifiersUI[0]].FocusAsync();

            }
        }

        private void FilterIdentifiersV2(ChangeEventArgs e)
        {
            string input = e.Value.ToString();
            var id = identifiers.Where(s => s.ToUpper().StartsWith(input.ToUpper())).ToList();

            //remove removed elements from refs
            var elements = identifiersUI.Except(id);
            identifiersUI = id;

            foreach (var element in elements)
            {
                refs.Remove(element);
            }
        }


    }
}
