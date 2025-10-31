using Microsoft.JSInterop;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace AdventureGame.Editor.Pages
{
    public partial class MapPage
    {
        // Injected in razor via @inject; also available here if needed


        private IJSObjectReference? _mapModule;

        public async Task RegisterMapHelpersAsync()
        {
            if (_mapModule != null) return;
            try
            {
                // Import the ES module from the app's wwwroot
                _mapModule = await JS.InvokeAsync<IJSObjectReference>("import", "/js/MapHelpers.js");
            }
            catch
            {
                // swallow - best-effort
            }
        }

        public async Task EnableMapDragAsync(ElementReference el)
        {
            if (_mapModule is not null)
            {
                try { await _mapModule.InvokeVoidAsync("enableDrag", el); } catch { }
            }
        }

        public async Task DisableMapDragAsync(ElementReference el)
        {
            if (_mapModule is not null)
            {
                try { await _mapModule.InvokeVoidAsync("disableDrag", el); } catch { }
            }
        }

        public async ValueTask DisposeMapModuleAsync()
        {
            if (_mapModule is not null)
            {
                try { await _mapModule.DisposeAsync(); } catch { }
                _mapModule = null;
            }
        }
    }
}
